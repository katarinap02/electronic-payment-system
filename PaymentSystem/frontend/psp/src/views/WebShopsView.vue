<template>
  <div class="webshops">
    <div class="header">
      <h1>WebShops Management</h1>
    </div>

    <div class="webshops-grid" v-if="webShops.length > 0">
      <div v-for="webShop in webShops" :key="webShop.id" class="webshop-card">
        <div class="card-header">
          <h2>{{ webShop.name }}</h2>
          <span :class="['status-badge', webShop.status.toLowerCase()]">
            {{ webShop.status }}
          </span>
        </div>
        
        <div class="card-body">
          <div class="info-row">
            <span class="label">URL:</span>
            <span class="value">{{ webShop.url }}</span>
          </div>
          <div class="info-row">
            <span class="label">Merchant ID:</span>
            <span class="value">{{ webShop.merchantId }}</span>
          </div>
          <div class="info-row">
            <span class="label">API Key:</span>
            <span class="value api-key">{{ webShop.apiKey.substring(0, 20) }}...</span>
          </div>
          <div class="info-row">
            <span class="label">Created:</span>
            <span class="value">{{ formatDate(webShop.createdAt) }}</span>
          </div>
        </div>

        <div class="card-footer">
          <h3>Payment Methods</h3>
          <div class="payment-methods">
            <div v-for="method in allPaymentMethods" :key="method.id" class="checkbox-group">
              <label>
                <input 
                  type="checkbox" 
                  :checked="isMethodEnabled(webShop, method.id)"
                  @change="togglePaymentMethod(webShop, method.id)"
                />
                {{ method.name }} ({{ method.code }})
              </label>
            </div>
          </div>
          <button @click="savePaymentMethods(webShop)" class="btn-primary">
            Save Changes
          </button>
        </div>
      </div>
    </div>

    <div v-else class="empty-state">
      <p>No webshops found.</p>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { webShopService } from '@/services/pspService'

const webShops = ref([])
const allPaymentMethods = ref([])
const pendingChanges = ref({})

const loadWebShops = async () => {
  try {
    webShops.value = await webShopService.getAllWebShops()
    allPaymentMethods.value = await webShopService.getAllPaymentMethods()
  } catch (error) {
    console.error('Failed to load webshops:', error)
  }
}

const isMethodEnabled = (webShop, methodId) => {
  if (pendingChanges.value[webShop.id]) {
    return pendingChanges.value[webShop.id].includes(methodId)
  }
  return webShop.paymentMethods.some(pm => pm.id === methodId)
}

const togglePaymentMethod = (webShop, methodId) => {
  if (!pendingChanges.value[webShop.id]) {
    pendingChanges.value[webShop.id] = webShop.paymentMethods.map(pm => pm.id)
  }
  
  const index = pendingChanges.value[webShop.id].indexOf(methodId)
  if (index > -1) {
    pendingChanges.value[webShop.id].splice(index, 1)
  } else {
    pendingChanges.value[webShop.id].push(methodId)
  }
}

const savePaymentMethods = async (webShop) => {
  try {
    const methodIds = pendingChanges.value[webShop.id] || webShop.paymentMethods.map(pm => pm.id)
    await webShopService.updatePaymentMethods(webShop.id, methodIds)
    delete pendingChanges.value[webShop.id]
    await loadWebShops()
    alert('Payment methods updated successfully!')
  } catch (error) {
    console.error('Failed to update payment methods:', error)
    alert('Failed to update payment methods')
  }
}

const formatDate = (date) => {
  return new Date(date).toLocaleDateString()
}

onMounted(() => {
  loadWebShops()
})
</script>

<style scoped>
.webshops {
  padding: 2rem 0;
}

.header {
  margin-bottom: 2rem;
}

h1 {
  color: #2c3e50;
}

.webshops-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(500px, 1fr));
  gap: 2rem;
}

.webshop-card {
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  overflow: hidden;
}

.card-header {
  background-color: #f8f9fa;
  padding: 1.5rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-bottom: 2px solid #e9ecef;
}

.card-header h2 {
  margin: 0;
  color: #2c3e50;
  font-size: 1.5rem;
}

.status-badge {
  padding: 0.25rem 0.75rem;
  border-radius: 12px;
  font-size: 0.875rem;
  font-weight: 500;
}

.status-badge.active {
  background-color: #d4edda;
  color: #155724;
}

.status-badge.inactive {
  background-color: #f8d7da;
  color: #721c24;
}

.card-body {
  padding: 1.5rem;
}

.info-row {
  display: flex;
  justify-content: space-between;
  margin-bottom: 0.75rem;
  padding: 0.5rem 0;
  border-bottom: 1px solid #f0f0f0;
}

.info-row .label {
  font-weight: 600;
  color: #7f8c8d;
}

.info-row .value {
  color: #2c3e50;
}

.api-key {
  font-family: monospace;
  font-size: 0.875rem;
}

.card-footer {
  padding: 1.5rem;
  background-color: #f8f9fa;
  border-top: 1px solid #e9ecef;
}

.card-footer h3 {
  margin: 0 0 1rem 0;
  color: #2c3e50;
  font-size: 1.1rem;
}

.payment-methods {
  margin-bottom: 1rem;
}

.checkbox-group {
  margin-bottom: 0.75rem;
}

.checkbox-group label {
  display: flex;
  align-items: center;
  cursor: pointer;
  color: #2c3e50;
}

.checkbox-group input[type="checkbox"] {
  margin-right: 0.5rem;
  width: 18px;
  height: 18px;
  cursor: pointer;
}

.btn-primary {
  width: 100%;
  background-color: #3498db;
  color: white;
  border: none;
  padding: 0.75rem 1.5rem;
  border-radius: 4px;
  cursor: pointer;
  font-size: 1rem;
  transition: background-color 0.3s;
}

.btn-primary:hover {
  background-color: #2980b9;
}

.empty-state {
  text-align: center;
  padding: 3rem;
  background: white;
  border-radius: 8px;
  color: #7f8c8d;
}
</style>
