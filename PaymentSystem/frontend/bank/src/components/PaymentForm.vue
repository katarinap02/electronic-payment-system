<template>
  <div class="payment-form">
    <h2>Payment</h2>
    <!-- Logo-i kartica -->
<div class="card-logos">
  <img src="https://upload.wikimedia.org/wikipedia/commons/0/04/Visa.svg" alt="Visa" class="card-logo">
  <img src="https://upload.wikimedia.org/wikipedia/commons/2/2a/Mastercard-logo.svg" alt="Mastercard" class="card-logo">
  <img src="https://upload.wikimedia.org/wikipedia/commons/f/fa/American_Express_logo_%282018%29.svg" alt="American Express" class="card-logo">
  <img src="https://upload.wikimedia.org/wikipedia/commons/b/b5/PayPal.svg" alt="PayPal" class="card-logo">
</div>
    
    <!-- Loading state -->
    <div v-if="loading" class="loading">
      Loading payment data...
    </div>
    
    <!-- Error state -->
    <div v-else-if="error" class="error">
      {{ error }}
    </div>
    
    <!-- Success state -->
    <div v-else-if="paymentData" class="payment-info">
      <h3>Payment Details</h3>
      <p><strong>Merchant:</strong> {{ paymentData.merchantName }}</p>
      <p><strong>Amount:</strong> {{ paymentData.amount }} {{ paymentData.currency }}</p>
      <p><strong>Payment ID:</strong> {{ paymentData.paymentId }}</p>
      <p><strong>Expires:</strong> {{ formatDate(paymentData.expiresAt) }}</p>
      <p><strong>Payment Type:</strong> {{ paymentData.isQrPayment ? 'QR Code' : 'Credit Card' }}</p>
    </div>
    
    <!-- Forma za karticu - SAMO ZA KARTIČNA PLAĆANJA -->
    <form v-if="paymentData && !paymentData.isQrPayment && !loading" @submit.prevent="submitPayment">
      <h3>Card Information</h3>
      
      <div class="form-group">
        <label>Card Number</label>
        <input v-model="cardInfo.cardNumber" 
               placeholder="1111 1111 1111 1111"
               required>
      </div>
      
      <div class="form-group">
        <label>Cardholder Name</label>
        <input v-model="cardInfo.cardholderName" 
               placeholder="JOHN DOE"
               required>
      </div>
      
      <div class="form-row">
        <div class="form-group">
          <label>Expiry Date (MM/YY)</label>
          <input v-model="cardInfo.expiryDate" 
                 placeholder="12/28"
                 required>
        </div>
        
        <div class="form-group">
          <label>CVV</label>
          <input v-model="cardInfo.cvv" 
                 type="password"
                 placeholder="123"
                 required>
        </div>
      </div>
      
      <button type="submit" :disabled="processing">
        {{ processing ? 'Processing...' : `Pay ${paymentData.amount} ${paymentData.currency}` }}
      </button>
    </form>
    
    <!-- Za QR plaćanja -->
    <div v-if="paymentData && paymentData.isQrPayment" class="qr-payment">
      <h3>QR Code Payment</h3>
      <p>Please scan the QR code to complete your payment.</p>
      <!-- Ovdje možeš dodati QR code komponentu -->
    </div>
    
    <!-- Ako nema podataka -->
    <div v-else-if="!loading && !paymentData && !error">
      No payment data available.
    </div>
  </div>
</template>

<script>
import { useRoute } from 'vue-router';
import axios from 'axios';

export default {
  name: 'PaymentForm',
  data() {
    return {
      paymentId: '',
      paymentData: null,
      cardInfo: {
        cardNumber: '',
        cardholderName: '',
        expiryDate: '',
        cvv: '',
        paymentId: '',
        isQrPayment: false
      },
      processing: false,
      error: '',
      loading: true
    };
  },
  async mounted() {
    const route = useRoute();
    this.paymentId = route.params.paymentId;
    this.cardInfo.paymentId = this.paymentId;
    
    console.log('Fetching payment data for:', this.paymentId);
    
    try {
      const response = await axios.get(`/api/payment/form/${this.paymentId}`);
      console.log('Payment data response:', response.data);
      
      if (response.data) {
        this.paymentData = response.data;
        this.cardInfo.isQrPayment = this.paymentData.isQrPayment;
      } else {
        this.error = 'No payment data received';
      }
      
    } catch (err) {
      console.error('Error fetching payment data:', err);
      console.error('Error response:', err.response);
      this.error = err.response?.data?.error || 'Payment not found or expired';
    } finally {
      this.loading = false;
    }
  },
  methods: {
    async submitPayment() {
      this.processing = true;
      this.error = '';
      
      try {
        const response = await axios.post('/api/payment/process', this.cardInfo);
        console.log('Payment response:', response.data);

        if (response.data.redirectUrl) {
          // Redirect to PSP frontend URL
          window.location.href = response.data.redirectUrl;
        } else if (response.data.success) {
          this.$router.push(`/payment/success/${this.paymentId}`);
        } else {
          this.error = response.data.error || 'Payment failed';
        }
      } catch (err) {
        console.error('Payment processing error:', err);
        this.error = err.response?.data?.error || 'Payment processing error';
      } finally {
        this.processing = false;
      }
    },
    formatDate(date) {
      const d = new Date(date);
      return d.toLocaleDateString() + ' ' + d.toLocaleTimeString();
    }
  }
};
</script>
<style scoped>
.payment-form {
  max-width: 500px;
  margin: 0 auto;
  padding: 30px;
  background: white;
  border-radius: 16px;
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.1);
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, sans-serif;
}

