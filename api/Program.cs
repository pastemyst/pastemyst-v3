using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Driver;
using OpenIddict.Client.WebIntegration;
using pastemyst.Jobs;
using pastemyst.Middleware;
using pastemyst.Services;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Services.AddLogging();
builder.Logging.AddConsole();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();

builder.Services.AddHttpLogging(_ => {});

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromSeconds(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = "pastemyst-session";
});

builder.Services.AddHttpClient();

builder.Services.AddSingleton<MongoService>();

builder.Services.TryAddSingleton<LanguageProvider>();
builder.Services.AddSingleton(s =>
    (IHostedService)s.GetRequiredService<LanguageProvider>()
);

builder.Services.TryAddSingleton<VersionProvider>();
builder.Services.AddSingleton(s =>
    (IHostedService)s.GetRequiredService<VersionProvider>()
);

builder.Services.TryAddSingleton<ChangelogProvider>();
builder.Services.AddSingleton(s =>
    (IHostedService)s.GetRequiredService<ChangelogProvider>()
);

builder.Services.AddScoped<IdProvider>();
builder.Services.AddScoped<ImageService>();
builder.Services.AddScoped<UserProvider>();
builder.Services.AddScoped<SettingsService>();
builder.Services.AddScoped<PasteService>();
builder.Services.AddScoped<ActionLogger>();
builder.Services.AddScoped<StatsService>();

builder.Services.AddQuartz(q =>
{
    q.UseMicrosoftDependencyInjectionJobFactory();

    q.AddJob<ExpirePastesJob>(opts => opts.WithIdentity(nameof(ExpirePastesJob)));
    q.AddTrigger(opts => opts
        .ForJob(nameof(ExpirePastesJob))
        .WithIdentity(nameof(ExpirePastesJob) + "-trigger")
        .WithCronSchedule("0 * * ? * *") // run every minute
    );

    q.AddJob<ExpireSesssionSettingsJob>(opts => opts.WithIdentity(nameof(ExpireSesssionSettingsJob)));
    q.AddTrigger(opts => opts
        .ForJob(nameof(ExpireSesssionSettingsJob))
        .WithIdentity(nameof(ExpireSesssionSettingsJob) + "-trigger")
        .WithCronSchedule("0 * * ? * *") // run every minute
    );
});

builder.Services.AddQuartzServer(options =>
{
    options.WaitForJobsToComplete = true;
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod()
    );

    options.AddPolicy("client", policy =>
        policy.WithOrigins(builder.Configuration["ClientUrl"] ?? throw new InvalidOperationException("Missing ClientUrl configuration"))
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
    );
});

builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
        options.UseMongoDb()
               .UseDatabase(new MongoClient(builder.Configuration.GetConnectionString("DefaultDb")).GetDatabase("openiddict"));
    })
    .AddClient(options =>
    {
        options.AllowAuthorizationCodeFlow();
        
        // TODO: prod certificates
        options.AddDevelopmentEncryptionCertificate()
               .AddDevelopmentSigningCertificate();

        options.UseAspNetCore()
               .EnableRedirectionEndpointPassthrough()
               .DisableTransportSecurityRequirement(); // TODO: disable https only on dev

        options.UseSystemNetHttp();
        
        var githubClientId = builder.Configuration["GitHub:ClientId"];
        var githubClientSecret = builder.Configuration["GitHub:ClientSecret"];
        
        var gitlabClientId = builder.Configuration["GitLab:ClientId"];
        var gitlabClientSecret = builder.Configuration["GitLab:ClientSecret"];

        var webProviders = options.UseWebProviders();

        if (githubClientId is not null && githubClientSecret is not null)
        {
            webProviders.AddGitHub(providerOptions =>
            {
                providerOptions.SetClientId(githubClientId)
                               .SetClientSecret(githubClientSecret)
                               .SetRedirectUri($"api/v3/login/{OpenIddictClientWebIntegrationConstants.Providers.GitHub}/callback")
                               .AddScopes("read:user");
            });
        }
        
        if (gitlabClientId is not null && gitlabClientSecret is not null)
        {
            webProviders.AddGitLab(providerOptions =>
            {
                providerOptions.SetClientId(gitlabClientId)
                               .SetClientSecret(gitlabClientSecret)
                               .SetRedirectUri($"api/v3/login/{OpenIddictClientWebIntegrationConstants.Providers.GitLab}/callback")
                               .AddScopes("read_user");
            });
        }
    })
    .AddServer(options =>
    {
        options.SetTokenEndpointUris("/api/v3/auth/connect/token");

        options.AllowClientCredentialsFlow();

        // TODO: prod certificates
        options.AddDevelopmentEncryptionCertificate()
               .AddDevelopmentSigningCertificate();

        // TODO: check this out :)
        options.DisableAccessTokenEncryption();

        options.UseAspNetCore()
               .EnableTokenEndpointPassthrough()
               .DisableTransportSecurityRequirement() // TODO: enable https in prod
               .EnableAuthorizationEndpointPassthrough();
    })
    .AddValidation(options =>
    {
        options.UseLocalServer();
        options.UseAspNetCore();
    });

builder.Services.AddAuthorization().AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.Cookie.Name = "pastemyst_auth";
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.MaxAge = options.ExpireTimeSpan;
    
    // on failed auth, don't redirect to login, just return 401
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
    
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
});

var app = builder.Build();

app.UseHttpLogging();

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("client");
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.MapControllers();

app.Run();
