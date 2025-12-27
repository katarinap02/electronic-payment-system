<template>
  <div class="admin-vehicles-page">
    <div class="page-header">
      <h1>Vehicle Management</h1>
      <button class="btn-add" @click="openCreateModal">
        Add New Vehicle
      </button>
    </div>

    <!-- Create/Edit Modal -->
    <div v-if="showModal" class="modal-overlay" @click.self="closeModal">
      <div class="modal-content">
        <div class="modal-header">
          <h2>{{ isEditing ? 'Edit Vehicle' : 'Create New Vehicle' }}</h2>
          <button class="btn-close" @click="closeModal">&times;</button>
        </div>

        <form @submit.prevent="submitForm" class="vehicle-form">
          <div class="form-grid">
            <div class="form-group">
              <label>Brand *</label>
              <input 
                v-model="formData.brand" 
                type="text" 
                required 
                maxlength="100"
                placeholder="e.g. Mercedes-Benz"
              />
            </div>

            <div class="form-group">
              <label>Model *</label>
              <input 
                v-model="formData.model" 
                type="text" 
                required 
                maxlength="100"
                placeholder="e.g. E-Class"
              />
            </div>

            <div class="form-group">
              <label>Year *</label>
              <input 
                v-model.number="formData.year" 
                type="number" 
                required 
                min="1900" 
                max="2100"
              />
            </div>

            <div class="form-group">
              <label>Category *</label>
              <select v-model.number="formData.category" required>
                <option :value="null" disabled>Select category</option>
                <option :value="1">Economy</option>
                <option :value="2">Comfort</option>
                <option :value="3">Luxury</option>
                <option :value="4">SUV</option>
                <option :value="5">Van</option>
                <option :value="6">Sport</option>
              </select>
            </div>

            <div class="form-group">
              <label>Price Per Day ($) *</label>
              <input 
                v-model.number="formData.pricePerDay" 
                type="number" 
                required 
                min="0.01" 
                max="10000"
                step="0.01"
              />
            </div>

            <div class="form-group">
              <label>Transmission *</label>
              <select v-model.number="formData.transmission" required>
                <option :value="null" disabled>Select transmission</option>
                <option :value="1">Manual</option>
                <option :value="2">Automatic</option>
              </select>
            </div>

            <div class="form-group">
              <label>Fuel Type *</label>
              <select v-model.number="formData.fuelType" required>
                <option :value="null" disabled>Select fuel type</option>
                <option :value="1">Petrol</option>
                <option :value="2">Diesel</option>
                <option :value="3">Electric</option>
                <option :value="4">Hybrid</option>
              </select>
            </div>

            <div class="form-group">
              <label>Seats *</label>
              <input 
                v-model.number="formData.seats" 
                type="number" 
                required 
                min="2" 
                max="50"
              />
            </div>

            <div class="form-group">
              <label>License Plate *</label>
              <input 
                v-model="formData.licensePlate" 
                type="text" 
                required 
                maxlength="20"
                placeholder="e.g. BG-123-AB"
              />
            </div>

            <div class="form-group">
              <label>Mileage (km) *</label>
              <input 
                v-model.number="formData.mileage" 
                type="number" 
                required 
                min="0" 
                max="1000000"
              />
            </div>

            <div class="form-group">
              <label>Color *</label>
              <input 
                v-model="formData.color" 
                type="text" 
                required 
                maxlength="50"
                placeholder="e.g. White"
              />
            </div>

            <div class="form-group" v-if="isEditing">
              <label>Status</label>
              <select v-model.number="formData.status">
                <option :value="1">Available</option>
                <option :value="2">Rented</option>
                <option :value="3">Maintenance</option>
                <option :value="4">Unavailable</option>
              </select>
            </div>

            <div class="form-group full-width">
              <label>Image URL</label>
              <input 
                v-model="formData.imageUrl" 
                type="text" 
                maxlength="500"
                placeholder="https://example.com/image.jpg"
              />
            </div>

            <div class="form-group full-width">
              <label>Description</label>
              <textarea 
                v-model="formData.description" 
                maxlength="1000"
                rows="4"
                placeholder="Vehicle description..."
              ></textarea>
            </div>
          </div>

          <div v-if="formError" class="form-error">
            {{ formError }}
          </div>

          <div class="form-actions">
            <button type="button" class="btn-cancel" @click="closeModal">
              Cancel
            </button>
            <button type="submit" class="btn-submit" :disabled="submitting">
              {{ submitting ? 'Saving...' : (isEditing ? 'Update Vehicle' : 'Create Vehicle') }}
            </button>
          </div>
        </form>
      </div>
    </div>

    <!-- Vehicles List -->
    <div class="vehicles-management">
      <div v-if="loading" class="loading-container">
        <div class="spinner"></div>
        <p>Loading vehicles...</p>
      </div>

      <div v-else-if="error" class="error-container">
        <p class="error-message">{{ error }}</p>
        <button class="btn-retry" @click="fetchVehicles">Retry</button>
      </div>

      <div v-else-if="vehicles.length > 0" class="vehicles-table-container">
        <table class="vehicles-table">
          <thead>
            <tr>
              <th>Brand</th>
              <th>Model</th>
              <th>Year</th>
              <th>Category</th>
              <th>Price/Day</th>
              <th>Status</th>
              <th>License</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="vehicle in vehicles" :key="vehicle.id">
              <td>{{ vehicle.brand }}</td>
              <td>{{ vehicle.model }}</td>
              <td>{{ vehicle.year }}</td>
              <td>{{ vehicle.category }}</td>
              <td>${{ vehicle.pricePerDay.toFixed(2) }}</td>
              <td>
                <span class="status-badge" :class="getStatusClass(vehicle.status)">
                  {{ vehicle.status }}
                </span>
              </td>
              <td>{{ vehicle.licensePlate }}</td>
              <td class="actions-cell">
                <button class="btn-edit" @click="editVehicle(vehicle)">
                  Edit
                </button>
                <button class="btn-delete" @click="deleteVehicle(vehicle.id)">
                  Delete
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <div v-else class="empty-container">
        <p>No vehicles found.</p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import { vehicleApi } from '../services/vehicleService';

