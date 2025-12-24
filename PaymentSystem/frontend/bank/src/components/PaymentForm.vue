<template>
  <div class="payment-form">
    <h2>Card Payment</h2>
    <form @submit.prevent="submitPayment">
      <div>
        <label>Card Number:</label>
        <input v-model="cardNumber" @input="validateCard" placeholder="1234 5678 9012 3456" />
        <div v-if="cardError" class="error">{{ cardError }}</div>
      </div>
      
      <div>
        <label>Expiry Date (MM/YY):</label>
        <input v-model="expiryDate" placeholder="12/25" />
      </div>
      
      <div>
        <label>CVV:</label>
        <input v-model="cvv" type="password" maxlength="3" />
      </div>
      
      <div>
        <label>Cardholder Name:</label>
        <input v-model="cardholderName" />
      </div>
      
      <div class="amount-display">
        <strong>Amount to pay:</strong> {{ amount }} {{ currency }}
      </div>
      
      <button type="submit" :disabled="!isFormValid">Pay Now</button>
      
      <div v-if="showQrOption" class="qr-option">
        <p>Or pay with QR code:</p>
        <button @click="generateQrCode">Generate QR Code</button>
      </div>
    </form>
  </div>
</template>