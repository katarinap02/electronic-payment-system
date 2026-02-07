# SSL Sertifikati za sve servise

## 🔐 Generisanje Backend Sertifikata

Za generisanje self-signed sertifikata za backend servise:

```powershell
.\generate-certs.ps1
```

Ova skripta će kreirati sertifikate za sve backend servise:

**Backend API Servisi:**
- `psp-api.pfx` / `psp-api.cer` - PSP server
- `webshop-api.pfx` / `webshop-api.cer` - WebShop server  
- `bank-api.pfx` / `bank-api.cer` - Bank server
- `paypal-api.pfx` / `paypal-api.cer` - PayPal server

**Frontend Servisi:**
- `frontend-webshop.pfx` / `frontend-webshop.cer` - WebShop frontend
- `frontend-psp.pfx` / `frontend-psp.cer` - PSP frontend
- `frontend-bank.pfx` / `frontend-bank.cer` - Bank frontend

**Lozinka za sve PFX fajlove:** `dev-cert-2024`

---

## 🔄 PSP Load Balancer - Automatska Konverzija Sertifikata

**PSP Nginx Load Balancer** automatski konvertuje `psp-api.pfx` u PEM format pri pokretanju.

### Kako radi

Kada pokrenete `docker compose up --build -d`:

1. Nginx kontejner se pokreće sa `nginx-entrypoint.sh`
2. Entrypoint proverava da li postoje `psp-api-nginx.crt` i `psp-api-nginx.key`
3. Ako ne postoje, automatski konvertuje `psp-api.pfx` → PEM format
4. Generiše fajlove u `./certs/` folderu (perzistentni):
   - `psp-api-nginx.crt` - Javni sertifikat
   - `psp-api-nginx.key` - Privatni ključ (bez lozinke)

### Rezultat

```
certs/
├── psp-api.pfx              # Originalni PFX (generiše generate-certs.ps1)
├── psp-api-nginx.crt        # ✅ Auto-generisano od nginx-entrypoint.sh
└── psp-api-nginx.key        # ✅ Auto-generisano od nginx-entrypoint.sh
```

**Napomena:** PEM fajlovi se generišu samo prvi put. Posle toga se ponovo koriste.

---

## 🌐 Generisanje Trusted Sertifikata za Frontend (mkcert)

**Problem:** Browser-i ne verifukuju self-signed sertifikatima i prikazuju security upozorenja.

**Rešenje:** Koristite `mkcert` za generisanje lokalno trusted sertifikata.

### Instalacija mkcert

**Opcija 1:** Automatsko preuzimanje (već postoji `mkcert.exe` u ovom folderu)

```powershell
# Već ste preuzeli mkcert.exe ako ste pokrenuli:
# Invoke-WebRequest -Uri "https://github.com/FiloSottile/mkcert/releases/download/v1.4.4/mkcert-v1.4.4-windows-amd64.exe" -OutFile "mkcert.exe"
```

**Opcija 2:** Package Manager

```powershell
# Chocolatey
choco install mkcert

# Scoop
scoop bucket add extras
scoop install mkcert
```

### Koraci za Konfiguraciju

#### 1. Instalacija Local CA

```powershell
.\mkcert.exe -install
```

Ova komanda:
- Kreira lokalni Certificate Authority (CA)
- Instalira CA u sistem trust store (Windows Certificate Store)
- **Napomena:** Za Firefox, potreban je dodatni korak (videti dole)

#### 2. Generisanje Localhost Sertifikata

```powershell
.\mkcert.exe localhost 127.0.0.1 ::1
```

Ova komanda generiše:
- `localhost+2.pem` - Javni ključ (certificate)
- `localhost+2-key.pem` - Privatni ključ

#### 3. Importovanje CA u Firefox (OPCIONALNO)

Firefox koristi svoj sopstveni certificate store:

1. Otvori Firefox
2. **Settings** → **Privacy & Security**
3. Scroll do **Certificates** → **View Certificates**
4. Klikni tab **Authorities**
5. Klikni **Import...**
6. Izaberi fajl: `C:\Users\<USERNAME>\AppData\Local\mkcert\rootCA.pem`
7. Čekiraj **Trust this CA to identify websites**
8. Klikni **OK**

**Pronalaženje CA lokacije:**
```powershell
.\mkcert.exe -CAROOT
# Output: C:\Users\<USERNAME>\AppData\Local\mkcert
```

---

## 📦 Struktura Sertifikata

Nakon pokretanja obe skripte, imaćete:

```
certs/
├── mkcert.exe                     # mkcert alat
├── generate-certs.ps1             # PowerShell skripta za backend certove
│
├── Backend API Sertifikati (Self-Signed):
│   ├── psp-api.pfx / .cer
│   ├── webshop-api.pfx / .cer
│   ├── bank-api.pfx / .cer
│   ├── paypal-api.pfx / .cer
│   ├── frontend-webshop.pfx / .cer (legacy - ne koristi se sa mkcert)
│   ├── frontend-psp.pfx / .cer (legacy - ne koristi se sa mkcert)
│   └── frontend-bank.pfx / .cer (legacy - ne koristi se sa mkcert)
│
└── Frontend Trusted Sertifikati (mkcert):
    ├── localhost+2.pem            # Public certificate
    └── localhost+2-key.pem        # Private key
```

---

## ✅ Provera Instalacije

### Backend Sertifikati

```powershell
# Lista PFX fajlova
ls *.pfx

# Provera validnosti
certutil -dump psp-api.pfx
```

### Frontend Trusted Sertifikati

```powershell
# Provera da li je CA instaliran
.\mkcert.exe -CAROOT

# Provera generisanih sertifikata
ls localhost+2*.pem
```

---

## 🔧 Usage u Vite (Frontend)

Nakon generisanja `localhost+2.pem` sertifikata, Vite je konfigurisan:

```javascript
// vite.config.js
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

---

## 🛡️ Security Napomene

### Development
- **Self-signed sertifikati** (PFX): Dobri za backend inter-service komunikaciju
- **mkcert trusted sertifikati** (PEM): Dobri za frontend development bez browser upozorenja

### Production
- **NIKADA** ne koristite self-signed sertifikate u production
- Koristite validne sertifikate od CA autoriteta (Let's Encrypt, Sectigo, DigiCert)
- Implementirajte certificate pinning za dodatnu sigurnost

---

## 🔄 Refresh Sertifikata

### Backend Sertifikati
```powershell
# Obriši stare
rm *.pfx, *.cer

# Generiši nove
.\generate-certs.ps1
```

### Frontend mkcert Sertifikati
```powershell
# Obriši stare
rm localhost+2*.pem

# Generiši nove
.\mkcert.exe localhost 127.0.0.1 ::1
```

---

## 📅 Validnost Sertifikata

- **Self-signed (PowerShell):** 2 godine
- **mkcert:** 2+ godine (do maja 2028)
