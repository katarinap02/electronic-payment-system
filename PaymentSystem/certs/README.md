# SSL Sertifikati za sve servise

## Generisanje sertifikata

Za generisanje self-signed sertifikata za development okruženje, pokrenite:

```powershell
.\generate-certs.ps1
```

Ova skripta će kreirati sertifikate za sve servise:
- `psp-api.pfx` / `psp-api.cer` - PSP server
- `webshop-api.pfx` / `webshop-api.cer` - WebShop server  
- `bank-api.pfx` / `bank-api.cer` - Bank server
- `paypal-api.pfx` / `paypal-api.cer` - PayPal server

PFX fajlovi sadrže privatni ključ za servere, a CER fajlovi su javni ključevi.

## Lozinka

Lozinka za sve PFX fajlove je: `dev-cert-2024`

## Napomena

Ovi sertifikati su namenjeni **samo za development**. Za production okruženje koristite validne sertifikate od overenih CA autoriteta (npr. Let's Encrypt).
