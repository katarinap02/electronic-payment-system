<template>
  <div class="date-selection">
    <h3 class="section-title">Select Rental Period</h3>
    <p class="section-subtitle">Choose your pickup and return dates</p>

    <div class="calendar-container">
      <div class="date-inputs">
        <div class="input-group">
          <label>Pickup Date</label>
          <input
            type="date"
            v-model="startDate"
            :min="minDate"
            @change="validateDates"
          />
        </div>
        <div class="input-group">
          <label>Return Date</label>
          <input
            type="date"
            v-model="endDate"
            :min="startDate || minDate"
            :disabled="!startDate"
            @change="validateDates"
          />
        </div>
      </div>

      <div v-if="daysCount > 0" class="rental-summary">
        <div class="summary-item">
          <span class="icon">📅</span>
          <span class="text">{{ daysCount }} day{{ daysCount > 1 ? 's' : '' }}</span>
        </div>
        <div class="summary-item">
          <span class="icon">📍</span>
          <span class="text">{{ formatDate(startDate) }} - {{ formatDate(endDate) }}</span>
        </div>
      </div>

      <div v-if="errorMessage" class="error-message">
        {{ errorMessage }}
      </div>
    </div>

    <div class="actions">
      <button class="btn-continue" @click="handleContinue" :disabled="!isValid">
        Continue to Insurance
      </button>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch } from 'vue';

const emit = defineEmits(['continue']);

const startDate = ref('');
const endDate = ref('');
const errorMessage = ref('');

// Get today's date in YYYY-MM-DD format
const minDate = computed(() => {
  const today = new Date();
  return today.toISOString().split('T')[0];
});

// Calculate number of days
const daysCount = computed(() => {
  if (!startDate.value || !endDate.value) return 0;
  
  const start = new Date(startDate.value);
  const end = new Date(endDate.value);
  const diff = Math.ceil((end - start) / (1000 * 60 * 60 * 24));
  
  return diff > 0 ? diff : 0;
});

// Validate dates
const isValid = computed(() => {
  return startDate.value && endDate.value && daysCount.value > 0 && !errorMessage.value;
});

const validateDates = () => {
  errorMessage.value = '';

  if (!startDate.value || !endDate.value) return;

  const start = new Date(startDate.value);
  const end = new Date(endDate.value);

  if (end <= start) {
    errorMessage.value = 'Return date must be after pickup date';
    return;
  }

  if (daysCount.value < 1) {
    errorMessage.value = 'Minimum rental period is 1 day';
    return;
  }

  if (daysCount.value > 90) {
    errorMessage.value = 'Maximum rental period is 90 days';
    return;
  }
};

const formatDate = (dateString) => {
  if (!dateString) return '';
  const date = new Date(dateString);
  return date.toLocaleDateString('en-US', { 
    month: 'short', 
    day: 'numeric', 
    year: 'numeric' 
  });
};

const handleContinue = () => {
  if (isValid.value) {
    emit('continue', {
      startDate: startDate.value,
      endDate: endDate.value,
      days: daysCount.value
    });
  }
};

// Watch for date changes
watch([startDate, endDate], () => {
  validateDates();
});
</script>

<style scoped>
.date-selection {
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

.calendar-container {
  background: #f9fafb;
  border-radius: 12px;
  padding: 30px;
  margin-bottom: 30px;
}

.date-inputs {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 20px;
  margin-bottom: 20px;
}

.input-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.input-group label {
  font-size: 14px;
  font-weight: 600;
  color: #374151;
}

.input-group input[type="date"] {
  padding: 12px;
  border: 2px solid #e5e7eb;
  border-radius: 8px;
  font-size: 15px;
  transition: all 0.2s;
  background: white;
}

.input-group input[type="date"]:focus {
  outline: none;
  border-color: #3b82f6;
}

.input-group input[type="date"]:disabled {
  background: #f3f4f6;
  cursor: not-allowed;
}

.rental-summary {
  display: flex;
  flex-direction: column;
  gap: 12px;
  padding: 20px;
  background: white;
  border-radius: 8px;
  border: 2px dashed #d1d5db;
}

.summary-item {
  display: flex;
  align-items: center;
  gap: 10px;
  font-size: 15px;
  color: #374151;
}

.summary-item .icon {
  font-size: 20px;
}

.summary-item .text {
  font-weight: 500;
}

.error-message {
  margin-top: 15px;
  padding: 12px;
  background: #fee2e2;
  color: #dc2626;
  border-radius: 8px;
  font-size: 14px;
  font-weight: 500;
}

.actions {
  display: flex;
  justify-content: flex-end;
}

.btn-continue {
  padding: 14px 32px;
  background: #3b82f6;
  color: white;
  border: none;
  border-radius: 8px;
  font-size: 16px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s;
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
  .date-inputs {
    grid-template-columns: 1fr;
  }
}
</style>
