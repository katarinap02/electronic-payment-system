-- Dodaj test račun za QR plaćanje (bez ON CONFLICT)
INSERT INTO "BankAccounts" ("AccountNumber", "SwiftCode", "Balance", "AvailableBalance", "ReservedBalance", "PendingCaptureBalance", "Currency", "IsMerchantAccount", "CustomerId")
VALUES ('1234567890123', 'BACXRSBG', 50000.00, 50000.00, 0, 0, 'RSD', false, 'CUST_QR_TEST');
