import axios from 'axios';

// Bank API je na portu 80 unutar Docker network-a
const API_BASE = '';

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