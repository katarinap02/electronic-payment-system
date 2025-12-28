<template>
  <div class="rental-summary">
    <h3 class="section-title">Rental Summary</h3>
    <p class="section-subtitle">Review your rental details before adding to cart</p>

    <div class="summary-container">
      <!-- Vehicle Information -->
      <div class="summary-section">
        <h4 class="section-heading">🚗 Vehicle</h4>
        <div class="info-card">
          <div class="vehicle-header">
            <img 
              v-if="vehicle.imageUrl" 
              :src="vehicle.imageUrl" 
              :alt="`${vehicle.brand} ${vehicle.model}`"
              class="vehicle-image"
            />
            <div>
              <h5>{{ vehicle.brand }} {{ vehicle.model }}</h5>
              <p class="vehicle-meta">{{ vehicle.year }} • {{ vehicle.category }}</p>
            </div>
          </div>
          <div class="price-line">
            <span class="label">Base Price</span>
            <span class="value">€{{ vehicle.pricePerDay.toFixed(2) }} × {{ rentalData.days }} day{{ rentalData.days > 1 ? 's' : '' }}</span>
            <span class="amount">€{{ (vehicle.pricePerDay * rentalData.days).toFixed(2) }}</span>
          </div>
        </div>
      </div>

      <!-- Rental Period -->
      <div class="summary-section">
        <h4 class="section-heading">📅 Rental Period</h4>
        <div class="info-card">
          <div class="date-range">
            <div class="date-item">
              <span class="date-label">Pickup</span>
              <span class="date-value">{{ formatDate(rentalData.startDate) }}</span>
            </div>
            <div class="arrow">→</div>
            <div class="date-item">
              <span class="date-label">Return</span>
              <span class="date-value">{{ formatDate(rentalData.endDate) }}</span>
            </div>
          </div>
          <div class="duration">
            <span class="duration-badge">{{ rentalData.days }} day{{ rentalData.days > 1 ? 's' : '' }} rental</span>
          </div>
        </div>
      </div>

      <!-- Insurance Package -->
      <div class="summary-section">
        <h4 class="section-heading">🛡️ Insurance</h4>
        <div class="info-card">
          <div v-if="rentalData.insurance" class="insurance-details">
            <div class="insurance-header">
              <h5>{{ rentalData.insurance.name }}</h5>
            </div>
            <p class="insurance-description">{{ rentalData.insurance.description }}</p>
            <div class="insurance-specs">
              <div class="spec">
                <span class="spec-label">Coverage</span>
                <span class="spec-value">€{{ formatNumber(rentalData.insurance.coverageLimit) }}</span>
              </div>
              <div class="spec">
                <span class="spec-label">Deductible</span>
                <span class="spec-value">€{{ formatNumber(rentalData.insurance.deductible) }}</span>
              </div>
            </div>
            <div class="price-line">
              <span class="label">Insurance Cost</span>
              <span class="value">€{{ rentalData.insurance.pricePerDay.toFixed(2) }} × {{ rentalData.days }} day{{ rentalData.days > 1 ? 's' : '' }}</span>
              <span class="amount">€{{ (rentalData.insurance.pricePerDay * rentalData.days).toFixed(2) }}</span>
            </div>
          </div>
          <div v-else class="no-insurance">
            <p>No insurance selected</p>
            <span class="amount">€0.00</span>
          </div>
        </div>
      </div>

      <!-- Additional Services -->
      <div class="summary-section">
        <h4 class="section-heading">✨ Additional Services</h4>
        <div class="info-card">
          <div v-if="rentalData.services && rentalData.services.length > 0" class="services-list">
            <div 
              v-for="service in rentalData.services" 
              :key="service.id"
              class="service-item"
            >
              <div class="service-info">
                <span class="service-name">{{ service.name }}</span>
                <span class="service-price">€{{ service.pricePerDay.toFixed(2) }}/day</span>
              </div>
              <span class="service-total">€{{ (service.pricePerDay * rentalData.days).toFixed(2) }}</span>
            </div>
            <div class="price-line total-services">
              <span class="label">Services Subtotal</span>
              <span class="amount">€{{ servicesTotal.toFixed(2) }}</span>
            </div>
          </div>
          <div v-else class="no-services">
            <p>No additional services selected</p>
            <span class="amount">€0.00</span>
          </div>
        </div>
      </div>

      <!-- Total Price -->
      <div class="summary-section total-section">
        <div class="total-card">
          <div class="total-breakdown">
            <div class="breakdown-line">
              <span>Vehicle ({{ rentalData.days }} days)</span>
              <span>€{{ vehicleTotal.toFixed(2) }}</span>
            </div>
            <div class="breakdown-line" v-if="rentalData.insurance">
              <span>Insurance ({{ rentalData.days }} days)</span>
              <span>€{{ insuranceTotal.toFixed(2) }}</span>
            </div>
            <div class="breakdown-line" v-if="rentalData.services && rentalData.services.length > 0">
              <span>Services ({{ rentalData.days }} days)</span>
              <span>€{{ servicesTotal.toFixed(2) }}</span>
            </div>
          </div>
          <div class="total-line">
            <span class="total-label">Total Amount</span>
            <span class="total-amount">€{{ grandTotal.toFixed(2) }}</span>
          </div>
        </div>
      </div>
    </div>

    <div class="actions">
      <button class="btn-back" @click="$emit('back')">
        Back
      </button>
      <button class="btn-confirm" @click="handleConfirm">
        Add to Cart
      </button>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue';

