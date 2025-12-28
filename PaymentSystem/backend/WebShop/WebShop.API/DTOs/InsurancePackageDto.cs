namespace WebShop.API.DTOs
{
    public class InsurancePackageDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal PricePerDay { get; set; }
        public decimal CoverageLimit { get; set; }
        public decimal Deductible { get; set; }
        public bool IsActive { get; set; }
    }
}
