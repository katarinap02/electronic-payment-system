<template>
  <div class="rentals-page">
    <div class="page-header">
      <h1>My Reservations</h1>
      <p>Overview of all your vehicle rentals</p>
    </div>

    <!-- Tabs for active and history -->
    <div class="tabs-container">
      <button 
        :class="['tab-btn', { active: activeTab === 'active' }]" 
        @click="activeTab = 'active'"
      >
        Active Reservations
      </button>
      <button 
        :class="['tab-btn', { active: activeTab === 'history' }]" 
        @click="activeTab = 'history'"
      >
        History
      </button>
    </div>

    <!-- Loading state -->
    <div v-if="loading" class="loading-container">
      <div class="spinner"></div>
      <p>Loading...</p>
    </div>

    <!-- Error state -->
    <div v-else-if="error" class="error-container">
      <p class="error-message">{{ error }}</p>
      <button @click="loadRentals" class="btn-retry">Try Again</button>
    </div>

    <!-- Empty state -->
    <div v-else-if="displayedRentals.length === 0" class="empty-state">
      <h3>{{ activeTab === 'active' ? 'No Active Reservations' : 'No History' }}</h3>
      <p>{{ activeTab === 'active' ? 'Rent a vehicle to see your active reservations' : 'Your reservation history will appear here' }}</p>
      <router-link to="/vehicles" class="btn-primary">Browse Vehicles</router-link>
    </div>

    <!-- Rentals list -->
    <div v-else class="rentals-list">
      <div v-for="rental in displayedRentals" :key="rental.id" class="rental-card">
        <div class="rental-header">
          <div class="vehicle-info">
            <h3>{{ rental.vehicleBrand }} {{ rental.vehicleModel }}</h3>
            <span class="category-badge">{{ getCategoryName(rental.vehicleCategory) }}</span>
          </div>
          <div :class="['status-badge', rental.status.toLowerCase()]">
            {{ getStatusText(rental.status) }}
          </div>
        </div>

        <div class="rental-body">
          <div class="rental-details">
            <!-- Dates -->
            <div class="detail-row">
              <span class="detail-label">Period:</span>
              <span class="detail-value">
                {{ formatDate(rental.startDate) }} - {{ formatDate(rental.endDate) }}
                ({{ rental.rentalDays }} days)
              </span>
            </div>

            <!-- License Plate -->
            <div class="detail-row">
              <span class="detail-label">License Plate:</span>
              <span class="detail-value">{{ rental.licensePlate }}</span>
            </div>

            <!-- Insurance -->
            <div class="detail-row" v-if="rental.insuranceType">
              <span class="detail-label">Insurance:</span>
              <span class="detail-value">
                {{ rental.insuranceType }} - {{ formatPrice(rental.insurancePrice, rental.currency) }}
              </span>
            </div>

            <!-- Additional Equipment -->
            <div class="detail-row" v-if="rental.additionalServices && rental.additionalServices.length > 0">
              <span class="detail-label">Additional Services:</span>
              <div class="services-list">
                <span v-for="service in rental.additionalServices" :key="service" class="service-tag">
                  {{ service }}
                </span>
                <span class="service-price">{{ formatPrice(rental.additionalServicesPrice, rental.currency) }}</span>
              </div>
            </div>

            <!-- Price -->
            <div class="detail-row price-row">
              <span class="detail-label">Total Price:</span>
              <span class="detail-value price">{{ formatPrice(rental.totalPrice, rental.currency) }}</span>
            </div>

            <!-- Payment Method -->
            <div class="detail-row">
              <span class="detail-label">Payment Method:</span>
              <span class="detail-value">{{ getPaymentMethodText(rental.paymentMethod) }}</span>
            </div>

            <!-- Payment ID -->
            <div class="detail-row">
              <span class="detail-label">Payment ID:</span>
              <span class="detail-value code">{{ rental.paymentId }}</span>
            </div>

            <!-- Notes -->
            <div class="detail-row" v-if="rental.notes">
              <span class="detail-label">Notes:</span>
              <span class="detail-value">{{ rental.notes }}</span>
            </div>

            <!-- Creation/Completion Dates -->
            <div class="detail-row timestamp">
              <span class="detail-label">Created:</span>
              <span class="detail-value">{{ formatDateTime(rental.createdAt) }}</span>
            </div>
            <div class="detail-row timestamp" v-if="rental.completedAt">
              <span class="detail-label">Completed:</span>
              <span class="detail-value">{{ formatDateTime(rental.completedAt) }}</span>
            </div>
            <div class="detail-row timestamp" v-if="rental.cancelledAt">
              <span class="detail-label">Cancelled:</span>
              <span class="detail-value">{{ formatDateTime(rental.cancelledAt) }}</span>
            </div>
          </div>

          <!-- Actions -->
          <div class="rental-actions" v-if="activeTab === 'active'">
            <button 
              @click="completeRental(rental.id)" 
              class="btn-complete"
              :disabled="rental.status !== 'Active'"
            >
              Complete Rental
            </button>
            <button 
              @click="cancelRental(rental.id)" 
              class="btn-cancel"
              :disabled="rental.status !== 'Active'"
            >
              Cancel Reservation
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue';
import { useAuthStore } from '@/stores/auth';
import rentalService from '@/services/rentalService';

