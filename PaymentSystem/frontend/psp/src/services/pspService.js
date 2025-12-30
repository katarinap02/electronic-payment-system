import api from './api'

export const authService = {
  async login(email, password) {
    const response = await api.post('/auth/login', { email, password })
    return response.data
  }
}

export const webShopService = {
  // SuperAdmin endpoints
  async getAllWebShops() {
    const response = await api.get('/superadmin/webshops')
    return response.data
  },

  async getWebShopById(id) {
    const response = await api.get(`/superadmin/webshops/${id}`)
    return response.data
  },

  async createWebShop(data) {
    const response = await api.post('/superadmin/webshops', data)
    return response.data
  },

  async updateWebShop(id, data) {
    const response = await api.put(`/superadmin/webshops/${id}`, data)
    return response.data
  },

  async deleteWebShop(id) {
    const response = await api.delete(`/superadmin/webshops/${id}`)
    return response.data
  },

  async assignAdmin(webShopId, userId) {
    const response = await api.post(`/superadmin/webshops/${webShopId}/admins/${userId}`)
    return response.data
  },

  async removeAdmin(webShopId, userId) {
    const response = await api.delete(`/superadmin/webshops/${webShopId}/admins/${userId}`)
    return response.data
  },

  async getWebShopAdmins(webShopId) {
    const response = await api.get(`/superadmin/webshops/${webShopId}/admins`)
    return response.data
  },

  // Admin endpoints
  async getMyWebShops() {
    const response = await api.get('/admin/webshops/my-webshops')
    return response.data
  },

  async addPaymentMethodToWebShop(webShopId, paymentMethodId) {
    const response = await api.post(`/admin/webshops/${webShopId}/payment-methods/${paymentMethodId}`)
    return response.data
  },

  async removePaymentMethodFromWebShop(webShopId, paymentMethodId) {
    const response = await api.delete(`/admin/webshops/${webShopId}/payment-methods/${paymentMethodId}`)
    return response.data
  },

  async getWebShopPaymentMethods(webShopId) {
    const response = await api.get(`/admin/webshops/${webShopId}/payment-methods`)
    return response.data
  }
}

export const paymentMethodService = {
  async getAllPaymentMethods() {
    const response = await api.get('/payment-methods')
    return response.data
  },

  async getActivePaymentMethods() {
    const response = await api.get('/payment-methods/active')
    return response.data
  }
}
