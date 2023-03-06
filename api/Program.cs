using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using pastemyst.DbContexts;
using pastemyst.Middleware;
using pastemyst.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Logging.ClearProviders();
builder.Services.AddLogging();
builder.Logging.AddConsole();

builder.Services.AddDbContext<DataContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultDb"))
        .UseSnakeCaseNamingConvention());

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromSeconds(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = "pastemyst-session";
});

builder.Services.AddHttpClient();

builder.Services.TryAddSingleton<ILanguageProvider, LanguageProvider>();
builder.Services.AddSingleton(s =>
    (IHostedService)s.GetRequiredService<ILanguageProvider>()
);

builder.Services.TryAddSingleton<IVersionProvider, VersionProvider>();
builder.Services.AddSingleton(s =>
    (IHostedService)s.GetRequiredService<IVersionProvider>()
);

builder.Services.TryAddSingleton<IChangelogProvider, ChangelogProvider>();
builder.Services.AddSingleton(s =>
    (IHostedService)s.GetRequiredService<IChangelogProvider>()
);

builder.Services.AddScoped<IIdProvider, IdProvider>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IUserProvider, UserProvider>();
builder.Services.AddScoped<IOAuthService, OAuthService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPastyService, PastyService>();
builder.Services.AddScoped<IPasteService, PasteService>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("http://localhost:3000")
            .AllowCredentials()
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

app.UseHttpLogging();

app.UseMiddleware<ExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.UseSession();

app.UseCors();

app.MapControllers();

app.Run();
