<template>
  <div class="my-webshops">
    <h1>My WebShops</h1>

    <div v-if="loading" class="loading">Loading...</div>

    <div v-else-if="webshops.length === 0" class="empty-state">
      <p>You are not assigned to any WebShops yet.</p>
    </div>

    <div v-else class="webshops-grid">
      <div v-for="webshop in webshops" :key="webshop.id" class="webshop-card">
        <div class="webshop-header">
          <h3>{{ webshop.name }}</h3>
          <span :class="['status-badge', webshop.status.toLowerCase()]">
            {{ webshop.status }}
          </span>
        </div>
        <div class="webshop-details">
          <p><strong>URL:</strong> {{ webshop.url }}</p>
          <p><strong>Merchant ID:</strong> {{ webshop.merchantId }}</p>
        </div>
        <div class="payment-methods-section">
          <h4>Payment Methods</h4>
          <div v-if="webshop.paymentMethods.length === 0" class="empty-payment-methods">
            No payment methods configured
          </div>
          <div v-else class="payment-methods-list">
            <div v-for="pm in webshop.paymentMethods" :key="pm.id" class="payment-method-item">
              <span>{{ pm.name }} ({{ pm.type }})</span>
              <button 
                @click="removePaymentMethod(webshop.id, pm.id)" 
                class="btn-sm-danger"
                :disabled="webshop.paymentMethods.length === 1"
              >
                Remove
              </button>
            </div>
          </div>
          <button @click="openAddPaymentMethodModal(webshop)" class="btn-add">
            + Add Payment Method
          </button>
        </div>
      </div>
    </div>

    <!-- Add Payment Method Modal -->
    <div v-if="showAddPaymentMethodModal" class="modal" @click.self="closeModal">
      <div class="modal-content">
        <h2>Add Payment Method to {{ selectedWebShop?.name }}</h2>
        <div class="form-group">
          <label>Select Payment Method</label>
          <select v-model="selectedPaymentMethodId">
            <option value="">-- Select --</option>
            <option 
              v-for="pm in availablePaymentMethods" 
              :key="pm.id" 
              :value="pm.id"
            >
              {{ pm.name }} ({{ pm.type }})
            </option>
          </select>
        </div>
        <div class="modal-actions">
          <button @click="closeModal" class="btn-secondary">Cancel</button>
          <button @click="addPaymentMethod" class="btn-primary" :disabled="!selectedPaymentMethodId">
            Add
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import { webShopService, paymentMethodService } from '@/services/pspService'

const webshops = ref([])
const loading = ref(false)
const showAddPaymentMethodModal = ref(false)
const selectedWebShop = ref(null)
const selectedPaymentMethodId = ref('')
const allPaymentMethods = ref([])

const loadMyWebShops = async () => {
  loading.value = true
  try {
    webshops.value = await webShopService.getMyWebShops()
  } catch (error) {
    console.error('Failed to load webshops:', error)
    alert('Failed to load webshops')
  } finally {
    loading.value = false
  }
}

const loadAvailablePaymentMethods = async () => {
  try {
    allPaymentMethods.value = await paymentMethodService.getActivePaymentMethods()
  } catch (error) {
    console.error('Failed to load payment methods:', error)
  }
}

const availablePaymentMethods = computed(() => {
  if (!selectedWebShop.value) return []
  
  const webshopPaymentMethodIds = selectedWebShop.value.paymentMethods.map(pm => pm.id)
  return allPaymentMethods.value.filter(pm => !webshopPaymentMethodIds.includes(pm.id))
})

const openAddPaymentMethodModal = (webshop) => {
  selectedWebShop.value = webshop
  selectedPaymentMethodId.value = ''
  showAddPaymentMethodModal.value = true
}

const addPaymentMethod = async () => {
  if (!selectedPaymentMethodId.value) return

  try {
    await webShopService.addPaymentMethodToWebShop(
      selectedWebShop.value.id,
      selectedPaymentMethodId.value
    )
    closeModal()
    loadMyWebShops()
  } catch (error) {
    console.error('Failed to add payment method:', error)
    alert(error.response?.data?.message || 'Failed to add payment method')
  }
}

