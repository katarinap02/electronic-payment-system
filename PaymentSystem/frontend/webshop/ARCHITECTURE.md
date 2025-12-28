# Component Architecture

## Component Hierarchy

```
VehicleDetail.vue
    │
    ├── [Button: "Add to Cart"]
    │
    └── RentalModal.vue (Teleported to body)
            │
            ├── Modal Header
            │   ├── Title (Dynamic based on step)
            │   └── Progress Indicator (4 dots)
            │
            └── Modal Body (Dynamic Component)
                │
                ├── Step 1: DateSelection.vue
                │   ├── Pickup Date Input
                │   ├── Return Date Input
                │   ├── Duration Display
                │   └── [Continue Button]
                │
                ├── Step 2: InsuranceSelection.vue
                │   ├── "No Insurance" Option
                │   ├── Insurance Cards (Radio Group)
                │   │   ├── Name & Price
                │   │   ├── Description
                │   │   ├── Coverage Limit
                │   │   └── Deductible
                │   ├── [Back Button]
                │   └── [Continue Button]
                │
                ├── Step 3: AdditionalServicesSelection.vue
                │   ├── Service Cards (Checkbox Group)
                │   │   ├── Icon
                │   │   ├── Name & Price
                │   │   └── Description
                │   ├── Selected Services Summary
                │   ├── [Back Button]
                │   └── [Continue Button]
                │
                └── Step 4: RentalSummary.vue
                    ├── Vehicle Info Card
                    ├── Rental Period Card
                    ├── Insurance Details Card
                    ├── Services List Card
                    ├── Total Price Card
                    ├── [Back Button]
                    └── [Add to Cart Button]
```

## Data Flow

```
User Action              Component                 Event Emitted              Parent Handles
────────────────────────────────────────────────────────────────────────────────────────────

[Select Dates]    →    DateSelection      →    @continue(dateData)    →    RentalModal
                                                                            ├─ Save to rentalData
                                                                            └─ Move to Step 2

[Select Insurance] →   InsuranceSelection →    @continue(insuranceData) → RentalModal
                                                                            ├─ Save to rentalData
                                                                            └─ Move to Step 3

[Select Services]  →   AdditionalServices →    @continue(servicesData)  → RentalModal
                                                                            ├─ Save to rentalData
                                                                            └─ Move to Step 4

[Confirm]         →    RentalSummary      →    @confirm(finalData)      → RentalModal
                                                                            └─ Emit to VehicleDetail

[Add to Cart]     →    VehicleDetail      →    Show notification        → User sees success
```

## State Management

### RentalModal State
```javascript
rentalData = {
  startDate: string,      // "2025-12-28"
  endDate: string,        // "2026-01-05"
  days: number,           // 8
  insurance: object | null, // InsurancePackageDto or null
  services: array         // [AdditionalServiceDto, ...]
}
```

### Step Navigation
```javascript
currentStep = 0 | 1 | 2 | 3
   Step 0: DateSelection
   Step 1: InsuranceSelection
   Step 2: AdditionalServicesSelection
   Step 3: RentalSummary
```

## API Service Layer

```
Components                API Services              Backend Endpoints
──────────────────────────────────────────────────────────────────────

InsuranceSelection  →  insuranceService.js  →  GET /api/insurance
                          ├─ getAllInsurance()
                          ├─ getActiveInsurance()
                          └─ getInsuranceById(id)

AdditionalServices  →  additionalService.js →  GET /api/services
                          ├─ getAllServices()
                          ├─ getAvailableServices()
                          └─ getServiceById(id)

[Both services use]  →  api.js (Axios instance)
                          ├─ Base URL configuration
                          ├─ Request interceptor (auth token)
                          └─ Response interceptor (logging)
```

## Transitions & Animations

### Modal Transitions
```css
.modal-enter-active     /* Modal appears */
.modal-leave-active     /* Modal disappears */
  ├─ Scale: 0.9 → 1.0
  └─ Opacity: 0 → 1

.slide-next-*          /* Moving forward */
  └─ TranslateX: +30px → 0

.slide-prev-*          /* Moving backward */
  └─ TranslateX: -30px → 0

.notification-*        /* Success notification */
  └─ TranslateX: +100px → 0
```

## CSS Class Structure

```
.modal-overlay                  /* Full-screen overlay */
  └── .modal-container          /* White card container */
      ├── .modal-header         /* Title + Progress + Close */
      │   ├── .header-content
      │   │   ├── h2
      │   │   └── .progress-indicator
      │   │       └── .progress-dot × 4
      │   └── .close-button
      │
      └── .modal-body           /* Scrollable content */
          └── [Dynamic Component]
              ├── .section-title
              ├── .section-subtitle
              ├── .info-card
              └── .actions
                  ├── .btn-back
                  └── .btn-continue
```

## Responsive Breakpoints

```
Desktop (> 968px)
├─ Modal: 900px max-width, centered
├─ Grid layouts: 2 columns
└─ Side-by-side buttons

Tablet (768px - 968px)
├─ Modal: Full width with padding
├─ Grid layouts: 2 columns
└─ Side-by-side buttons

Mobile (< 768px)
├─ Modal: Bottom sheet style
├─ Grid layouts: 1 column
└─ Stacked buttons
```

## Error Handling

```
Component Level
├─ Loading states (spinner)
├─ Error states (retry button)
└─ Empty states (no data message)

Validation
├─ Date validation (min/max, range)
├─ Required field checking
└─ Business rule validation

API Level
├─ Request interceptor (add auth)
├─ Response interceptor (log errors)
└─ Promise rejection handling
```

## Performance Optimizations

- **Lazy Loading**: Modal components loaded on demand
- **Teleport**: Modal rendered at body level (avoid z-index issues)
- **Computed Properties**: Price calculations cached
- **Transitions**: Hardware-accelerated CSS transforms
- **Event Delegation**: Minimal event listeners

## Accessibility Features

- **Keyboard Navigation**: Tab, Enter, Escape
- **ARIA Labels**: Close buttons, form inputs
- **Focus Management**: Auto-focus on modal open
- **Color Contrast**: WCAG AA compliant
- **Screen Reader**: Semantic HTML structure

## Testing Strategy

```
Unit Tests
├─ Date calculation logic
├─ Price calculation logic
└─ Validation functions

Component Tests
├─ Button click handlers
├─ Form input changes
└─ Emit events

Integration Tests
├─ Step navigation flow
├─ Data persistence across steps
└─ API service mocking

E2E Tests
├─ Complete rental flow
├─ Error scenarios
└─ Mobile responsiveness
```

---

This architecture provides:
✅ Clear separation of concerns
✅ Reusable components
✅ Predictable data flow
✅ Easy testing and maintenance
✅ Scalable structure
