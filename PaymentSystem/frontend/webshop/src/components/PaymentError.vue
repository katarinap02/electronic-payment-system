<template>
  <div class="payment-result-page">
    <div class="result-container error">
      <div class="icon-wrapper">
        <div class="error-icon">⚠</div>
      </div>
      <h1>Payment Error</h1>
      <p class="message">An unexpected error occurred while processing your payment. Please contact support if the problem persists.</p>
      
      <div v-if="errorDetails" class="error-details">
        <h3>Error Details</h3>
        <div class="detail-row">
          <span class="label">Error Code:</span>
          <span class="value">{{ errorDetails.code || 'UNKNOWN' }}</span>
        </div>
        <div class="detail-row">
          <span class="label">Message:</span>
          <span class="value">{{ errorDetails.message || 'An unexpected error occurred' }}</span>
        </div>
        <div v-if="orderDetails" class="detail-row">
          <span class="label">Order ID:</span>
          <span class="value">{{ orderDetails.orderId }}</span>
        </div>
      </div>

      <div class="actions">
        <button @click="$router.push('/vehicles')" class="btn-primary">
          Back to Shopping
        </button>
        <button @click="contactSupport" class="btn-secondary">
          Contact Support
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
const errorDetails = ref(null)

onMounted(() => {
  if (route.query.orderId) {
    orderDetails.value = {
      orderId: route.query.orderId
    }
  }
  
  errorDetails.value = {
    code: route.query.errorCode || 'UNKNOWN',
    message: route.query.errorMessage || 'An unexpected error occurred during payment processing'
  }
})

const contactSupport = () => {
  // Open email client or redirect to support page
  window.location.href = 'mailto:support@carental.com?subject=Payment Error - Order ' + (orderDetails.value?.orderId || 'Unknown')
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

.error-icon {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 100px;
  height: 100px;
  border-radius: 50%;
  background: linear-gradient(135deg, #f59e0b 0%, #d97706 100%);
  color: white;
  font-size: 60px;
  font-weight: bold;
  animation: pulse 2s ease-in-out infinite;
}

@keyframes pulse {
  0%, 100% { transform: scale(1); }
  50% { transform: scale(1.05); }
}

h1 {
  color: #f59e0b;
  font-size: 32px;
  margin-bottom: 15px;
}

.message {
  color: #6b7280;
  font-size: 16px;
  margin-bottom: 40px;
  line-height: 1.6;
}

.error-details {
  background: #fffbeb;
  border: 2px solid #fef3c7;
  border-radius: 12px;
  padding: 30px;
  margin-bottom: 40px;
  text-align: left;
}

.error-details h3 {
  color: #1f2937;
  margin-bottom: 20px;
  font-size: 18px;
}

.detail-row {
  display: flex;
  justify-content: space-between;
  padding: 12px 0;
  border-bottom: 1px solid #fef3c7;
  gap: 20px;
}

.detail-row:last-child {
  border-bottom: none;
}

.detail-row .label {
  color: #92400e;
  font-weight: 500;
  flex-shrink: 0;
}

.detail-row .value {
  color: #1f2937;
  font-weight: 600;
  text-align: right;
  word-break: break-word;
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
  background: linear-gradient(135deg, #f59e0b 0%, #d97706 100%);
  color: white;
}

.btn-primary:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(245, 158, 11, 0.4);
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
