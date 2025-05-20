using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PasteMyst.Web.Jobs;
using PasteMyst.Web.Middleware;
using PasteMyst.Web.Services;
using Quartz;
using Quartz.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.File(
        path: "logs/app-.log",
        rollingInterval: RollingInterval.Month,
        retainedFileCountLimit: 12,
        rollOnFileSizeLimit: true,
        shared: true
    )
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

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

builder.Services.AddSingleton<MongoService>();

builder.Services.TryAddSingleton<LanguageProvider>();
builder.Services.AddSingleton(s =>
    (IHostedService)s.GetRequiredService<LanguageProvider>()
);

builder.Services.AddSingleton<VersionProvider>();
builder.Services.AddSingleton<ChangelogProvider>();

builder.Services.AddScoped<IdProvider>();
builder.Services.AddScoped<ImageService>();
builder.Services.AddScoped<UserProvider>();
builder.Services.AddScoped<OAuthService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UserContext>();
builder.Services.AddScoped<EncryptionContext>();
builder.Services.AddScoped<SettingsService>();
builder.Services.AddScoped<PasteService>();
builder.Services.AddScoped<ActionLogger>();
builder.Services.AddScoped<StatsService>();
builder.Services.AddScoped<AnnouncementService>();

builder.Services.AddQuartz(q =>
{
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

    q.AddJob<ExpireAccessTokensJob>(opts => opts.WithIdentity(nameof(ExpireAccessTokensJob)));
    q.AddTrigger(opts => opts
        .ForJob(nameof(ExpireAccessTokensJob))
        .WithIdentity(nameof(ExpireAccessTokensJob) + "-trigger")
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

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState.Values.SelectMany(x => x.Errors.Select(e => e.ErrorMessage)).ToList();

        var errorResponse = new { StatusCode = HttpStatusCode.BadRequest, Message = string.Join(" ", errors) };

        return new ObjectResult(errorResponse)
        {
            StatusCode = (int)HttpStatusCode.BadRequest
        };
    };
});

var app = builder.Build();

app.UseHttpLogging();

app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<UserContextMiddleware>();
app.UseMiddleware<EncryptionMiddleware>();

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
