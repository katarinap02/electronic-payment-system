import { defineStore } from 'pinia';
import { authApi } from '@/services/api';
import router from '@/router';

export const useAuthStore = defineStore('auth', {
  state: () => ({
    user: null,
    token: localStorage.getItem('token') || null,
    isLoading: false,
    error: null,
  }),

  getters: {
    isAuthenticated: (state) => !!state.token,
    userName: (state) => state.user ? `${state.user.name} ${state.user.surname}` : '',
  },

  actions: {
    async register(userData) {
      this.isLoading = true;
      this.error = null;
      
      try {
        const response = await authApi.register(userData);
        
        if (response.data.success) {
          // Snimi token i user
          localStorage.setItem('token', response.data.data.token);
          this.token = response.data.data.token;
          this.user = response.data.data.user;
          
          // Preusmeri na dashboard
          router.push('/dashboard');
          return { success: true };
        }
      } catch (error) {
        this.error = error.response?.data?.message || 'Registracija nije uspela';
        return { success: false, error: this.error };
      } finally {
        this.isLoading = false;
      }
    },

    async login(credentials) {
      this.isLoading = true;
      this.error = null;
      
      try {
        const response = await authApi.login(credentials);
        
        if (response.data.success) {
          localStorage.setItem('token', response.data.data.token);
          this.token = response.data.data.token;
          this.user = response.data.data.user;
          
          router.push('/dashboard');
          return { success: true };
        }
      } catch (error) {
        this.error = error.response?.data?.message || 'Prijava nije uspela';
        return { success: false, error: this.error };
      } finally {
        this.isLoading = false;
      }
    },

    logout() {
      localStorage.removeItem('token');
      this.token = null;
      this.user = null;
      router.push('/login');
    },

  },
});