# Payment System - Multi-Environment Setup

Elektronski platni sistem sa podrškom za WebShop, PSP (Payment Service Provider), Bank, i PayPal integraciju.

---

## 🚀 Quick Start

### Single Machine (Sve na jednom računaru)

```powershell
# 1. Generiši SSL sertifikate
cd certs
.\generate-certs.ps1

# 2. Kreiraj .env fajl
cp .env.example .env
# Ažuriraj vrednosti u .env

# 3. Start sistem
docker compose up --build -d

# 4. Provera
docker ps
```

**Servisi:**
- WebShop: https://localhost:5173
- PSP: https://localhost:5442
- Bank: https://localhost:5172
- Seq Logs: http://localhost:5341

---

### Distributed Setup (Dva računara)

WebShop na jednom računaru, PSP/Bank/PayPal na drugom.

**📖 Detaljne instrukcije:** [QUICK-REFERENCE.md](QUICK-REFERENCE.md)

**Kratko:**

**Računar 1 - WebShop:**
```powershell
# Ažuriraj .env.webshop sa IP adresom računara 2
docker compose -f docker-compose.webshop.yml --env-file .env.webshop up -d
```

**Računar 2 - Services:**
```powershell
# Ažuriraj .env.services sa IP adresom računara 1
docker compose -f docker-compose.services.yml --env-file .env.services up -d
```

---

## 📂 Struktura

```
PaymentSystem/
├── backend/              # .NET APIs (PSP, Bank, WebShop, PayPal)
├── frontend/             # Vue.js apps (webshop, psp, bank)
├── certs/                # SSL sertifikati
│
├── docker-compose.yml              # Single machine
├── docker-compose.webshop.yml      # WebShop only
├── docker-compose.services.yml     # PSP/Bank/PayPal only
│
└── QUICK-REFERENCE.md              # Setup guide
```

---

## 🔐 SSL Sertifikati

```powershell
cd certs
.\generate-certs.ps1           # Backend (self-signed)
.\mkcert.exe -install          # Frontend (trusted)
.\mkcert.exe localhost 127.0.0.1 ::1
```

---

## 🛠️ Osnovne Komande

### Single Machine
```powershell
# Start
docker compose up -d

# Stop
docker compose down

# Rebuild
docker compose up --build -d

# Logovi
docker compose logs -f [service-name]
```

### Distributed
```powershell
# Računar 1 - WebShop
docker compose -f docker-compose.webshop.yml --env-file .env.webshop up -d
docker compose -f docker-compose.webshop.yml down

# Računar 2 - Services  
docker compose -f docker-compose.services.yml --env-file .env.services up -d
docker compose -f docker-compose.services.yml down
```

---

## 📖 Dokumentacija

- [QUICK-REFERENCE.md](QUICK-REFERENCE.md) - **Distributed setup guide**
- [HTTPS_SETUP.md](HTTPS_SETUP.md) - HTTPS arhitektura
- [certs/README.md](certs/README.md) - Sertifikati (backend + mkcert)

---

## 🏗️ Arhitektura

**Single Machine:**
```
docker-compose.yml
├── WebShop (frontend + API + DB)
├── PSP (3 instances + LB + DB)
├── Bank (frontend + API + DB)
├── PayPal (API + DB)
└── Seq (logging)
```

**Distributed:**
```
Računar 1: docker-compose.webshop.yml
├── frontend-webshop (5173)
├── webshop-api (5440)
└── postgres-webshop (5435)

Računar 2: docker-compose.services.yml
├── psp-lb + 3x psp-api (5442)
├── bank-api + frontend (5441, 5172)
├── paypal-api (5443)
└── seq (5341)
```

---

## 🔍 PSP Load Balancer

PSP koristi **Nginx load balancer** sa **3 instance**:
- `psp-api-1` - Pokreće migracije
- `psp-api-2` - Read-only
- `psp-api-3` - Read-only

**Monitoring:**
```powershell
# Nginx status
Invoke-WebRequest http://localhost:5448/nginx_status

# Logovi
docker logs psp-lb
docker logs psp-api-1
```

---

## 📊 Portovi

| Service           | Port | Protocol |
|-------------------|------|----------|
| WebShop Frontend  | 5173 | HTTPS    |
| WebShop API       | 5440 | HTTPS    |
| PSP Load Balancer | 5442 | HTTPS    |
| PSP Frontend      | 5174 | HTTPS    |
| Bank API          | 5441 | HTTPS    |
| Bank Frontend     | 5172 | HTTPS    |
| PayPal API        | 5443 | HTTPS    |
| Seq Logs          | 5341 | HTTP     |

---

## 🐛 Troubleshooting

**PSP ne radi:**
```powershell
docker ps --filter "name=psp"
docker logs psp-api-1
docker exec psp-lb nginx -t
```

**Database errors:**
```powershell
docker ps --filter "name=db"
docker exec webshop-db pg_isready -U postgres
```

**SSL errors:**
```powershell
cd certs
.\generate-certs.ps1
docker compose down
docker compose up --build -d
```

---

## 🎯 End-to-End Test

1. Otvori https://localhost:5173 (WebShop)
2. Login
3. Izaberi vozilo → Start rental
4. Klikni Pay → Redirectuje na PSP
5. Izaberi Credit Card → Redirectuje na Bank
6. Završi plaćanje
7. Proveri logove u Seq (http://localhost:5341)

---

## 📝 License

MIT License

