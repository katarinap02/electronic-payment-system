<template>
  <div class="vehicle-card">
    <div class="vehicle-image">
      <img 
        :src="vehicle.imageUrl || 'https://via.placeholder.com/300x200?text=No+Image'" 
        :alt="`${vehicle.brand} ${vehicle.model}`"
      />
      <span class="status-badge" :class="getStatusClass(vehicle.status)">
        {{ vehicle.status }}
      </span>
    </div>
    <div class="vehicle-content">
      <div class="vehicle-header">
        <h3>{{ vehicle.brand }} {{ vehicle.model }}</h3>
        <span class="category-badge">{{ vehicle.category }}</span>
      </div>
      <p class="vehicle-year">{{ vehicle.year }}</p>
      <div class="vehicle-specs">
        <div class="spec-item">
          <span class="spec-label">Transmission:</span>
          <span class="spec-value">{{ vehicle.transmission }}</span>
        </div>
        <div class="spec-item">
          <span class="spec-label">Fuel:</span>
          <span class="spec-value">{{ vehicle.fuelType }}</span>
        </div>
        <div class="spec-item">
          <span class="spec-label">Seats:</span>
          <span class="spec-value">{{ vehicle.seats }}</span>
        </div>
        <div class="spec-item">
          <span class="spec-label">Mileage:</span>
          <span class="spec-value">{{ formatMileage(vehicle.mileage) }}</span>
        </div>
      </div>
      <div class="vehicle-details">
        <p class="color">
          <strong>Color:</strong> {{ vehicle.color }}
        </p>
        <p class="license">
          <strong>License:</strong> {{ vehicle.licensePlate }}
        </p>
      </div>
      <div class="vehicle-footer">
        <div class="price">
          <span class="price-amount">€{{ vehicle.pricePerDay.toFixed(2) }}</span>
          <span class="price-label">/ day</span>
        </div>
        <button 
          class="btn-details" 
          @click="$emit('view-details', vehicle.id)"
          :disabled="vehicle.status !== 'Available'"
        >
          {{ vehicle.status === 'Available' ? 'View Details' : 'Unavailable' }}
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { defineProps, defineEmits } from 'vue';

defineProps({
  vehicle: {
    type: Object,
    required: true
  }
});

defineEmits(['view-details']);

const formatMileage = (mileage) => {
  return `${mileage.toLocaleString()} km`;
};

const getStatusClass = (status) => {
  const statusMap = {
    'Available': 'status-available',
    'Rented': 'status-rented',
    'Maintenance': 'status-maintenance',
    'Unavailable': 'status-unavailable'
  };
  return statusMap[status] || '';
};
</script>

<style scoped>
.vehicle-card {
  background: white;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  overflow: hidden;
  transition: transform 0.3s, box-shadow 0.3s;
  height: 100%;
  display: flex;
  flex-direction: column;
}

.vehicle-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.15);
}

.vehicle-image {
  position: relative;
  width: 100%;
  height: 200px;
  overflow: hidden;
  background: #f5f5f5;
}

.vehicle-image img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.status-badge {
  position: absolute;
  top: 12px;
  right: 12px;
  padding: 6px 12px;
  border-radius: 20px;
  font-size: 12px;
  font-weight: 600;
  text-transform: uppercase;
  color: white;
}

.status-available {
  background: #10b981;
}

.status-rented {
  background: #f59e0b;
}

.status-maintenance {
  background: #ef4444;
}

.status-unavailable {
  background: #6b7280;
}

.vehicle-content {
  padding: 20px;
  flex: 1;
  display: flex;
  flex-direction: column;
}

.vehicle-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 8px;
}

.vehicle-header h3 {
  margin: 0;
  font-size: 20px;
  color: #1f2937;
  font-weight: 600;
}

.category-badge {
  padding: 4px 10px;
  background: #e0e7ff;
  color: #4f46e5;
  border-radius: 12px;
  font-size: 12px;
  font-weight: 600;
}

.vehicle-year {
  color: #6b7280;
  margin: 0 0 16px 0;
  font-size: 14px;
}

.vehicle-specs {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 10px;
  margin-bottom: 16px;
}

.spec-item {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.spec-label {
  font-size: 12px;
  color: #6b7280;
  font-weight: 500;
}

.spec-value {
  font-size: 14px;
  color: #1f2937;
  font-weight: 600;
}

.vehicle-details {
  margin-bottom: 16px;
  padding-top: 12px;
  border-top: 1px solid #e5e7eb;
}

.vehicle-details p {
  margin: 6px 0;
  font-size: 14px;
  color: #4b5563;
}

.vehicle-footer {
  margin-top: auto;
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-top: 16px;
  border-top: 1px solid #e5e7eb;
}

.price {
  display: flex;
  align-items: baseline;
  gap: 4px;
}

.price-amount {
  font-size: 28px;
  font-weight: 700;
  color: #4f46e5;
}

.price-label {
  font-size: 14px;
  color: #6b7280;
}

.btn-details {
  padding: 10px 20px;
  background: #4f46e5;
  color: white;
  border: none;
  border-radius: 8px;
  font-weight: 600;
  cursor: pointer;
  transition: background 0.3s;
  font-size: 14px;
}

.btn-details:hover:not(:disabled) {
  background: #4338ca;
}

.btn-details:disabled {
  background: #9ca3af;
  cursor: not-allowed;
}
</style>
