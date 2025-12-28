import api from './api';

export const insuranceApi = {
  // Get all insurance packages
  getAllInsurance() {
    return api.get('/insurance');
  },

  // Get active insurance packages
  getActiveInsurance() {
    return api.get('/insurance/active');
  },

  // Get insurance package by ID
  getInsuranceById(id) {
    return api.get(`/insurance/${id}`);
  }
};
