import api from './api';

const rentalService = {
  /**
   * Kreira novi rental zapis nakon uspešne uplate
   */
  async createRental(rentalData) {
    try {
      const response = await api.post('/rentals', rentalData);
      return response.data;
    } catch (error) {
      console.error('Error creating rental:', error);
      throw error;
    }
  },

  /**
   * Dohvata sve aktivne rentale za korisnika
   */
  async getActiveRentals(userId) {
    try {
      const response = await api.get(`/rentals/user/${userId}/active`);
      return response.data;
    } catch (error) {
      console.error('Error fetching active rentals:', error);
      throw error;
    }
  },

  /**
   * Dohvata istoriju rentala za korisnika
   */
  async getRentalHistory(userId) {
    try {
      const response = await api.get(`/rentals/user/${userId}/history`);
      return response.data;
    } catch (error) {
      console.error('Error fetching rental history:', error);
      throw error;
    }
  },

  /**
   * Dohvata sve rentale za korisnika (aktivni + istorija)
   */
  async getAllUserRentals(userId) {
    try {
      const response = await api.get(`/rentals/user/${userId}`);
      return response.data;
    } catch (error) {
      console.error('Error fetching all rentals:', error);
      throw error;
    }
  },

  /**
   * Dohvata rental po ID-u
   */
  async getRentalById(id) {
    try {
      const response = await api.get(`/rentals/${id}`);
      return response.data;
    } catch (error) {
      console.error('Error fetching rental:', error);
      throw error;
    }
  },

  /**
   * Dohvata rental po payment ID-u
   */
  async getRentalByPaymentId(paymentId) {
    try {
      const response = await api.get(`/rentals/payment/${paymentId}`);
      return response.data;
    } catch (error) {
      console.error('Error fetching rental by payment ID:', error);
      throw error;
    }
  },

  /**
   * Ažurira status rentala
   */
  async updateRentalStatus(id, status) {
    try {
      const response = await api.put(`/rentals/${id}/status`, { status });
      return response.data;
    } catch (error) {
      console.error('Error updating rental status:', error);
      throw error;
    }
  },

  /**
   * Otkazuje rental
   */
  async cancelRental(id) {
    try {
      const response = await api.delete(`/rentals/${id}`);
      return response.data;
    } catch (error) {
      console.error('Error cancelling rental:', error);
      throw error;
    }
  }
};

export default rentalService;
