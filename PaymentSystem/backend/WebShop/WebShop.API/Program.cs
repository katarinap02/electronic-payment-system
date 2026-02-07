using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using System.Text;
using System.Text.Json;
using WebShop.API.Data;
using WebShop.API.Middleware;
using WebShop.API.Repositories;
using WebShop.API.Services;

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

var logsPath = builder.Configuration["Serilog:FilePath"] ?? "/logs/webshop";
Directory.CreateDirectory(logsPath);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithProcessId()
    .Enrich.WithThreadId()
    .Enrich.WithProperty("ServiceName", "Webshop.API")
    .Enrich.WithProperty("TimeSyncStatus", timeSyncStatus)
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{ServiceName}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: Path.Combine(logsPath, "webshop-api-.json"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        restrictedToMinimumLevel: LogEventLevel.Information,
        formatter: new Serilog.Formatting.Json.JsonFormatter())
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
    // Dozvoli sve proxy-je u Docker mreži
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});
builder.Services.AddHttpContextAccessor();

// User services
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<PasswordService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<AuthService>();

// Vehicle services
builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
builder.Services.AddScoped<IVehicleService, VehicleService>();

// Insurance and Services
builder.Services.AddScoped<IInsurancePackageRepository, InsurancePackageRepository>();
builder.Services.AddScoped<IInsurancePackageService, InsurancePackageService>();
builder.Services.AddScoped<IAdditionalServiceRepository, AdditionalServiceRepository>();
builder.Services.AddScoped<IAdditionalServiceService, AdditionalServiceService>();
builder.Services.AddHostedService<FileIntegrityMonitorService>();
// Rental services
builder.Services.AddScoped<RentalRepository>();
builder.Services.AddScoped<RentalService>();

//Namestanje CORS-a
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

//JWT autentikacija
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

builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("WebShopDb")));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Apply migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    
    try
    {
        var dbContext = services.GetRequiredService<AppDbContext>();
        var passwordService = services.GetRequiredService<PasswordService>();
        
        dbContext.Database.Migrate();
        
        DatabaseInitializer.Initialize(dbContext);

        UserSeedData.SeedAdminUser(dbContext, passwordService);

        VehicleSeedData.SeedVehicles(dbContext);
        
        InsurancePackageSeedData.SeedInsurancePackages(dbContext);
        
        AdditionalServiceSeedData.SeedAdditionalServices(dbContext);

        RentalSeedData.SeedRentals(dbContext);
    }
    catch (Exception ex)
    {
        throw;
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// IMPORTANT: UseCors must be before UseRouting
app.UseCors("AllowFrontend");

app.UseRouting();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseForwardedHeaders();
app.UseSerilogRequestLogging();
app.UseAuditLogging();
app.MapControllers();

app.Run();
