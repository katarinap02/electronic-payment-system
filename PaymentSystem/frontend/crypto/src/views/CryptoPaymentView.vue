<template>
  <div class="crypto-payment-page">
    <div class="container">
      <div class="header">
        <h1> Crypto Payment</h1>
        <p>Pay securely with Ethereum (ETH)</p>
      </div>

      <div v-if="loading" class="loading-state">
        <div class="spinner"></div>
        <p>Loading payment details...</p>
      </div>

      <div v-else-if="error" class="error-state">
        <div class="error-icon"></div>
        <h2>Error</h2>
        <p>{{ error }}</p>
        <button @click="goBack" class="btn-back">Go Back</button>
      </div>

      <!-- Success State -->
      <div v-else-if="payment && (payment.status === 'COMPLETED' || payment.status === 'CAPTURED')" class="success-state">
        <div class="success-icon">✓</div>
        <h2>Payment Successful!</h2>
        <p>Your crypto payment has been confirmed</p>
        
        <div class="success-details">
          <div class="detail-row">
            <span class="label">Amount Paid:</span>
            <span class="value">{{ payment.amountInEth }} ETH (€{{ payment.amount }})</span>
          </div>
          <div class="detail-row">
            <span class="label">Merchant:</span>
            <span class="value">{{ payment.merchantName }}</span>
          </div>
          <div v-if="payment.txHash" class="detail-row">
            <span class="label">Transaction Hash:</span>
            <a :href="`https://sepolia.etherscan.io/tx/${payment.txHash}`" target="_blank" class="tx-link-small">
              {{ payment.txHash.substring(0, 20) }}...
            </a>
          </div>
        </div>

        <button @click="redirectToWebshop('success', payment.txHash)" class="btn-return">
          Return to WebShop
        </button>
      </div>

      <!-- Failed State -->
      <div v-else-if="payment && (payment.status === 'EXPIRED' || payment.status === 'FAILED' || payment.status === 'CANCELLED')" class="failed-state">
        <div class="failed-icon">✕</div>
        <h2>Payment {{ payment.status === 'EXPIRED' ? 'Expired' : payment.status === 'CANCELLED' ? 'Cancelled' : 'Failed' }}</h2>
        <p>{{ payment.status === 'EXPIRED' ? 'The payment time has expired' : payment.status === 'CANCELLED' ? 'You cancelled the payment' : 'There was an issue processing your payment' }}</p>
        
        <button @click="redirectToWebshop('failed', payment.txHash)" class="btn-return">
          Return to WebShop
        </button>
      </div>

      <!-- Payment In Progress -->
      <div v-else-if="payment" class="payment-content">
        <!-- Timer -->
        <CountdownTimer 
          v-if="payment.expiresAt && !isCompleted"
          :expiresAt="payment.expiresAt"
          @expired="handleExpired"
        />

        <!-- Status -->
        <PaymentStatus 
          :status="payment.status"
          :confirmations="payment.confirmations"
          :requiredConfirmations="1"
          :txHash="payment.txHash"
        />

        <!-- Payment Info -->
        <PaymentInfo 
          v-if="!isCompleted"
          :merchantName="payment.merchantName"
          :amount="payment.amount"
          :amountInCrypto="payment.amountInEth"
          :exchangeRate="payment.exchangeRate || 1680"
          :walletAddress="payment.walletAddress"
        />

        <!-- QR Code -->
        <QRCodeDisplay 
          v-if="!isCompleted && payment.walletAddress"
          :walletAddress="payment.walletAddress"
          :amountInWei="calculateWei(payment.amountInEth)"
          :chainId="11155111"
        />

        <!-- Instructions -->
        <div v-if="!isCompleted" class="instructions">
          <h3> How to Pay</h3>
          <ol>
            <li>Open your MetaMask or Ethereum wallet</li>
            <li>Scan the QR code above OR copy the wallet address</li>
            <li>Send exactly <strong>{{ payment.amountInEth }} ETH</strong></li>
            <li>Wait for confirmation (this page will auto-update)</li>
          </ol>
        </div>

        <!-- Transaction Info -->
        <div v-if="payment.txHash" class="tx-info">
          <h4>Transaction Hash:</h4>
          <a :href="`https://sepolia.etherscan.io/tx/${payment.txHash}`" target="_blank" class="tx-link">
            {{ payment.txHash }}
          </a>
        </div>

        <!-- Cancel Button -->
        <button v-if="!isCompleted && payment.status === 'PENDING'" @click="cancelPayment" class="btn-cancel">
          Cancel Payment
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useRoute } from 'vue-router'
import cryptoApi from '../services/cryptoApi'
import PaymentInfo from '../components/PaymentInfo.vue'
import QRCodeDisplay from '../components/QRCodeDisplay.vue'
import CountdownTimer from '../components/CountdownTimer.vue'
import PaymentStatus from '../components/PaymentStatus.vue'

