<template>
  <div class="services-selection">
    <h3 class="section-title">Additional Services</h3>
    <p class="section-subtitle">Enhance your rental experience with optional extras</p>

    <div v-if="loading" class="loading">
      <div class="spinner"></div>
      <p>Loading additional services...</p>
    </div>

    <div v-else-if="error" class="error-container">
      <p class="error-text">{{ error }}</p>
      <button class="btn-retry" @click="loadServices">Retry</button>
    </div>

    <div v-else class="services-list">
      <div 
        v-for="service in availableServices"
        :key="service.id"
        class="service-card"
        :class="{ 
          selected: isSelected(service.id),
          unavailable: !service.isAvailable 
        }"
        @click="toggleService(service)"
      >
        <div class="card-header">
          <div class="checkbox">
            <span v-if="isSelected(service.id)" class="checked">✓</span>
          </div>
          
          <div class="service-icon" v-if="service.iconUrl">
            <img :src="service.iconUrl" :alt="service.name" />
          </div>
          <div class="service-icon-default" v-else>
            {{ getDefaultIcon(service.name) }}
          </div>

          <div class="service-info">
            <h4>{{ service.name }}</h4>
            <p class="price">+€{{ service.pricePerDay.toFixed(2) }} / day</p>
          </div>
        </div>

        <p class="description">{{ service.description }}</p>

        <div v-if="!service.isAvailable" class="unavailable-badge">
          Currently Unavailable
        </div>
      </div>

      <div v-if="availableServices.length === 0" class="empty-state">
        <p>No additional services available at this time.</p>
      </div>
    </div>

    <div v-if="selectedServices.length > 0" class="selection-summary">
      <h4>Selected Services ({{ selectedServices.length }})</h4>
      <div class="summary-items">
        <div 
          v-for="service in selectedServiceDetails"
          :key="service.id"
          class="summary-item"
        >
          <span class="name">{{ service.name }}</span>
          <span class="price">+€{{ service.pricePerDay.toFixed(2) }}/day</span>
        </div>
      </div>
    </div>

    <div class="actions">
      <button class="btn-back" @click="$emit('back')">
        Back
      </button>
      <button class="btn-continue" @click="handleContinue">
        Continue to Summary
      </button>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue';
import { additionalServiceApi } from '../services/additionalService';

const emit = defineEmits(['continue', 'back']);

const availableServices = ref([]);
const selectedServices = ref([]);
const loading = ref(false);
const error = ref(null);

const selectedServiceDetails = computed(() => {
  return availableServices.value.filter(service => 
    selectedServices.value.includes(service.id)
  );
});

const isSelected = (serviceId) => {
  return selectedServices.value.includes(serviceId);
};

const loadServices = async () => {
  loading.value = true;
  error.value = null;

  try {
    const response = await additionalServiceApi.getAllServices();
    const data = response.data.data || response.data;
    
    availableServices.value = Array.isArray(data) ? data : [];
  } catch (err) {
    console.error('Error loading services:', err);
    error.value = 'Failed to load additional services. Please try again.';
  } finally {
    loading.value = false;
  }
};

const toggleService = (service) => {
  if (!service.isAvailable) return;

  const index = selectedServices.value.indexOf(service.id);
  if (index > -1) {
    selectedServices.value.splice(index, 1);
  } else {
    selectedServices.value.push(service.id);
  }
};

const getDefaultIcon = (serviceName) => {
  const name = serviceName.toLowerCase();
  if (name.includes('gps') || name.includes('navigation')) return '🗺️';
  if (name.includes('child') || name.includes('seat')) return '👶';
  if (name.includes('wifi')) return '📶';
  if (name.includes('insurance')) return '🛡️';
  if (name.includes('winter') || name.includes('snow')) return '❄️';
  if (name.includes('bike') || name.includes('rack')) return '🚲';
  if (name.includes('driver')) return '👨‍✈️';
  if (name.includes('fuel')) return '⛽';
  return '✨';
};

