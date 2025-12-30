<template>
  <div class="dashboard">
    <h1>Dashboard</h1>
    <div class="welcome-card">
      <h2>Welcome, {{ authStore.user?.name }} {{ authStore.user?.surname }}!</h2>
      <p class="role-badge">{{ authStore.user?.role }}</p>
    </div>

    <div class="stats-grid" v-if="authStore.isSuperAdmin">
      <div class="stat-card">
        <h3>Total WebShops</h3>
        <p class="stat-number">{{ totalWebShops }}</p>
      </div>
      <div class="stat-card">
        <h3>Active Payment Methods</h3>
        <p class="stat-number">{{ totalPaymentMethods }}</p>
      </div>
    </div>

    <div class="stats-grid" v-if="authStore.isAdmin">
      <div class="stat-card">
        <h3>My WebShops</h3>
        <p class="stat-number">{{ myWebShopsCount }}</p>
      </div>
    </div>

    <div class="quick-actions">
      <h3>Quick Actions</h3>
      <div class="actions-grid">
        <router-link to="/webshops" class="action-card" v-if="authStore.isSuperAdmin">
          <div class="action-icon">🏪</div>
          <h4>Manage WebShops</h4>
          <p>Create, update and manage all WebShops</p>
        </router-link>
        <router-link to="/my-webshops" class="action-card" v-if="authStore.isAdmin">
          <div class="action-icon">🏪</div>
          <h4>My WebShops</h4>
          <p>Manage your assigned WebShops</p>
        </router-link>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { webShopService, paymentMethodService } from '@/services/pspService'

const authStore = useAuthStore()
const totalWebShops = ref(0)
const totalPaymentMethods = ref(0)
const myWebShopsCount = ref(0)

onMounted(async () => {
  try {
    if (authStore.isSuperAdmin) {
      const webShops = await webShopService.getAllWebShops()
      totalWebShops.value = webShops.length || 0
      
      const paymentMethods = await paymentMethodService.getActivePaymentMethods()
      totalPaymentMethods.value = paymentMethods.length || 0
    } else if (authStore.isAdmin) {
      const myWebShops = await webShopService.getMyWebShops()
      myWebShopsCount.value = myWebShops.length || 0
    }
  } catch (error) {
    console.error('Failed to load dashboard data:', error)
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

.welcome-card {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  padding: 2rem;
  border-radius: 12px;
  margin-bottom: 2rem;
}

.welcome-card h2 {
  margin-bottom: 0.5rem;
}

.role-badge {
  display: inline-block;
  background: rgba(255, 255, 255, 0.2);
  padding: 0.5rem 1rem;
  border-radius: 20px;
  font-size: 0.9rem;
  font-weight: 600;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 1.5rem;
  margin-bottom: 2rem;
}

.stat-card {
  background: white;
  padding: 1.5rem;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.stat-card h3 {
  color: #7f8c8d;
  font-size: 0.9rem;
  margin-bottom: 1rem;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.stat-number {
  font-size: 2.5rem;
  font-weight: bold;
  color: #2c3e50;
}

.quick-actions h3 {
  color: #2c3e50;
  margin-bottom: 1.5rem;
}

.actions-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
  gap: 1.5rem;
}

.action-card {
  background: white;
  padding: 2rem;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  text-decoration: none;
  color: inherit;
  transition: transform 0.2s, box-shadow 0.2s;
}

.action-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.15);
}

.action-icon {
  font-size: 3rem;
  margin-bottom: 1rem;
}

.action-card h4 {
  color: #2c3e50;
  margin-bottom: 0.5rem;
}

.action-card p {
  color: #7f8c8d;
  font-size: 0.9rem;
}
</style>