/* Logo-i kartica na vrhu */
.card-logos {
  display: flex;
  justify-content: center;
  gap: 20px;
  margin-bottom: 30px;
  padding-bottom: 20px;
  border-bottom: 2px solid #f0f0f0;
}

.card-logo {
  height: 40px;
  width: auto;
  opacity: 0.8;
  transition: opacity 0.3s ease;
  filter: grayscale(30%);
}

.card-logo:hover {
  opacity: 1;
  filter: grayscale(0%);
}

.payment-form h2 {
  text-align: center;
  color: #2d3748;
  margin-bottom: 25px;
  font-size: 28px;
  font-weight: 700;
  letter-spacing: -0.5px;
}

.payment-form h3 {
  color: #4a5568;
  margin: 25px 0 15px;
  font-size: 18px;
  font-weight: 600;
}

.payment-info {
  background: #f8fafc;
  padding: 20px;
  border-radius: 12px;
  margin-bottom: 25px;
  border-left: 4px solid #4299e1;
}

.payment-info p {
  margin: 10px 0;
  color: #4a5568;
  font-size: 15px;
  display: flex;
  justify-content: space-between;
}

.payment-info p strong {
  color: #2d3748;
  font-weight: 600;
}

/* Form styles */
.form-group {
  margin-bottom: 20px;
}

.form-group label {
  display: block;
  margin-bottom: 8px;
  color: #4a5568;
  font-weight: 500;
  font-size: 14px;
}

.form-group input {
  width: 100%;
  padding: 14px 16px;
  border: 2px solid #e2e8f0;
  border-radius: 10px;
  font-size: 16px;
  transition: all 0.3s ease;
  background: white;
  color: #2d3748;
  box-sizing: border-box;
}

.form-group input:focus {
  outline: none;
  border-color: #4299e1;
  box-shadow: 0 0 0 3px rgba(66, 153, 225, 0.15);
}

.form-group input::placeholder {
  color: #a0aec0;
}

.form-row {
  display: flex;
  gap: 20px;
}

.form-row .form-group {
  flex: 1;
  margin-bottom: 0;
}

button {
  width: 100%;
  padding: 16px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  border: none;
  border-radius: 10px;
  font-size: 16px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
  margin-top: 25px;
  letter-spacing: 0.5px;
}

button:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 7px 20px rgba(102, 126, 234, 0.4);
}

button:active:not(:disabled) {
  transform: translateY(0);
}

button:disabled {
  opacity: 0.6;
  cursor: not-allowed;
  background: #a0aec0;
}

.qr-payment {
  text-align: center;
  padding: 30px;
  background: #f0f9ff;
  border-radius: 12px;
  margin-top: 20px;
  border: 2px dashed #90cdf4;
}

.qr-payment h3 {
  color: #2b6cb0;
  margin-top: 0;
}

.qr-payment p {
  color: #4a5568;
  margin-bottom: 20px;
}

.loading, .error {
  text-align: center;
  padding: 40px 20px;
  border-radius: 12px;
  margin: 20px 0;
}

.loading {
  background: #f7fafc;
  color: #4a5568;
  font-size: 16px;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 12px;
}

.loading::after {
  content: '';
  width: 20px;
  height: 20px;
  border: 3px solid #e2e8f0;
  border-top-color: #4299e1;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

.error {
  background: #fed7d7;
  color: #c53030;
  border: 2px solid #fc8181;
  font-weight: 500;
}



.payment-form > * {
  animation: fadeIn 0.5s ease forwards;
}
</style>