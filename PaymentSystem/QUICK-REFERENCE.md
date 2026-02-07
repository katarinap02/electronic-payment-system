# ⚡ Quick Reference - Distributed Setup

## 📍 Računar 1 - WebShop Stack

### 1️⃣ Priprema
```powershell
# Kopiraj:
# - certs/ folder
# - docker-compose.webshop.yml
# - .env.webshop.example

# Rename
cp .env.webshop.example .env.webshop
```

### 2️⃣ Konfiguracija
```powershell
# Otvori .env.webshop
# Ažuriraj IP adresu računara 2:
PSP_EXTERNAL_URL=https://192.168.1.100:5442
```

### 3️⃣ Start
```powershell
docker compose -f docker-compose.webshop.yml --env-file .env.webshop up --build -d
```

### 4️⃣ Provera
- https://localhost:5173 - WebShop Frontend
- https://localhost:5440/swagger - WebShop API

---

## 📍 Računar 2 - Services Stack

### 1️⃣ Priprema
```powershell
# Kopiraj:
# - certs/ folder
# - backend/ (PSP, Bank, PayPal)
# - frontend/ (psp, bank)
# - docker-compose.services.yml
# - .env.services.example

# Rename
cp .env.services.example .env.services
```

### 2️⃣ Konfiguracija
```powershell
# Otvori .env.services
# Ažuriraj IP adresu računara 1:
WEBSHOP_EXTERNAL_URL=https://192.168.1.101:5440
```

### 3️⃣ Start
```powershell
docker compose -f docker-compose.services.yml --env-file .env.services up --build -d
```

### 4️⃣ Provera
- https://localhost:5442 - PSP Load Balancer
- https://localhost:5174 - PSP Frontend
- https://localhost:5441/swagger - Bank API
- https://localhost:5172 - Bank Frontend
- https://localhost:5443/swagger - PayPal API
- http://localhost:5341 - Seq Logs

---

## 🧪 Network Test

```powershell
# Sa računara 1 → test računar 2 (PSP port)
Test-NetConnection -ComputerName 192.168.1.100 -Port 5442

# Sa računara 2 → test računar 1 (WebShop port)
Test-NetConnection -ComputerName 192.168.1.101 -Port 5440

# Ili HTTPS test
Invoke-WebRequest -Uri https://192.168.1.100:5442/api/health -SkipCertificateCheck
```

---

## 🔥 Firewall Rules

### Računar 1 - WebShop:
```powershell
New-NetFirewallRule -DisplayName "WebShop API" -Direction Inbound -LocalPort 5440 -Protocol TCP -Action Allow
```

### Računar 2 - Services:
```powershell
New-NetFirewallRule -DisplayName "PSP LB" -Direction Inbound -LocalPort 5442 -Protocol TCP -Action Allow
New-NetFirewallRule -DisplayName "Bank API" -Direction Inbound -LocalPort 5441 -Protocol TCP -Action Allow
New-NetFirewallRule -DisplayName "PayPal API" -Direction Inbound -LocalPort 5443 -Protocol TCP -Action Allow
```

---

## 📊 Port Overview

### Računar 1 (WebShop):
| Port | Service           | Protocol |
|------|-------------------|----------|
| 5173 | WebShop Frontend  | HTTPS    |
| 5440 | WebShop API       | HTTPS    |
| 5435 | PostgreSQL        | TCP      |

### Računar 2 (Services):
| Port | Service           | Protocol |
|------|-------------------|----------|
| 5442 | PSP Load Balancer | HTTPS    |
| 5448 | Nginx Status      | HTTP     |
| 5174 | PSP Frontend      | HTTPS    |
| 5441 | Bank API          | HTTPS    |
| 5172 | Bank Frontend     | HTTPS    |
| 5443 | PayPal API        | HTTPS    |
| 5341 | Seq Logs          | HTTP     |
| 5433 | PostgreSQL (PSP)  | TCP      |
| 5434 | PostgreSQL (Bank) | TCP      |
| 5436 | PostgreSQL (PayPal)| TCP     |

---

## 🛑 Stop Services

### Računar 1:
```powershell
docker compose -f docker-compose.webshop.yml down
```

### Računar 2:
```powershell
docker compose -f docker-compose.services.yml down
```

---

## 🔄 Rebuild

### Računar 1:
```powershell
docker compose -f docker-compose.webshop.yml --env-file .env.webshop up --build -d
```

### Računar 2:
```powershell
docker compose -f docker-compose.services.yml --env-file .env.services up --build -d
```

---

## 📋 Logovi

### Računar 1:
```powershell
docker compose -f docker-compose.webshop.yml logs -f
docker logs webshop-api
docker logs frontend-webshop
```

### Računar 2:
```powershell
docker compose -f docker-compose.services.yml logs -f
docker logs psp-api-1
docker logs psp-lb
docker logs bank-api
```

---

## 📝 Pronalaženje IP Adrese

```powershell
# Windows
ipconfig | Select-String "IPv4"

# Rezultat:
#   IPv4 Address. . . . . . . . . . . : 192.168.1.100
```

---

## ✅ End-to-End Test

1. **Računar 1**: Otvori https://localhost:5173
2. Login → Izaberi vozilo → Start rental
3. Klikni **Pay** → Redirectuje na računar 2 PSP
4. **Računar 2**: PSP stranica se učita
5. Izaberi **Credit Card** → Redirectuje na Bank
6. **Završi plaćanje** na Bank frontend-u
7. Callback na PSP → Callback na WebShop (računar 1)
8. **Računar 2**: Proveri Seq logove (http://localhost:5341)

---

## 🔍 Monitoring

### PSP Load Balancer Status:
```powershell
Test-NetConnection -ComputerName 192.168.1.100 -Port 5442

# 3. Check PSP is running (računar 2)
docker ps --filter "name=psp"
```

**Problem:** PSP cannot callback to WebShop
```powershell
# 1. Check firewall (računar 1)
Get-NetFirewallRule -DisplayName "*WebShop*"

# 2. Test connectivity (računar 2)
Test-NetConnection -ComputerName 192.168.1.101 -Port 5440

# 3. Verify .env.services has correct IP
Get-Content .env.services | Select-String "WEBSHOP_EXTERNAL_URL"
```

**Problem:** SSL Certificate errors
```powershell
# Regeneriši sertifikate
cd certs
.\generate-certs.ps1

# Rebuild
docker compose down
docker compose up --build -d
Get-NetFirewallRule -DisplayName "*PSP*"

# 2. Test connectivity (računar 1)
.\test-network.ps1 -TargetIP 192.168.1.100 -TargetType PSP

# 3. Check PSP is running (računar 2)
docker ps --filter "name=psp"
```

**Problem:** PSP cannot callback to WebShop
```powershell
# 1. Check firewall (računar 1)
Get-NetFirewallRule -DisplayName "*WebShop*"

# 2. Test connectivity (računar 2)
.\test-network.ps1 -TargetIP 192.168.1.101 -TargetType WebShop

# 3. Verify .env.services has correct IP
Get-Content .env.services | Select-String "WEBSHOP_EXTERNAL_URL"
```
