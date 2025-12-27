<template>
  <div class="vehicles-page">
    <div class="page-header">
      <h1>Vehicle Fleet</h1>
      <p>Browse our collection of premium vehicles</p>
    </div>

    <!-- Search and Filter Section -->
    <div class="filters-container">
      <div class="filters-header">
        <h2>Find Your Perfect Vehicle</h2>
        <button class="btn-reset" @click="resetFilters" v-if="hasActiveFilters">
          Clear Filters
        </button>
      </div>

      <div class="filters-grid">
        <div class="filter-group">
          <label>Category</label>
          <select v-model="filters.category">
            <option :value="null">All Categories</option>
            <option value="1">Economy</option>
            <option value="2">Comfort</option>
            <option value="3">Luxury</option>
            <option value="4">SUV</option>
            <option value="5">Van</option>
            <option value="6">Sport</option>
          </select>
        </div>

        <div class="filter-group">
          <label>Transmission</label>
          <select v-model="filters.transmission">
            <option :value="null">All</option>
            <option value="1">Manual</option>
            <option value="2">Automatic</option>
          </select>
        </div>

        <div class="filter-group">
          <label>Fuel Type</label>
          <select v-model="filters.fuelType">
            <option :value="null">All</option>
            <option value="1">Petrol</option>
            <option value="2">Diesel</option>
            <option value="3">Electric</option>
            <option value="4">Hybrid</option>
          </select>
        </div>

        <div class="filter-group">
          <label>Status</label>
          <select v-model="filters.status">
            <option :value="null">All</option>
            <option value="1">Available</option>
            <option value="2">Rented</option>
            <option value="3">Maintenance</option>
            <option value="4">Unavailable</option>
          </select>
        </div>

        <div class="filter-group">
          <label>Min Seats</label>
          <input type="number" v-model.number="filters.minSeats" min="2" max="50" />
        </div>

        <div class="filter-group">
          <label>Max Seats</label>
          <input type="number" v-model.number="filters.maxSeats" min="2" max="50" />
        </div>

        <div class="filter-group">
          <label>Min Price ($/day)</label>
          <input type="number" v-model.number="filters.minPrice" min="0" step="0.01" />
        </div>

        <div class="filter-group">
          <label>Max Price ($/day)</label>
          <input type="number" v-model.number="filters.maxPrice" min="0" step="0.01" />
        </div>

        <div class="filter-group">
          <label>Brand</label>
          <input type="text" v-model="filters.brand" placeholder="e.g. BMW, Mercedes" />
        </div>

        <div class="filter-group">
          <label>Year</label>
          <input type="number" v-model.number="filters.year" min="1900" max="2100" />
        </div>
      </div>

      <div class="filters-actions">
        <button class="btn-search" @click="applyFilters">
          Search Vehicles
        </button>
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="loading" class="loading-container">
      <div class="spinner"></div>
      <p>Loading vehicles...</p>
    </div>

    <!-- Error State -->
    <div v-else-if="error" class="error-container">
      <p class="error-message">{{ error }}</p>
      <button class="btn-retry" @click="fetchVehicles">Retry</button>
    </div>

    <!-- Vehicles Grid -->
    <div v-else-if="vehicles.length > 0" class="vehicles-section">
      <div class="vehicles-info">
        <p>Found {{ vehicles.length }} vehicle(s)</p>
      </div>
      <div class="vehicles-grid">
        <VehicleCard
          v-for="vehicle in vehicles"
          :key="vehicle.id"
          :vehicle="vehicle"
          @view-details="viewVehicleDetails"
        />
      </div>
    </div>

    <!-- Empty State -->
    <div v-else class="empty-container">
      <p>No vehicles found matching your criteria.</p>
      <button class="btn-reset" @click="resetFilters">Reset Filters</button>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import VehicleCard from './VehicleCard.vue';
import { vehicleApi } from '../services/vehicleService';

const router = useRouter();

const vehicles = ref([]);
const loading = ref(false);
const error = ref(null);

const filters = ref({
  category: null,
  transmission: null,
  fuelType: null,
  minPrice: null,
  maxPrice: null,
  minSeats: null,
  maxSeats: null,
  status: null,
  brand: '',
  year: null
});

const hasActiveFilters = computed(() => {
  return Object.entries(filters.value).some(([key, value]) => {
    if (key === 'brand') return value !== '';
    return value !== null;
  });
});

const fetchVehicles = async () => {
  loading.value = true;
  error.value = null;
  
  try {
    const response = await vehicleApi.getAllVehicles();
    vehicles.value = response.data.data || response.data;
  } catch (err) {
    console.error('Error fetching vehicles:', err);
    error.value = 'Failed to load vehicles. Please try again.';
  } finally {
    loading.value = false;
  }
};

