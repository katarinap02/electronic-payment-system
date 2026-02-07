<template>
  <div class="qr-code-section">
    <h3> Scan QR Code</h3>
    <p class="instruction">Use your MetaMask or Ethereum wallet to scan</p>
    
    <div class="qr-wrapper">
      <canvas ref="qrCanvas" class="qr-code"></canvas>
    </div>
    
    <p class="hint">Or copy the wallet address above and send manually</p>
  </div>
</template>

<script setup>
import { ref, onMounted, watch } from 'vue'
import QRCode from 'qrcode'

const props = defineProps({
  walletAddress: { type: String, required: true },
  amountInWei: { type: String, required: true },
  chainId: { type: Number, default: 11155111 } // Sepolia testnet
})

const qrCanvas = ref(null)

// Format: ethereum:{address}@{chainId}?value={amountInWei}
const generateQRCode = async () => {
  if (!qrCanvas.value) return
  
  const qrData = `ethereum:${props.walletAddress}@${props.chainId}?value=${props.amountInWei}`
  
  try {
    await QRCode.toCanvas(qrCanvas.value, qrData, {
      width: 250,
      margin: 2,
      color: {
        dark: '#000000',
        light: '#FFFFFF'
      }
    })
  } catch (err) {
    console.error('Failed to generate QR code:', err)
  }
}

onMounted(() => {
  generateQRCode()
})

watch(() => [props.walletAddress, props.amountInWei], () => {
  generateQRCode()
})
</script>

<style scoped>
.qr-code-section {
  background: white;
  border-radius: 16px;
  padding: 30px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
  text-align: center;
  margin-bottom: 20px;
}

.qr-code-section h3 {
  color: #1f2937;
  margin-bottom: 10px;
  font-size: 20px;
}

.instruction {
  color: #6b7280;
  margin-bottom: 25px;
  font-size: 14px;
}

.qr-wrapper {
  display: inline-block;
  padding: 20px;
  background: white;
  border-radius: 12px;
  border: 3px solid #3b82f6;
  margin-bottom: 20px;
}

.qr-code {
  display: block;
}

.hint {
  color: #9ca3af;
  font-size: 13px;
  font-style: italic;
}
</style>
