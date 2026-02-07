# Crypto Payment Service

## ?? Ethereum/Blockchain Payment Gateway

Mikroservis za procesiranje crypto pla?anja (ETH) preko Sepolia testnet-a.

---

## ??? Arhitektura

### **Stack:**
- **.NET 8** - Web API
- **PostgreSQL** - Baza podataka
- **Nethereum** - Ethereum blockchain integracija
- **Infura** - Ethereum node provider (Sepolia testnet)
- **CoinGecko API** - Real-time exchange rates

### **Port:** 5004

---

## ?? Setup

Detaljne instrukcije: [QUICKSTART.md](QUICKSTART.md)

**Kratko:**
1. Kreiraj Infura API key (https://infura.io)
2. Setup MetaMask wallet (Sepolia testnet)
3. Dobavi testnet ETH (https://sepoliafaucet.com)
4. Ažuriraj `appsettings.json`:
   - `InfuraApiUrl`
   - `MerchantWalletAddress`
5. `dotnet run`

---

## ?? API Endpoints

| Method | Endpoint | Opis |
|--------|----------|------|
| GET | `/api/crypto/test-connection` | Test Infura + wallet setup |
| POST | `/api/crypto/create-payment` | Kreira novo pla?anje (PSP poziva) |
| GET | `/api/crypto/transaction/{id}` | Dohvata status transakcije |
| POST | `/api/crypto/cancel/{id}` | Otkazuje pending pla?anje |

---

## ??? Baza

**Database:** `CryptoPaymentDb`

**Tabele:**
- `crypto_transactions` - Sve crypto transakcije
- `audit_logs` - PCI DSS 5.1 compliance

**Enkripcija:**
- Wallet adrese - **AES-256**
- Transaction hash-evi - **AES-256**

---

## ?? Bezbednost (PCI DSS)

? **Zahtev 2:** Protect Account Data
- Wallet adrese enkriptovane u bazi
- Privatni klju?evi NIKADA nisu na serveru

? **Zahtev 4:** Strong Access Control
- HMAC autentifikacija (može se dodati izme?u PSP-a i Crypto.API)

? **Zahtev 5.1:** Track and Monitor
- Kompletan audit log svih akcija
- IP adresa, timestamp, result

---

## ?? Tok Pla?anja

1. **PSP** ? `POST /api/crypto/create-payment`
2. **Response:** Wallet adresa + iznos u ETH + QR kod data
3. **Korisnik** ? Skenira QR ili copy/paste adresu ? MetaMask ? Send
4. **BlockchainMonitorService** ? Polluje blockchain svakih 10s
5. **Pronalazi** incoming transakciju ? Status: `CONFIRMING`
6. **?eka** 1+ potvrdu ? Status: `CAPTURED`
7. **Frontend** (polling) ? Redirect na PSP
8. **PSP** ? Redirect na WebShop

---

## ?? Testing

```powershell
# Test Infura connection
Invoke-WebRequest http://localhost:5004/api/crypto/test-connection

# Create test payment
curl -X POST http://localhost:5004/api/crypto/create-payment `
  -H "Content-Type: application/json" `
  -d '{...}'
```

---

## ?? Troubleshooting

Vidi: [VISUAL_SETUP_GUIDE.md](VISUAL_SETUP_GUIDE.md)

---

## ?? Notes

- **Testnet:** Sepolia (ne Mainnet!)
- **Exchange rate:** Live snapshot (CoinGecko)
- **Monitoring interval:** 10 sekundi
- **Payment expiry:** 15 minuta
- **Min confirmations:** 1
