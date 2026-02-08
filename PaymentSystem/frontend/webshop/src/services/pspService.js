import axios from 'axios'

const PSP_API_URL = 'http://localhost:5002/api'

// These should match WebShop registration in PSP
const MERCHANT_ID = 'WEBSHOP_001'
const MERCHANT_PASSWORD = 'webshop-api-key-change-this'

export const pspService = {
  async initializePayment(orderData) {
    const response = await axios.post(`${PSP_API_URL}/payments/initialize`, {
      merchantId: MERCHANT_ID,
      merchantPassword: MERCHANT_PASSWORD,
      amount: orderData.amount,
      currency: orderData.currency,
      merchantOrderId: orderData.orderId,
      customerId: orderData.customerId,
      merchantTimestamp: new Date().toISOString()
    })
    return response.data
  }
}
