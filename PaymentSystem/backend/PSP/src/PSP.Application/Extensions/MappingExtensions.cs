using PSP.Application.DTOs.PaymentMethods;
using PSP.Application.DTOs.WebShops;
using PSP.Domain.Entities;

namespace PSP.Application.Extensions;

public static class MappingExtensions
{
    public static WebShopAdminResponse ToWebShopAdminResponse(this WebShopAdmin webShopAdmin)
    {
        return new WebShopAdminResponse
        {
            UserId = webShopAdmin.UserId,
            Email = webShopAdmin.User.Email,
            Name = webShopAdmin.User.Name,
            Surname = webShopAdmin.User.Surname,
            AssignedAt = webShopAdmin.AssignedAt
        };
    }

    public static WebShopResponse ToWebShopResponse(this WebShop webShop)
    {
        return new WebShopResponse
        {
            Id = webShop.Id,
            Name = webShop.Name,
            Url = webShop.Url,
            MerchantId = webShop.MerchantId,
            ApiKey = webShop.ApiKey,
            Status = webShop.Status.ToString(),
            PaymentMethods = webShop.WebShopPaymentMethods
                .Where(wp => wp.IsEnabled)
                .Select(wp => wp.PaymentMethod.ToPaymentMethodDTO(wp.IsEnabled))
                .ToList(),
            CreatedAt = webShop.CreatedAt
        };
    }

    public static PaymentMethodDTO ToPaymentMethodDTO(this PaymentMethod paymentMethod, bool isEnabled = true)
    {
        return new PaymentMethodDTO
        {
            Id = paymentMethod.Id,
            Name = paymentMethod.Name,
            Code = paymentMethod.Code,
            Type = paymentMethod.Type.ToString(),
            Description = paymentMethod.Description,
            IsEnabled = isEnabled
        };
    }
}
