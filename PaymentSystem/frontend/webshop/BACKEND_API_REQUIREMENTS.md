# Backend API Requirements

## ⚠️ Missing Endpoints

Frontend trenutno koristi **mock podatke** jer ovi endpointi nisu implementirani na backendu.

## Required Endpoints

### 1. Insurance Packages API

#### GET /api/insurance
Vraća sve pakete osiguranja.

**Response Format:**
```json
{
  "data": [
    {
      "id": 1,
      "name": "Basic",
      "description": "Basic coverage for collision and theft",
      "pricePerDay": 5.00,
      "coverageLimit": 10000,
      "deductible": 1000,
      "isActive": true
    },
    {
      "id": 2,
      "name": "Standard",
      "description": "Comprehensive coverage including collision, theft, and third-party liability",
      "pricePerDay": 10.00,
      "coverageLimit": 25000,
      "deductible": 500,
      "isActive": true
    }
  ]
}
```

#### GET /api/insurance/active
Vraća samo aktivne pakete osiguranja (isActive = true).

**Response Format:** Isti kao `/api/insurance` ali filtrirano.

#### GET /api/insurance/{id}
Vraća jedan paket osiguranja po ID-u.

**Response Format:**
```json
{
  "data": {
    "id": 1,
    "name": "Basic",
    "description": "Basic coverage for collision and theft",
    "pricePerDay": 5.00,
    "coverageLimit": 10000,
    "deductible": 1000,
    "isActive": true
  }
}
```

---

### 2. Additional Services API

#### GET /api/services
Vraća sve dodatne usluge.

**Response Format:**
```json
{
  "data": [
    {
      "id": 1,
      "name": "GPS Navigation",
      "description": "Advanced GPS navigation system with real-time traffic updates",
      "pricePerDay": 5.00,
      "isAvailable": true,
      "iconUrl": null
    },
    {
      "id": 2,
      "name": "Child Safety Seat",
      "description": "Safety-approved child seat suitable for children aged 0-4 years",
      "pricePerDay": 3.00,
      "isAvailable": true,
      "iconUrl": null
    }
  ]
}
```

#### GET /api/services/available
Vraća samo dostupne usluge (isAvailable = true).

**Response Format:** Isti kao `/api/services` ali filtrirano.

#### GET /api/services/{id}
Vraća jednu uslugu po ID-u.

**Response Format:**
```json
{
  "data": {
    "id": 1,
    "name": "GPS Navigation",
    "description": "Advanced GPS navigation system",
    "pricePerDay": 5.00,
    "isAvailable": true,
    "iconUrl": null
  }
}
```

---

## Database Schema Suggestions

### Insurance Package Table
```sql
CREATE TABLE insurance_packages (
    id INT PRIMARY KEY AUTO_INCREMENT,
    name VARCHAR(100) NOT NULL,
    description TEXT,
    price_per_day DECIMAL(10, 2) NOT NULL,
    coverage_limit DECIMAL(10, 2) NOT NULL,
    deductible DECIMAL(10, 2) NOT NULL,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);
```

### Additional Services Table
```sql
CREATE TABLE additional_services (
    id INT PRIMARY KEY AUTO_INCREMENT,
    name VARCHAR(100) NOT NULL,
    description TEXT,
    price_per_day DECIMAL(10, 2) NOT NULL,
    is_available BOOLEAN DEFAULT true,
    icon_url VARCHAR(255),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);
```

---

## Sample Data (SQL Inserts)

### Insurance Packages
```sql
INSERT INTO insurance_packages (name, description, price_per_day, coverage_limit, deductible, is_active) VALUES
('Basic', 'Basic coverage for collision and theft. Suitable for experienced drivers.', 5.00, 10000, 1000, true),
('Standard', 'Comprehensive coverage including collision, theft, and third-party liability. Recommended for most rentals.', 10.00, 25000, 500, true),
('Premium', 'Full coverage with minimal deductible. Includes windscreen, tire, and undercarriage protection.', 15.00, 50000, 200, true),
('Full Coverage', 'Maximum protection with zero deductible. All damages covered, no questions asked.', 20.00, 100000, 0, true);
```

