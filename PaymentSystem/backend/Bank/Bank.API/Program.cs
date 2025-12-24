// Bank/Bank.API/Program.cs
using Bank.API.Data;
using Bank.API.Repositories;
using Bank.API.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<BankAccountRepository>();
builder.Services.AddScoped<PaymentTransactionRepository>();
builder.Services.AddScoped<CardTokenRepository>();

builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<CardService>();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("BankDb")));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// AUTO-CREATE DATABASE
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();