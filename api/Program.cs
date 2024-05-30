using Microsoft.Extensions.DependencyInjection.Extensions;
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

builder.Services.AddHttpLogging(o => {});

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromSeconds(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = "pastemyst-session";
});

builder.Services.AddHttpClient();

builder.Services.AddSingleton<IMongoService, MongoService>();

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
builder.Services.AddScoped<IUserContext, UserContext>();
builder.Services.AddScoped<ISettingsService, SettingsService>();
builder.Services.AddScoped<IPasteService, PasteService>();
builder.Services.AddScoped<IActionLogger, ActionLogger>();
builder.Services.AddScoped<IStatsService, StatsService>();

builder.Services.AddQuartz(q =>
{
    q.UseMicrosoftDependencyInjectionJobFactory();

    q.AddJob<ExpirePastesJob>(opts => opts.WithIdentity(nameof(ExpirePastesJob)));
    q.AddTrigger(opts => opts
        .ForJob(nameof(ExpirePastesJob))
        .WithIdentity(nameof(ExpirePastesJob) + "-trigger")
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

var app = builder.Build();

app.UseHttpLogging();

app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<UserContextMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.UseSession();

app.UseCors("client");
app.UseCors();

app.MapControllers();

app.Run();
