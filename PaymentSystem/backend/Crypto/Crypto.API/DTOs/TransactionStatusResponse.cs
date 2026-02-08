namespace Crypto.API.DTOs
{
    public record TransactionStatusResponse(
        string CryptoPaymentId,
        string Status,
        decimal Amount,
        string Currency,
        decimal AmountInEth,
        string TransactionHash,
        DateTime? CompletedAt,
        string WalletAddress,
        string MerchantName
    );
}
