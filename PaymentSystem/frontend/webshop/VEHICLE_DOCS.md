# Vehicle Management System - Frontend

Frontend aplikacija za rent-a-car sistem sa pretragom vozila i admin panelom.

## Struktura projekta

### Komponente

#### Javne komponente (bez autentifikacije):
- **VehicleList.vue** - Prikaz svih vozila sa filterima i pretragom
- **VehicleCard.vue** - Kartica za prikaz pojedinačnog vozila
- **VehicleDetail.vue** - Detaljni prikaz vozila

#### Admin komponente (zahtevaju autentifikaciju):
- **AdminVehicles.vue** - Upravljanje vozilima (CRUD operacije)

### Servisi

- **vehicleService.js** - API pozivi za vozila
  - getAllVehicles() - Preuzimanje svih vozila
  - getVehicleById(id) - Preuzimanje vozila po ID-u
  - searchVehicles(params) - Pretraga sa filterima
  - createVehicle(data) - Kreiranje vozila (Admin)
  - updateVehicle(id, data) - Ažuriranje vozila (Admin)
  - deleteVehicle(id) - Brisanje vozila (Admin)

### Rute

- `/vehicles` - Lista svih vozila
- `/vehicles/:id` - Detalji vozila
- `/admin/vehicles` - Admin panel za upravljanje vozilima
- `/login` - Prijava
- `/register` - Registracija
- `/dashboard` - Korisnički dashboard

## Karakteristike

### Pretraga i filtriranje
- Kategorija (Economy, Comfort, Luxury, SUV, Van, Sport)
- Tip menjača (Manual, Automatic)
- Tip goriva (Petrol, Diesel, Electric, Hybrid)
- Status vozila (Available, Rented, Maintenance, Unavailable)
- Opseg cena (min/max)
- Broj sedišta (min/max)
- Brend vozila (tekstualna pretraga)
- Godina proizvodnje

### Admin funkcionalnosti
- Kreiranje novih vozila
- Ažuriranje postojećih vozila
- Brisanje vozila (soft delete)
- Tabela sa svim vozilima
- Modal forma za unos/izmenu

### Dizajn
- Moderan, čist UI
- Responzivan dizajn
- Smooth animacije i tranzicije
- Intuitivna navigacija
- Status badges sa bojama
- Loading i error states

## Enums

### VehicleCategory
```
1 = Economy
2 = Comfort
3 = Luxury
4 = SUV
5 = Van
6 = Sport
```

### TransmissionType
```
1 = Manual
2 = Automatic
```

### FuelType
```
1 = Petrol
2 = Diesel
3 = Electric
4 = Hybrid
```

### VehicleStatus
```
1 = Available
2 = Rented
3 = Maintenance
4 = Unavailable
```

## Instalacija i pokretanje

```bash
# Instalacija zavisnosti
npm install

# Pokretanje development servera
npm run dev

# Build za produkciju
npm run build
```

## API Endpoint

Backend API treba da bude pokrenut na: `http://localhost:5000/api`

Ovo se može promeniti u `src/services/api.js` fajlu.

## TODO
- Implementirati autentifikaciju za admin panel
- Dodati paginaciju za listu vozila
- Dodati sortiranje kolona u admin tabeli
- Implementirati rent funkcionalnost
- Dodati upload slika za vozila
- Implementirati validaciju forme na frontendu
