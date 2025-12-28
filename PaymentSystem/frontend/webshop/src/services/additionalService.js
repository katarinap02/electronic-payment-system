import api from './api';

export const additionalServiceApi = {
  // Get all additional services
  getAllServices() {
    return api.get('/services');
  },

  // Get available additional services
  getAvailableServices() {
    return api.get('/services/available');
  },

  // Get service by ID
  getServiceById(id) {
    return api.get(`/services/${id}`);
  }
};
