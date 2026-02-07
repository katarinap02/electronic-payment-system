using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using PayPal.API.Data;
using PayPal.API.Middleware;
using PayPal.API.Repositories;
using PayPal.API.Service;
using Serilog;
using Serilog.Events;
using System.Text.Json;

//PROVERA DA LI JE VREME SINHRONIZOVANO
string timeSyncStatus;
try
{
    using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
    var json = client.GetStringAsync("http://worldtimeapi.org/api/timezone/UTC").Result;
    var apiTime = DateTimeOffset.FromUnixTimeSeconds(JsonDocument.Parse(json).RootElement.GetProperty("unixtime").GetInt64()).UtcDateTime;
    timeSyncStatus = Math.Abs((DateTime.UtcNow - apiTime).TotalSeconds) < 60 ? "SYNC_OK" : "SYNC_FAIL";
}
catch
{
    timeSyncStatus = "SYNC_HOST_NTP";
}

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel for HTTPS only
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(443, listenOptions =>
    {
        var certPath = builder.Configuration["Https:CertificatePath"] ?? "/app/certs/paypal-api.pfx";
        var certPassword = builder.Configuration["Https:CertificatePassword"] ?? "dev-cert-2024";
        
        if (File.Exists(certPath))
        {
            listenOptions.UseHttps(certPath, certPassword);
        }
        else
        {
            Console.WriteLine($"WARNING: Certificate not found at {certPath}. Using development certificate.");
            listenOptions.UseHttps();
        }
    });
});

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

// DbContext
builder.Services.AddDbContext<PayPalDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// KONFIGURI�E SERILOG
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithProcessId()
    .Enrich.WithThreadId()
    .Enrich.WithProperty("ServiceName", "PayPal.API")
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .Enrich.WithProperty("TimeSyncStatus", timeSyncStatus)
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{ServiceName}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.Seq(
        serverUrl: builder.Configuration["Seq:ServerUrl"] ?? "http://seq:80",
        apiKey: builder.Configuration["Seq:ApiKey"],
        restrictedToMinimumLevel: LogEventLevel.Information)
    .CreateLogger();
builder.Host.UseSerilog(Log.Logger);
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor
        | ForwardedHeaders.XForwardedProto;
    // Dozvoli sve proxy-je u Docker mre�i
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<PaypalTransactionRepository>();
builder.Services.AddScoped<AuditLogRepository>();

builder.Services.AddScoped<EncryptionService>();

builder.Services.AddScoped<PayPalService>();
builder.Services.AddHttpClient();

// Seed Data Service
//builder.Services.AddScoped<SeedDataService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



builder.Host.UseSerilog();

// CORS (dozvoli PSP frontendu da komunicira)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowPSP",
        policy =>
        {
            policy.WithOrigins("http://localhost:5174", "https://localhost:5442", "https://localhost:5443", "https://psp-api:443")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PayPalDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Applying PayPal database migrations...");
        dbContext.Database.EnsureCreated();
        logger.LogInformation("Migrations applied successfully!");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error applying migrations");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseCors("AllowPSP");
app.UseForwardedHeaders();
app.UseSerilogRequestLogging();
app.UseAuditLogging();

app.UseAuthorization();
app.MapControllers();

app.Run();