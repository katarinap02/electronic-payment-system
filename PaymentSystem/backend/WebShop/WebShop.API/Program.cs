using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WebShop.API.Data;
using WebShop.API.Repositories;
using WebShop.API.Services;

var builder = WebApplication.CreateBuilder(args);

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
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    try
    {
        var dbContext = services.GetRequiredService<AppDbContext>();
        var passwordService = services.GetRequiredService<PasswordService>();
        
        logger.LogInformation("Applying database migrations...");
        dbContext.Database.Migrate();
        logger.LogInformation("Database migrations applied successfully.");
        
        logger.LogInformation("Ensuring Rentals table exists...");
        DatabaseInitializer.Initialize(dbContext);
        logger.LogInformation("Database initialization completed.");

        logger.LogInformation("Seeding users...");
        UserSeedData.SeedAdminUser(dbContext, passwordService);
        logger.LogInformation("Users seeded successfully.");

        logger.LogInformation("Seeding vehicles...");
        VehicleSeedData.SeedVehicles(dbContext);
        logger.LogInformation("Vehicles seeded successfully.");
        
        logger.LogInformation("Seeding insurance packages...");
        InsurancePackageSeedData.SeedInsurancePackages(dbContext);
        logger.LogInformation("Insurance packages seeded successfully.");
        
        logger.LogInformation("Seeding additional services...");
        AdditionalServiceSeedData.SeedAdditionalServices(dbContext);
        logger.LogInformation("Additional services seeded successfully.");

        logger.LogInformation("Seeding rentals...");
        RentalSeedData.SeedRentals(dbContext);
        logger.LogInformation("Rentals seeded successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
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

app.MapControllers();

app.Run();
