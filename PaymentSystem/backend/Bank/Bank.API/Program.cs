using Bank.API.Data;
using Bank.API.Middleware;
using Bank.API.Repositories;
using Bank.API.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using System.Text.Json;
using System.Text.Json.Serialization;

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
        var certPath = builder.Configuration["Https:CertificatePath"] ?? "/app/certs/bank-api.pfx";
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

//Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithProcessId()
    .Enrich.WithThreadId()
    .Enrich.WithProperty("ServiceName", "Bank.API")
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

builder.Services.AddScoped<BankAccountRepository>();
builder.Services.AddScoped<PaymentTransactionRepository>();
builder.Services.AddScoped<CardTokenRepository>();
builder.Services.AddScoped<CardRepository>();
builder.Services.AddScoped<CustomerRepository>();

builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<CardService>();
builder.Services.AddScoped<SeedDataService>();
builder.Services.AddHttpClient<NbsQrCodeService>(); // HttpClient za NBS API
builder.Services.AddHostedService<AutoCaptureBackgroundService>();



// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("BankDb")));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Namestanje CORS-a
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.SetIsOriginAllowed(origin => true) // Allow any origin in development
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var app = builder.Build();

// INICIJALIZUJ BAZU (migracije + kolone)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    DatabaseInitializer.Initialize(dbContext);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// SEED TEST DATA
using (var scope = app.Services.CreateScope())
{
    var seedService = scope.ServiceProvider.GetRequiredService<SeedDataService>();
    try
    {
        seedService.InitializeTestData();
        Console.WriteLine("Test data seeded successfully!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error seeding test data: {ex.Message}");
    }
}

app.UseMiddleware<HmacValidationMiddleware>();

app.UseRouting();
app.UseCors("AllowFrontend");
app.UseForwardedHeaders();
app.UseSerilogRequestLogging();
app.UseAuditLogging();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
