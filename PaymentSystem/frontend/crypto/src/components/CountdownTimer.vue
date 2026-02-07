<template>
  <div class="countdown-timer" :class="{ warning: isWarning, critical: isCritical }">
    <div class="timer-icon"></div>
    <div class="timer-display">
      <span class="time">{{ displayTime }}</span>
      <span class="label">{{ statusText }}</span>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'

const props = defineProps({
  expiresAt: { type: String, required: true }
})

const emit = defineEmits(['expired'])

const timeRemaining = ref(0)
let intervalId = null

const calculateTimeRemaining = () => {
  const now = new Date().getTime()
  const expiry = new Date(props.expiresAt).getTime()
  const diff = Math.max(0, expiry - now)
  
  timeRemaining.value = Math.floor(diff / 1000)
  
  if (diff <= 0) {
    emit('expired')
    if (intervalId) clearInterval(intervalId)
  }
}

const displayTime = computed(() => {
  const minutes = Math.floor(timeRemaining.value / 60)
  const seconds = timeRemaining.value % 60
  return `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`
})

const isWarning = computed(() => timeRemaining.value <= 300 && timeRemaining.value > 60) // 5 min
const isCritical = computed(() => timeRemaining.value <= 60) // 1 min

const statusText = computed(() => {
  if (timeRemaining.value <= 0) return 'Expired'
  if (isCritical.value) return 'Time running out!'
  if (isWarning.value) return 'Hurry up!'
  return 'Time remaining'
})

onMounted(() => {
  calculateTimeRemaining()
  intervalId = setInterval(calculateTimeRemaining, 1000)
})

onUnmounted(() => {
  if (intervalId) clearInterval(intervalId)
})
</script>

<style scoped>
.countdown-timer {
  background: linear-gradient(135deg, #10b981 0%, #059669 100%);
  border-radius: 16px;
  padding: 20px;
  display: flex;
  align-items: center;
  gap: 15px;
  box-shadow: 0 4px 20px rgba(16, 185, 129, 0.3);
  margin-bottom: 20px;
  transition: all 0.3s;
}

.countdown-timer.warning {
  background: linear-gradient(135deg, #f59e0b 0%, #d97706 100%);
  box-shadow: 0 4px 20px rgba(245, 158, 11, 0.3);
}

.countdown-timer.critical {
  background: linear-gradient(135deg, #ef4444 0%, #dc2626 100%);
  box-shadow: 0 4px 20px rgba(239, 68, 68, 0.3);
  animation: pulse 1s infinite;
}

@keyframes pulse {
  0%, 100% { transform: scale(1); }
  50% { transform: scale(1.02); }
}

.timer-icon {
  font-size: 40px;
}

.timer-display {
  display: flex;
  flex-direction: column;
  gap: 5px;
}

.time {
  color: white;
  font-size: 32px;
  font-weight: bold;
  font-family: 'Courier New', monospace;
}

.label {
  color: rgba(255, 255, 255, 0.9);
  font-size: 14px;
  font-weight: 500;
}
</style>
