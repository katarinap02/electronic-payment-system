using Bank.API.Data;
using Bank.API.Middleware;
using Bank.API.Repositories;
using Bank.API.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

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
            policy.WithOrigins("http://localhost:5174", "http://localhost:5172", "http://frontend-bank:5173")
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

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
