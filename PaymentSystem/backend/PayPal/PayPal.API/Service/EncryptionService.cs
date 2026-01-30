using System.Security.Cryptography;
using System.Text;

namespace PayPal.API.Service
{
    public class EncryptionService
    {
        private readonly byte[] _key;

        public EncryptionService(IConfiguration config)
        {
            // Čita ENCRYPTION_KEY iz .env fajla 
            var keyString = config["ENCRYPTION_KEY"]
                ?? throw new Exception("ENCRYPTION_KEY nije postavljen u .env fajlu");

            _key = Convert.FromBase64String(keyString);
        }

        // Šifruje tekst
        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return plainText;

            using var aes = Aes.Create();
            aes.Key = _key;
            aes.GenerateIV(); // Generiše nasumični Initialization Vector

            var encryptor = aes.CreateEncryptor();
            var bytes = Encoding.UTF8.GetBytes(plainText);
            var encrypted = encryptor.TransformFinalBlock(bytes, 0, bytes.Length);

            // Spojimo IV + enkriptovane podatke
            // IV se čuva zajedno sa podacima jer je potreban za dekripciju
            var result = aes.IV.Concat(encrypted).ToArray();
            return Convert.ToBase64String(result);
        }

        // Dešifruje tekst 
        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) return cipherText;

            var fullCipher = Convert.FromBase64String(cipherText);

            using var aes = Aes.Create();
            aes.Key = _key;

            var iv = fullCipher.Take(16).ToArray();
            var cipher = fullCipher.Skip(16).ToArray();

            aes.IV = iv;
            var decryptor = aes.CreateDecryptor();
            var decrypted = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);

            return Encoding.UTF8.GetString(decrypted);
        }
    }
}
