using Bank.API.Repositories;
using static Bank.API.Models.PaymentTransaction;

namespace Bank.API.Services
{
    public class AutoCaptureBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AutoCaptureBackgroundService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1); // Proverava svaki sat
        private readonly TimeSpan _captureAge = TimeSpan.FromHours(20);   // Capture posle 24h

        public AutoCaptureBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<AutoCaptureBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("AutoCapture Background Service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var paymentService = scope.ServiceProvider.GetRequiredService<PaymentService>();
                        var transactionRepo = scope.ServiceProvider.GetRequiredService<PaymentTransactionRepository>();

                        // Pronađi stare AUTHORIZED transakcije
                        var oldTransactions = transactionRepo.GetAuthorizedTransactionsOlderThan(_captureAge);

                        if (oldTransactions.Any())
                        {
                            _logger.LogInformation($"Found {oldTransactions.Count()} transactions for auto-capture");

                            foreach (var transaction in oldTransactions)
                            {
                                try
                                {
                                    // Proveri da li transakcija ima sve potrebne podatke
                                    if (transaction.CustomerAccountId.HasValue &&
                                        transaction.MerchantAccountId > 0)
                                    {
                                        // Pozovi TransferReservedToMerchant
                                        var accountRepo = scope.ServiceProvider.GetRequiredService<BankAccountRepository>();

                                        bool transferred = accountRepo.FinalizeCapture(
                                            transaction.CustomerAccountId.Value,
                                            transaction.MerchantAccountId,
                                            transaction.Amount,
                                            transaction.Currency);

                                        if (transferred)
                                        {
                                            // Ažuriraj status transakcije na CAPTURED
                                            transactionRepo.UpdateStatus(transaction.Id, TransactionStatus.CAPTURED);

                                            _logger.LogInformation($"Auto-captured transaction {transaction.PaymentId} " +
                                                                  $"(Amount: {transaction.Amount} {transaction.Currency})");
                                        }
                                        else
                                        {
                                            _logger.LogWarning($"Failed to auto-capture transaction {transaction.PaymentId}");
                                        }
                                    }
                                    else
                                    {
                                        _logger.LogWarning($"Transaction {transaction.PaymentId} missing account IDs");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, $"Error auto-capturing transaction {transaction.PaymentId}");
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in AutoCaptureBackgroundService");
                }

                // Čekaj sledeći interval
                await Task.Delay(_checkInterval, stoppingToken);
            }
        }
    }
}
