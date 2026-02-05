using System.Text;
using System.Text.Json;
using Bank.API.DTOs;
using QRCoder;

namespace Bank.API.Services
{
    /// <summary>
    /// Servis za generisanje i validaciju NBS IPS QR kodova putem NBS API-ja
    /// </summary>
    public class NbsQrCodeService
    {
        private readonly HttpClient _httpClient;
        private const string NBS_API_BASE_URL = "https://nbs.rs/QRcode/api/qr/v1";

        public NbsQrCodeService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Generiše NBS IPS QR kod kao base64 sliku
        /// </summary>
        public async Task<QrCodeGenerationResult> GenerateQrCodeAsync(QrCodePayload payload, int size = 300)
        {
            try
            {
                // Pošto NBS API zahteva registrovane račune u IPS sistemu,
                // koristimo LOKALNI generator koji pravi validan IPS format
                return GenerateQrCodeLocally(payload, size);
            }
            catch (Exception ex)
            {
                return new QrCodeGenerationResult
                {
                    Success = false,
                    ErrorMessage = $"Error generating QR code: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Generiše QR kod LOKALNO prema NBS IPS specifikaciji
        /// </summary>
        private QrCodeGenerationResult GenerateQrCodeLocally(QrCodePayload payload, int size = 300)
        {
            try
            {
                // Format iznosa prema NBS specifikaciji
                string amountFormatted;
                if (payload.Currency.Equals("RSD", StringComparison.OrdinalIgnoreCase))
                {
                    amountFormatted = $"RSD{payload.Amount:F2}".Replace(".", ",");
                }
                else
                {
                    // Za devize - formato sa prefiksom valute (npr. EUR27,00)
                    amountFormatted = $"{payload.Currency}{payload.Amount:F2}".Replace(".", ",");
                }

                // Ime primaoca - max 70 karaktera
                var receiverName = string.IsNullOrWhiteSpace(payload.ReceiverName)
                    ? "Trgovac"
                    : payload.ReceiverName.Length > 70
                        ? payload.ReceiverName.Substring(0, 70)
                        : payload.ReceiverName;

                // Svrha plaćanja - max 35 karaktera
                var paymentPurpose = string.IsNullOrWhiteSpace(payload.PaymentPurpose)
                    ? "Placanje"
                    : payload.PaymentPurpose.Length > 35
                        ? payload.PaymentPurpose.Substring(0, 35)
                        : payload.PaymentPurpose;

                // IBAN sa RS prefiksom
                var accountNumber = payload.ReceiverAccountNumber;
                if (!accountNumber.StartsWith("RS", StringComparison.OrdinalIgnoreCase))
                {
                    accountNumber = $"RS35{accountNumber}"; // Dodaj RS prefix ako nema
                }

                // Kreiraj NBS IPS format teksta prema specifikaciji
                // Format: K:PR|V:01|C:1|R:IBAN|N:Ime|I:Iznos|SF:Sifra|S:Svrha|RO:Poziv
                var ipsText = $"K:PR|V:01|C:1|R:{accountNumber}|N:{receiverName}|I:{amountFormatted}|SF:289|S:{paymentPurpose}|RO:";


                // Generiši QR kod koristeći QRCoder library
                using var qrGenerator = new QRCodeGenerator();
                using var qrCodeData = qrGenerator.CreateQrCode(ipsText, QRCodeGenerator.ECCLevel.M);
                using var qrCode = new PngByteQRCode(qrCodeData);
                
                var qrCodeImage = qrCode.GetGraphic(20); // 20 pixels per module
                var base64Image = Convert.ToBase64String(qrCodeImage);

             

                return new QrCodeGenerationResult
                {
                    Success = true,
                    QrCodeBase64 = base64Image,
                    QrCodeDataUrl = $"data:image/png;base64,{base64Image}",
                    Payload = ipsText
                };
            }
            catch (Exception ex)
            {
                return new QrCodeGenerationResult
                {
                    Success = false,
                    ErrorMessage = $"Error generating local QR code: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Generiše QR kod putem NBS API-ja (zahteva registrovane račune)
        /// </summary>
        private async Task<QrCodeGenerationResult> GenerateQrCodeViaApiAsync(QrCodePayload payload, int size = 300)
        {
            try
            {
                // Format iznosa prema NBS specifikaciji:
                // - Za RSD: "RSD123,45"
                // - Za devize (EUR, USD): samo "123,45" (BEZ oznake valute!)
                string amountFormatted;
                if (payload.Currency.Equals("RSD", StringComparison.OrdinalIgnoreCase))
                {
                    amountFormatted = $"RSD{payload.Amount:F2}".Replace(".", ",");
                }
                else
                {
                    // Za devize (EUR, USD) - samo iznos bez valute
                    amountFormatted = $"{payload.Amount:F2}".Replace(".", ",");
                }

                // Ime primaoca - max 70 karaktera, obavezno polje
                var receiverName = string.IsNullOrWhiteSpace(payload.ReceiverName) 
                    ? "Trgovac" 
                    : payload.ReceiverName.Length > 70 
                        ? payload.ReceiverName.Substring(0, 70) 
                        : payload.ReceiverName;

                // Svrha plaćanja - max 35 karaktera, bez specijalnih znakova
                // DIREKTNO skraćujemo na 30 karaktera za sigurnost (multi-byte UTF-8)
                var paymentPurpose = string.IsNullOrWhiteSpace(payload.PaymentPurpose)
                    ? "Placanje"
                    : payload.PaymentPurpose.Length > 30
                        ? payload.PaymentPurpose.Substring(0, 30)
                        : payload.PaymentPurpose;

                // NBS API očekuje samo broj računa (18 cifara) BEZ RS35 prefiksa!
                // IBAN: RS35260005601001611379 -> šaljemo: 260005601001611379
                var accountNumber = payload.ReceiverAccountNumber;
                if (accountNumber.StartsWith("RS", StringComparison.OrdinalIgnoreCase))
                {
                    accountNumber = accountNumber.Substring(4); // Skloni RS35
                }

                // Kreiraj JSON payload za NBS API
                var nbsPayload = new
                {
                    K = "PR", // Payment Request - tip QR koda
                    V = "01", // Verzija
                    C = "1",  // Character set (1 = UTF-8)
                    R = accountNumber, // Broj računa (18 cifara, BEZ RS prefiksa)
                    N = receiverName, // Naziv primaoca (max 70 karaktera)
                    I = amountFormatted, // Iznos: RSD123,45 ili 123,45 (za devize)
                    S = paymentPurpose, // Svrha plaćanja (max 35 karaktera)
                    SF = "189", // Šifra plaćanja (189 - ostalo)
                    RO = "" // Poziv na broj (opciono)
                };

                var jsonContent = JsonSerializer.Serialize(nbsPayload);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

              

                // Pozovi NBS API za generisanje QR koda - koristi /generate endpoint koji vraća JSON
                var response = await _httpClient.PostAsync($"{NBS_API_BASE_URL}/generate?lang=sr_RS_Latn", content);

                if (response.IsSuccessStatusCode)
                {
                    // NBS vraća JSON sa base64 slikom
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    
                    // Proveri da li je generisanje uspešno
                    if (jsonResponse.TryGetProperty("image", out var imageProperty))
                    {
                        var base64Image = imageProperty.GetString() ?? string.Empty;
                        
                        

                        return new QrCodeGenerationResult
                        {
                            Success = true,
                            QrCodeBase64 = base64Image,
                            QrCodeDataUrl = $"data:image/png;base64,{base64Image}",
                            Payload = jsonContent
                        };
                    }
                    else
                    {

                        return new QrCodeGenerationResult
                        {
                            Success = false,
                            ErrorMessage = "NBS API response missing image data"
                        };
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                   

                    return new QrCodeGenerationResult
                    {
                        Success = false,
                        ErrorMessage = $"Failed to generate QR code: {response.StatusCode}"
                    };
                }
            }
            catch (Exception ex)
            {

                return new QrCodeGenerationResult
                {
                    Success = false,
                    ErrorMessage = $"Error generating QR code: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Validira NBS IPS QR kod korišćenjem NBS API-ja
        /// </summary>
        public async Task<bool> ValidateQrCodeAsync(string qrCodeText)
        {
            try
            {
                var content = new StringContent(qrCodeText, Encoding.UTF8, "text/plain");
                var response = await _httpClient.PostAsync($"{NBS_API_BASE_URL}/validate?lang=sr_RS_Latn", content);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
                    
                    // Proveri da li je code = 0 (uspešna validacija)
                    if (jsonResult.TryGetProperty("s", out var status))
                    {
                        if (status.TryGetProperty("code", out var code))
                        {
                            return code.GetInt32() == 0;
                        }
                    }
                }

                return false;
            }
            catch (Exception ex)
            {

                return false;
            }
        }
    }

    /// <summary>
    /// Payload za generisanje QR koda
    /// </summary>
    public class QrCodePayload
    {
        public string PaymentId { get; set; } = string.Empty;
        public string ReceiverAccountNumber { get; set; } = string.Empty;
        public string ReceiverName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "RSD";
        public string PaymentPurpose { get; set; } = string.Empty;
    }

    /// <summary>
    /// Rezultat generisanja QR koda
    /// </summary>
    public class QrCodeGenerationResult
    {
        public bool Success { get; set; }
        public string QrCodeBase64 { get; set; } = string.Empty;
        public string QrCodeDataUrl { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
