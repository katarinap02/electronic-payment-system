namespace Crypto.API.DTOs
{
    public record CreatePaymentResponse(
        string CryptoPaymentId,
        string PaymentUrl,
        string WalletAddress,
        decimal AmountInEth,
        decimal ExchangeRate,
        DateTime ExpiresAt,
        string Status
    );
}
