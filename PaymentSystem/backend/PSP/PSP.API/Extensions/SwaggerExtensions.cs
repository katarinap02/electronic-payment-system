using Microsoft.OpenApi.Models;

namespace PSP.API.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "PSP API - Payment Service Provider",
                Version = "v1",
                Description = "Payment Service Provider API za upravljanje webshopovima i metodama plaćanja",
                Contact = new OpenApiContact
                {
                    Name = "PSP Team",
                    Email = "admin@psp.com"
                }
            });

            // JWT Authentication configuration for Swagger
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header. Enter your token in the text input below (without 'Bearer' prefix).",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }
}
