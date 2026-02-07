# HTTPS Komunikacija - Celokupna arhitektura

Ovaj dokument opisuje konfiguraciju sigurne **HTTPS-only** komunikacije za ceo sistem.

## 📋 Pregled

- **Svi servisi**: **Samo HTTPS** (bez HTTP)
- **Sertifikati**: Self-signed sertifikati za development okruženje
- **Enkripcija**: TLS 1.2+

## 🔧 Konfiguracija

### 1. Generisanje SSL Sertifikata

Pre pokretanja sistema, generišite SSL sertifikate za sve servise:

```powershell
cd PaymentSystem\certs
.\generate-certs.ps1
```

Skripta će kreirati sertifikate za:
- **PSP API**: `psp-api.pfx` / `psp-api.cer`
- **WebShop API**: `webshop-api.pfx` / `webshop-api.cer`
- **Bank API**: `bank-api.pfx` / `bank-api.cer`
- **PayPal API**: `paypal-api.pfx` / `paypal-api.cer`

**Lozinka za sve**: `dev-cert-2024`

### 1.5. Trusted Sertifikati za Frontend (mkcert)

**Problem:** Browser-i pokazuju security upozorenja za self-signed sertifikate na frontend aplikacijama.

**Rešenje:** Koristite `mkcert` za generisanje lokalno trusted sertifikata za HTTPS frontend development.

#### Instalacija mkcert

```powershell
cd PaymentSystem\certs

# Opcija 1: Preuzimanje exe fajla
Invoke-WebRequest -Uri "https://github.com/FiloSottile/mkcert/releases/download/v1.4.4/mkcert-v1.4.4-windows-amd64.exe" -OutFile "mkcert.exe"

# Opcija 2: Chocolatey
choco install mkcert

# Opcija 3: Scoop
scoop bucket add extras
scoop install mkcert
```

#### Generisanje Trusted Sertifikata

```powershell
# 1. Instaliraj local CA u sistem trust store
.\mkcert.exe -install

# 2. Generiši sertifikat za localhost
.\mkcert.exe localhost 127.0.0.1 ::1
```

Ovo generiše:
- `localhost+2.pem` - Public certificate
- `localhost+2-key.pem` - Private key

#### Firefox Setup (Opcionalno)

Firefox koristi sopstveni certificate store:

1. Otvori Firefox → **Settings** → **Privacy & Security**
2. **Certificates** → **View Certificates** → **Authorities**
3. **Import...** → Izaberi: `C:\Users\<USERNAME>\AppData\Local\mkcert\rootCA.pem`
4. Čekiraj **Trust this CA to identify websites** → **OK**

```powershell
# Pronalaženje CA lokacije
.\mkcert.exe -CAROOT
```

#### Vite Konfiguracija

Frontend Vite serveri su već konfigurisani:

```javascript
// frontend/webshop/vite.config.js
import fs from 'fs'
import path from 'path'

export default defineConfig({
  server: {
    https: {
      cert: fs.readFileSync(path.resolve(__dirname, './certs/localhost+2.pem')),
      key: fs.readFileSync(path.resolve(__dirname, './certs/localhost+2-key.pem'))
    },
    port: 5173,
    host: true
  }
})
```

**Rezultat:**
- ✅ Frontend servisi dostupni preko HTTPS bez browser upozorenja
- ✅ https://localhost:5173 (WebShop)
- ✅ https://localhost:5174 (PSP)
- ✅ https://localhost:5172 (Bank)

### 2. Docker Konfiguracija

Svi servisi su konfigurisani u `docker-compose.yml` da koriste **samo HTTPS**:

```yaml
webshop-api:
  environment:
    - ASPNETCORE_URLS=https://+:443
    - Https__CertificatePath=/app/certs/webshop-api.pfx
    - Https__CertificatePassword=dev-cert-2024
  ports:
    - "5440:443"  # HTTPS only
  volumes:
    - ./certs:/app/certs:ro

bank-api:
  ports:
    - "5441:443"  # HTTPS only

psp-api:
  ports:
    - "5442:443"  # HTTPS only

paypal-api:
  ports:
    - "5443:443"  # HTTPS only
```

### 3. Backend Konfiguracija

Svi backend servisi koriste istu Kestrel konfiguraciju (primer za PSP):

```csharp
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(443, listenOptions =>
    {
        var certPath = builder.Configuration["Https:CertificatePath"] ?? "/app/certs/psp-api.pfx";
        var certPassword = builder.Configuration["Https:CertificatePassword"] ?? "dev-cert-2024";
        
        if (File.Exists(certPath))
        {
            listenOptions.UseHttps(certPath, certPassword);
        }
        else
        {
            Console.WriteLine($"WARNING: Certificate not found. Using development certificate.");
            listenOptions.UseHttps();
        }
    });
});
```

### 4. Inter-Service Komunikacija (HTTPS)

