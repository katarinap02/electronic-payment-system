<template>
  <Teleport to="body">
    <Transition name="modal">
      <div v-if="isOpen" class="modal-overlay" @click.self="closeModal">
        <div class="modal-container">
          <div class="modal-header">
            <div class="header-content">
              <h2>{{ stepTitles[currentStep] }}</h2>
              <div class="progress-indicator">
                <div 
                  v-for="step in 4" 
                  :key="step"
                  class="progress-dot"
                  :class="{ 
                    active: step - 1 === currentStep,
                    completed: step - 1 < currentStep 
                  }"
                ></div>
              </div>
            </div>
            <button class="close-button" @click="closeModal" aria-label="Close">
              ✕
            </button>
          </div>

          <div class="modal-body">
            <Transition :name="transitionName" mode="out-in">
              <component
                :is="currentComponent"
                :key="currentStep"
                :vehicle="vehicle"
                :rental-data="rentalData"
                @continue="handleContinue"
                @back="handleBack"
                @confirm="handleConfirm"
              />
            </Transition>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup>
import { ref, computed, watch } from 'vue';
import DateSelection from './DateSelection.vue';
import InsuranceSelection from './InsuranceSelection.vue';
import AdditionalServicesSelection from './AdditionalServicesSelection.vue';
import RentalSummary from './RentalSummary.vue';

const props = defineProps({
  isOpen: {
    type: Boolean,
    default: false
  },
  vehicle: {
    type: Object,
    required: true
  }
});

const emit = defineEmits(['close', 'confirm']);

const currentStep = ref(0);
const transitionName = ref('slide-next');
const rentalData = ref({
  startDate: null,
  endDate: null,
  days: 0,
  insurance: null,
  services: []
});

const steps = [
  DateSelection,
  InsuranceSelection,
  AdditionalServicesSelection,
  RentalSummary
];

const stepTitles = [
  'Select Dates',
  'Choose Insurance',
  'Add Services',
  'Review & Confirm'
];

const currentComponent = computed(() => steps[currentStep.value]);

const handleContinue = (data) => {
  // Save data from current step
  rentalData.value = {
    ...rentalData.value,
    ...data
  };

  // Move to next step
  transitionName.value = 'slide-next';
  currentStep.value++;
};

const handleBack = () => {
  if (currentStep.value > 0) {
    transitionName.value = 'slide-prev';
    currentStep.value--;
  }
};

const handleConfirm = (finalData) => {
  emit('confirm', finalData);
  resetModal();
};

const closeModal = () => {
  emit('close');
  setTimeout(() => {
    resetModal();
  }, 300); // Wait for transition to complete
};

const resetModal = () => {
  currentStep.value = 0;
  rentalData.value = {
    startDate: null,
    endDate: null,
    days: 0,
    insurance: null,
    services: []
  };
};

// Reset modal when closed
watch(() => props.isOpen, (newValue) => {
  if (!newValue) {
    setTimeout(() => {
      resetModal();
    }, 300);
  }
});
</script>

<style scoped>
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(0, 0, 0, 0.6);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 9999;
  padding: 20px;
  backdrop-filter: blur(4px);
}

.modal-container {
  background: white;
  border-radius: 16px;
  width: 100%;
  max-width: 900px;
  max-height: 90vh;
  display: flex;
  flex-direction: column;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
  overflow: hidden;
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 24px 30px;
  border-bottom: 2px solid #e5e7eb;
  background: linear-gradient(135deg, #f9fafb, #ffffff);
}

.header-content {
  flex: 1;
}

.header-content h2 {
  font-size: 24px;
  font-weight: 700;
  color: #1f2937;
  margin: 0 0 12px 0;
}

.progress-indicator {
  display: flex;
  gap: 8px;
}

.progress-dot {
  width: 40px;
  height: 4px;
  background: #e5e7eb;
  border-radius: 2px;
  transition: all 0.3s;
}

.progress-dot.active {
  background: #3b82f6;
}

.progress-dot.completed {
  background: #10b981;
}

.close-button {
  width: 36px;
  height: 36px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: transparent;
  border: none;
  border-radius: 8px;
  font-size: 24px;
  color: #6b7280;
  cursor: pointer;
  transition: all 0.2s;
  flex-shrink: 0;
  margin-left: 20px;
}

.close-button:hover {
  background: #f3f4f6;
  color: #1f2937;
}

.modal-body {
  flex: 1;
  overflow-y: auto;
  position: relative;
}

/* Modal transitions */
.modal-enter-active,
.modal-leave-active {
  transition: all 0.3s ease;
}

.modal-enter-from {
  opacity: 0;
  transform: scale(0.9);
}

.modal-leave-to {
  opacity: 0;
  transform: scale(0.9);
}

.modal-enter-from .modal-overlay,
.modal-leave-to .modal-overlay {
  background: rgba(0, 0, 0, 0);
}

/* Step transitions */
.slide-next-enter-active,
.slide-next-leave-active,
.slide-prev-enter-active,
.slide-prev-leave-active {
  transition: all 0.3s ease;
}

.slide-next-enter-from {
  opacity: 0;
  transform: translateX(30px);
}

.slide-next-leave-to {
  opacity: 0;
  transform: translateX(-30px);
}

.slide-prev-enter-from {
  opacity: 0;
  transform: translateX(-30px);
}

.slide-prev-leave-to {
  opacity: 0;
  transform: translateX(30px);
}

/* Scrollbar styling */
.modal-body::-webkit-scrollbar {
  width: 8px;
}

.modal-body::-webkit-scrollbar-track {
  background: #f1f1f1;
}

.modal-body::-webkit-scrollbar-thumb {
  background: #cbd5e1;
  border-radius: 4px;
}

.modal-body::-webkit-scrollbar-thumb:hover {
  background: #94a3b8;
}

@media (max-width: 768px) {
  .modal-overlay {
    padding: 0;
    align-items: flex-end;
  }

  .modal-container {
    max-width: 100%;
    max-height: 95vh;
    border-radius: 16px 16px 0 0;
  }

  .modal-header {
    padding: 20px;
  }

  .header-content h2 {
    font-size: 20px;
  }

  .progress-dot {
    width: 30px;
  }

  .close-button {
    width: 32px;
    height: 32px;
    font-size: 20px;
    margin-left: 12px;
  }
}
</style>
