using PSP.API.Repositories.Implementations;
using PSP.API.Repositories.Interfaces;

namespace PSP.API.Extensions;

public static class RepositoryExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IWebShopRepository, WebShopRepository>();
        services.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();

        return services;
    }
}