const applyFilters = async () => {
  loading.value = true;
  error.value = null;

  try {
    // Build search params - only include non-null/non-empty values
    const searchParams = {};
    
    Object.entries(filters.value).forEach(([key, value]) => {
      if (key === 'brand') {
        if (value && value.trim() !== '') {
          searchParams[key] = value.trim();
        }
      } else if (value !== null && value !== '') {
        searchParams[key] = value;
      }
    });

    // If no filters are active, just get all vehicles
    if (Object.keys(searchParams).length === 0) {
      await fetchVehicles();
      return;
    }

    const response = await vehicleApi.searchVehicles(searchParams);
    vehicles.value = response.data.data || response.data;
  } catch (err) {
    console.error('Error searching vehicles:', err);
    error.value = 'Failed to search vehicles. Please try again.';
  } finally {
    loading.value = false;
  }
};

const resetFilters = () => {
  filters.value = {
    category: null,
    transmission: null,
    fuelType: null,
    minPrice: null,
    maxPrice: null,
    minSeats: null,
    maxSeats: null,
    status: null,
    brand: '',
    year: null
  };
  fetchVehicles();
};

const viewVehicleDetails = (vehicleId) => {
  router.push(`/vehicles/${vehicleId}`);
};

onMounted(() => {
  fetchVehicles();
});
</script>

<style scoped>
.vehicles-page {
  max-width: 1400px;
  margin: 0 auto;
  padding: 40px 20px;
}

.page-header {
  text-align: center;
  margin-bottom: 40px;
}

.page-header h1 {
  font-size: 36px;
  color: #1f2937;
  margin: 0 0 12px 0;
  font-weight: 700;
}

.page-header p {
  font-size: 18px;
  color: #6b7280;
  margin: 0;
}

/* Filters Section */
.filters-container {
  background: white;
  border-radius: 12px;
  padding: 30px;
  margin-bottom: 40px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.filters-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
}

.filters-header h2 {
  font-size: 24px;
  color: #1f2937;
  margin: 0;
  font-weight: 600;
}

.filters-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 20px;
  margin-bottom: 24px;
}

.filter-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.filter-group label {
  font-size: 14px;
  font-weight: 600;
  color: #374151;
}

.filter-group select,
.filter-group input {
  padding: 10px 12px;
  border: 1px solid #d1d5db;
  border-radius: 8px;
  font-size: 14px;
  transition: border-color 0.3s;
  background: white;
}

.filter-group select:focus,
.filter-group input:focus {
  outline: none;
  border-color: #4f46e5;
  box-shadow: 0 0 0 3px rgba(79, 70, 229, 0.1);
}

.filters-actions {
  display: flex;
  justify-content: center;
  gap: 12px;
}

.btn-search {
  padding: 12px 32px;
  background: #4f46e5;
  color: white;
  border: none;
  border-radius: 8px;
  font-weight: 600;
  font-size: 16px;
  cursor: pointer;
  transition: background 0.3s;
}

.btn-search:hover {
  background: #4338ca;
}

.btn-reset {
  padding: 8px 16px;
  background: #ef4444;
  color: white;
  border: none;
  border-radius: 8px;
  font-weight: 600;
  font-size: 14px;
  cursor: pointer;
  transition: background 0.3s;
}

.btn-reset:hover {
  background: #dc2626;
}

/* Vehicles Section */
.vehicles-section {
  margin-top: 40px;
}

.vehicles-info {
  margin-bottom: 20px;
  text-align: center;
}

.vehicles-info p {
  font-size: 16px;
  color: #6b7280;
  font-weight: 500;
}

.vehicles-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
  gap: 24px;
}

/* Loading State */
.loading-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 80px 20px;
  gap: 20px;
}

.spinner {
  width: 50px;
  height: 50px;
  border: 4px solid #e5e7eb;
  border-top-color: #4f46e5;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.loading-container p {
  font-size: 16px;
  color: #6b7280;
}

/* Error State */
.error-container {
  text-align: center;
  padding: 80px 20px;
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
  transition: background 0.3s;
}

.btn-retry:hover {
  background: #4338ca;
}

/* Empty State */
.empty-container {
  text-align: center;
  padding: 80px 20px;
}

.empty-container p {
  font-size: 18px;
  color: #6b7280;
  margin-bottom: 20px;
}

/* Responsive Design */
@media (max-width: 768px) {
  .page-header h1 {
    font-size: 28px;
  }

  .filters-grid {
    grid-template-columns: 1fr;
  }

  .vehicles-grid {
    grid-template-columns: 1fr;
  }

  .filters-header {
    flex-direction: column;
    gap: 16px;
    align-items: flex-start;
  }
}
</style>
