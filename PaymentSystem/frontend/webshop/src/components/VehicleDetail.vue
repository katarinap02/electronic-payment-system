<template>
  <div class="vehicle-detail-page">
    <div v-if="loading" class="loading-container">
      <div class="spinner"></div>
      <p>Loading vehicle details...</p>
    </div>

    <div v-else-if="error" class="error-container">
      <p class="error-message">{{ error }}</p>
      <button class="btn-back" @click="goBack">Go Back</button>
    </div>

    <div v-else-if="vehicle" class="vehicle-detail">
      <button class="btn-back-arrow" @click="goBack">
        &larr; Back to Vehicles
      </button>

      <div class="detail-grid">
        <div class="detail-image-section">
          <img 
            :src="vehicle.imageUrl || 'https://via.placeholder.com/600x400?text=No+Image'" 
            :alt="`${vehicle.brand} ${vehicle.model}`"
          />
        </div>

        <div class="detail-info-section">
          <div class="detail-header">
            <div>
              <span class="category-badge">{{ vehicle.category }}</span>
              <h1>{{ vehicle.brand }} {{ vehicle.model }}</h1>
              <p class="year">Year: {{ vehicle.year }}</p>
            </div>
            <span class="status-badge" :class="getStatusClass(vehicle.status)">
              {{ vehicle.status }}
            </span>
          </div>

          <div class="price-section">
            <div class="price">
              <span class="price-amount">€{{ vehicle.pricePerDay.toFixed(2) }}</span>
              <span class="price-label">per day</span>
            </div>
          </div>

          <div class="specs-section">
            <h2>Specifications</h2>
            <div class="specs-grid">
              <div class="spec-item">
                <span class="spec-label">Transmission</span>
                <span class="spec-value">{{ vehicle.transmission }}</span>
              </div>
              <div class="spec-item">
                <span class="spec-label">Fuel Type</span>
                <span class="spec-value">{{ vehicle.fuelType }}</span>
              </div>
              <div class="spec-item">
                <span class="spec-label">Seats</span>
                <span class="spec-value">{{ vehicle.seats }} passengers</span>
              </div>
              <div class="spec-item">
                <span class="spec-label">Mileage</span>
                <span class="spec-value">{{ formatMileage(vehicle.mileage) }}</span>
              </div>
              <div class="spec-item">
                <span class="spec-label">Color</span>
                <span class="spec-value">{{ vehicle.color }}</span>
              </div>
              <div class="spec-item">
                <span class="spec-label">License Plate</span>
                <span class="spec-value">{{ vehicle.licensePlate }}</span>
              </div>
            </div>
          </div>

          <div v-if="vehicle.description" class="description-section">
            <h2>Description</h2>
            <p>{{ vehicle.description }}</p>
          </div>

          <div class="action-section">
            <button 
              class="btn-rent" 
              @click="openRentalModal"
              :disabled="vehicle.status !== 'Available' || !isAuthenticated"
            >
              {{ !isAuthenticated ? 'Login to Add to Cart' : (vehicle.status === 'Available' ? 'Add to Cart' : 'Not Available') }}
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Rental Modal -->
    <RentalModal
      :is-open="isModalOpen"
      :vehicle="vehicle"
      @close="closeRentalModal"
      @confirm="handleRentalConfirm"
    />

    <!-- Success Notification -->
    <Transition name="notification">
      <div v-if="showSuccessNotification" class="success-notification">
        <div class="notification-content">
          <span class="icon">✓</span>
          <span class="message">Vehicle added to cart successfully!</span>
        </div>
      </div>
    </Transition>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useAuthStore } from '../stores/auth';
import { vehicleApi } from '../services/vehicleService';
import { pspService } from '../services/pspService';
import RentalModal from './RentalModal.vue';

const route = useRoute();
const router = useRouter();
const authStore = useAuthStore();

const vehicle = ref(null);
const loading = ref(false);
const error = ref(null);
const isModalOpen = ref(false);
const showSuccessNotification = ref(false);

// Computed property za reaktivno praćenje autentifikacije
const isAuthenticated = computed(() => authStore.isAuthenticated);

const fetchVehicleDetails = async () => {
  loading.value = true;
  error.value = null;

  try {
    const response = await vehicleApi.getVehicleById(route.params.id);
    vehicle.value = response.data.data || response.data;
  } catch (err) {
    console.error('Error fetching vehicle details:', err);
    error.value = 'Failed to load vehicle details. Please try again.';
  } finally {
    loading.value = false;
  }
};

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

