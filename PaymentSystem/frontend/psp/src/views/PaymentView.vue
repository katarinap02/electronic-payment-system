<template>
  <div class="payment-page">
    <div v-if="loading" class="loading">Loading payment details...</div>

    <div v-else-if="error" class="error-state">
      <h2>⚠️ Error</h2>
      <p>{{ error }}</p>
      <button @click="goBackToShop" class="btn-back">Go Back</button>
    </div>

    <div v-else-if="payment" class="payment-container">
      <div class="payment-header">
        <h1>Complete Your Payment</h1>
        <p class="merchant-name">{{ payment.webShopName }}</p>
      </div>

      <div class="payment-details">
        <div class="detail-card">
          <h3>Order Details</h3>
          <div class="detail-row">
            <span class="label">Order ID:</span>
            <span class="value">{{ payment.merchantOrderId }}</span>
          </div>
          <div class="detail-row">
            <span class="label">Amount:</span>
            <span class="value amount">{{ payment.amount }} {{ payment.currency }}</span>
          </div>
          <div class="detail-row">
            <span class="label">Status:</span>
            <span :class="['status-badge', payment.status.toLowerCase()]">{{ payment.status }}</span>
          </div>
        </div>

        <div v-if="payment.status === 'Pending'" class="payment-method-selection">
          <h3>Select Payment Method</h3>
          <p class="selection-hint">Choose how you want to pay</p>

          <div v-if="payment.availablePaymentMethods && payment.availablePaymentMethods.length > 0" class="payment-methods">
            <button 
              v-for="method in payment.availablePaymentMethods"
              :key="method.id"
              class="payment-method-btn"
              @click="selectPaymentMethod(method.id)"
              :disabled="selecting"
            >
              <div class="method-info">
                <h4>{{ method.name }}</h4>
                <p>{{ method.type }} payment</p>
              </div>
              <div class="method-arrow">→</div>
            </button>
          </div>
          <div v-else class="no-methods">
            <p>⚠️ No payment methods available for this merchant</p>
          </div>
        </div>
        
        <div v-else-if="payment.status === 'Failed'" class="payment-failed-message">
          <h3>❌ Payment Failed</h3>
          <p>This payment transaction has been cancelled or failed.</p>
          <a href="http://localhost:5173/vehicles" class="btn-return">Return to Shop</a>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import axios from 'axios'
import { useAuthStore } from '@/stores/auth'

const auth = useAuthStore()
const route = useRoute()

const payment = ref(null)
const loading = ref(false)
const error = ref(null)
const selecting = ref(false)

const loadPaymentDetails = async () => {
  loading.value = true
  error.value = null

  auth.logout()

  try {
    const token = route.query.token

    if (!token || !/^[a-f0-9]{32}$/i.test(token)) {
      error.value = 'Invalid or missing access token'
      loading.value = false
      return
    }

    const response = await axios.get(`http://localhost:5002/api/payments/${route.params.id}?token=${token}`)
    console.log('Payment details loaded:', response.data)
    payment.value = response.data
  } catch (err) {
    console.error('Failed to load payment:', err)
    error.value = err.response?.data?.message || 'Failed to load payment details'
    // If unauthorized, redirect to error page
    debugger
    if (err.response?.status === 401) {
      window.location.href = 'http://localhost:5173/payment-error?errorCode=UNAUTHORIZED&errorMessage=Invalid or expired payment link'
    }
  } finally {
    loading.value = false
  }
}

