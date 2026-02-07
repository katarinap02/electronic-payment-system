<template>
  <div class="payment-status">
    <div v-if="status === 'pending'" class="status pending">
      <div class="spinner">○</div>
      <h2>Plaćanje u toku</h2>
      <p>Čekamo potvrdu transakcije na Ethereum mreži...</p>
      <p class="tx-hash">TX Hash: {{ transactionHash }}</p>
    </div>

    <div v-else-if="status === 'processing'" class="status processing">
      <div class="spinner">◐</div>
      <h2>Obrada transakcije</h2>
      <p>Vaša transakcija se obrađuje...</p>
      <p class="tx-hash">TX Hash: {{ transactionHash }}</p>
    </div>

    <div v-else-if="status === 'completed'" class="status success">
      <div class="icon">●</div>
      <h2>Plaćanje uspešno!</h2>
      <p>Vaša transakcija je potvrđena na blockchainu.</p>
      <p class="tx-hash">TX Hash: {{ transactionHash }}</p>
      <a :href="`https://sepolia.etherscan.io/tx/${transactionHash}`" target="_blank" class="etherscan-link">
        Pogledaj na Etherscan
      </a>
    </div>

    <div v-else-if="status === 'failed'" class="status error">
      <div class="icon">⊗</div>
      <h2>Plaćanje neuspešno</h2>
      <p>{{ errorMessage || 'Došlo je do greške pri obradi transakcije.' }}</p>
      <button @click="$emit('retry')" class="retry-btn">Pokušaj ponovo</button>
    </div>

    <div v-else-if="status === 'cancelled'" class="status cancelled">
      <div class="icon">✕</div>
      <h2>Plaćanje otkazano</h2>
      <p>Korisnik je otkazao transakciju.</p>
      <button @click="$emit('retry')" class="retry-btn">Pokušaj ponovo</button>
    </div>
  </div>
</template>

<script setup>
import { defineProps, defineEmits } from 'vue'

defineProps({
  status: {
    type: String,
    required: true,
    validator: (value) => ['pending', 'processing', 'completed', 'failed', 'cancelled'].includes(value)
  },
  transactionHash: String,
  errorMessage: String
})

defineEmits(['retry'])
</script>

<style scoped>
.payment-status {
  padding: 2rem;
  text-align: center;
}

.status {
  max-width: 500px;
  margin: 0 auto;
  padding: 2rem;
  border-radius: 12px;
  background: white;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.icon,
.spinner {
  font-size: 4rem;
  margin-bottom: 1rem;
  animation: none;
}

.spinner {
  animation: spin 1s linear infinite;
}

@keyframes spin {
  from {
    transform: rotate(0deg);
  }
  to {
    transform: rotate(360deg);
  }
}

.pending {
  border: 2px solid #ff9800;
}

.pending .spinner {
  color: #ff9800;
}

.processing {
  border: 2px solid #2196f3;
}

.processing .spinner {
  color: #2196f3;
}

.success {
  border: 2px solid #4caf50;
}

.success .icon {
  color: #4caf50;
}

.error {
  border: 2px solid #f44336;
}

.error .icon {
  color: #f44336;
}

.cancelled {
  border: 2px solid #9e9e9e;
}

.cancelled .icon {
  color: #9e9e9e;
}

h2 {
  margin: 0.5rem 0;
  color: #333;
}

p {
  color: #666;
  margin: 0.5rem 0;
}

.tx-hash {
  font-family: monospace;
  font-size: 0.85rem;
  word-break: break-all;
  background: #f5f5f5;
  padding: 0.5rem;
  border-radius: 4px;
  margin-top: 1rem;
}

.etherscan-link {
  display: inline-block;
  margin-top: 1rem;
  padding: 0.75rem 1.5rem;
  background: #21325b;
  color: white;
  text-decoration: none;
  border-radius: 6px;
  transition: background 0.3s;
}

.etherscan-link:hover {
  background: #1a2847;
}

.retry-btn {
  margin-top: 1rem;
  padding: 0.75rem 1.5rem;
  background: #2196f3;
  color: white;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-size: 1rem;
  transition: background 0.3s;
}

.retry-btn:hover {
  background: #1976d2;
}
</style>
