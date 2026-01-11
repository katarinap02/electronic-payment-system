using Microsoft.EntityFrameworkCore;

namespace WebShop.API.Data
{
    public static class DatabaseInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            // Primeni migracije
            context.Database.Migrate();

            // Kreiraj Rentals tabelu ako ne postoji
            CreateRentalsTable(context);
        }

        private static void CreateRentalsTable(AppDbContext context)
        {
            try
            {
                var connection = context.Database.GetDbConnection();
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    // Proveri da li tabela već postoji
                    command.CommandText = @"
                        SELECT COUNT(*)
                        FROM information_schema.tables
                        WHERE table_name = 'Rentals'";

                    var exists = Convert.ToInt32(command.ExecuteScalar()) > 0;

                    if (!exists)
                    {
                        // Kreiraj tabelu
                        command.CommandText = @"
                            CREATE TABLE ""Rentals"" (
                                ""Id"" SERIAL PRIMARY KEY,
                                ""UserId"" bigint NOT NULL,
                                ""VehicleId"" bigint NOT NULL,
                                ""StartDate"" timestamp with time zone NOT NULL,
                                ""EndDate"" timestamp with time zone NOT NULL,
                                ""RentalDays"" integer NOT NULL,
                                ""AdditionalServices"" text,
                                ""AdditionalServicesPrice"" decimal(18,2) NOT NULL,
                                ""InsuranceType"" text,
                                ""InsurancePrice"" decimal(18,2) NOT NULL,
                                ""VehiclePricePerDay"" decimal(18,2) NOT NULL,
                                ""TotalPrice"" decimal(18,2) NOT NULL,
                                ""PaymentId"" varchar(50) NOT NULL,
                                ""GlobalTransactionId"" varchar(100),
                                ""Currency"" varchar(10) NOT NULL DEFAULT 'EUR',
                                ""PaymentMethod"" varchar(20) NOT NULL,
                                ""Status"" varchar(20) NOT NULL DEFAULT 'Active',
                                ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                ""CompletedAt"" timestamp with time zone,
                                ""CancelledAt"" timestamp with time zone,
                                ""Notes"" varchar(500),
                                CONSTRAINT ""FK_Rentals_Users_UserId"" FOREIGN KEY (""UserId"") REFERENCES ""Users"" (""Id"") ON DELETE CASCADE,
                                CONSTRAINT ""FK_Rentals_Vehicles_VehicleId"" FOREIGN KEY (""VehicleId"") REFERENCES ""Vehicles"" (""Id"") ON DELETE CASCADE
                            );
                            
                            CREATE INDEX ""IX_Rentals_UserId"" ON ""Rentals"" (""UserId"");
                            CREATE INDEX ""IX_Rentals_VehicleId"" ON ""Rentals"" (""VehicleId"");";
                        
                        command.ExecuteNonQuery();
                        Console.WriteLine("Rentals table created successfully!");
                    }
                    else
                    {
                        Console.WriteLine("Rentals table already exists.");
                    }
                }

                connection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating Rentals table: {ex.Message}");
            }
        }
    }
}