const authStore = useAuthStore();

const activeTab = ref('active');
const activeRentals = ref([]);
const historyRentals = ref([]);
const loading = ref(false);
const error = ref(null);

const displayedRentals = computed(() => {
  return activeTab.value === 'active' ? activeRentals.value : historyRentals.value;
});

const loadRentals = async () => {
  if (!authStore.user?.id) {
    error.value = 'You must be logged in to view reservations';
    return;
  }

  loading.value = true;
  error.value = null;

  try {
    const [active, history] = await Promise.all([
      rentalService.getActiveRentals(authStore.user.id),
      rentalService.getRentalHistory(authStore.user.id)
    ]);

    activeRentals.value = active;
    historyRentals.value = history;
  } catch (err) {
    console.error('Error loading rentals:', err);
    error.value = 'Error loading reservations. Please try again.';
  } finally {
    loading.value = false;
  }
};

const completeRental = async (id) => {
  if (!confirm('Are you sure you want to complete this rental?')) return;

  try {
    await rentalService.updateRentalStatus(id, 'Completed');
    await loadRentals();
    alert('Rental completed successfully!');
  } catch (err) {
    console.error('Error completing rental:', err);
    alert('Error completing rental.');
  }
};

const cancelRental = async (id) => {
  if (!confirm('Are you sure you want to cancel this reservation?')) return;

  try {
    await rentalService.cancelRental(id);
    await loadRentals();
    alert('Reservation cancelled successfully!');
  } catch (err) {
    console.error('Error cancelling rental:', err);
    alert('Error cancelling reservation.');
  }
};

const getCategoryName = (category) => {
  // Keep original English names
  return category;
};

const getStatusText = (status) => {
  // Keep original English statuses
  return status;
};

const getPaymentMethodText = (method) => {
  const methods = {
    'CreditCard': 'Credit Card',
    'QR_CODE': 'IPS QR Code',
    'CARD': 'Card'
  };
  return methods[method] || method;
};

const formatPrice = (price, currency) => {
  if (!price) return '-';
  return `${parseFloat(price).toFixed(2)} ${currency}`;
};

const formatDate = (dateString) => {
  if (!dateString) return '-';
  const date = new Date(dateString);
  return date.toLocaleDateString('sr-RS', { day: '2-digit', month: '2-digit', year: 'numeric' });
};

const formatDateTime = (dateString) => {
  if (!dateString) return '-';
  const date = new Date(dateString);
  return date.toLocaleString('sr-RS', { 
    day: '2-digit', 
    month: '2-digit', 
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  });
};

onMounted(() => {
  loadRentals();
});
</script>

<style scoped>
.rentals-page {
  max-width: 1200px;
  margin: 0 auto;
  padding: 20px;
}

.page-header {
  text-align: center;
  margin-bottom: 30px;
}

.page-header h1 {
  font-size: 2.5rem;
  color: #2c3e50;
  margin-bottom: 10px;
}

.page-header p {
  font-size: 1.1rem;
  color: #7f8c8d;
}

.tabs-container {
  display: flex;
  gap: 10px;
  margin-bottom: 30px;
  border-bottom: 2px solid #ecf0f1;
}

.tab-btn {
  padding: 12px 24px;
  background: none;
  border: none;
  font-size: 1rem;
  font-weight: 500;
  color: #7f8c8d;
  cursor: pointer;
  transition: all 0.3s ease;
  border-bottom: 3px solid transparent;
  margin-bottom: -2px;
}

.tab-btn:hover {
  color: #3498db;
}

.tab-btn.active {
  color: #3498db;
  border-bottom-color: #3498db;
}

.loading-container {
  text-align: center;
  padding: 60px 20px;
}

