import api from './api';

// Enums mapping
export const vehicleCategoryNames = {
  1: "Economy",
  2: "Comfort",
  3: "Luxury",
  4: "SUV",
  5: "Van",
  6: "Sport"
};

export const transmissionNames = {
  1: "Manual",
  2: "Automatic"
};

export const fuelTypeNames = {
  1: "Petrol",
  2: "Diesel",
  3: "Electric",
  4: "Hybrid"
};

export const vehicleStatusNames = {
  1: "Available",
  2: "Rented",
  3: "Maintenance",
  4: "Unavailable"
};

// Vehicle API
export const vehicleApi = {
  // Get all vehicles
  getAllVehicles: () => api.get('/vehicles'),
  
  // Get vehicle by ID
  getVehicleById: (id) => api.get(`/vehicles/${id}`),
  
  // Search/filter vehicles
  searchVehicles: (searchParams) => api.post('/vehicles/search', searchParams),
  
  // Create vehicle (Admin only)
  createVehicle: (vehicleData) => api.post('/vehicles', vehicleData),
  
  // Update vehicle (Admin only)
  updateVehicle: (id, vehicleData) => api.put(`/vehicles/${id}`, vehicleData),
  
  // Delete vehicle (Admin only)
  deleteVehicle: (id) => api.delete(`/vehicles/${id}`),
};

// Helper function to map enum values to names
export const mapVehicleEnums = (vehicle) => {
  return {
    ...vehicle,
    categoryName: vehicleCategoryNames[vehicle.category] || vehicle.category,
    transmissionName: transmissionNames[vehicle.transmission] || vehicle.transmission,
    fuelTypeName: fuelTypeNames[vehicle.fuelType] || vehicle.fuelType,
    statusName: vehicleStatusNames[vehicle.status] || vehicle.status,
  };
};

export default vehicleApi;
