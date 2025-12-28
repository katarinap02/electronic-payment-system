<template>
  <div class="payment-form">
    <h2>Payment</h2>
    
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
        
        if (response.data.success) {
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