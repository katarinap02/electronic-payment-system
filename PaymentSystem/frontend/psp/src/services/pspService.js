import api from './api'

export const authService = {
  async login(email, password) {
    const response = await api.post('/auth/login', { email, password })
    return response.data
  }
}

export const webShopService = {
  async getAllWebShops() {
    const response = await api.get('/webshops')
    return response.data
  },

  async getWebShopById(id) {
    const response = await api.get(`/webshops/${id}`)
    return response.data
  },

  async updatePaymentMethods(id, paymentMethodIds) {
    const response = await api.put(`/webshops/${id}/payment-methods`, { paymentMethodIds })
    return response.data
  },

  async getAllPaymentMethods() {
    const response = await api.get('/webshops/payment-methods')
    return response.data
  }
}
