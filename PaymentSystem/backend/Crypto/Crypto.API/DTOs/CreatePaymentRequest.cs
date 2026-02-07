namespace Crypto.API.DTOs
{
    public record CreatePaymentRequest(
        string PspTransactionId,
        decimal Amount,
        string Currency,
        string MerchantId,
        string ReturnUrl,
        string CancelUrl
    );
}