const route = useRoute()
const payment = ref(null)
const loading = ref(true)
const error = ref(null)
let pollingInterval = null

const cryptoPaymentId = computed(() => route.params.cryptoPaymentId)

const isCompleted = computed(() => {
  return payment.value && ['CAPTURED', 'COMPLETED', 'EXPIRED', 'FAILED', 'CANCELLED'].includes(payment.value.status)
})

const loadPaymentDetails = async () => {
  try {
    const data = await cryptoApi.getTransactionStatus(cryptoPaymentId.value)
    payment.value = data
    loading.value = false
  } catch (err) {
    console.error('Error loading payment:', err)
    error.value = err.response?.data?.message || 'Failed to load payment details'
    loading.value = false
  }
}

const startPolling = () => {
  pollingInterval = setInterval(async () => {
    if (!isCompleted.value) {
      await loadPaymentDetails()
    } else {
      stopPolling()
    }
  }, 10000) // Poll every 10 seconds
}

const stopPolling = () => {
  if (pollingInterval) {
    clearInterval(pollingInterval)
    pollingInterval = null
  }
}

const calculateWei = (ethAmount) => {
  // Convert ETH to Wei (1 ETH = 10^18 Wei)
  const wei = Math.floor(ethAmount * 1e18)
  return wei.toString()
}

const handleExpired = () => {
  error.value = 'Payment time has expired'
  setTimeout(() => {
    redirectToWebshop('failed', null)
  }, 2000)
}

const cancelPayment = async () => {
  if (confirm('Are you sure you want to cancel this payment?')) {
    try {
      await cryptoApi.cancelPayment(cryptoPaymentId.value)
      redirectToWebshop('cancelled', null)
    } catch (err) {
      console.error('Error cancelling payment:', err)
      alert('Failed to cancel payment')
    }
  }
}

const redirectToWebshop = (status, txHash) => {
  // Redirect directly to webshop after payment
  const webshopUrl = import.meta.env.VITE_WEBSHOP_FRONTEND_URL || 'http://localhost:5173'
  
  // Determine target page based on status
  const isSuccess = status === 'success' || status === 'completed'
  let callbackUrl = `${webshopUrl}/payment-${isSuccess ? 'success' : 'failed'}`
  
  // Build query parameters
  const params = new URLSearchParams()
  
  // Add orderId from payment object (sent by PSP)
  if (payment.value?.merchantOrderId) {
    params.append('orderId', payment.value.merchantOrderId)
  }
  
  // Add crypto payment details
  params.append('cryptoPaymentId', cryptoPaymentId.value)
  params.append('paymentMethod', 'CRYPTO')
  
  if (txHash) {
    params.append('txHash', txHash)
  }
  
  if (payment.value?.amount) {
    params.append('amount', payment.value.amount)
  }
  
  if (payment.value?.currency) {
    params.append('currency', payment.value.currency)
  }
  
  callbackUrl += `?${params.toString()}`
  console.log('Redirecting to webshop:', callbackUrl)
  
  window.location.href = callbackUrl
}

const goBack = () => {
  window.history.back()
}

onMounted(() => {
  loadPaymentDetails()
  startPolling()
})

onUnmounted(() => {
  stopPolling()
})
</script>

<style scoped>
.crypto-payment-page {
  min-height: 100vh;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  padding: 40px 20px;
}

.container {
  max-width: 800px;
  margin: 0 auto;
}

.header {
  text-align: center;
  color: white;
  margin-bottom: 40px;
}

.header h1 {
  font-size: 36px;
  margin-bottom: 10px;
}

.header p {
  font-size: 18px;
  opacity: 0.9;
}

.loading-state {
  background: white;
  border-radius: 16px;
  padding: 60px;
  text-align: center;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.2);
}

