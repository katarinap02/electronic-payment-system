using PSP.Application.Common;
using PSP.Application.DTOs.WebShops;
using PSP.Application.DTOs.PaymentMethods;

namespace PSP.Application.Interfaces.Services;

public interface IWebShopService
{
    Task<Result<IEnumerable<WebShopResponse>>> GetAllWebShopsAsync();
    Task<Result<WebShopResponse>> GetWebShopByIdAsync(int id);
    Task<Result<WebShopResponse>> CreateWebShopAsync(CreateWebShopRequest request);
    Task<Result<WebShopResponse>> UpdateWebShopAsync(int id, UpdateWebShopRequest request);
    Task<Result> DeleteWebShopAsync(int id);
    Task<Result> AddPaymentMethodToWebShopAsync(int webShopId, int paymentMethodId);
    Task<Result> RemovePaymentMethodFromWebShopAsync(int webShopId, int paymentMethodId);
    Task<Result<IEnumerable<PaymentMethodDTO>>> GetWebShopPaymentMethodsAsync(int webShopId);
}