const vehicles = ref([]);
const loading = ref(false);
const error = ref(null);
const showModal = ref(false);
const isEditing = ref(false);
const submitting = ref(false);
const formError = ref(null);
const editingId = ref(null);

const formData = ref({
  brand: '',
  model: '',
  year: new Date().getFullYear(),
  category: null,
  pricePerDay: 0,
  transmission: null,
  fuelType: null,
  seats: 5,
  imageUrl: '',
  licensePlate: '',
  mileage: 0,
  color: '',
  description: '',
  status: 1
});

const resetForm = () => {
  formData.value = {
    brand: '',
    model: '',
    year: new Date().getFullYear(),
    category: null,
    pricePerDay: 0,
    transmission: null,
    fuelType: null,
    seats: 5,
    imageUrl: '',
    licensePlate: '',
    mileage: 0,
    color: '',
    description: '',
    status: 1
  };
  isEditing.value = false;
  editingId.value = null;
  formError.value = null;
};

const fetchVehicles = async () => {
  loading.value = true;
  error.value = null;
  
  try {
    console.log('Fetching vehicles from API...');
    const response = await vehicleApi.getAllVehicles();
    console.log('Vehicles response:', response);
    vehicles.value = response.data;
    console.log('Vehicles loaded:', vehicles.value.length);
  } catch (err) {
    console.error('Error fetching vehicles:', err);
    console.error('Error response:', err.response);
    console.error('Error message:', err.message);
    console.error('Error status:', err.response?.status);
    console.error('Error data:', err.response?.data);
    error.value = `Failed to load vehicles: ${err.response?.data?.message || err.message}`;
  } finally {
    loading.value = false;
  }
};

const openCreateModal = () => {
  resetForm();
  showModal.value = true;
};

const closeModal = () => {
  showModal.value = false;
  resetForm();
};

