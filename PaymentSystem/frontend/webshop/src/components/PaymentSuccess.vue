<template>
  <div class="payment-result-page">
    <div class="result-container success">
      <div class="icon-wrapper">
        <div class="success-icon">✓</div>
      </div>
      <h1>Payment Successful!</h1>
      <p class="message">Thank you for your purchase. Your payment has been processed successfully.</p>
      
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
        <button @click="$router.push('/vehicles')" class="btn-primary">
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
import { useRoute } from 'vue-router'

const route = useRoute()
const orderDetails = ref(null)

onMounted(() => {
  // Get order details from query params if available
  if (route.query.orderId) {
    orderDetails.value = {
      orderId: route.query.orderId,
      amount: route.query.amount || 'N/A',
      currency: route.query.currency || 'EUR'
    }
  }
})
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
</style>
