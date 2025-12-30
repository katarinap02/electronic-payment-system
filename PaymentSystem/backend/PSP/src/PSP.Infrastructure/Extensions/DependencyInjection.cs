using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PSP.Application.Interfaces.Repositories;
using PSP.Application.Interfaces.Services;
using PSP.Infrastructure.Persistence;
using PSP.Infrastructure.Persistence.Repositories;
using PSP.Infrastructure.Repositories;
using PSP.Infrastructure.Security;
using PSP.Infrastructure.Services;

namespace PSP.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IWebShopRepository, WebShopRepository>();
        services.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();
        services.AddScoped<IWebShopAdminRepository, WebShopAdminRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();

        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IAuthService, AuthService>();

        services.AddScoped<IWebShopService, WebShopService>();
        services.AddScoped<IWebShopAdminService, WebShopAdminService>();
        services.AddScoped<IPaymentMethodService, PaymentMethodService>();
        services.AddScoped<PaymentService>();

        return services;
    }
}
