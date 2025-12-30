import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { authService } from '@/services/pspService'

export const useAuthStore = defineStore('auth', () => {
  const token = ref(localStorage.getItem('psp_token') || null)
  const user = ref(JSON.parse(localStorage.getItem('psp_user') || 'null'))

  const isAuthenticated = computed(() => !!token.value)

  async function login(email, password) {
    try {
      const response = await authService.login(email, password)
      token.value = response.token
      user.value = {
        email: response.email,
        name: response.name,
        role: response.role
      }
      
      localStorage.setItem('psp_token', response.token)
      localStorage.setItem('psp_user', JSON.stringify(user.value))
      
      return true
    } catch (error) {
      console.error('Login failed:', error)
      return false
    }
  }

  function logout() {
    token.value = null
    user.value = null
    localStorage.removeItem('psp_token')
    localStorage.removeItem('psp_user')
  }

  return {
    token,
    user,
    isAuthenticated,
    login,
    logout
  }
})
