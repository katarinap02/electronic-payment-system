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
            _logger.LogInformation("[BANK-AUTOCAPTURE] STARTED | CheckInterval: {CheckInterval}m | CaptureAge: {CaptureAge}h",
                _checkInterval.TotalMinutes, _captureAge.TotalHours);

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
                            _logger.LogInformation("[BANK-AUTOCAPTURE] BATCH_START | Count: {Count} | AgeThreshold: {Age}h",
                                oldTransactions.Count(), _captureAge.TotalHours);

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

                                            _logger.LogInformation("[BANK-AUTOCAPTURE] CAPTURED | PaymentId: {PaymentId} | TxId: {TxId} | MerchantId: {MerchantId} | Amount: {Amount} {Currency}",
                                                transaction.PaymentId, transaction.Id, transaction.MerchantId, transaction.Amount, transaction.Currency);
                                        }
                                        else
                                        {
                                            _logger.LogWarning("[BANK-AUTOCAPTURE] FAILED | Event: TRANSFER_FAILED | PaymentId: {PaymentId} | TxId: {TxId} | MerchantId: {MerchantId}",
                                                 transaction.PaymentId, transaction.Id, transaction.MerchantId);
                                        }
                                    }
                                    else
                                    {
                                        _logger.LogWarning("[BANK-AUTOCAPTURE] SKIPPED | Event: MISSING_ACCOUNTS | PaymentId: {PaymentId} | TxId: {TxId} | HasCustomerAcc: {HasCustomer} | HasMerchantAcc: {HasMerchant}",
                                            transaction.PaymentId, transaction.Id, transaction.CustomerAccountId.HasValue, transaction.MerchantAccountId > 0);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, "[BANK-AUTOCAPTURE] ERROR | PaymentId: {PaymentId} | TxId: {TxId} | ErrorType: {ErrorType}",
                                         transaction.PaymentId, transaction.Id, ex.GetType().Name);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[BANK-AUTOCAPTURE] SERVICE_ERROR | ErrorType: {ErrorType}", ex.GetType().Name);
                }

                // Čekaj sledeći interval
                await Task.Delay(_checkInterval, stoppingToken);
            }
        }
    }
}