const props = defineProps({
  vehicle: {
    type: Object,
    required: true
  },
  rentalData: {
    type: Object,
    required: true
  }
});

const emit = defineEmits(['confirm', 'back']);

const vehicleTotal = computed(() => {
  return props.vehicle.pricePerDay * props.rentalData.days;
});

const insuranceTotal = computed(() => {
  return props.rentalData.insurance 
    ? props.rentalData.insurance.pricePerDay * props.rentalData.days 
    : 0;
});

const servicesTotal = computed(() => {
  if (!props.rentalData.services || props.rentalData.services.length === 0) {
    return 0;
  }
  return props.rentalData.services.reduce((total, service) => {
    return total + (service.pricePerDay * props.rentalData.days);
  }, 0);
});

const grandTotal = computed(() => {
  return vehicleTotal.value + insuranceTotal.value + servicesTotal.value;
});

const formatDate = (dateString) => {
  if (!dateString) return '';
  const date = new Date(dateString);
  return date.toLocaleDateString('en-US', { 
    weekday: 'short',
    month: 'short', 
    day: 'numeric', 
    year: 'numeric' 
  });
};

const formatNumber = (num) => {
  return num.toLocaleString('en-US');
};

const handleConfirm = () => {
  emit('confirm', {
    vehicle: props.vehicle,
    ...props.rentalData,
    totals: {
      vehicle: vehicleTotal.value,
      insurance: insuranceTotal.value,
      services: servicesTotal.value,
      total: grandTotal.value
    }
  });
};
</script>

<style scoped>
.rental-summary {
  padding: 20px;
}

.section-title {
  font-size: 24px;
  font-weight: 700;
  color: #1f2937;
  margin-bottom: 8px;
}

.section-subtitle {
  font-size: 14px;
  color: #6b7280;
  margin-bottom: 30px;
}

.summary-container {
  display: flex;
  flex-direction: column;
  gap: 20px;
  margin-bottom: 30px;
}

.summary-section {
  background: white;
}

.section-heading {
  font-size: 16px;
  font-weight: 700;
  color: #1f2937;
  margin-bottom: 12px;
}

.info-card {
  background: #f9fafb;
  border: 2px solid #e5e7eb;
  border-radius: 12px;
  padding: 20px;
}

.vehicle-header {
  display: flex;
  align-items: center;
  gap: 16px;
  margin-bottom: 16px;
  padding-bottom: 16px;
  border-bottom: 1px solid #e5e7eb;
}

.vehicle-image {
  width: 80px;
  height: 60px;
  object-fit: cover;
  border-radius: 8px;
}

.vehicle-header h5 {
  font-size: 18px;
  font-weight: 700;
  color: #1f2937;
  margin: 0 0 4px 0;
}

.vehicle-meta {
  font-size: 13px;
  color: #6b7280;
  margin: 0;
}

.date-range {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  margin-bottom: 16px;
}

