namespace WebShop.API.DTOs
{
    /// <summary>
    /// DTO za kreiranje nove kupovine/rentala
    /// </summary>
    public class CreateRentalDto
    {
        public long UserId { get; set; }
        public long VehicleId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        // Dodatne usluge (array of service IDs ili naziva)
        public List<string>? AdditionalServices { get; set; }
        public decimal AdditionalServicesPrice { get; set; }
        
        // Osiguranje
        public string? InsuranceType { get; set; }
        public decimal InsurancePrice { get; set; }
        
        // Payment info
        public string PaymentId { get; set; } = string.Empty;
        public string? GlobalTransactionId { get; set; }
        public decimal TotalPrice { get; set; }
        public string Currency { get; set; } = "EUR";
        public string PaymentMethod { get; set; } = string.Empty;
        
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO za prikaz rental informacija
    /// </summary>
    public class RentalDto
    {
        public int Id { get; set; }
        
        // User info
        public long UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        
        // Vehicle info
        public long VehicleId { get; set; }
        public string VehicleBrand { get; set; } = string.Empty;
        public string VehicleModel { get; set; } = string.Empty;
        public string VehicleCategory { get; set; } = string.Empty;
        public string LicensePlate { get; set; } = string.Empty;
        
        // Rental period
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int RentalDays { get; set; }
        
        // Services
        public List<string> AdditionalServices { get; set; } = new();
        public decimal AdditionalServicesPrice { get; set; }
        
        // Insurance
        public string? InsuranceType { get; set; }
        public decimal InsurancePrice { get; set; }
        
        // Pricing
        public decimal VehiclePricePerDay { get; set; }
        public decimal TotalPrice { get; set; }
        
        // Payment
        public string PaymentId { get; set; } = string.Empty;
        public string? GlobalTransactionId { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        
        // Status
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO za update rental statusa
    /// </summary>
    public class UpdateRentalStatusDto
    {
        public string Status { get; set; } = string.Empty; // Active, Completed, Cancelled
        public string? Notes { get; set; }
    }
}

