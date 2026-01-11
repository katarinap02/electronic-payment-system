using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Bank.API.Data
{
    public static class DatabaseInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            // Primeni migracije
            context.Database.Migrate();

            // Dodaj PaymentMethodCode kolonu ako ne postoji
            AddPaymentMethodCodeColumn(context);
        }

        private static void AddPaymentMethodCodeColumn(AppDbContext context)
        {
            try
            {
                var connection = context.Database.GetDbConnection();
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    // Proveri da li kolona već postoji
                    command.CommandText = @"
                        SELECT COUNT(*)
                        FROM information_schema.columns
                        WHERE table_name = 'PaymentTransactions'
                        AND column_name = 'PaymentMethodCode'";

                    var exists = Convert.ToInt32(command.ExecuteScalar()) > 0;

                    if (!exists)
                    {
                        // Dodaj kolonu
                        command.CommandText = @"
                            ALTER TABLE ""PaymentTransactions""
                            ADD COLUMN ""PaymentMethodCode"" text";
                        command.ExecuteNonQuery();
                        Console.WriteLine("PaymentMethodCode column added successfully!");
                    }
                    else
                    {
                        Console.WriteLine("PaymentMethodCode column already exists.");
                    }
                }

                connection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding PaymentMethodCode column: {ex.Message}");
            }
        }
    }
}
