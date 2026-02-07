<template>
  <div class="payment-result-page">
    <div class="result-container success">
      <div class="icon-wrapper">
        <div class="success-icon">✓</div>
      </div>
      <h1>Payment Successful!</h1>
      <p class="message">Thank you for your purchase. Your payment has been processed successfully.</p>
      
      <!-- Rental creation status -->
      <div v-if="rentalCreationStatus === 'success'" class="status-message success-message">
        Your reservation has been confirmed and saved!
      </div>
      <div v-else-if="rentalCreationStatus === 'error'" class="status-message error-message">
        Payment successful but reservation could not be saved. Please contact support with your order ID.
      </div>
      <div v-else class="status-message pending-message">
        Saving your reservation...
      </div>
      
      <div v-if="orderDetails" class="order-details">
        <h3>Order Details</h3>
        <div class="detail-row">
          <span class="label">Order ID:</span>
          <span class="value">{{ orderDetails.orderId }}</span>
        </div>
        <div class="detail-row">
          <span class="label">Amount:</span>
          <span class="value">{{ orderDetails.amount }} {{ orderDetails.currency }}</span>
        </div>
        <div class="detail-row">
          <span class="label">Status:</span>
          <span class="value status-success">Completed</span>
        </div>
      </div>

      <div class="actions">
        <button @click="$router.push('/rentals')" class="btn-primary" v-if="rentalCreationStatus === 'success'">
          View My Reservations
        </button>
        <button @click="$router.push('/vehicles')" class="btn-secondary">
          Continue Shopping
        </button>
        <button @click="$router.push('/dashboard')" class="btn-secondary">
          View Dashboard
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import rentalService from '@/services/rentalService'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()
const orderDetails = ref(null)
const rentalCreationStatus = ref('pending') // pending, success, error

onMounted(async () => {
  console.log('PaymentSuccess mounted, ALL route query params:', JSON.stringify(route.query, null, 2));
  
  // Get order details from query params if available
  if (route.query.orderId) {
    orderDetails.value = {
      orderId: route.query.orderId,
      amount: route.query.amount || 'N/A',
      currency: route.query.currency || 'EUR'
    }
  }

  // Create rental record
  await createRentalFromPayment();
})

const createRentalFromPayment = async () => {
  try {
    // Proveri da li je korisnik ulogovan
    if (!authStore.user || !authStore.user.id) {
      console.warn('User not logged in, cannot create rental');
      rentalCreationStatus.value = 'error';
      return;
    }

    // Učitaj rental podatke iz sessionStorage
    const pendingRentalJson = sessionStorage.getItem('pendingRental');
    if (!pendingRentalJson) {
      console.warn('No pending rental data found in sessionStorage');
      rentalCreationStatus.value = 'error';
      return;
    }

    const pendingRental = JSON.parse(pendingRentalJson);
    console.log('Pending rental data:', pendingRental);

    // Proveri da li orderId odgovara
    if (route.query.orderId && pendingRental.orderId !== route.query.orderId) {
      console.warn('Order ID mismatch:', route.query.orderId, 'vs', pendingRental.orderId);
    }

    // Extract payment ID from route query (supports both bank and crypto payments)
    const paymentId = route.query.bankPaymentId || route.query.cryptoPaymentId || `PAY_${Date.now()}`;
    
    // Get global transaction ID (bank transaction ID or crypto txHash)
    const globalTransactionId = route.query.bankPaymentId || route.query.txHash || null;
    
    // Get payment method from query parameter
    const paymentMethod = route.query.paymentMethod || 'CreditCard';
    console.log('Payment method:', paymentMethod);
    console.log('Payment ID:', paymentId);
    console.log('Global Transaction ID:', globalTransactionId);
    
    // Konvertuj datume u ISO 8601 format sa UTC time zone
    const formatDateToUTC = (dateString) => {
      if (!dateString) return null;
      const date = new Date(dateString);
      return date.toISOString(); // Konvertuje u format: 2026-01-22T00:00:00.000Z
    };

    // Pripremi podatke za kreiranje rental zapisa
    const rentalData = {
      userId: authStore.user.id,
      vehicleId: pendingRental.vehicleId,
      startDate: formatDateToUTC(pendingRental.startDate),
      endDate: formatDateToUTC(pendingRental.endDate),
      rentalDays: pendingRental.rentalDays,
      additionalServices: pendingRental.additionalServices,
      additionalServicesPrice: pendingRental.additionalServicesPrice,
      insuranceType: pendingRental.insuranceType,
      insurancePrice: pendingRental.insurancePrice,
      vehiclePricePerDay: pendingRental.vehiclePricePerDay,
      totalPrice: pendingRental.totalPrice,
      currency: pendingRental.currency,
      paymentId: paymentId,
      globalTransactionId: globalTransactionId,
      paymentMethod: paymentMethod
    };

    console.log('Creating rental with data:', rentalData);

    // Kreiraj rental zapis preko API-ja
    const createdRental = await rentalService.createRental(rentalData);
    console.log('Rental created successfully:', createdRental);
    
    rentalCreationStatus.value = 'success';

    // Obriši pending rental iz sessionStorage
    sessionStorage.removeItem('pendingRental');
    
  } catch (error) {
    console.error('Error creating rental:', error);
    rentalCreationStatus.value = 'error';
  }
}
</script>