const selectPaymentMethod = async (paymentMethodId) => {
  selecting.value = true
  
  try {
    // Step 1: Select payment method on PSP
    const pspResponse = await axios.post(`http://localhost:5002/api/payments/${route.params.id}/select-method`, {
      paymentMethodId: paymentMethodId
    })
    
    console.log('PSP Response:', pspResponse.data)
    
    // // Step 2: Call Bank API directly to get payment form URL
    // const bankRequest = {
    //   merchantId: pspResponse.data.merchantId,
    //   amount: pspResponse.data.amount,
    //   currency: pspResponse.data.currency,
    //   stan: pspResponse.data.stan,
    //   pspTimestamp: pspResponse.data.pspTimestamp,
    //   successUrl: pspResponse.data.successUrl,
    //   failedUrl: pspResponse.data.failedUrl,
    //   errorUrl: pspResponse.data.errorUrl
    // }
    
    // console.log('Bank Request:', bankRequest)
    
    // const bankResponse = await axios.post('http://localhost:5001/api/payment/initiate', bankRequest)
    
    // console.log('Bank Response:', bankResponse.data)
    
    // Step 3: Redirect to payment provider (Bank, PayPal, or Crypto)
    const redirectUrl = pspResponse.data.bankPaymentUrl || pspResponse.data.approvalUrl || pspResponse.data.cryptoPaymentUrl || pspResponse.data.redirectUrl
    if (redirectUrl) {
      window.location.href = redirectUrl
    } else {
      throw new Error('Payment URL not received')
    }

  } catch (err) {
    console.error('Failed to select payment method:', err)
    console.error('Error details:', err.response?.data)
    alert(err.response?.data?.error || err.response?.data?.message || err.message || 'Failed to select payment method')
    selecting.value = false
  }
}

onMounted(async () => {
  const urlToken = route.query.token // Može biti PSP token (32 hex) ili PayPal token (PAYID-...)
  const isPspToken = /^[a-f0-9]{32}$/i.test(urlToken || '')
  
  // Bank params
  const status = route.query.status
  const bankPaymentId = route.query.bankPaymentId
  
  // PayPal params  
  const payPalPayerId = route.query.PayerID
  
  // 1. Bank callback (vratio se sa banke)
  if (status && bankPaymentId) {
    try {
      const response = await axios.get(`http://localhost:5002/api/payments/${route.params.id}/bank-callback`, {
        params: { status, paymentId: bankPaymentId }
      })
      
      if (response.data && response.data.redirectUrl) {
        window.location.href = response.data.redirectUrl
        return
      }
    } catch (err) {
      console.error('Failed to process bank callback:', err)
      error.value = 'Failed to process payment result'
    }
  }
  
  // 2. PayPal Success (nije PSP token [PAYID-...] + ima PayerID)
  else if (!isPspToken && payPalPayerId) {
    try {
      const response = await axios.get(`http://localhost:5002/api/payments/${route.params.id}/paypal-callback`, {
        params: { token: urlToken, payerId: payPalPayerId }
      })
      
      if (response.data && response.data.redirectUrl) {
        window.location.href = response.data.redirectUrl
        return
      }
    } catch (err) {
      console.error('Failed to process PayPal callback:', err)
      error.value = 'Failed to process PayPal payment'
    }
  }

  // 3. PayPal Cancel (nije PSP token + nema PayerID)
  else if (!isPspToken && urlToken && !payPalPayerId) {
    error.value = 'Payment was cancelled by user'
    loading.value = false
    
    try {
      await axios.post(`http://localhost:5002/api/payments/${route.params.id}/cancel`)
    } catch (err) {
      console.error('Failed to cancel payment:', err)
    }
  }
  
  // 4. Crypto Callback (ima status i txHash parametar)
  else if (route.query.status && route.query.txHash) {
    try {
      const cryptoStatus = route.query.status
      const txHash = route.query.txHash
      
      const response = await axios.get(`http://localhost:5002/api/payments/${route.params.id}/crypto-callback`, {
        params: { status: cryptoStatus, txHash: txHash }
      })
      
      if (response.data && response.data.redirectUrl) {
        window.location.href = response.data.redirectUrl
        return
      }
    } catch (err) {
      console.error('Failed to process Crypto callback:', err)
      error.value = 'Failed to process Crypto payment'
    }
  }
  
  // 5. Normalno učitavanje (ima validan PSP token - 32 hex)
  else if (isPspToken) {
    await loadPaymentDetails()
  }
  
  // 6. Nema tokena ili nešto ne valja
  else {
    error.value = 'Invalid or missing payment link'
    loading.value = false
  }
})
const goBackToShop = () => {
  window.location.href = 'http://localhost:5173/vehicles'
}
</script>

<style scoped>
.payment-page {
  min-height: 100vh;
  background: #f3f4f6;
  padding: 40px 20px;
}

.loading {
  text-align: center;
  color: #4b5563;
  font-size: 18px;
  padding: 100px 20px;
}

.error-state {
  max-width: 500px;
  margin: 100px auto;
  background: white;
  padding: 40px;
  border-radius: 16px;
  text-align: center;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.2);
}