const goBack = () => {
  router.push('/vehicles');
};

const openRentalModal = () => {
  console.log('openRentalModal called - isAuthenticated:', isAuthenticated.value);
  
  // Provera da li je korisnik ulogovan
  if (!isAuthenticated.value) {
    alert('Please login to add vehicles to cart.');
    router.push('/login');
    return;
  }
  
  isModalOpen.value = true;
};

const closeRentalModal = () => {
  isModalOpen.value = false;
};

const handleRentalConfirm = async (rentalData) => {
  console.log('Rental confirmed:', rentalData);
  
  try {
    // Generate unique order ID
    const orderId = `ORDER-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
    
    // Determine currency code for display
    const currencyCode = rentalData.currency === 0 ? 'EUR' : (rentalData.currency === 1 ? 'USD' : 'RSD');
    
    // Convert USD display amount back to EUR for bank payment (all bank accounts are in EUR)
    // User selected USD and sees 115 USD, but bank needs 100 EUR
    const paymentAmount = rentalData.currency === 1 
      ? rentalData.totals.total / 1.15  // Convert USD back to EUR (115 USD / 1.15 = 100 EUR)
      : rentalData.totals.total;        // EUR - no conversion needed
    
    // Sačuvaj rental podatke u sessionStorage za kasnije kreiranje rental zapisa
    const rentalInfo = {
      orderId: orderId,
      vehicleId: vehicle.value.id,
      startDate: rentalData.startDate,
      endDate: rentalData.endDate,
      rentalDays: rentalData.days,
      insuranceId: rentalData.insurance?.id || null,
      insuranceType: rentalData.insurance?.name || null,
      insurancePrice: rentalData.insurance ? rentalData.insurance.pricePerDay * rentalData.days : 0,
      additionalServiceIds: rentalData.services?.map(s => s.id) || [],
      additionalServices: rentalData.services?.map(s => s.name) || [],
      additionalServicesPrice: rentalData.totals.services,
      vehiclePricePerDay: vehicle.value.pricePerDay,
      totalPrice: rentalData.totals.total, // Keep USD display amount for user
      currency: currencyCode,
      timestamp: Date.now()
    };
    
    sessionStorage.setItem('pendingRental', JSON.stringify(rentalInfo));
    console.log('Rental info saved to sessionStorage:', rentalInfo);
    console.log('Display amount:', rentalData.totals.total, currencyCode);
    console.log('Payment amount (EUR for bank):', paymentAmount, 'EUR');
    
    // Proveri da li je korisnik ulogovan (kao u PaymentSuccess.vue)
    if (!authStore.user || !authStore.user.id) {
      console.error('User not logged in, cannot initialize payment');
      alert('Please login to continue with payment');
      router.push('/login');
      return;
    }
    
    // Debug: Log user info
    console.log('✅ Auth Store User:', authStore.user);
    console.log('✅ User ID:', authStore.user.id);
    
    // Dobij Customer ID (kao što se dobija userId u PaymentSuccess.vue)
    const customerId = authStore.user.id.toString();
    console.log('✅ Customer ID to send:', customerId);
    
    // Initialize payment with PSP
    // For EUR: send 100 EUR
    // For USD: send 100 EUR for transaction, but PSP should display 115 USD to user
    const paymentResponse = await pspService.initializePayment({
      orderId: orderId,
      customerId: customerId,  // Customer ID from auth store (same as userId in PaymentSuccess)
      amount: paymentAmount,            // EUR amount for bank transaction (100 EUR)
      currency: 0,                      // Always EUR (0) for bank - all accounts are in EUR
      displayAmount: rentalData.totals.total,  // Display amount (115 USD or 100 EUR)
      displayCurrency: rentalData.currency     // Display currency (0=EUR, 1=USD)
    });
    
    console.log('Payment initialized:', paymentResponse);
    
    // Close modal
    isModalOpen.value = false;
    
    // Redirect to PSP payment page (URL already contains access token)
    window.location.href = paymentResponse.paymentUrl;
    
  } catch (error) {
    console.error('Payment initialization failed:', error);
    alert('Failed to initialize payment. Please try again.');
    isModalOpen.value = false;
  }
};

onMounted(() => {
  fetchVehicleDetails();
});
</script>

<style scoped>
.vehicle-detail-page {
  max-width: 1200px;
  margin: 0 auto;
  padding: 40px 20px;
}

.btn-back-arrow {
  padding: 10px 20px;
  background: white;
  color: #4b5563;
  border: 1px solid #e5e7eb;
  border-radius: 8px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s;
  margin-bottom: 30px;
  font-size: 15px;
}

.btn-back-arrow:hover {
  background: #f9fafb;
  border-color: #d1d5db;
}

.detail-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 40px;
  background: white;
  border-radius: 12px;
  padding: 40px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.detail-image-section img {
  width: 100%;
  height: auto;
  border-radius: 12px;
  object-fit: cover;
}

.detail-info-section {
  display: flex;
  flex-direction: column;
  gap: 30px;
}

.detail-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
}

.detail-header h1 {
  font-size: 32px;
  margin: 12px 0 8px 0;
  color: #1f2937;
}

.year {
  color: #6b7280;
  font-size: 16px;
}

.category-badge {
  display: inline-block;
  padding: 6px 14px;
  background: #e0e7ff;
  color: #4f46e5;
  border-radius: 12px;
  font-size: 13px;
  font-weight: 600;
}

.status-badge {
  padding: 8px 16px;
  border-radius: 20px;
  font-size: 13px;
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

.price-section {
  padding: 24px;
  background: #f9fafb;
  border-radius: 12px;
}

.price {
  display: flex;
  align-items: baseline;
  gap: 8px;
}

.price-amount {
  font-size: 42px;
  font-weight: 700;
  color: #4f46e5;
}

.price-label {
  font-size: 18px;
  color: #6b7280;
}

.specs-section h2,
.description-section h2 {
  font-size: 20px;
  margin-bottom: 16px;
  color: #1f2937;
}

.specs-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 20px;
}

.spec-item {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.spec-label {
  font-size: 13px;
  color: #6b7280;
  font-weight: 500;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.spec-value {
  font-size: 16px;
  color: #1f2937;
  font-weight: 600;
}

.description-section p {
  color: #4b5563;
  line-height: 1.7;
  font-size: 15px;
}

.action-section {
  padding-top: 20px;
  border-top: 1px solid #e5e7eb;
}

.btn-rent {
  width: 100%;
  padding: 16px;
  background: #4f46e5;
  color: white;
  border: none;
  border-radius: 10px;
  font-weight: 600;
  font-size: 18px;
  cursor: pointer;
  transition: background 0.3s;
}

.btn-rent:hover:not(:disabled) {
  background: #4338ca;
}

.btn-rent:disabled {
  background: #9ca3af;
  cursor: not-allowed;
}

.loading-container,
.error-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 80px 20px;
  gap: 20px;
}

.spinner {
  width: 50px;
  height: 50px;
  border: 4px solid #e5e7eb;
  border-top-color: #4f46e5;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.error-message {
  color: #ef4444;
  font-size: 18px;
}

.btn-back {
  padding: 12px 24px;
  background: #4f46e5;
  color: white;
  border: none;
  border-radius: 8px;
  font-weight: 600;
  cursor: pointer;
}

.btn-back:hover {
  background: #4338ca;
}

/* Success Notification */
.success-notification {
  position: fixed;
  top: 20px;
  right: 20px;
  z-index: 10000;
}

.notification-content {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 16px 24px;
  background: #10b981;
  color: white;
  border-radius: 12px;
  box-shadow: 0 10px 30px rgba(16, 185, 129, 0.3);
  font-weight: 600;
  font-size: 15px;
}

.notification-content .icon {
  width: 24px;
  height: 24px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: rgba(255, 255, 255, 0.3);
  border-radius: 50%;
  font-weight: bold;
}

/* Notification transitions */
.notification-enter-active,
.notification-leave-active {
  transition: all 0.3s ease;
}

.notification-enter-from {
  opacity: 0;
  transform: translateX(100px);
}

.notification-leave-to {
  opacity: 0;
  transform: translateY(-20px);
}

@media (max-width: 968px) {
  .detail-grid {
    grid-template-columns: 1fr;
    padding: 24px;
  }

  .detail-header {
    flex-direction: column;
    gap: 16px;
  }

  .specs-grid {
    grid-template-columns: 1fr;
  }

  .success-notification {
    top: 10px;
    right: 10px;
    left: 10px;
  }
}
</style>
