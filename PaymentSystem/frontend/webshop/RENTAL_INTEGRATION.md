# Vehicle Rental System - Frontend Integration

## Overview
This implementation adds a multi-step rental process with insurance packages and additional services selection to the vehicle rental system.

## Features

### 1. Multi-Step Rental Modal
A modal with 4 progressive steps:
- **Step 1: Date Selection** - Choose rental period
- **Step 2: Insurance Selection** - Select insurance package (optional)
- **Step 3: Additional Services** - Select extra services (optional)
- **Step 4: Summary & Confirm** - Review and confirm rental details

### 2. Components Created

#### Core Components
- **RentalModal.vue** - Main modal container orchestrating the rental flow
- **DateSelection.vue** - Calendar-based date picker for rental period
- **InsuranceSelection.vue** - Radio button group for insurance package selection
- **AdditionalServicesSelection.vue** - Checkbox group for additional services
- **RentalSummary.vue** - Final summary with price breakdown

#### API Services
- **insuranceService.js** - API calls for insurance packages
- **additionalService.js** - API calls for additional services

### 3. Updated Components
- **VehicleDetail.vue** - Integrated with RentalModal, button changed to "Add to Cart"

## API Endpoints Used

### Insurance Packages
```
GET /api/insurance              - Get all insurance packages
GET /api/insurance/active       - Get active insurance packages only
GET /api/insurance/{id}         - Get single insurance package
```

### Additional Services
```
GET /api/services               - Get all additional services
GET /api/services/available     - Get available services only
GET /api/services/{id}          - Get single service
```

## Data Models

### InsurancePackageDto
```javascript
{
  id: number,
  name: string,
  description: string,
  pricePerDay: number,
  coverageLimit: number,
  deductible: number,
  isActive: boolean
}
```

### AdditionalServiceDto
```javascript
{
  id: number,
  name: string,
  description: string,
  pricePerDay: number,
  isAvailable: boolean,
  iconUrl: string | null
}
```

## User Flow

1. **View Vehicle** - User browses vehicle details
2. **Click "Add to Cart"** - Opens rental modal
3. **Select Dates** - Choose pickup and return dates
4. **Select Insurance** - Choose insurance package or "No Insurance"
5. **Select Services** - Choose additional services (0 or more)
6. **Review Summary** - See complete breakdown with total price
7. **Confirm** - Add to cart with success notification

## Price Calculation

All prices are per day and multiplied by the rental duration:

```javascript
Vehicle Cost = pricePerDay × days
Insurance Cost = insurance.pricePerDay × days
Services Cost = Σ(service.pricePerDay × days)
Total = Vehicle Cost + Insurance Cost + Services Cost
```

## Styling

- Modern, clean design with smooth transitions
- Responsive layout for mobile devices
- Progress indicator showing current step
- Color-coded status badges and notifications
- Hover effects and animations

## Key Features

### Date Selection
- Minimum date is today
- Return date must be after pickup date
- Visual feedback with rental period summary
- Date validation with error messages

### Insurance Selection
- "No Insurance" option available
- "Recommended" badge on Standard package
- Displays coverage limit and deductible
- Radio button selection (single choice)

### Additional Services
- Multiple services can be selected
- Disabled state for unavailable services
- Default icons based on service name
- Summary of selected services with prices

### Rental Summary
- Complete breakdown of all selections
- Visual vehicle card with image
- Date range display
- Itemized pricing
- Prominent total in gradient card

## Success Notification

After confirming rental:
- Green success notification appears
- Auto-dismisses after 3 seconds
- Animated entrance/exit
- Fixed position (top-right)

## Future Enhancements

- [ ] Add to cart store integration
- [ ] Backend API calls for booking
- [ ] Payment processing integration
- [ ] Email confirmation
- [ ] Booking history
- [ ] Cancel/modify booking

## Usage Example

```vue
<template>
  <VehicleDetail />
</template>

<script setup>
import VehicleDetail from '@/components/VehicleDetail.vue';
</script>
```

The modal is automatically integrated and will open when the "Add to Cart" button is clicked.

## Notes

- All API endpoints expect base URL: `http://localhost:5000/api`
- No authentication required for viewing insurance/services
- Prices displayed in EUR (€)
- Component uses Vue 3 Composition API
- Fully responsive design
- Accessible with keyboard navigation
