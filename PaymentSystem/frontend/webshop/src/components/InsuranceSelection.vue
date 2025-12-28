<template>
  <div class="insurance-selection">
    <h3 class="section-title">Select Insurance Package</h3>
    <p class="section-subtitle">Protect your rental with comprehensive coverage</p>

    <div v-if="loading" class="loading">
      <div class="spinner"></div>
      <p>Loading insurance packages...</p>
    </div>

    <div v-else-if="error" class="error-container">
      <p class="error-text">{{ error }}</p>
      <button class="btn-retry" @click="loadInsurance">Retry</button>
    </div>

    <div v-else class="insurance-list">
      <!-- No Insurance Option -->
      <div 
        class="insurance-card no-insurance"
        :class="{ selected: selectedInsurance === null }"
        @click="selectInsurance(null)"
      >
        <div class="card-header">
          <div class="radio-button">
            <span v-if="selectedInsurance === null" class="checked">✓</span>
          </div>
          <div>
            <h4>No Insurance</h4>
            <p class="price">€0.00 / day</p>
          </div>
        </div>
        <p class="description">Continue without insurance coverage</p>
      </div>

      <!-- Insurance Packages -->
      <div 
        v-for="insurance in insurancePackages"
        :key="insurance.id"
        class="insurance-card"
        :class="{ 
          selected: selectedInsurance?.id === insurance.id,
          recommended: isRecommended(insurance)
        }"
        @click="selectInsurance(insurance)"
      >
        <div v-if="isRecommended(insurance)" class="recommended-badge">
          ⭐ Recommended
        </div>
        
        <div class="card-header">
          <div class="radio-button">
            <span v-if="selectedInsurance?.id === insurance.id" class="checked">✓</span>
          </div>
          <div>
            <h4>{{ insurance.name }}</h4>
            <p class="price">€{{ insurance.pricePerDay.toFixed(2) }} / day</p>
          </div>
        </div>

        <p class="description">{{ insurance.description }}</p>

        <div class="coverage-details">
          <div class="coverage-item">
            <span class="label">Coverage Limit:</span>
            <span class="value">€{{ formatNumber(insurance.coverageLimit) }}</span>
          </div>
          <div class="coverage-item">
            <span class="label">Deductible:</span>
            <span class="value">€{{ formatNumber(insurance.deductible) }}</span>
          </div>
        </div>
      </div>
    </div>

    <div class="actions">
      <button class="btn-back" @click="$emit('back')">
        Back
      </button>
      <button class="btn-continue" @click="handleContinue" :disabled="!canContinue">
        Continue to Services
      </button>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue';
import { insuranceApi } from '../services/insuranceService';

const emit = defineEmits(['continue', 'back']);

const insurancePackages = ref([]);
const selectedInsurance = ref(null);
const loading = ref(false);
const error = ref(null);

const canContinue = computed(() => {
  // User can continue with or without insurance
  return true;
});

const isRecommended = (insurance) => {
  return insurance.name.toLowerCase() === 'standard';
};

const loadInsurance = async () => {
  loading.value = true;
  error.value = null;

  try {
    const response = await insuranceApi.getAllInsurance();
    const data = response.data.data || response.data;
    
    // Filter only active insurance packages
    insurancePackages.value = Array.isArray(data) 
      ? data.filter(pkg => pkg.isActive) 
      : [];
  } catch (err) {
    console.error('Error loading insurance:', err);
    error.value = 'Failed to load insurance packages. Please try again.';
  } finally {
    loading.value = false;
  }
};

const selectInsurance = (insurance) => {
  selectedInsurance.value = insurance;
};

const formatNumber = (num) => {
  return num.toLocaleString('en-US');
};

const handleContinue = () => {
  emit('continue', {
    insurance: selectedInsurance.value
  });
};

onMounted(() => {
  loadInsurance();
});
</script>

<style scoped>
.insurance-selection {
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

.insurance-list {
  display: flex;
  flex-direction: column;
  gap: 16px;
  margin-bottom: 30px;
}

.insurance-card {
  position: relative;
  padding: 24px;
  background: white;
  border: 2px solid #e5e7eb;
  border-radius: 12px;
  cursor: pointer;
  transition: all 0.2s;
}

.insurance-card:hover {
  border-color: #3b82f6;
  box-shadow: 0 4px 12px rgba(59, 130, 246, 0.1);
}

.insurance-card.selected {
  border-color: #3b82f6;
  background: #eff6ff;
  box-shadow: 0 4px 16px rgba(59, 130, 246, 0.2);
}

.insurance-card.no-insurance {
  background: #f9fafb;
}

.insurance-card.recommended {
  border-color: #f59e0b;
}

.recommended-badge {
  position: absolute;
  top: -12px;
  right: 20px;
  background: linear-gradient(135deg, #f59e0b, #d97706);
  color: white;
  padding: 6px 16px;
  border-radius: 20px;
  font-size: 13px;
  font-weight: 700;
  box-shadow: 0 2px 8px rgba(245, 158, 11, 0.3);
}

.card-header {
  display: flex;
  align-items: center;
  gap: 16px;
  margin-bottom: 12px;
}

.radio-button {
  width: 24px;
  height: 24px;
  border: 2px solid #d1d5db;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  transition: all 0.2s;
}

.insurance-card.selected .radio-button {
  border-color: #3b82f6;
  background: #3b82f6;
}

.radio-button .checked {
  color: white;
  font-size: 14px;
  font-weight: bold;
}

.card-header h4 {
  font-size: 20px;
  font-weight: 700;
  color: #1f2937;
  margin: 0;
}

.price {
  font-size: 18px;
  font-weight: 700;
  color: #3b82f6;
  margin: 4px 0 0 0;
}

.description {
  font-size: 14px;
  color: #6b7280;
  line-height: 1.5;
  margin-bottom: 16px;
}

.coverage-details {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 12px;
  padding-top: 16px;
  border-top: 1px solid #e5e7eb;
}

.coverage-item {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.coverage-item .label {
  font-size: 12px;
  color: #6b7280;
  font-weight: 500;
}

.coverage-item .value {
  font-size: 16px;
  color: #1f2937;
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

.btn-continue:hover:not(:disabled) {
  background: #2563eb;
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(59, 130, 246, 0.4);
}

.btn-continue:disabled {
  background: #d1d5db;
  cursor: not-allowed;
  transform: none;
}

@media (max-width: 768px) {
  .coverage-details {
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
