namespace Crypto.API.DTOs
{
    public record CreatePaymentRequest(
        string PspTransactionId,
        decimal Amount,
        string Currency,
        string MerchantId,
        string? CustomerId, // Optional: User/Customer ID from WebShop
        string ReturnUrl,
        string CancelUrl
    );
}
