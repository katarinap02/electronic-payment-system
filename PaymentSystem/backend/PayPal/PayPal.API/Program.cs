using Microsoft.EntityFrameworkCore;
using PayPal.API.Data;
using PayPal.API.Repositories;
using PayPal.API.Service;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

// DbContext
builder.Services.AddDbContext<PayPalDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<PaypalTransactionRepository>();
builder.Services.AddScoped<AuditLogRepository>();

builder.Services.AddScoped<EncryptionService>();

builder.Services.AddScoped<PayPalService>();
builder.Services.AddHttpClient();

// Seed Data Service
//builder.Services.AddScoped<SeedDataService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS (dozvoli PSP frontendu da komunicira)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowPSP",
        policy =>
        {
            policy.WithOrigins("http://localhost:5174", "http://localhost:5002", "http://psp-api:80")
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

// SEED TEST DATA 
//using (var scope = app.Services.CreateScope())
//{
//    var seedService = scope.ServiceProvider.GetRequiredService<SeedDataService>();
//    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

//    try
//    {
//        seedService.InitializeTestData();
//        logger.LogInformation("PayPal seed data initialized successfully!");
//    }
//    catch (Exception ex)
//    {
//        logger.LogError(ex, "Error seeding PayPal test data");
//    }
//}

//app.UseMiddleware<AuditMiddleware>();

app.UseRouting();
app.UseCors("AllowPSP");

app.UseAuthorization();
app.MapControllers();

app.Run();