using System.Security.Claims;

namespace WebShop.API.Middleware
{
    public class AuditLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuditLoggingMiddleware> _logger;

        public AuditLoggingMiddleware(RequestDelegate next, ILogger<AuditLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // PRIKUPLJANJE PODATAKA
            var ipAddress = GetClientIpAddress(context);
            // User ID - iz JWT tokena ili identity
            var userId = GetUserId(context);
            var path = context.Request.Path;
            var method = context.Request.Method;
            // Protokol 
            var scheme = GetRequestScheme(context);
            var startTime = DateTime.UtcNow;
            // Korrelation ID - povezuje logove iz različitih servisa
            var correlationId = GetOrCreateCorrelationId(context);

            // LOGOVANJE POČETKA ZAHTEVA 

            using (_logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId,
                ["IpAddress"] = ipAddress,
                ["UserId"] = userId,
                ["RequestPath"] = path,
                ["HttpMethod"] = method,
                ["Scheme"] = scheme,
                ["StartTime"] = startTime,
                ["Timestamp"] = startTime
            }))
            {
                _logger.LogInformation(
                    "[AUDIT] Request started: {Scheme} {Method} {Path} | IP: {Ip} | User: {User} | CorrelationId: {CorrelationId}",
                    scheme, method, path, ipAddress, userId, correlationId);

                try
                {
                    await _next(context);

                    // LOGOVANJE USPEŠNOG ZAVRŠETKA 

                    var duration = DateTime.UtcNow - startTime;

                    _logger.LogInformation(
                        "[AUDIT] Request completed: {Method} {Path} | Status: {StatusCode} | Duration: {DurationMs}ms | User: {User}",
                        method, path, context.Response.StatusCode, duration.TotalMilliseconds, userId);
                }
                catch (Exception ex)
                {
                    // LOGOVANJE GREŠKE 

                    _logger.LogError(ex,
                        "[AUDIT] Request failed: {Method} {Path} | Status: {StatusCode} | Error: {ErrorMessage} | User: {User}",
                        method, path, context.Response.StatusCode, ex.Message, userId);

                    throw;
                }
            }
        }

        private string GetClientIpAddress(HttpContext context)
        {
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',')[0].Trim();
            }

            var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
            {
                return realIp;
            }

            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        private string GetUserId(HttpContext context)
        {
            // Standardni JWT claimovi (sub = subject = user ID)
            var userId = context.User.FindFirst("sub")?.Value
                ?? context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? context.User.FindFirst("id")?.Value
                ?? context.User.Identity?.Name;

            // Ako nema autentikacije, proveri da li je merchant API poziv (PSP scenario)
            if (string.IsNullOrEmpty(userId))
            {
                // Proveri custom header za merchant ID (za servis-servis komunikaciju)
                userId = context.Request.Headers["X-Merchant-ID"].FirstOrDefault()
                    ?? context.Request.Headers["X-API-Key"].FirstOrDefault();
            }

            return userId ?? "anonymous";
        }

        private string GetRequestScheme(HttpContext context)
        {
            var forwardedProto = context.Request.Headers["X-Forwarded-Proto"].FirstOrDefault();

            if (!string.IsNullOrEmpty(forwardedProto))
            {
                return forwardedProto; // https ili http
            }

            return context.Request.Scheme;
        }

        private string GetOrCreateCorrelationId(HttpContext context)
        {
            // Proveri da li klijent već ima correlation ID
            var existingId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault();

            if (!string.IsNullOrEmpty(existingId))
            {
                return existingId;
            }

            // Generiši novi
            var newId = Guid.NewGuid().ToString("N")[..12];

            // Sačuvaj za response header (pomaže pri debugiranju)
            context.Response.Headers["X-Correlation-Id"] = newId;

            return newId;
        }
    }
}