const submitForm = async () => {
  submitting.value = true;
  formError.value = null;

  try {
    // Prepare data without empty optional fields
    const submitData = {
      brand: formData.value.brand,
      model: formData.value.model,
      year: formData.value.year,
      category: formData.value.category,
      pricePerDay: formData.value.pricePerDay,
      transmission: formData.value.transmission,
      fuelType: formData.value.fuelType,
      seats: formData.value.seats,
      licensePlate: formData.value.licensePlate,
      mileage: formData.value.mileage,
      color: formData.value.color
    };

    if (formData.value.imageUrl) {
      submitData.imageUrl = formData.value.imageUrl;
    }

    if (formData.value.description) {
      submitData.description = formData.value.description;
    }

    console.log('Submitting vehicle data:', submitData);

    if (isEditing.value) {
      submitData.status = formData.value.status;
      console.log('Updating vehicle with ID:', editingId.value);
      const response = await vehicleApi.updateVehicle(editingId.value, submitData);
      console.log('Update response:', response);
    } else {
      console.log('Creating new vehicle');
      const response = await vehicleApi.createVehicle(submitData);
      console.log('Create response:', response);
    }

    await fetchVehicles();
    closeModal();
  } catch (err) {
    console.error('Error submitting form:', err);
    console.error('Error response:', err.response);
    console.error('Error status:', err.response?.status);
    console.error('Error data:', err.response?.data);
    console.error('Error headers:', err.response?.headers);
    
    const errorMessage = err.response?.data?.message 
      || err.response?.data?.title
      || err.response?.data?.errors 
      || err.message 
      || 'Failed to save vehicle. Please try again.';
    
    console.error('Final error message:', errorMessage);
    formError.value = typeof errorMessage === 'object' ? JSON.stringify(errorMessage) : errorMessage;
  } finally {
    submitting.value = false;
  }
};

const editVehicle = (vehicle) => {
  isEditing.value = true;
  editingId.value = vehicle.id;
  
  // Map enum names back to numbers
  const categoryMap = { 'Economy': 1, 'Comfort': 2, 'Luxury': 3, 'SUV': 4, 'Van': 5, 'Sport': 6 };
  const transmissionMap = { 'Manual': 1, 'Automatic': 2 };
  const fuelTypeMap = { 'Petrol': 1, 'Diesel': 2, 'Electric': 3, 'Hybrid': 4 };
  const statusMap = { 'Available': 1, 'Rented': 2, 'Maintenance': 3, 'Unavailable': 4 };
  
  formData.value = {
    brand: vehicle.brand,
    model: vehicle.model,
    year: vehicle.year,
    category: categoryMap[vehicle.category] || vehicle.category,
    pricePerDay: vehicle.pricePerDay,
    transmission: transmissionMap[vehicle.transmission] || vehicle.transmission,
    fuelType: fuelTypeMap[vehicle.fuelType] || vehicle.fuelType,
    seats: vehicle.seats,
    imageUrl: vehicle.imageUrl || '',
    licensePlate: vehicle.licensePlate,
    mileage: vehicle.mileage,
    color: vehicle.color,
    description: vehicle.description || '',
    status: statusMap[vehicle.status] || vehicle.status
  };
  
  showModal.value = true;
};

const deleteVehicle = async (id) => {
  if (!confirm('Are you sure you want to delete this vehicle?')) {
    return;
  }

  try {
    await vehicleApi.deleteVehicle(id);
    await fetchVehicles();
  } catch (err) {
    console.error('Error deleting vehicle:', err);
    alert('Failed to delete vehicle. Please try again.');
  }
};

const getStatusClass = (status) => {
  const statusMap = {
    'Available': 'status-available',
    'Rented': 'status-rented',
    'Maintenance': 'status-maintenance',
    'Unavailable': 'status-unavailable'
  };
  return statusMap[status] || '';
};

onMounted(() => {
  fetchVehicles();
});
</script>

<style scoped>
.admin-vehicles-page {
  max-width: 1400px;
  margin: 0 auto;
  padding: 40px 20px;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 40px;
}

.page-header h1 {
  font-size: 32px;
  color: #1f2937;
  margin: 0;
}

.btn-add {
  padding: 12px 24px;
  background: #4f46e5;
  color: white;
  border: none;
  border-radius: 8px;
  font-weight: 600;
  cursor: pointer;
  transition: background 0.3s;
  font-size: 15px;
}

.btn-add:hover {
  background: #4338ca;
}

/* Modal Styles */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
  padding: 20px;
  overflow-y: auto;
}

.modal-content {
  background: white;
  border-radius: 12px;
  width: 100%;
  max-width: 800px;
  max-height: 90vh;
  overflow-y: auto;
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 24px 30px;
  border-bottom: 1px solid #e5e7eb;
}

.modal-header h2 {
  font-size: 24px;
  margin: 0;
  color: #1f2937;
}