.spinner {
  border: 4px solid #f3f3f3;
  border-top: 4px solid #3498db;
  border-radius: 50%;
  width: 50px;
  height: 50px;
  animation: spin 1s linear infinite;
  margin: 0 auto 20px;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.error-container {
  text-align: center;
  padding: 60px 20px;
}

.error-message {
  color: #e74c3c;
  font-size: 1.1rem;
  margin-bottom: 20px;
}

.btn-retry {
  padding: 10px 24px;
  background-color: #3498db;
  color: white;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-size: 1rem;
}

.btn-retry:hover {
  background-color: #2980b9;
}

.empty-state {
  text-align: center;
  padding: 60px 20px;
}

.empty-icon {
  font-size: 4rem;
  margin-bottom: 20px;
}

.empty-state h3 {
  color: #2c3e50;
  margin-bottom: 10px;
}

.empty-state p {
  color: #7f8c8d;
  margin-bottom: 30px;
}

.btn-primary {
  display: inline-block;
  padding: 12px 30px;
  background-color: #3498db;
  color: white;
  text-decoration: none;
  border-radius: 6px;
  font-weight: 500;
  transition: background-color 0.3s ease;
}

.btn-primary:hover {
  background-color: #2980b9;
}

.rentals-list {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.rental-card {
  background: white;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  overflow: hidden;
  transition: transform 0.3s ease, box-shadow 0.3s ease;
}

.rental-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.15);
}

.rental-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 20px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
}

.vehicle-info h3 {
  font-size: 1.5rem;
  margin: 0 0 8px 0;
}

.category-badge {
  display: inline-block;
  padding: 4px 12px;
  background-color: rgba(255, 255, 255, 0.2);
  border-radius: 20px;
  font-size: 0.85rem;
  font-weight: 500;
}

.status-badge {
  padding: 8px 16px;
  border-radius: 20px;
  font-weight: 600;
  font-size: 0.9rem;
}

.status-badge.active {
  background-color: #2ecc71;
  color: white;
}

.status-badge.completed {
  background-color: #95a5a6;
  color: white;
}

.status-badge.cancelled {
  background-color: #e74c3c;
  color: white;
}

.rental-body {
  padding: 20px;
}

.rental-details {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.detail-row {
  display: flex;
  align-items: flex-start;
  gap: 12px;
  padding: 8px 0;
  border-bottom: 1px solid #ecf0f1;
}

.detail-row:last-child {
  border-bottom: none;
}

.detail-label {
  font-weight: 600;
  color: #2c3e50;
  min-width: 160px;
  flex-shrink: 0;
}

.detail-value {
  color: #7f8c8d;
  flex: 1;
}

.detail-value.price {
  font-size: 1.3rem;
  font-weight: 700;
  color: #27ae60;
}

.detail-value.code {
  font-family: 'Courier New', monospace;
  background-color: #ecf0f1;
  padding: 4px 8px;
  border-radius: 4px;
  font-size: 0.9rem;
}

.price-row {
  margin-top: 8px;
  padding-top: 16px;
  border-top: 2px solid #ecf0f1;
}

.services-list {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  align-items: center;
}

.service-tag {
  display: inline-block;
  padding: 4px 12px;
  background-color: #3498db;
  color: white;
  border-radius: 16px;
  font-size: 0.85rem;
}

.service-price {
  font-weight: 600;
  color: #3498db;
}

.timestamp {
  font-size: 0.9rem;
}

.timestamp .detail-label,
.timestamp .detail-value {
  color: #95a5a6;
}

.rental-actions {
  display: flex;
  gap: 12px;
  margin-top: 20px;
  padding-top: 20px;
  border-top: 2px solid #ecf0f1;
}

.btn-complete,
.btn-cancel {
  flex: 1;
  padding: 12px;
  border: none;
  border-radius: 6px;
  font-size: 1rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
}

.btn-complete {
  background-color: #2ecc71;
  color: white;
}

.btn-complete:hover:not(:disabled) {
  background-color: #27ae60;
}

.btn-cancel {
  background-color: #e74c3c;
  color: white;
}

.btn-cancel:hover:not(:disabled) {
  background-color: #c0392b;
}

.btn-complete:disabled,
.btn-cancel:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

@media (max-width: 768px) {
  .page-header h1 {
    font-size: 2rem;
  }

  .rental-header {
    flex-direction: column;
    gap: 12px;
    align-items: flex-start;
  }

  .detail-row {
    flex-direction: column;
    gap: 4px;
  }

  .detail-label {
    min-width: auto;
  }

  .rental-actions {
    flex-direction: column;
  }
}
</style>