WebShop → PSP komunikacija preko HTTPS:

```csharp
builder.Services.AddHttpClient("PSPClient", client =>
{
    client.BaseAddress = new Uri("https://psp-api:443/api");
    client.Timeout = TimeSpan.FromSeconds(30);
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler();
    if (builder.Environment.IsDevelopment())
    {
        handler.ServerCertificateCustomValidationCallback = 
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
    }
    return handler;
});
```

## 🚀 Pokretanje Sistema

### Korak 1: Generisanje sertifikata

**Backend sertifikati (PFX):**
```powershell
cd PaymentSystem\certs
.\generate-certs.ps1
```

**Frontend trusted sertifikati (mkcert + PEM):**
```powershell
# U istom folderu (PaymentSystem\certs)
.\mkcert.exe -install
.\mkcert.exe localhost 127.0.0.1 ::1
```

**Rezultat:**
- Backend: `*.pfx` i `*.cer` fajlovi za sve servise
- Frontend: `localhost+2.pem` i `localhost+2-key.pem`

### Korak 2: Pokretanje Docker kontejnera

```powershell
cd PaymentSystem
docker compose down
docker compose up --build -d
```

### Korak 3: Provera HTTPS komunikacije

**WebShop API**:
```bash
curl -k https://localhost:5440/api/health
```

**Bank API**:
```bash
curl -k https://localhost:5441/api/health
```

**PSP API**:
```bash
curl -k https://localhost:5442/api/health
```

**PayPal API**:
```bash
curl -k https://localhost:5443/api/health
```

*Napomena: `-k` flag preskače validaciju sertifikata (samo za development)*

## 🔍 Testiranje

### Test HTTPS komunikacije WebShop → PSP
```bash
curl -k -X POST https://localhost:5442/api/payments \
  -H "Content-Type: application/json" \
  -d '{"amount": 100, "currency": "RSD"}'
```

### Validacija sertifikata
```powershell
# Prikaz detalja sertifikata
Get-PfxCertificate -FilePath .\certs\psp-api.pfx

# Provera isteka
openssl pkcs12 -in .\certs\psp-api.pfx -nodes -passin pass:dev-cert-2024 | openssl x509 -noout -dates
```

## 📊 Arhitektura Komunikacije

```
┌─────────────┐         HTTPS          ┌─────────────┐
│  Frontend   │ ───────────────────────>│  WebShop    │
│  (HTTPS)    │     via Vite Proxy      │   :5440     │
│  :5173      │                         │   (HTTPS)   │
└─────────────┘                         └──────┬──────┘
                                               │
                                               │ HTTPS
                                               │
                                        ┌──────▼──────┐
                                        │     PSP     │
                                        │   :5442     │
                                        │   (HTTPS)   │
                                        └──────┬──────┘
                                               │
                          ┌────────────────────┼────────────────────┐
                          │ HTTPS              │ HTTPS              │ HTTPS
                          │                    │                    │
                   ┌──────▼──────┐      ┌──────▼──────┐     ┌──────▼──────┐
                   │    Bank     │      │   PayPal    │     │  WebShop    │
                   │   :5441     │      │   :5443     │     │   :5440     │
                   │   (HTTPS)   │      │   (HTTPS)   │     │   (HTTPS)   │
                   └─────────────┘      └─────────────┘     └─────────────┘
```

**Napomena:** Sva komunikacija (frontend ↔ backend i backend ↔ backend) koristi HTTPS sa TLS 1.2+ enkripcijom.

## 📝 Portovi

| Servis | HTTPS Port | Pristup |
|--------|-----------|---------|
| WebShop API | 5440 | https://localhost:5440 |
| Bank API | 5441 | https://localhost:5441 |
| PSP API | 5442 | https://localhost:5442 |
| PayPal API | 5443 | https://localhost:5443 |
| **Frontend WebShop** | 5173 | https://localhost:5173 |
| **Frontend Bank** | 5172 | https://localhost:5172 |
| **Frontend PSP** | 5174 | https://localhost:5174 |

**Napomena**: 
- Svi backend servisi koriste **self-signed PFX** sertifikate (`*.pfx`)
- Svi frontend servisi koriste **mkcert trusted PEM** sertifikate (`localhost+2.pem`)
- Frontend development serveri koriste HTTPS sa mkcert sertifikatima (bez browser upozorenja)

## 🔒 Sigurnost

### Development Okruženje
- ✅ Self-signed PFX sertifikati za backend (inter-service komunikacija)
- ✅ mkcert trusted PEM sertifikati za frontend (browser pristup)
- ✅ Validacija isključena za backend-backend komunikaciju (`DangerousAcceptAnyServerCertificateValidator`)
- ✅ **Samo HTTPS komunikacija** za sve servise (backend i frontend)
- ✅ TLS 1.2+ enkripcija svuda

