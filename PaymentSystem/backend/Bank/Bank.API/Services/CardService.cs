using Bank.API.DTOs;
using Bank.API.Models;
using Bank.API.Repositories;
using System.Security.Cryptography;
using System.Text;

namespace Bank.API.Services
{
    public class CardService
    {
        private readonly CardRepository _cardRepo;
        private readonly CardTokenRepository _tokenRepo;

        public CardService(
            CardRepository cardRepo,
            CardTokenRepository tokenRepo)
        {
            _cardRepo = cardRepo;
            _tokenRepo = tokenRepo;

        }

        //Tokenizacija PAN-a 
        public string TokenizePan(string pan, string merchantId)
        {
            try
            {
                if (!_cardRepo.ValidateCardNumber(pan))
                    throw new ArgumentException("Invalid card number");

                // Generiše sigurni hash za PAN
                using var sha256 = SHA256.Create();
                var panBytes = Encoding.UTF8.GetBytes(pan + merchantId + GetSalt());
                var hashBytes = sha256.ComputeHash(panBytes);

                // Vrati tokenizovanu verziju (prvi i poslednji deo maskirani)
                var maskedPan = MaskPan(pan);
                var token = "tok_" + Convert.ToBase64String(hashBytes)
                    .Replace("+", "")
                    .Replace("/", "")
                    .Replace("=", "")
                    .Substring(0, 16);

                return token;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        // Validacija kartice za plaćanje
        public CardValidationResult ValidateCardForPayment(CardInformation cardInfo, decimal amount)
        {
            try
            {
                var result = new CardValidationResult();

                // Luhn validacija
                if (!_cardRepo.ValidateCardNumber(cardInfo.CardNumber))
                {
                    result.IsValid = false;
                    result.ErrorMessage = "Invalid card number";
                    return result;
                }

                // Validacija datuma
                var expiryParts = cardInfo.ExpiryDate.Split('/');
                if (expiryParts.Length != 2)
                {
                    result.IsValid = false;
                    result.ErrorMessage = "Invalid expiry date format";
                    return result;
                }

                if (!_cardRepo.ValidateExpiryDate(expiryParts[0], expiryParts[1]))
                {
                    result.IsValid = false;
                    result.ErrorMessage = "Card has expired";
                    return result;
                }

                // Provera CVV formata
                if (string.IsNullOrEmpty(cardInfo.Cvv) ||
                   (cardInfo.Cvv.Length != 3 && cardInfo.Cvv.Length != 4))
                {
                    result.IsValid = false;
                    result.ErrorMessage = "Invalid CVV";
                    return result;
                }

                // Provera cardholder name
                if (string.IsNullOrWhiteSpace(cardInfo.CardholderName))
                {
                    result.IsValid = false;
                    result.ErrorMessage = "Cardholder name is required";
                    return result;
                }

                // 5. Maskirani PAN za logovanje
                result.MaskedPan = MaskPan(cardInfo.CardNumber);
                result.IsValid = true;


                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public string GenerateCardHash(string pan)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var panBytes = System.Text.Encoding.UTF8.GetBytes(pan + GetCardHashSalt());
            var hashBytes = sha256.ComputeHash(panBytes);
            return Convert.ToBase64String(hashBytes);
        }

        private string GetCardHashSalt()
        {
            return Environment.GetEnvironmentVariable("CARD_HASH_SALT") ??
                   "default-card-salt-change-in-production";
        }

        // Maskiranje PAN-a za logovanje (PCI DSS)
        private string MaskPan(string pan)
        {
            if (string.IsNullOrEmpty(pan) || pan.Length < 8)
                return "****";

            return pan.Substring(0, 4) + new string('*', pan.Length - 8) + pan.Substring(pan.Length - 4);
        }

        // Salt za hash
        private string GetSalt()
        {
            return Environment.GetEnvironmentVariable("CARD_HASH_SALT") ??
                   "default-salt-change-in-production";
        }

        public class CardValidationResult
        {
            public bool IsValid { get; set; }
            public string MaskedPan { get; set; }
            public string ErrorMessage { get; set; }
        }
    }
}