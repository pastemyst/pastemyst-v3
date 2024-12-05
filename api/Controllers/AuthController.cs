using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using OpenIddict.Abstractions;
using OpenIddict.Client.WebIntegration;
using pastemyst.Exceptions;
using pastemyst.Models;
using pastemyst.Services;
using Settings = pastemyst.Models.Settings;

namespace pastemyst.Controllers;

[ApiController]
[Route("/api/v3")]
public class AuthController(
    MongoService mongoService,
    IConfiguration configuration,
    IHttpClientFactory httpClientFactory,
    ImageService imageService,
    ActionLogger actionLogger,
    IdProvider idProvider
    ) : ControllerBase
{
    [HttpGet("login/{provider}")]
    public IActionResult Login(string provider)
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = HttpContext.Request.GetEncodedUrl()
        };
        
        return Challenge(properties, [provider]);
    }

    [HttpGet("login/{provider}/callback"), HttpPost("login/{provider}/callback")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> HandleCallback(string provider)
    {
        var result = await HttpContext.AuthenticateAsync(provider);

        if (result.Principal is null)
        {
            throw new HttpException(HttpStatusCode.InternalServerError, "result.Principal is null for some reason");
        }

        var providerId = result.Principal.GetClaim("id");
        
        if (providerId is null)
        {
            throw new HttpException(HttpStatusCode.InternalServerError, "providerId is null for some reason");
        }
        
        var existingUser = mongoService.Users.Find(u => u.ProviderName == provider && u.ProviderId == providerId).FirstOrDefault();

        if (existingUser is not null)
        {
            var identity = new ClaimsIdentity(
                authenticationType: "ExternalLogin",
                nameType: ClaimTypes.Name,
                roleType: ClaimTypes.Role);

            identity.AddClaim(new Claim(ClaimTypes.Name, existingUser.Username));
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, existingUser.Id));
            
            var properties = new AuthenticationProperties
            {
                RedirectUri = configuration["ClientUrl"]
            };

            return SignIn(new ClaimsPrincipal(identity), properties, CookieAuthenticationDefaults.AuthenticationScheme);
        }
        else
        {
            var identity = new ClaimsIdentity(
                authenticationType: "ExternalLogin",
                nameType: ClaimTypes.Name,
                roleType: ClaimTypes.Role);

            var avatarClaim = provider switch
            {
                OpenIddictClientWebIntegrationConstants.Providers.GitHub => "avatar_url",
                OpenIddictClientWebIntegrationConstants.Providers.GitLab => "avatar_url",
                _ => null
            };
            
            if (avatarClaim is null) return BadRequest("Unknown authentication provider");
            
            var avatarUrl = result.Principal.GetClaim(avatarClaim);
            
            if (avatarUrl is null) return BadRequest($"Avatar URL is missing for some reason for {provider}");

            identity.AddClaim(new Claim("provider", provider));
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, providerId));
            identity.AddClaim(new Claim("avatarUrl", avatarUrl));
            
            var usernameClaim = provider switch
            {
                OpenIddictClientWebIntegrationConstants.Providers.GitHub => "login",
                OpenIddictClientWebIntegrationConstants.Providers.GitLab => "username",
                _ => null
            };
            
            if (usernameClaim is null) return BadRequest("Unknown authentication provider");

            var username = result.Principal.GetClaim(usernameClaim);
            
            var properties = new AuthenticationProperties
            {
                RedirectUri = $"{configuration["ClientUrl"]}/create-account?username={username}"
            };

            return SignIn(new ClaimsPrincipal(identity), properties, CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }

    [Authorize]
    [HttpGet("auth/register")]
    public async Task<IActionResult> Register([FromQuery, Required, MaxLength(20), RegularExpression(@"^[\w\d\.\-_]*$")] string username)
    {
        var usernameFilter = Builders<User>.Filter.Eq(u => u.Username, username);
        var existingUser = await mongoService.Users.Find(usernameFilter).FirstOrDefaultAsync();

        if (existingUser is not null)
        {
            throw new HttpException(HttpStatusCode.BadRequest, "Username is already taken.");
        }

        var id = await idProvider.GenerateId(async id => await mongoService.Users.Find(u => u.Id == id).FirstOrDefaultAsync() is not null);

        var principalUser = HttpContext.User;

        var client = httpClientFactory.CreateClient();
        var response = await client.GetAsync(principalUser.GetClaim("avatarUrl"));
        var imageBytes = await response.Content.ReadAsByteArrayAsync();

        var avatarId = await imageService.UploadImageAsync(
            imageBytes,
            response.Content.Headers.ContentType?.MediaType ?? "image/png"
        );

        var user = new User
        {
            Id = id,
            CreatedAt = DateTime.UtcNow,
            Username = username,
            AvatarId = avatarId,
            ProviderName = principalUser.GetClaim("provider"),
            ProviderId = principalUser.GetClaim(ClaimTypes.NameIdentifier),
            UserSettings = new UserSettings(),
            Settings = new Settings()
        };

        await mongoService.Users.InsertOneAsync(user);

        await actionLogger.LogActionAsync(ActionLogType.UserCreated, user.Id);
        
        var identity = new ClaimsIdentity(
            authenticationType: "ExternalLogin",
            nameType: ClaimTypes.Name,
            roleType: ClaimTypes.Role);

        identity.AddClaim(new Claim(ClaimTypes.Name, user.Username));
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id));
        
        var properties = new AuthenticationProperties
        {
            RedirectUri = configuration["ClientUrl"]
        };

        return SignIn(new ClaimsPrincipal(identity), properties, CookieAuthenticationDefaults.AuthenticationScheme);
    }

    [HttpGet("auth/self")]
    [Authorize]
    public async Task<IActionResult> GetSelf()
    {
        var id = HttpContext.User.GetClaim(ClaimTypes.NameIdentifier);
        var self = await mongoService.Users.Find(u => u.Id == id).FirstAsync();

        if (self is null) return Unauthorized();
        return Ok(self);
    }

    [HttpGet("auth/logout")]
    public IActionResult Logout()
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = configuration["ClientUrl"]
        };
        
        return SignOut(properties);
    }
}
