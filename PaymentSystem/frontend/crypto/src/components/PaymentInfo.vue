<template>
  <div class="payment-info-card">
    <h2>Payment Details</h2>
    
    <div class="info-row" v-if="merchantName">
      <span class="label">Web shop:</span>
      <span class="value">{{ merchantName }}</span>
    </div>
    
    <div class="info-row">
      <span class="label">Amount (EUR):</span>
      <span class="value">{{ amount }} EUR</span>
    </div>
    
    <div class="info-row highlight">
      <span class="label">Amount (ETH):</span>
      <span class="value crypto">{{ amountInCrypto }} ETH</span>
    </div>
    
    <div class="info-row">
      <span class="label">Exchange Rate:</span>
      <span class="value">1 ETH = {{ exchangeRate }} EUR</span>
    </div>
    
    <div class="wallet-section">
      <label>Send ETH to:</label>
      <div class="wallet-address-box">
        <code>{{ walletAddress }}</code>
        <button @click="copyAddress" class="copy-btn">
          {{ copied ? 'Copied!' : 'Copy' }}
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const props = defineProps({
  merchantName: { type: String, default: '' },
  amount: { type: Number, required: true },
  amountInCrypto: { type: Number, required: true },
  exchangeRate: { type: Number, required: true },
  walletAddress: { type: String, required: true }
})

const copied = ref(false)

const copyAddress = () => {
  navigator.clipboard.writeText(props.walletAddress)
  copied.value = true
  setTimeout(() => { copied.value = false }, 2000)
}
</script>

<style scoped>
.payment-info-card {
  background: white;
  border-radius: 16px;
  padding: 30px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
  margin-bottom: 20px;
}

.payment-info-card h2 {
  color: #1f2937;
  margin-bottom: 25px;
  font-size: 24px;
}

.info-row {
  display: flex;
  justify-content: space-between;
  padding: 12px 0;
  border-bottom: 1px solid #e5e7eb;
}

.info-row.highlight {
  background: #eff6ff;
  padding: 15px;
  border-radius: 8px;
  border: 2px solid #3b82f6;
  margin: 10px 0;
}

.label {
  color: #6b7280;
  font-weight: 500;
}

.value {
  color: #1f2937;
  font-weight: 600;
}

.value.crypto {
  color: #3b82f6;
  font-size: 20px;
}

.wallet-section {
  margin-top: 25px;
}

.wallet-section label {
  display: block;
  color: #6b7280;
  margin-bottom: 10px;
  font-weight: 500;
}

.wallet-address-box {
  display: flex;
  gap: 10px;
  align-items: center;
  background: #f9fafb;
  padding: 15px;
  border-radius: 8px;
  border: 2px solid #e5e7eb;
}

.wallet-address-box code {
  flex: 1;
  color: #1f2937;
  font-size: 14px;
  word-break: break-all;
}

.copy-btn {
  background: #3b82f6;
  color: white;
  border: none;
  padding: 10px 20px;
  border-radius: 6px;
  cursor: pointer;
  font-weight: 600;
  white-space: nowrap;
  transition: all 0.3s;
}

.copy-btn:hover {
  background: #2563eb;
  transform: translateY(-2px);
}
</style>
