using PSP.API.Services.Implementations;
using PSP.API.Services.Interfaces;

namespace PSP.API.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IWebShopService, WebShopService>();
        services.AddScoped<IPaymentMethodService, PaymentMethodService>();

        return services;
    }
}
