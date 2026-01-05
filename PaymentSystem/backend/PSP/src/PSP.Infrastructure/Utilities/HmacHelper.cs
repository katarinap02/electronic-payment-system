using System.Security.Cryptography;
using System.Text;

namespace PSP.Infrastructure.Utilities
{
    public static class HmacHelper
    {
        //koristiti SHA256
        public static string GenerateSignature(string merchantId, string timestamp,
    string requestBody, string secretKey)
        {
            var message = $"{merchantId}{timestamp}{requestBody}";
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            var messageBytes = Encoding.UTF8.GetBytes(message);

            using var hmac = new HMACSHA256(keyBytes);
            var hash = hmac.ComputeHash(messageBytes);

            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }
}
