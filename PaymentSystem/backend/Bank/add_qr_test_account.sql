-- Dodaj test customer za QR plaćanja
INSERT INTO "Customers" ("Id", "FullName", "EmailHash", "PhoneHash", "Status", "CreatedAt", "WebShopUserIdHash")
VALUES ('CUST_QR_TEST', 'QR Test Customer', 'qr@test.com', '+381601234567', 1, NOW(), 'qr-test-hash')
ON CONFLICT ("Id") DO NOTHING;

-- Dodaj test račun sa ID-om koji koristi ConfirmQrPayment
INSERT INTO "BankAccounts" ("AccountNumber", "SwiftCode", "Balance", "AvailableBalance", "ReservedBalance", "PendingCaptureBalance", "Currency", "IsMerchantAccount", "CustomerId")
VALUES ('1234567890123', 'BACXRSBG', 50000.00, 50000.00, 0, 0, 'RSD', false, 'CUST_QR_TEST')
ON CONFLICT ("AccountNumber") DO UPDATE SET "Balance" = 50000.00, "AvailableBalance" = 50000.00;
