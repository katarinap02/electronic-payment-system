namespace Bank.API.Services
{
    public class CardService
    {
        private readonly ILogger<CardService> _logger;

        public CardService(ILogger<CardService> logger)
        {
            _logger = logger;
        }
        public bool ValidateByLuhn(string cardNumber)
        {
            _logger.LogInformation($"Validating card by Luhn: {MaskCardNumber(cardNumber)}");

            cardNumber = cardNumber.Replace(" ", "").Replace("-", "");

            int sum = 0;
            bool pom = false;

            for (int i = cardNumber.Length - 1; i >= 0; i--)
            {
                if (!char.IsDigit(cardNumber[i]))
                    return false;

                int digit = int.Parse(cardNumber[i].ToString());

                if (pom) //da bi uzeo svaku drugu
                {
                    digit *= 2;
                    if (digit > 9)
                        digit = (digit % 10) + 1;
                }

                sum += digit;
                pom = !pom;
            }

            return (sum % 10 == 0);
        }

        public bool ValidateExpiryDate(string expiryDate)
        {
            try
            {
                var parts = expiryDate.Split('/');
                if (parts.Length != 2) return false;

                int month = int.Parse(parts[0]);
                int year = int.Parse("20" + parts[1]); // "25" → 2025

                if (month < 1 || month > 12) return false;

                var expiry = new DateTime(year, month, 1).AddMonths(1).AddDays(-1);
                return expiry >= DateTime.Now;
            }
            catch
            {
                return false;
            }
        }

        public string TokenizeCard(string cardNumber)
        {
            var token = "tok_" + Guid.NewGuid().ToString("N").Substring(0, 16);
            _logger.LogInformation($"Tokenized card: {MaskCardNumber(cardNumber)} → {token}");
            return token;
        }

        // Maskiranje kartice za logging (PCI DSS)
        private string MaskCardNumber(string cardNumber)
        {
            if (string.IsNullOrEmpty(cardNumber) || cardNumber.Length < 4)
                return "****";

            return $"**** **** **** {cardNumber.Substring(cardNumber.Length - 4)}";
        }
    }

}
