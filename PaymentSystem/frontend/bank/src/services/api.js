import axios from 'axios';

// Bank API URL - koristi environment varijablu ili prazan string za proxy
const API_BASE = import.meta.env.VITE_API_BASE_URL || '';

const api = axios.create({
  baseURL: API_BASE,
  headers: {
    'Content-Type': 'application/json'
  }
});

export const paymentApi = {
  getPaymentForm: (paymentId) => api.get(`/api/payment/form/${paymentId}`),
  processPayment: (data) => api.post('/api/payment/process', data)
};

export default api;