const removePaymentMethod = async (webShopId, paymentMethodId) => {
  if (!confirm('Are you sure you want to remove this payment method?')) return

  try {
    await webShopService.removePaymentMethodFromWebShop(webShopId, paymentMethodId)
    loadMyWebShops()
  } catch (error) {
    console.error('Failed to remove payment method:', error)
    alert(error.response?.data?.message || 'Failed to remove payment method')
  }
}

const closeModal = () => {
  showAddPaymentMethodModal.value = false
  selectedWebShop.value = null
  selectedPaymentMethodId.value = ''
}

onMounted(() => {
  loadMyWebShops()
  loadAvailablePaymentMethods()
})
</script>

<style scoped>
.my-webshops {
  padding: 2rem 0;
}

h1 {
  color: #2c3e50;
  margin-bottom: 2rem;
}

.loading {
  text-align: center;
  padding: 2rem;
  color: #7f8c8d;
}

.empty-state {
  text-align: center;
  padding: 3rem;
  background: white;
  border-radius: 8px;
  color: #7f8c8d;
}

.webshops-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(400px, 1fr));
  gap: 1.5rem;
}

.webshop-card {
  background: white;
  border-radius: 8px;
  padding: 1.5rem;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.webshop-header {
  display: flex;
  justify-content: space-between;
  align-items: start;
  margin-bottom: 1rem;
}

.webshop-header h3 {
  color: #2c3e50;
  margin: 0;
}

.status-badge {
  padding: 0.25rem 0.75rem;
  border-radius: 12px;
  font-size: 0.8rem;
  font-weight: 600;
}

.status-badge.active {
  background-color: #d4edda;
  color: #155724;
}

.status-badge.inactive {
  background-color: #f8d7da;
  color: #721c24;
}

.status-badge.suspended {
  background-color: #fff3cd;
  color: #856404;
}

.webshop-details {
  margin-bottom: 1.5rem;
}

.webshop-details p {
  margin: 0.5rem 0;
  color: #34495e;
  font-size: 0.9rem;
}

.payment-methods-section {
  border-top: 1px solid #ecf0f1;
  padding-top: 1rem;
}

.payment-methods-section h4 {
  color: #2c3e50;
  margin-bottom: 0.75rem;
  font-size: 1rem;
}

.empty-payment-methods {
  color: #7f8c8d;
  font-style: italic;
  padding: 0.5rem 0;
  font-size: 0.9rem;
}

.payment-methods-list {
  margin-bottom: 1rem;
}

.payment-method-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0.75rem;
  background-color: #f8f9fa;
  border-radius: 4px;
  margin-bottom: 0.5rem;
}

.payment-method-item span {
  font-size: 0.9rem;
  color: #2c3e50;
}

.btn-sm-danger {
  padding: 0.25rem 0.75rem;
  background-color: #e74c3c;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 0.85rem;
}

.btn-sm-danger:disabled {
  background-color: #95a5a6;
  cursor: not-allowed;
}

.btn-add {
  width: 100%;
  padding: 0.5rem;
  background-color: #3498db;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 0.9rem;
  transition: background-color 0.2s;
}

.btn-add:hover {
  background-color: #2980b9;
}

.modal {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-color: rgba(0, 0, 0, 0.5);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
}

.modal-content {
  background: white;
  padding: 2rem;
  border-radius: 8px;
  max-width: 500px;
  width: 90%;
}

.modal-content h2 {
  margin-bottom: 1.5rem;
  color: #2c3e50;
}

.form-group {
  margin-bottom: 1.5rem;
}

.form-group label {
  display: block;
  margin-bottom: 0.5rem;
  color: #2c3e50;
  font-weight: 500;
}

.form-group select {
  width: 100%;
  padding: 0.5rem;
  border: 1px solid #ddd;
  border-radius: 4px;
}

.modal-actions {
  display: flex;
  gap: 1rem;
  justify-content: flex-end;
}

.btn-primary, .btn-secondary {
  padding: 0.5rem 1rem;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 0.9rem;
}

.btn-primary {
  background-color: #3498db;
  color: white;
}

.btn-primary:disabled {
  background-color: #95a5a6;
  cursor: not-allowed;
}

.btn-secondary {
  background-color: #95a5a6;
  color: white;
}
</style>