.error-state h2 {
  color: #dc2626;
  margin-bottom: 20px;
}

.error-state p {
  color: #6b7280;
  margin-bottom: 30px;
}

.btn-back {
  background: #667eea;
  color: white;
  border: none;
  padding: 12px 30px;
  border-radius: 8px;
  cursor: pointer;
  font-weight: 600;
  transition: all 0.3s;
}

.btn-back:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(102, 126, 234, 0.4);
}

.payment-container {
  max-width: 700px;
  margin: 0 auto;
}

.payment-header {
  text-align: center;
  margin-bottom: 40px;
}

.payment-header h1 {
  color: #1f2937;
  font-size: 32px;
  margin-bottom: 10px;
}

.merchant-name {
  color: #6b7280;
  font-size: 18px;
  font-weight: 500;
}

.payment-details {
  background: white;
  border-radius: 16px;
  padding: 30px;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.2);
}

.detail-card {
  margin-bottom: 40px;
  padding-bottom: 30px;
  border-bottom: 2px solid #f3f4f6;
}

.detail-card h3 {
  color: #1f2937;
  margin-bottom: 20px;
  font-size: 20px;
}

.detail-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 12px 0;
}

.detail-row .label {
  color: #6b7280;
  font-weight: 500;
}

.detail-row .value {
  color: #1f2937;
  font-weight: 600;
}

.detail-row .value.amount {
  font-size: 24px;
  color: #667eea;
}

.status-badge {
  padding: 6px 16px;
  border-radius: 20px;
  font-size: 14px;
  font-weight: 600;
  text-transform: capitalize;
}

.status-badge.pending {
  background-color: #fef3c7;
  color: #92400e;
}

.status-badge.processing {
  background-color: #dbeafe;
  color: #1e40af;
}

.status-badge.success {
  background-color: #d1fae5;
  color: #065f46;
}

.status-badge.failed,
.status-badge.error {
  background-color: #fee2e2;
  color: #991b1b;
}

.payment-method-selection h3 {
  color: #1f2937;
  margin-bottom: 10px;
  font-size: 20px;
}

.selection-hint {
  color: #6b7280;
  margin-bottom: 20px;
}

.payment-methods {
  display: flex;
  flex-direction: column;
  gap: 15px;
}

.payment-method-btn {
  display: flex;
  align-items: center;
  gap: 20px;
  padding: 20px;
  background: #f9fafb;
  border: 2px solid #e5e7eb;
  border-radius: 12px;
  cursor: pointer;
  transition: all 0.3s;
  text-align: left;
  width: 100%;
}

.payment-method-btn:hover {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border-color: #667eea;
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(102, 126, 234, 0.3);
}

.payment-method-btn:hover .method-icon,
.payment-method-btn:hover .method-info h4,
.payment-method-btn:hover .method-info p,
.payment-method-btn:hover .method-arrow {
  color: white;
}

.payment-method-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.no-methods {
  padding: 30px;
  text-align: center;
  background: #fef3c7;
  border: 2px solid #fbbf24;
  border-radius: 12px;
  color: #92400e;
}

.method-icon {
  font-size: 40px;
  flex-shrink: 0;
}

.method-info {
  flex: 1;
}

.method-info h4 {
  color: #1f2937;
  margin: 0 0 5px 0;
  font-size: 18px;
  font-weight: 600;
}

.method-info p {
  color: #6b7280;
  margin: 0;
  font-size: 14px;
}

.method-arrow {
  font-size: 24px;
  color: #9ca3af;
  font-weight: bold;
}

.payment-failed-message {
  background: #fee2e2;
  border: 2px solid #fca5a5;
  border-radius: 12px;
  padding: 40px;
  text-align: center;
}

.payment-failed-message h3 {
  color: #991b1b;
  margin-bottom: 15px;
  font-size: 20px;
}

.payment-failed-message p {
  color: #6b7280;
  margin-bottom: 25px;
}

.btn-return {
  display: inline-block;
  padding: 12px 30px;
  background: linear-gradient(135deg, #ef4444 0%, #dc2626 100%);
  color: white;
  text-decoration: none;
  border-radius: 8px;
  font-weight: 600;
  transition: all 0.3s;
}

.btn-return:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(239, 68, 68, 0.4);
}
</style>