<style scoped>
.payment-result-page {
  min-height: 100vh;
  background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 20px;
}

.result-container {
  max-width: 600px;
  width: 100%;
  background: white;
  border-radius: 16px;
  padding: 60px 40px;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.15);
  text-align: center;
}

.icon-wrapper {
  margin-bottom: 30px;
}

.success-icon {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 100px;
  height: 100px;
  border-radius: 50%;
  background: linear-gradient(135deg, #10b981 0%, #059669 100%);
  color: white;
  font-size: 60px;
  font-weight: bold;
  animation: scaleIn 0.5s ease-out;
}

@keyframes scaleIn {
  from {
    transform: scale(0);
    opacity: 0;
  }
  to {
    transform: scale(1);
    opacity: 1;
  }
}

h1 {
  color: #10b981;
  font-size: 32px;
  margin-bottom: 15px;
}

.message {
  color: #6b7280;
  font-size: 16px;
  margin-bottom: 40px;
  line-height: 1.6;
}

.order-details {
  background: #f9fafb;
  border-radius: 12px;
  padding: 30px;
  margin-bottom: 40px;
  text-align: left;
}

.order-details h3 {
  color: #1f2937;
  margin-bottom: 20px;
  font-size: 18px;
}

.detail-row {
  display: flex;
  justify-content: space-between;
  padding: 12px 0;
  border-bottom: 1px solid #e5e7eb;
}

.detail-row:last-child {
  border-bottom: none;
}

.detail-row .label {
  color: #6b7280;
  font-weight: 500;
}

.detail-row .value {
  color: #1f2937;
  font-weight: 600;
}

.status-success {
  color: #10b981;
}

.actions {
  display: flex;
  gap: 15px;
  justify-content: center;
  flex-wrap: wrap;
}

.btn-primary,
.btn-secondary {
  padding: 14px 30px;
  border: none;
  border-radius: 8px;
  font-size: 16px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s;
}

.btn-primary {
  background: linear-gradient(135deg, #10b981 0%, #059669 100%);
  color: white;
}

.btn-primary:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(16, 185, 129, 0.4);
}

.btn-secondary {
  background: white;
  color: #6b7280;
  border: 2px solid #e5e7eb;
}

.btn-secondary:hover {
  background: #f9fafb;
  border-color: #d1d5db;
}

.status-message {
  padding: 12px 20px;
  border-radius: 8px;
  margin-bottom: 30px;
  font-weight: 600;
  font-size: 14px;
}

.success-message {
  background-color: #d1fae5;
  color: #065f46;
  border: 1px solid #10b981;
}

.error-message {
  background-color: #fee2e2;
  color: #991b1b;
  border: 1px solid #ef4444;
}

.pending-message {
  background-color: #fef3c7;
  color: #92400e;
  border: 1px solid #f59e0b;
}
</style>
