<template>
  <div class="payment-result-page">
    <div class="result-container failed">
      <div class="icon-wrapper">
        <div class="failed-icon">✕</div>
      </div>
      <h1>Payment Failed</h1>
      <p class="message">Unfortunately, your payment could not be processed. Please try again or use a different payment method.</p>
      
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
          <span class="value status-failed">Failed</span>
        </div>
      </div>

      <div class="actions">
        <button @click="retryPayment" class="btn-primary">
          Try Again
        </button>
        <button @click="$router.push('/vehicles')" class="btn-secondary">
          Back to Shopping
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'

const route = useRoute()
const router = useRouter()
const orderDetails = ref(null)

onMounted(() => {
  if (route.query.orderId) {
    orderDetails.value = {
      orderId: route.query.orderId,
      amount: route.query.amount || 'N/A',
      currency: route.query.currency || 'EUR'
    }
  }
})

const retryPayment = () => {
  // Go back to vehicle selection
  router.push('/vehicles')
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

.failed-icon {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 100px;
  height: 100px;
  border-radius: 50%;
  background: linear-gradient(135deg, #ef4444 0%, #dc2626 100%);
  color: white;
  font-size: 60px;
  font-weight: bold;
  animation: shake 0.5s ease-out;
}

@keyframes shake {
  0%, 100% { transform: translateX(0); }
  25% { transform: translateX(-10px); }
  75% { transform: translateX(10px); }
}

h1 {
  color: #ef4444;
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

.status-failed {
  color: #ef4444;
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
  background: linear-gradient(135deg, #ef4444 0%, #dc2626 100%);
  color: white;
}

.btn-primary:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(239, 68, 68, 0.4);
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
</style>
