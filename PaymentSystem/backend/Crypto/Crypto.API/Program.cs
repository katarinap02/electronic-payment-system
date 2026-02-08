using Microsoft.EntityFrameworkCore;
using Crypto.API.Data;
using Crypto.API.Repositories;
using Crypto.API.Services;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

var logsPath = builder.Configuration["Serilog:FilePath"] ?? "/logs/crypto";
Directory.CreateDirectory(logsPath);

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(443, listenOptions =>
    {
        var certPath = builder.Configuration["Https:CertificatePath"] ?? "/app/certs/crypto-api.pfx";
        var certPassword = builder.Configuration["Https:CertificatePassword"] ?? "dev-cert-2024";
        
        if (File.Exists(certPath))
        {
            listenOptions.UseHttps(certPath, certPassword);
            Console.WriteLine($"HTTPS configured with certificate: {certPath}");
        }
        else
        {
            Console.WriteLine($"Certificate not found at {certPath}, using default HTTPS configuration");
            listenOptions.UseHttps();
        }
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

builder.Services.AddDbContext<CryptoDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<CryptoTransactionRepository>();
builder.Services.AddScoped<AuditLogRepository>();

builder.Services.AddScoped<EncryptionService>();
builder.Services.AddScoped<Web3Service>();
builder.Services.AddScoped<EtherscanService>();
builder.Services.AddScoped<CryptoPaymentService>();

// Disabled: Using direct MetaMask confirmation instead of blockchain monitoring
// builder.Services.AddHostedService<BlockchainMonitorService>();

builder.Services.AddHttpClient();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowPSP",
        policy =>
        {
            policy.SetIsOriginAllowed(origin => true)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        var dbContext = services.GetRequiredService<CryptoDbContext>();
        var encryptionService = services.GetRequiredService<EncryptionService>();
        
        logger.LogInformation("Applying Crypto database migrations...");
        dbContext.Database.EnsureCreated();
        logger.LogInformation("Crypto database migrations applied successfully!");

        // Seed merchant wallets
        logger.LogInformation("Seeding merchant wallets...");
        Crypto.API.Data.MerchantWalletSeedData.SeedMerchantWallets(dbContext, encryptionService);
        logger.LogInformation("Merchant wallets seeding completed!");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error applying Crypto database migrations or seeding data");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseCors("AllowPSP");

app.UseAuthorization();
app.MapControllers();

app.Run();
