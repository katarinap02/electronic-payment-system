using WebShop.API.Models;

namespace WebShop.API.Data
{
    public static class InsurancePackageSeedData
    {
        public static void SeedInsurancePackages(AppDbContext context)
        {
            if (context.InsurancePackages.Any())
                return; // Already seeded

            var packages = new List<InsurancePackage>
            {
                new InsurancePackage
                {
                    Name = "Basic",
                    Description = "Minimum required insurance. Covers third-party liability only. Not recommended for long trips.",
                    PricePerDay = 5.00m,
                    CoverageLimit = 10000.00m,
                    Deductible = 1000.00m,
                    IsActive = true
                },
                new InsurancePackage
                {
                    Name = "Standard",
                    Description = "Covers collision damage and theft with reduced deductible. Good balance between cost and protection.",
                    PricePerDay = 10.00m,
                    CoverageLimit = 25000.00m,
                    Deductible = 500.00m,
                    IsActive = true
                },
                new InsurancePackage
                {
                    Name = "Premium",
                    Description = "Comprehensive coverage including windshield damage, tire punctures, and personal belongings up to 500 EUR.",
                    PricePerDay = 15.00m,
                    CoverageLimit = 50000.00m,
                    Deductible = 250.00m,
                    IsActive = true
                },
                new InsurancePackage
                {
                    Name = "Full Coverage",
                    Description = "Zero deductible! All-inclusive protection with 24/7 roadside assistance, towing, and replacement vehicle.",
                    PricePerDay = 20.00m,
                    CoverageLimit = 100000.00m,
                    Deductible = 0.00m,
                    IsActive = true
                }
            };

            context.InsurancePackages.AddRange(packages);
            context.SaveChanges();
        }
    }
}
