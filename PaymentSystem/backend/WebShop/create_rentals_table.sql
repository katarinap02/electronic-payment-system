-- Kreiranje Rentals tabele
CREATE TABLE IF NOT EXISTS "Rentals" (
    "Id" SERIAL PRIMARY KEY,
    "UserId" BIGINT NOT NULL,
    "VehicleId" BIGINT NOT NULL,
    "StartDate" TIMESTAMP NOT NULL,
    "EndDate" TIMESTAMP NOT NULL,
    "RentalDays" INTEGER NOT NULL,
    "AdditionalServices" VARCHAR(1000),
    "AdditionalServicesPrice" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "InsuranceType" VARCHAR(50),
    "InsurancePrice" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "VehiclePricePerDay" DECIMAL(18,2) NOT NULL,
    "TotalPrice" DECIMAL(18,2) NOT NULL,
    "PaymentId" VARCHAR(50) NOT NULL UNIQUE,
    "GlobalTransactionId" VARCHAR(100),
    "Currency" VARCHAR(10) NOT NULL DEFAULT 'EUR',
    "PaymentMethod" VARCHAR(20) NOT NULL,
    "Status" VARCHAR(20) NOT NULL DEFAULT 'Active',
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT NOW(),
    "CompletedAt" TIMESTAMP,
    "CancelledAt" TIMESTAMP,
    "Notes" VARCHAR(500),
    
    CONSTRAINT "FK_Rentals_Users" FOREIGN KEY ("UserId") REFERENCES "Users"("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Rentals_Vehicles" FOREIGN KEY ("VehicleId") REFERENCES "Vehicles"("Id") ON DELETE RESTRICT
);

-- Kreiranje indeksa za bolje performanse
CREATE INDEX IF NOT EXISTS "IX_Rentals_UserId" ON "Rentals"("UserId");
CREATE INDEX IF NOT EXISTS "IX_Rentals_VehicleId" ON "Rentals"("VehicleId");
CREATE INDEX IF NOT EXISTS "IX_Rentals_PaymentId" ON "Rentals"("PaymentId");
CREATE INDEX IF NOT EXISTS "IX_Rentals_Status" ON "Rentals"("Status");
CREATE INDEX IF NOT EXISTS "IX_Rentals_CreatedAt" ON "Rentals"("CreatedAt" DESC);