const handleContinue = () => {
  emit('continue', {
    services: selectedServiceDetails.value
  });
};

onMounted(() => {
  loadServices();
});
</script>

<style scoped>
.services-selection {
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

.loading {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 60px 20px;
  gap: 15px;
}

.spinner {
  width: 40px;
  height: 40px;
  border: 4px solid #e5e7eb;
  border-top-color: #3b82f6;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.error-container {
  text-align: center;
  padding: 40px 20px;
}

.error-text {
  color: #dc2626;
  margin-bottom: 20px;
}

.btn-retry {
  padding: 10px 20px;
  background: #3b82f6;
  color: white;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  font-weight: 600;
}

.services-list {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 16px;
  margin-bottom: 30px;
}

.service-card {
  position: relative;
  padding: 20px;
  background: white;
  border: 2px solid #e5e7eb;
  border-radius: 12px;
  cursor: pointer;
  transition: all 0.2s;
}

.service-card:hover:not(.unavailable) {
  border-color: #3b82f6;
  box-shadow: 0 4px 12px rgba(59, 130, 246, 0.1);
  transform: translateY(-2px);
}

.service-card.selected {
  border-color: #3b82f6;
  background: #eff6ff;
  box-shadow: 0 4px 16px rgba(59, 130, 246, 0.2);
}

.service-card.unavailable {
  opacity: 0.5;
  cursor: not-allowed;
  background: #f9fafb;
}

.card-header {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 12px;
}

.checkbox {
  width: 20px;
  height: 20px;
  border: 2px solid #d1d5db;
  border-radius: 4px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  transition: all 0.2s;
}

.service-card.selected .checkbox {
  border-color: #3b82f6;
  background: #3b82f6;
}

.checkbox .checked {
  color: white;
  font-size: 12px;
  font-weight: bold;
}

.service-icon {
  width: 40px;
  height: 40px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.service-icon img {
  width: 100%;
  height: 100%;
  object-fit: contain;
}

.service-icon-default {
  width: 40px;
  height: 40px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 28px;
  flex-shrink: 0;
}

.service-info {
  flex: 1;
}

.service-info h4 {
  font-size: 16px;
  font-weight: 700;
  color: #1f2937;
  margin: 0;
}

.price {
  font-size: 14px;
  font-weight: 700;
  color: #10b981;
  margin: 4px 0 0 0;
}

.description {
  font-size: 13px;
  color: #6b7280;
  line-height: 1.5;
  margin: 0;
}

.unavailable-badge {
  position: absolute;
  top: 12px;
  right: 12px;
  background: #ef4444;
  color: white;
  padding: 4px 10px;
  border-radius: 12px;
  font-size: 11px;
  font-weight: 700;
}

.empty-state {
  grid-column: 1 / -1;
  text-align: center;
  padding: 60px 20px;
  color: #6b7280;
}

.selection-summary {
  background: #f0fdf4;
  border: 2px solid #86efac;
  border-radius: 12px;
  padding: 20px;
  margin-bottom: 30px;
}

.selection-summary h4 {
  font-size: 16px;
  font-weight: 700;
  color: #166534;
  margin: 0 0 16px 0;
}

.summary-items {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.summary-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 10px;
  background: white;
  border-radius: 8px;
}

.summary-item .name {
  font-size: 14px;
  color: #374151;
  font-weight: 500;
}

.summary-item .price {
  font-size: 14px;
  color: #10b981;
  font-weight: 700;
}

.actions {
  display: flex;
  justify-content: space-between;
  gap: 12px;
}

.btn-back,
.btn-continue {
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

.btn-continue {
  background: #3b82f6;
  color: white;
}

.btn-continue:hover {
  background: #2563eb;
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(59, 130, 246, 0.4);
}

@media (max-width: 768px) {
  .services-list {
    grid-template-columns: 1fr;
  }

  .actions {
    flex-direction: column-reverse;
  }

  .btn-back,
  .btn-continue {
    width: 100%;
  }
}
</style>
