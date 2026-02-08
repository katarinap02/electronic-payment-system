namespace Crypto.API.DTOs
{
    public record CreatePaymentResponse(
        string CryptoPaymentId,
        string PaymentUrl,
        string WalletAddress, // Decrypted wallet address
        decimal AmountInEth,
        decimal ExchangeRate,
        string Status
    );
}
