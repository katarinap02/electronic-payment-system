using Crypto.API.Repositories;

namespace Crypto.API.Services
{
    public class BlockchainMonitorService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BlockchainMonitorService> _logger;
        private readonly int _intervalSeconds;

        public BlockchainMonitorService(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            ILogger<BlockchainMonitorService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _intervalSeconds = int.Parse(configuration["CryptoSettings:MonitoringIntervalSeconds"] ?? "30");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Blockchain Monitor Service started. Interval: {Interval}s", _intervalSeconds);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var transactionRepo = scope.ServiceProvider.GetRequiredService<CryptoTransactionRepository>();
                        var paymentService = scope.ServiceProvider.GetRequiredService<CryptoPaymentService>();
                        var etherscanService = scope.ServiceProvider.GetRequiredService<EtherscanService>();
                        var encryption = scope.ServiceProvider.GetRequiredService<EncryptionService>();
                        var auditRepo = scope.ServiceProvider.GetRequiredService<AuditLogRepository>();

                        // 1. Proveri expired transakcije
                        var expiredTxs = await transactionRepo.GetExpiredTransactionsAsync();
                        foreach (var tx in expiredTxs)
                        {
                            await paymentService.ExpireTransactionAsync(tx.CryptoPaymentId);
                        }

                        // 2. Monitor PENDING transakcije
                        var pendingTxs = await transactionRepo.GetPendingTransactionsAsync();
                        foreach (var tx in pendingTxs)
                        {
                            var walletAddress = tx.WalletAddress;

                            var result = await etherscanService.FindIncomingTransactionAsync(
                                walletAddress,
                                tx.AmountInCrypto,
                                tx.CreatedAt);

                            if (result.HasValue)
                            {
                                _logger.LogInformation("Found incoming transaction for {CryptoPaymentId}: {TxHash}",
                                    tx.CryptoPaymentId, result.Value.TxHash);

                                await paymentService.ConfirmTransactionAsync(tx.CryptoPaymentId, result.Value.TxHash!);
                            }
                        }

                        // 3. Monitor CONFIRMING transakcije
                        var confirmingTxs = await transactionRepo.GetConfirmingTransactionsAsync();
                        foreach (var tx in confirmingTxs)
                        {
                            if (string.IsNullOrEmpty(tx.EncryptedTransactionHash))
                                continue;

                            var txHash = tx.EncryptedTransactionHash; // No decryption needed
                            var confirmations = await etherscanService.GetTransactionConfirmationsAsync(txHash);

                            if (confirmations != tx.Confirmations)
                            {
                                _logger.LogInformation(
                                    "Confirmations updated for {CryptoPaymentId}: {Confirmations}",
                                    tx.CryptoPaymentId,
                                    confirmations);

                                await paymentService.CaptureTransactionAsync(tx.CryptoPaymentId);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in blockchain monitoring cycle");
                }

                await Task.Delay(TimeSpan.FromSeconds(_intervalSeconds), stoppingToken);
            }

            _logger.LogInformation("Blockchain Monitor Service stopped.");
        }
    }
}
