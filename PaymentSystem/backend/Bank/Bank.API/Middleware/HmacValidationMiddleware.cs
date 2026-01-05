using System.Security.Cryptography;
using System.Text;

namespace Bank.API.Middleware
{
    public class HmacValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly ILogger<HmacValidationMiddleware> _logger;

        public HmacValidationMiddleware(RequestDelegate next, IConfiguration configuration, ILogger<HmacValidationMiddleware> logger)
        {
            _next = next;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Proveri samo /api/payment/initiate endpoint
            if (context.Request.Path.StartsWithSegments("/api/payment/initiate"))
            {
                _logger.LogInformation("Validating HMAC for: {Path}", context.Request.Path);

                try
                {
                    // 1. Proveri da li postoje header-i
                    if (!context.Request.Headers.TryGetValue("X-Merchant-ID", out var merchantId) ||
                        !context.Request.Headers.TryGetValue("X-Timestamp", out var timestamp) ||
                        !context.Request.Headers.TryGetValue("X-Signature", out var signature))
                    {
                        context.Response.StatusCode = 401;
                        await context.Response.WriteAsJsonAsync(new { error = "Missing HMAC headers" });
                        return;
                    }

                    // 2. Pročitaj request body
                    context.Request.EnableBuffering();
                    string requestBody;

                    using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true))
                    {
                        requestBody = await reader.ReadToEndAsync();
                        context.Request.Body.Position = 0;
                    }

                    // 3. Dobij secret key
                    var secretKey = _configuration["HmacSettings:SecretKey"];
                    if (string.IsNullOrEmpty(secretKey))
                    {
                        throw new Exception("HMAC secret not configured");
                    }

                    // 4. Validiraj HMAC
                    var isValid = ValidateHmac(merchantId, timestamp, signature, requestBody, secretKey);

                    if (!isValid)
                    {
                        context.Response.StatusCode = 401;
                        await context.Response.WriteAsJsonAsync(new { error = "Invalid HMAC signature" });
                        return;
                    }

                    _logger.LogInformation("HMAC validation successful for Merchant: {MerchantId}", merchantId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "HMAC validation failed");
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsJsonAsync(new { error = "Authentication error" });
                    return;
                }
            }

            await _next(context);
        }

        private bool ValidateHmac(string merchantId, string timestamp, string receivedSignature, string requestBody, string secretKey)
        {
            // 1. Proveri timestamp (max 5 minuta stari)
            if (!DateTime.TryParse(timestamp, out var requestTime) ||
                (DateTime.UtcNow - requestTime).TotalMinutes > 30)
            {
                return false;
            }

            // 2. Generiši očekivani potpis
            var message = $"{merchantId}{timestamp}{requestBody}";
            var expectedSignature = GenerateHmac(message, secretKey);

            // 3. Uporedi
            return string.Equals(receivedSignature, expectedSignature, StringComparison.OrdinalIgnoreCase);
        }

        private string GenerateHmac(string message, string secretKey)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }
}
