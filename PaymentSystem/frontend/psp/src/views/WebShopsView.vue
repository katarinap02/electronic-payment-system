<template>
  <div class="webshops">
    <div class="header">
      <h1>WebShops Management</h1>
      <button @click="showCreateModal = true" class="btn-primary">Create New WebShop</button>
    </div>

    <div v-if="loading" class="loading">Loading...</div>

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
          <p><strong>API Key:</strong> <code>{{ webshop.apiKey }}</code></p>
          <p><strong>Payment Methods:</strong> {{ webshop.paymentMethods.length }}</p>
        </div>
        <div class="webshop-actions">
          <button @click="editWebShop(webshop)" class="btn-secondary">Edit</button>
          <button @click="manageAdmins(webshop)" class="btn-info">Admins</button>
          <button @click="deleteWebShop(webshop.id)" class="btn-danger">Delete</button>
        </div>
      </div>
    </div>

    <!-- Create/Edit Modal -->
    <div v-if="showCreateModal || showEditModal" class="modal" @click.self="closeModals">
      <div class="modal-content">
        <h2>{{ showEditModal ? 'Edit WebShop' : 'Create New WebShop' }}</h2>
        <form @submit.prevent="saveWebShop">
          <div class="form-group">
            <label>Name</label>
            <input v-model="formData.name" required type="text" />
          </div>
          <div class="form-group">
            <label>URL</label>
            <input v-model="formData.url" required type="url" />
          </div>
          <div class="form-group">
            <label>Merchant ID</label>
            <input v-model="formData.merchantId" required type="text" />
          </div>
          <div class="form-group" v-if="showEditModal">
            <label>Status</label>
            <select v-model="formData.status">
              <option>Active</option>
              <option>Inactive</option>
              <option>Suspended</option>
            </select>
          </div>
          <div class="modal-actions">
            <button type="button" @click="closeModals" class="btn-secondary">Cancel</button>
            <button type="submit" class="btn-primary">Save</button>
          </div>
        </form>
      </div>
    </div>

    <!-- Manage Admins Modal -->
    <div v-if="showAdminsModal" class="modal" @click.self="closeModals">
      <div class="modal-content">
        <h2>Manage Admins for {{ selectedWebShop?.name }}</h2>
        <div class="admins-list">
          <h3>Current Admins</h3>
          <div v-if="webshopAdmins.length === 0" class="empty-state">
            No admins assigned
          </div>
          <div v-else v-for="admin in webshopAdmins" :key="admin.userId" class="admin-item">
            <span>{{ admin.name }} {{ admin.surname }} ({{ admin.email }})</span>
            <button @click="removeAdmin(admin.userId)" class="btn-sm-danger">Remove</button>
          </div>
        </div>
        <div class="add-admin-form">
          <h3>Add Admin</h3>
          <div class="form-group">
            <label>User ID</label>
            <input v-model.number="newAdminUserId" type="number" placeholder="Enter user ID" />
          </div>
          <button @click="addAdmin" class="btn-primary">Add Admin</button>
        </div>
        <div class="modal-actions">
          <button @click="closeModals" class="btn-secondary">Close</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { webShopService } from '@/services/pspService'

const webshops = ref([])
const loading = ref(false)
const showCreateModal = ref(false)
const showEditModal = ref(false)
const showAdminsModal = ref(false)
const selectedWebShop = ref(null)
const webshopAdmins = ref([])
const newAdminUserId = ref(null)

const formData = ref({
  name: '',
  url: '',
  merchantId: '',
  status: 'Active'
})

const loadWebShops = async () => {
  loading.value = true
  try {
    webshops.value = await webShopService.getAllWebShops()
  } catch (error) {
    console.error('Failed to load webshops:', error)
    alert('Failed to load webshops')
  } finally {
    loading.value = false
  }
}

const editWebShop = (webshop) => {
  selectedWebShop.value = webshop
  formData.value = {
    name: webshop.name,
    url: webshop.url,
    merchantId: webshop.merchantId,
    status: webshop.status
  }
  showEditModal.value = true
}

const saveWebShop = async () => {
  try {
    if (showEditModal.value) {
      await webShopService.updateWebShop(selectedWebShop.value.id, formData.value)
    } else {
      await webShopService.createWebShop(formData.value)
    }
    closeModals()
    loadWebShops()
  } catch (error) {
    console.error('Failed to save webshop:', error)
    alert(error.response?.data?.message || 'Failed to save webshop')
  }
}

