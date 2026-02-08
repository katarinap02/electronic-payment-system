using Microsoft.EntityFrameworkCore;
using Crypto.API.Data;
using Crypto.API.Repositories;
using Crypto.API.Services;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

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
            policy.WithOrigins("http://localhost:5174", "http://localhost:5002", "http://localhost:5175", "http://psp-api:80")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
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
