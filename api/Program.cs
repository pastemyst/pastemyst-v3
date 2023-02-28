using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using pastemyst.DbContexts;
using pastemyst.Middleware;
using pastemyst.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddDbContext<DataContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultDb"))
           .UseSnakeCaseNamingConvention());

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.TryAddSingleton<ILanguageProvider, LanguageProvider>();
builder.Services.AddSingleton<IHostedService>(s =>
    (IHostedService)s.GetRequiredService<ILanguageProvider>()
);

builder.Services.TryAddSingleton<IVersionProvider, VersionProvider>();
builder.Services.AddSingleton<IHostedService>(s =>
    (IHostedService)s.GetRequiredService<IVersionProvider>()
);

builder.Services.TryAddSingleton<IChangelogProvider, ChangelogProvider>();
builder.Services.AddSingleton<IHostedService>(s =>
    (IHostedService)s.GetRequiredService<IChangelogProvider>()
);

builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IIdProvider, IdProvider>();

var app = builder.Build();

app.UseHttpLogging();

app.UseMiddleware<ExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();