### Additional Services
```sql
INSERT INTO additional_services (name, description, price_per_day, is_available, icon_url) VALUES
('GPS Navigation', 'Advanced GPS navigation system with real-time traffic updates and points of interest.', 5.00, true, null),
('Child Safety Seat', 'Safety-approved child seat suitable for children aged 0-4 years (up to 18kg).', 3.00, true, null),
('Booster Seat', 'Booster seat for older children aged 4-12 years (15-36kg).', 2.50, true, null),
('WiFi Hotspot', 'Mobile WiFi hotspot with unlimited data. Connect up to 5 devices.', 4.00, true, null),
('Winter Tires', 'Premium winter tires for safe driving in snow and ice conditions.', 8.00, true, null),
('Ski Rack', 'Roof-mounted ski rack. Carries up to 6 pairs of skis or 4 snowboards.', 6.00, true, null),
('Bike Rack', 'Rear-mounted bike rack. Suitable for 2-3 bikes depending on vehicle model.', 7.00, true, null),
('Additional Driver', 'Add a second driver to your rental. Both drivers must be present at pickup.', 5.00, true, null),
('Full Tank Fuel', 'Start with a full tank of fuel. Return empty for convenience.', 10.00, true, null),
('Airport Delivery', 'We deliver the vehicle directly to the airport terminal.', 15.00, false, null);
```

---

## C# Controller Examples (ASP.NET Core)

### InsuranceController.cs
```csharp
[ApiController]
[Route("api/[controller]")]
public class InsuranceController : ControllerBase
{
    private readonly IInsuranceService _insuranceService;

    public InsuranceController(IInsuranceService insuranceService)
    {
        _insuranceService = insuranceService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var packages = await _insuranceService.GetAllPackagesAsync();
        return Ok(new { data = packages });
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        var packages = await _insuranceService.GetActivePackagesAsync();
        return Ok(new { data = packages });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var package = await _insuranceService.GetPackageByIdAsync(id);
        if (package == null)
            return NotFound();
        return Ok(new { data = package });
    }
}
```

### ServicesController.cs
```csharp
[ApiController]
[Route("api/[controller]")]
public class ServicesController : ControllerBase
{
    private readonly IAdditionalServicesService _servicesService;

    public ServicesController(IAdditionalServicesService servicesService)
    {
        _servicesService = servicesService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var services = await _servicesService.GetAllServicesAsync();
        return Ok(new { data = services });
    }

    [HttpGet("available")]
    public async Task<IActionResult> GetAvailable()
    {
        var services = await _servicesService.GetAvailableServicesAsync();
        return Ok(new { data = services });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var service = await _servicesService.GetServiceByIdAsync(id);
        if (service == null)
            return NotFound();
        return Ok(new { data = service });
    }
}
```

---

## Testing Endpoints

### Using cURL

```bash
# Test Insurance
curl http://localhost:5000/api/insurance
curl http://localhost:5000/api/insurance/active
curl http://localhost:5000/api/insurance/1

# Test Services
curl http://localhost:5000/api/services
curl http://localhost:5000/api/services/available
curl http://localhost:5000/api/services/1
```

### Using Postman

1. Create a new collection: "Rental System"
2. Add requests:
   - GET `http://localhost:5000/api/insurance`
   - GET `http://localhost:5000/api/insurance/active`
   - GET `http://localhost:5000/api/services`
   - GET `http://localhost:5000/api/services/available`

---

## Current Status

✅ **Frontend:** Implementirano sa mock podacima (radi bez backendu)  
❌ **Backend:** Endpointi nisu implementirani (404 error)

### Temporary Solution
Frontend koristi mock podatke ako backend ne odgovori. To znači da možeš testirati cijeli tok već sada!

### Permanent Solution
Implementiraj gornje endpointe na backendu prema specifikaciji.

---

## Notes

- Svi endpointi su **public** (ne zahtevaju autentifikaciju)
- Response format koristi `{ data: [...] }` wrapper
- Decimal polja su `pricePerDay`, `coverageLimit`, `deductible`
- Boolean polja su `isActive`, `isAvailable`
- CORS mora biti omogućen za `http://localhost:5173` (Vite dev server)
