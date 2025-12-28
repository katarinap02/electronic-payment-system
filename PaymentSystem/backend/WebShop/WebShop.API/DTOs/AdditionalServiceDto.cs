namespace WebShop.API.DTOs
{
    public class AdditionalServiceDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal PricePerDay { get; set; }
        public bool IsAvailable { get; set; }
        public string? IconUrl { get; set; }
    }
}