.spinner {
  width: 60px;
  height: 60px;
  border: 5px solid #f3f4f6;
  border-top: 5px solid #3b82f6;
  border-radius: 50%;
  margin: 0 auto 20px;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.error-state {
  background: white;
  border-radius: 16px;
  padding: 60px;
  text-align: center;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.2);
}

.error-icon {
  font-size: 64px;
  margin-bottom: 20px;
}

.error-state h2 {
  color: #dc2626;
  margin-bottom: 15px;
}

.error-state p {
  color: #6b7280;
  margin-bottom: 30px;
}

.success-state {
  background: white;
  border-radius: 16px;
  padding: 60px;
  text-align: center;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.2);
  animation: fadeIn 0.5s;
}

.success-icon {
  width: 80px;
  height: 80px;
  background: #10b981;
  border-radius: 50%;
  margin: 0 auto 20px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 48px;
  color: white;
  animation: scaleIn 0.5s ease-out;
}

@keyframes scaleIn {
  0% { transform: scale(0); }
  50% { transform: scale(1.1); }
  100% { transform: scale(1); }
}

.success-state h2 {
  color: #10b981;
  margin-bottom: 15px;
  font-size: 28px;
}

.success-state > p {
  color: #6b7280;
  margin-bottom: 30px;
  font-size: 16px;
}

.success-details {
  background: #f9fafb;
  border-radius: 12px;
  padding: 25px;
  margin-bottom: 30px;
  text-align: left;
}

.detail-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
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

.tx-link-small {
  color: #3b82f6;
  text-decoration: none;
  font-family: monospace;
  font-size: 13px;
}

.tx-link-small:hover {
  text-decoration: underline;
}

.failed-state {
  background: white;
  border-radius: 16px;
  padding: 60px;
  text-align: center;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.2);
  animation: fadeIn 0.5s;
}

.failed-icon {
  width: 80px;
  height: 80px;
  background: #ef4444;
  border-radius: 50%;
  margin: 0 auto 20px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 48px;
  color: white;
  animation: scaleIn 0.5s ease-out;
}

.failed-state h2 {
  color: #ef4444;
  margin-bottom: 15px;
  font-size: 28px;
}

.failed-state p {
  color: #6b7280;
  margin-bottom: 30px;
  font-size: 16px;
}

.btn-return {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  border: none;
  padding: 14px 40px;
  border-radius: 8px;
  cursor: pointer;
  font-weight: 600;
  font-size: 16px;
  transition: all 0.3s;
  box-shadow: 0 4px 15px rgba(102, 126, 234, 0.3);
}

.btn-return:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 25px rgba(102, 126, 234, 0.5);
}

.btn-back,
.btn-cancel {
  background: #667eea;
  color: white;
  border: none;
  padding: 12px 30px;
  border-radius: 8px;
  cursor: pointer;
  font-weight: 600;
  font-size: 16px;
  transition: all 0.3s;
}

.btn-back:hover,
.btn-cancel:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(102, 126, 234, 0.4);
}

.btn-cancel {
  background: #ef4444;
  width: 100%;
  margin-top: 20px;
}

.btn-cancel:hover {
  background: #dc2626;
  box-shadow: 0 6px 20px rgba(239, 68, 68, 0.4);
}

.payment-content {
  animation: fadeIn 0.5s;
}

@keyframes fadeIn {
  from { opacity: 0; transform: translateY(20px); }
  to { opacity: 1; transform: translateY(0); }
}

.instructions {
  background: white;
  border-radius: 16px;
  padding: 30px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
  margin-bottom: 20px;
}

.instructions h3 {
  color: #1f2937;
  margin-bottom: 20px;
  font-size: 20px;
}

.instructions ol {
  color: #4b5563;
  line-height: 1.8;
  padding-left: 20px;
}

.instructions li {
  margin-bottom: 10px;
}

.instructions strong {
  color: #3b82f6;
}

.tx-info {
  background: white;
  border-radius: 16px;
  padding: 20px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
  margin-bottom: 20px;
}

.tx-info h4 {
  color: #1f2937;
  margin-bottom: 10px;
  font-size: 16px;
}

.tx-link {
  color: #3b82f6;
  word-break: break-all;
  text-decoration: none;
  font-family: monospace;
  font-size: 14px;
}

.tx-link:hover {
  text-decoration: underline;
}
</style>
