namespace Crypto.API.DTOs
{
    public record TransactionStatusResponse(
        string CryptoPaymentId,
        string Status,
        decimal Amount,
        string Currency,
        decimal AmountInEth,
        string TransactionHash,
        int Confirmations,
        DateTime? CompletedAt,
        DateTime ExpiresAt,
        string WalletAddress,
        string MerchantName
    );
}
