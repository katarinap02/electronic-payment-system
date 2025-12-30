using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using PSP.API.Data;
using PSP.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Register Repositories and Services
builder.Services.AddRepositories();
builder.Services.AddServices();

// HTTP Client for Bank and WebShop communication
builder.Services.AddHttpClient("BankAPI", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["BankApiUrl"] ?? "http://bank-api:80");
});
builder.Services.AddHttpClient("WebShopAPI", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["WebShopApiUrl"] ?? "http://webshop-api:80");
});

// CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:5174")
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

builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PSPDb")));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerConfiguration();

var app = builder.Build();

// Apply migrations and seed data
app.ApplyMigrationsAndSeed();

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
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
