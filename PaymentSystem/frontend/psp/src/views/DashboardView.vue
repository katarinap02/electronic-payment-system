<template>
  <div class="dashboard">
    <h1>PSP Dashboard</h1>
    <div class="stats-grid">
      <div class="stat-card">
        <h3>Total Merchants</h3>
        <p class="stat-number">{{ stats.totalMerchants }}</p>
      </div>
      <div class="stat-card">
        <h3>Active Transactions</h3>
        <p class="stat-number">{{ stats.activeTransactions }}</p>
      </div>
      <div class="stat-card">
        <h3>Total Revenue</h3>
        <p class="stat-number">{{ stats.totalRevenue }} RSD</p>
      </div>
      <div class="stat-card">
        <h3>Success Rate</h3>
        <p class="stat-number">{{ stats.successRate }}%</p>
      </div>
    </div>
    <div class="welcome-message">
      <h2>Welcome, {{ authStore.user?.name }}!</h2>
      <p>You are logged in as {{ authStore.user?.role }}</p>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()

const stats = ref({
  totalMerchants: 0,
  activeTransactions: 0,
  totalRevenue: 0,
  successRate: 0
})

onMounted(() => {
  // TODO: Fetch real stats from API
  stats.value = {
    totalMerchants: 5,
    activeTransactions: 23,
    totalRevenue: 150000,
    successRate: 98.5
  }
})
</script>

<style scoped>
.dashboard {
  padding: 2rem 0;
}

h1 {
  color: #2c3e50;
  margin-bottom: 2rem;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 1.5rem;
  margin-bottom: 3rem;
}

.stat-card {
  background: white;
  padding: 2rem;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  text-align: center;
}

.stat-card h3 {
  color: #7f8c8d;
  font-size: 0.9rem;
  margin-bottom: 1rem;
  text-transform: uppercase;
}

.stat-number {
  color: #2c3e50;
  font-size: 2rem;
  font-weight: bold;
}

.welcome-message {
  background: white;
  padding: 2rem;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.welcome-message h2 {
  color: #2c3e50;
  margin-bottom: 0.5rem;
}

.welcome-message p {
  color: #7f8c8d;
}
</style>