.btn-close {
  background: none;
  border: none;
  font-size: 32px;
  color: #6b7280;
  cursor: pointer;
  line-height: 1;
  padding: 0;
  width: 32px;
  height: 32px;
}

.btn-close:hover {
  color: #1f2937;
}

.vehicle-form {
  padding: 30px;
}

.form-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 20px;
  margin-bottom: 24px;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.form-group.full-width {
  grid-column: 1 / -1;
}

.form-group label {
  font-size: 14px;
  font-weight: 600;
  color: #374151;
}

.form-group input,
.form-group select,
.form-group textarea {
  padding: 10px 12px;
  border: 1px solid #d1d5db;
  border-radius: 8px;
  font-size: 14px;
  transition: border-color 0.3s;
}

.form-group input:focus,
.form-group select:focus,
.form-group textarea:focus {
  outline: none;
  border-color: #4f46e5;
  box-shadow: 0 0 0 3px rgba(79, 70, 229, 0.1);
}

.form-error {
  padding: 12px;
  background: #fef2f2;
  border: 1px solid #fecaca;
  border-radius: 8px;
  color: #ef4444;
  margin-bottom: 20px;
  font-size: 14px;
}

.form-actions {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}

.btn-cancel {
  padding: 10px 20px;
  background: white;
  color: #4b5563;
  border: 1px solid #d1d5db;
  border-radius: 8px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s;
}

.btn-cancel:hover {
  background: #f9fafb;
}

.btn-submit {
  padding: 10px 20px;
  background: #4f46e5;
  color: white;
  border: none;
  border-radius: 8px;
  font-weight: 600;
  cursor: pointer;
  transition: background 0.3s;
}

.btn-submit:hover:not(:disabled) {
  background: #4338ca;
}

.btn-submit:disabled {
  background: #9ca3af;
  cursor: not-allowed;
}

/* Table Styles */
.vehicles-table-container {
  background: white;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  overflow: hidden;
}

.vehicles-table {
  width: 100%;
  border-collapse: collapse;
}

.vehicles-table thead {
  background: #f9fafb;
}

.vehicles-table th {
  padding: 16px;
  text-align: left;
  font-weight: 600;
  color: #374151;
  font-size: 14px;
  border-bottom: 2px solid #e5e7eb;
}

.vehicles-table td {
  padding: 16px;
  border-bottom: 1px solid #e5e7eb;
  color: #4b5563;
  font-size: 14px;
}

.vehicles-table tbody tr:hover {
  background: #f9fafb;
}

.status-badge {
  padding: 4px 10px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: 600;
  text-transform: uppercase;
  color: white;
  display: inline-block;
}

.status-available {
  background: #10b981;
}

.status-rented {
  background: #f59e0b;
}

.status-maintenance {
  background: #ef4444;
}

.status-unavailable {
  background: #6b7280;
}

.actions-cell {
  display: flex;
  gap: 8px;
}

.btn-edit,
.btn-delete {
  padding: 6px 12px;
  border: none;
  border-radius: 6px;
  font-weight: 600;
  font-size: 13px;
  cursor: pointer;
  transition: all 0.3s;
}

.btn-edit {
  background: #e0e7ff;
  color: #4f46e5;
}

.btn-edit:hover {
  background: #c7d2fe;
}

.btn-delete {
  background: #fef2f2;
  color: #ef4444;
}

.btn-delete:hover {
  background: #fecaca;
}

/* Loading & Error States */
.loading-container,
.error-container,
.empty-container {
  text-align: center;
  padding: 80px 20px;
}

.spinner {
  width: 50px;
  height: 50px;
  border: 4px solid #e5e7eb;
  border-top-color: #4f46e5;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin: 0 auto 20px;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.error-message {
  color: #ef4444;
  font-size: 18px;
  margin-bottom: 20px;
}

.btn-retry {
  padding: 12px 24px;
  background: #4f46e5;
  color: white;
  border: none;
  border-radius: 8px;
  font-weight: 600;
  cursor: pointer;
}

.btn-retry:hover {
  background: #4338ca;
}

@media (max-width: 768px) {
  .form-grid {
    grid-template-columns: 1fr;
  }

  .vehicles-table-container {
    overflow-x: auto;
  }

  .page-header {
    flex-direction: column;
    gap: 20px;
    align-items: flex-start;
  }
}
</style>
