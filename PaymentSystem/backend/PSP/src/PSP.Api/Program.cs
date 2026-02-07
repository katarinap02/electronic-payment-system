using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;
using PSP.Infrastructure.Extensions;
using PSP.Infrastructure.Middleware;
using Serilog;
using Serilog.Events;
using System.Text;
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
    // HTTPS port only
    serverOptions.ListenAnyIP(443, listenOptions =>
    {
        var certPath = builder.Configuration["Https:CertificatePath"] ?? "/app/certs/psp-api.pfx";
        var certPassword = builder.Configuration["Https:CertificatePassword"] ?? "dev-cert-2024";
        
        if (File.Exists(certPath))
        {
            listenOptions.UseHttps(certPath, certPassword);
        }
        else
        {
            // Fallback - generisanje dev sertifikata u runtime-u
            Console.WriteLine($"WARNING: Certificate not found at {certPath}. Using development certificate.");
            listenOptions.UseHttps();
        }
    });
});

// Add Infrastructure layer (DbContext, Repositories, Services)
builder.Services.AddInfrastructure(builder.Configuration);

// CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.SetIsOriginAllowed(origin => true) // Allow any origin in development
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

// JWT authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"] ?? "fallback-secret-key-min-32-chars"))
        };
    });

//Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithProcessId()
    .Enrich.WithThreadId()
    .Enrich.WithProperty("ServiceName", "PSP.API") 
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

builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerConfiguration();

var app = builder.Build();

// Apply migrations and seed data (samo jedna instanca u load balanced setup-u)
var runMigrations = builder.Configuration.GetValue<bool>("RUN_MIGRATIONS", true);
if (runMigrations)
{
    await app.ApplyMigrationsAndSeedAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "PSP API v1");
        options.RoutePrefix = string.Empty; // Swagger UI at root
        options.DocumentTitle = "PSP API Documentation";
    });
}

app.UseCors("AllowFrontend");
app.UseForwardedHeaders();
app.UseSerilogRequestLogging();
app.UseAuditLogging();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