const deleteWebShop = async (id) => {
  if (!confirm('Are you sure you want to delete this webshop?')) return
  
  try {
    await webShopService.deleteWebShop(id)
    loadWebShops()
  } catch (error) {
    console.error('Failed to delete webshop:', error)
    alert('Failed to delete webshop')
  }
}

const manageAdmins = async (webshop) => {
  selectedWebShop.value = webshop
  try {
    webshopAdmins.value = await webShopService.getWebShopAdmins(webshop.id)
  } catch (error) {
    webshopAdmins.value = []
  }
  showAdminsModal.value = true
}

const addAdmin = async () => {
  if (!newAdminUserId.value) return
  
  try {
    await webShopService.assignAdmin(selectedWebShop.value.id, newAdminUserId.value)
    webshopAdmins.value = await webShopService.getWebShopAdmins(selectedWebShop.value.id)
    newAdminUserId.value = null
  } catch (error) {
    console.error('Failed to add admin:', error)
    alert(error.response?.data?.message || 'Failed to add admin')
  }
}

const removeAdmin = async (userId) => {
  try {
    await webShopService.removeAdmin(selectedWebShop.value.id, userId)
    webshopAdmins.value = await webShopService.getWebShopAdmins(selectedWebShop.value.id)
  } catch (error) {
    console.error('Failed to remove admin:', error)
    alert('Failed to remove admin')
  }
}

const closeModals = () => {
  showCreateModal.value = false
  showEditModal.value = false
  showAdminsModal.value = false
  selectedWebShop.value = null
  formData.value = { name: '', url: '', merchantId: '', status: 'Active' }
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
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
}

h1 {
  color: #2c3e50;
}

.loading {
  text-align: center;
  padding: 2rem;
  color: #7f8c8d;
}

.webshops-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
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
  margin-bottom: 1rem;
}

.webshop-details p {
  margin: 0.5rem 0;
  color: #34495e;
  font-size: 0.9rem;
}

code {
  background-color: #f4f4f4;
  padding: 0.2rem 0.4rem;
  border-radius: 3px;
  font-size: 0.85rem;
}

.webshop-actions {
  display: flex;
  gap: 0.5rem;
}

.btn-primary, .btn-secondary, .btn-danger, .btn-info {
  padding: 0.5rem 1rem;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 0.9rem;
  transition: opacity 0.2s;
}

.btn-primary {
  background-color: #3498db;
  color: white;
}

.btn-secondary {
  background-color: #95a5a6;
  color: white;
}

.btn-danger {
  background-color: #e74c3c;
  color: white;
}

.btn-info {
  background-color: #16a085;
  color: white;
}

.btn-primary:hover, .btn-secondary:hover, .btn-danger:hover, .btn-info:hover {
  opacity: 0.9;
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
  max-height: 80vh;
  overflow-y: auto;
}

.modal-content h2 {
  margin-bottom: 1.5rem;
  color: #2c3e50;
}

.form-group {
  margin-bottom: 1rem;
}

.form-group label {
  display: block;
  margin-bottom: 0.5rem;
  color: #2c3e50;
  font-weight: 500;
}

.form-group input, .form-group select {
  width: 100%;
  padding: 0.5rem;
  border: 1px solid #ddd;
  border-radius: 4px;
}

.modal-actions {
  display: flex;
  gap: 1rem;
  justify-content: flex-end;
  margin-top: 1.5rem;
}

.admins-list {
  margin-bottom: 2rem;
}

.admins-list h3 {
  color: #2c3e50;
  margin-bottom: 1rem;
}

.admin-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0.75rem;
  background-color: #f8f9fa;
  border-radius: 4px;
  margin-bottom: 0.5rem;
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

.add-admin-form {
  padding: 1rem;
  background-color: #f8f9fa;
  border-radius: 4px;
  margin-bottom: 1rem;
}

.add-admin-form h3 {
  color: #2c3e50;
  margin-bottom: 1rem;
}

.empty-state {
  color: #7f8c8d;
  font-style: italic;
  padding: 1rem;
  text-align: center;
}
</style>