.date-item {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.date-label {
  font-size: 12px;
  color: #6b7280;
  font-weight: 600;
  text-transform: uppercase;
}

.date-value {
  font-size: 14px;
  color: #1f2937;
  font-weight: 600;
}

.arrow {
  font-size: 24px;
  color: #3b82f6;
  font-weight: bold;
}

.duration {
  padding-top: 12px;
  border-top: 1px solid #e5e7eb;
}

.duration-badge {
  display: inline-block;
  background: #eff6ff;
  color: #3b82f6;
  padding: 6px 12px;
  border-radius: 20px;
  font-size: 13px;
  font-weight: 700;
}

.insurance-header h5 {
  font-size: 16px;
  font-weight: 700;
  color: #1f2937;
  margin: 0 0 8px 0;
}

.insurance-description {
  font-size: 13px;
  color: #6b7280;
  margin: 0 0 16px 0;
  line-height: 1.5;
}

.insurance-specs {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 12px;
  margin-bottom: 16px;
  padding-bottom: 16px;
  border-bottom: 1px solid #e5e7eb;
}

.spec {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.spec-label {
  font-size: 11px;
  color: #6b7280;
  font-weight: 600;
  text-transform: uppercase;
}

.spec-value {
  font-size: 14px;
  color: #1f2937;
  font-weight: 700;
}

.services-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.service-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 12px;
  background: white;
  border-radius: 8px;
}

.service-info {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.service-name {
  font-size: 14px;
  color: #1f2937;
  font-weight: 600;
}

.service-price {
  font-size: 12px;
  color: #6b7280;
}

.service-total {
  font-size: 14px;
  color: #10b981;
  font-weight: 700;
}

.price-line {
  display: grid;
  grid-template-columns: auto 1fr auto;
  gap: 12px;
  align-items: center;
}

.price-line .label {
  font-size: 14px;
  color: #374151;
  font-weight: 500;
}

.price-line .value {
  font-size: 12px;
  color: #6b7280;
  text-align: right;
}

.price-line .amount {
  font-size: 16px;
  color: #1f2937;
  font-weight: 700;
  text-align: right;
}

.price-line.total-services {
  margin-top: 12px;
  padding-top: 12px;
  border-top: 1px solid #e5e7eb;
  grid-template-columns: 1fr auto;
}

.no-insurance,
.no-services {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.no-insurance p,
.no-services p {
  font-size: 14px;
  color: #6b7280;
  margin: 0;
}

.no-insurance .amount,
.no-services .amount {
  font-size: 16px;
  color: #6b7280;
  font-weight: 600;
}

.total-section {
  margin-top: 10px;
}

.total-card {
  background: linear-gradient(135deg, #3b82f6, #2563eb);
  border-radius: 12px;
  padding: 24px;
  color: white;
}

.total-breakdown {
  display: flex;
  flex-direction: column;
  gap: 10px;
  margin-bottom: 16px;
  padding-bottom: 16px;
  border-bottom: 1px solid rgba(255, 255, 255, 0.2);
}

.breakdown-line {
  display: flex;
  justify-content: space-between;
  font-size: 14px;
  opacity: 0.9;
}

.total-line {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.total-label {
  font-size: 18px;
  font-weight: 600;
}

.total-amount {
  font-size: 32px;
  font-weight: 700;
}

.actions {
  display: flex;
  justify-content: space-between;
  gap: 12px;
}

.btn-back,
.btn-confirm {
  padding: 14px 32px;
  border: none;
  border-radius: 8px;
  font-size: 16px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s;
}

.btn-back {
  background: white;
  color: #6b7280;
  border: 2px solid #e5e7eb;
}

.btn-back:hover {
  background: #f9fafb;
  border-color: #d1d5db;
}

.btn-confirm {
  background: #10b981;
  color: white;
  flex: 1;
}

.btn-confirm:hover {
  background: #059669;
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(16, 185, 129, 0.4);
}

@media (max-width: 768px) {
  .date-range {
    flex-direction: column;
  }

  .arrow {
    transform: rotate(90deg);
  }

  .insurance-specs {
    grid-template-columns: 1fr;
  }

  .actions {
    flex-direction: column-reverse;
  }

  .btn-back,
  .btn-confirm {
    width: 100%;
  }
}
</style>
