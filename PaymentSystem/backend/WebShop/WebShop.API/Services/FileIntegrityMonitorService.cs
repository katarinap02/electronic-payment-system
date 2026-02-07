using System.Security.Cryptography;
using System.Text.Json;

namespace WebShop.API.Services
{
    public class FileIntegrityMonitorService : BackgroundService
    {
        private readonly ILogger<FileIntegrityMonitorService> _logger;
        private readonly string _monitorPath;
        private readonly string _baselineFile;
        private readonly string _alertPath;

        public FileIntegrityMonitorService(ILogger<FileIntegrityMonitorService> logger, IConfiguration config)
        {
            _logger = logger;
            _monitorPath = config["Serilog:FilePath"] ?? "/logs/bank";
            _baselineFile = config["Fim:BaselinePath"] ?? "/app/fim/bank_baseline.json";
            _alertPath = Path.Combine(Path.GetDirectoryName(_baselineFile)!, "alerts.log");

            Directory.CreateDirectory(_monitorPath);
            Directory.CreateDirectory(Path.GetDirectoryName(_baselineFile)!);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("[FIM] STARTED | Path: {Path} | Baseline: {Baseline}",
                _monitorPath, _baselineFile);

            // Sačekaj da Serilog kreira prvi fajl
            await WaitForFirstLogFileAsync(stoppingToken);

            // Kreiraj inicijalni baseline
            await CreateBaselineAsync();

            // GLAVNO: Samo periodična provera (FileSystemWatcher ne radi dobro u Dockeru)
            while (!stoppingToken.IsCancellationRequested)
            {
                await VerifyIntegrityAsync();
                _logger.LogDebug("[FIM] VERIFY_COMPLETE | Next check in 30s");
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }

        private async Task WaitForFirstLogFileAsync(CancellationToken ct)
        {
            var attempts = 0;
            while (!ct.IsCancellationRequested && attempts < 20) // Max 2 minuta
            {
                var files = Directory.GetFiles(_monitorPath, "*.json");
                if (files.Any())
                {
                    _logger.LogInformation("[FIM] LOG_FILE_FOUND | {File}", files.First());
                    return;
                }

                _logger.LogDebug("[FIM] WAITING_FOR_LOG_FILE | Attempt {Attempt}", attempts + 1);
                await Task.Delay(TimeSpan.FromSeconds(6), ct);
                attempts++;
            }

            _logger.LogWarning("[FIM] NO_LOG_FILE_FOUND | Starting anyway");
        }

        private async Task CreateBaselineAsync()
        {
            if (!Directory.Exists(_monitorPath))
            {
                _logger.LogError("[FIM] MONITOR_PATH_MISSING | {Path}", _monitorPath);
                return;
            }

            var files = Directory.GetFiles(_monitorPath, "*.json");
            var baseline = new Dictionary<string, FileBaseline>();

            foreach (var file in files)
            {
                try
                {
                    baseline[file] = await CreateBaselineEntryAsync(file);
                    _logger.LogInformation("[FIM] BASELINE_ENTRY | {File} | Size: {Size} | Hash: {HashShort}",
                        file, baseline[file].Size, baseline[file].Hash[..8]);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[FIM] BASELINE_ENTRY_FAILED | {File}", file);
                }
            }

            await SaveBaselineAsync(baseline);
            _logger.LogInformation("[FIM] BASELINE_CREATED | Files: {Count}", baseline.Count);
        }

        private async Task<FileBaseline> CreateBaselineEntryAsync(string filepath)
        {
            // OBAVEZNO: Zatvori fajl pre nego što čitaš hash
            await Task.Delay(100); // Sačekaj da Serilog zatvori fajl

            var info = new FileInfo(filepath);

            return new FileBaseline
            {
                Path = filepath,
                Size = info.Length,
                LastModified = info.LastWriteTimeUtc,
                Hash = await CalculateHashAsync(filepath),
                CreatedAt = DateTime.UtcNow
            };
        }

        private async Task<string> CalculateHashAsync(string filepath)
        {
            using var sha256 = SHA256.Create();

            // Koristi FileStream sa ShareRead da ne blokira Serilog
            await using var stream = new FileStream(
                filepath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite); // KLJUČNO: Dozvoljava čitanje dok se piše

            var hash = await sha256.ComputeHashAsync(stream);
            return Convert.ToHexString(hash);
        }

        private async Task VerifyIntegrityAsync()
        {
            if (!File.Exists(_baselineFile))
            {
                _logger.LogWarning("[FIM] BASELINE_MISSING | Recreating...");
                await CreateBaselineAsync();
                return;
            }

            var baseline = await LoadBaselineAsync();
            if (baseline == null) return;

            var currentFiles = Directory.GetFiles(_monitorPath, "*.json");
            var alerts = new List<string>();

            // Proveri postojeće fajlove iz baseline-a
            foreach (var (path, expected) in baseline)
            {
                if (!File.Exists(path))
                {
                    alerts.Add($"MISSING: {path}");
                    continue;
                }

                var info = new FileInfo(path);

                // Preskoči ako se ništa nije promenilo
                if (info.LastWriteTimeUtc == expected.LastModified && info.Length == expected.Size)
                {
                    continue; // Nema promene, brzo preskoči
                }

                // Detaljna provera
                var currentHash = await CalculateHashAsync(path);

                if (info.Length < expected.Size)
                {
                    alerts.Add($"TRUNCATED: {path} ({expected.Size} -> {info.Length})");
                }
                else if (currentHash != expected.Hash)
                {
                    // Razlikuj append od modifikacije
                    if (info.Length > expected.Size + 100)
                    {
                        _logger.LogInformation("[FIM] APPEND_DETECTED | {Path} | {OldSize} -> {NewSize}",
                            path, expected.Size, info.Length);
                        // Ažuriraj baseline za novi sadržaj
                        baseline[path] = await CreateBaselineEntryAsync(path);
                        await SaveBaselineAsync(baseline);
                    }
                    else
                    {
                        alerts.Add($"TAMPERED: {path} (hash changed, size {info.Length})");
                    }
                }
            }

            // Proveri nove fajlove
            foreach (var file in currentFiles)
            {
                if (!baseline.ContainsKey(file))
                {
                    _logger.LogInformation("[FIM] NEW_FILE | {File}", file);
                    baseline[file] = await CreateBaselineEntryAsync(file);
                }
            }

            // Snimi ažurirani baseline
            await SaveBaselineAsync(baseline);

            // Pošalji alerte
            foreach (var alert in alerts)
            {
                SendAlert(alert);
            }

            if (alerts.Any())
            {
                _logger.LogError("[FIM] ALERTS_SENT | Count: {Count}", alerts.Count);
            }
        }

        private async Task<Dictionary<string, FileBaseline>?> LoadBaselineAsync()
        {
            try
            {
                var json = await File.ReadAllTextAsync(_baselineFile);
                return JsonSerializer.Deserialize<Dictionary<string, FileBaseline>>(json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[FIM] BASELINE_LOAD_FAILED");
                return null;
            }
        }

        private async Task SaveBaselineAsync(Dictionary<string, FileBaseline> baseline)
        {
            var json = JsonSerializer.Serialize(baseline, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            await File.WriteAllTextAsync(_baselineFile, json);
        }

        private void SendAlert(string message)
        {
            var timestamp = DateTime.UtcNow.ToString("O");
            var fullMessage = $"[{timestamp}] {message}";

            // 1. U Seq (GLAVNO)
            _logger.LogError("[FIM-ALERT] {Message}", message);

            // 2. U lokalni fajl (backup)
            try
            {
                File.AppendAllText(_alertPath, fullMessage + Environment.NewLine);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[FIM] ALERT_FILE_WRITE_FAILED");
            }

            // 3. U konzolu (vidljivo odmah)
            Console.WriteLine($"!!! FIM ALERT: {message}");
        }
    }

    public class FileBaseline
    {
        public string Path { get; set; } = string.Empty;
        public long Size { get; set; }
        public DateTime LastModified { get; set; }
        public string Hash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