### Production Okruženje

**⚠️ VAŽNO**: Za production okruženje:

1. **Koristite validne sertifikate** od CA autoriteta (Let's Encrypt, DigiCert)
2. **Uklonite** `DangerousAcceptAnyServerCertificateValidator`
3. **Omogućite** punu validaciju sertifikata
4. **Koristite** tajne za certificate passwords (Azure Key Vault, AWS Secrets Manager)
5. **Frontend aplikacije**: Deploy sa reverse proxy-jem (Nginx, Caddy) koji automatski upravlja HTTPS
6. **Certificate renewal**: Automatizujte sa Let's Encrypt + certbot ili cloud provider-om

**Primer production konfiguracije:**

```csharp
.ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler();
    
    // NE prihvataj nevažeće sertifikate
    handler.ServerCertificateCustomValidationCallback = null;
    
    // Opciono: Dodaj custom validaciju
    handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
    {
        if (errors == SslPolicyErrors.None) return true;
        
        _logger.LogError("Invalid certificate: {Errors}", errors);
        return false;
    };
    
    return handler;
});
```

## 🐛 Troubleshooting

### Problem: "The SSL connection could not be established"

**Rešenje:**
```bash
# Proveri da li su sertifikati prisutni
ls PaymentSystem\certs\*.pfx

# Regeneriši sertifikate
cd PaymentSystem\certs
.\generate-certs.ps1

# Restartuj kontejnere
cd ..
docker compose restart
```

### Problem: "Unable to read data from the transport connection"

**Rešenje:**
```bash
# Proveri logove
docker logs psp-api
docker logs webshop-api
docker logs bank-api

# Restartuj sve servise
docker compose down
docker compose up -d
```

### Problem: "Certificate validation failed"

**Rešenje:**
```bash
# Proveri lozinku sertifikata u docker-compose.yml
# Mora biti: dev-cert-2024

# Verifikuj da sertifikati postoje
ls certs/
```

### Problem: "Frontend ne može da konektuje na backend"

**Rešenje:**
```bash
# Proveri da li frontend koristi HTTPS URL-ove
# U .env ili vite config:
VITE_API_URL=https://localhost:5440/api
```

### Problem: Browser prikazuje "Certificate not trusted" upozorenje

**Rešenje:**
```powershell
# Proveri da li је mkcert CA instaliran
cd PaymentSystem\certs
.\mkcert.exe -CAROOT

# Reinstaliraj CA ako treba
.\mkcert.exe -uninstall
.\mkcert.exe -install

# Generiši nove sertifikate
rm localhost+2*.pem
.\mkcert.exe localhost 127.0.0.1 ::1

# Restartuj frontend kontejnere
cd ..
docker restart frontend-webshop frontend-psp frontend-bank
```

**Za Firefox:**
```
Settings → Privacy & Security → Certificates → View Certificates → Authorities → Import
Izaberi: C:\Users\<USERNAME>\AppData\Local\mkcert\rootCA.pem
Čekiraj "Trust this CA to identify websites"
```

### Problem: "ENOENT: no such file or directory, open '/app/certs/localhost+2.pem'"

**Rešenje:**
```powershell
# Proveri da li sertifikati postoje u certs folderu
ls PaymentSystem\certs\localhost+2*.pem

# Ako ne postoje, generiši ih
cd PaymentSystem\certs
.\mkcert.exe localhost 127.0.0.1 ::1

# Proveri da li su mount-ovani u Docker
docker exec frontend-webshop ls -la /app/certs/
```

## 📚 Dodatni Resursi

- [ASP.NET Core HTTPS Configuration](https://learn.microsoft.com/en-us/aspnet/core/security/https)
- [Docker Secrets Management](https://docs.docker.com/engine/swarm/secrets/)
- [Let's Encrypt - Free SSL Certificates](https://letsencrypt.org/)
- [mkcert - Simple local HTTPS](https://github.com/FiloSottile/mkcert)

## ✅ Checklist

Pre deploy-a na production:

- [ ] Validni CA sertifikati instalirani za sve servise
- [ ] Certificate passwords u secret manageru
- [ ] `DangerousAcceptAnyServerCertificateValidator` uklonjen
- [ ] HTTPS redirekcija omogućena
- [ ] Certificate renewal automatizovan
- [ ] Monitoring SSL isteka postavljen
- [ ] Firewall pravila konfigurisana
- [ ] HSTS headers omogućeni
- [ ] Frontend aplikacije deploy-ovane sa validnim CA sertifikatima (ne mkcert)
- [ ] Reverse proxy (Nginx/Caddy) konfigurisan za frontend HTTPS
- [ ] Content Security Policy (CSP) headeri konfigurisani

---

**Verzija**: 3.0 (HTTPS Everywhere - Backend & Frontend)  
**Datum**: Februar 2026  
**Autor**: Payment System Team
