-- Obriši testni račun i customer-a
DELETE FROM "BankAccounts" WHERE "AccountNumber" = '1234567890123';
DELETE FROM "Customers" WHERE "Id" = 'CUST_QR_TEST';
