import axios from 'axios';

const API_BASE_URL = import.meta.env.VITE_CRYPTO_API_URL || 'http://localhost:5004';

export default {
  async getTransactionStatus(cryptoPaymentId) {
    try {
      const response = await axios.get(`${API_BASE_URL}/api/crypto/transaction/${cryptoPaymentId}`);
      return response.data;
    } catch (error) {
      console.error('Error fetching transaction status:', error);
      throw error;
    }
  },

  async cancelPayment(cryptoPaymentId) {
    try {
      const response = await axios.post(`${API_BASE_URL}/api/crypto/cancel/${cryptoPaymentId}`);
      return response.data;
    } catch (error) {
      console.error('Error cancelling payment:', error);
      throw error;
    }
  }
};
