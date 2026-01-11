import { defineStore } from 'pinia';
import { authApi } from '@/services/api';
import router from '@/router';

// Helper funkcija za učitavanje iz localStorage
const loadFromStorage = (key) => {
  try {
    const item = localStorage.getItem(key);
    if (key === 'user' && item) {
      return JSON.parse(item);
    }
    return item;
  } catch (error) {
    console.error(`Error loading ${key} from localStorage:`, error);
    return null;
  }
};

export const useAuthStore = defineStore('auth', {
  state: () => ({
    user: loadFromStorage('user'),
    token: loadFromStorage('token'),
    isLoading: false,
    error: null,
  }),

  getters: {
    isAuthenticated: (state) => {
      const hasToken = !!state.token;
      const hasUser = !!state.user;
      console.log('isAuthenticated check:', { hasToken, hasUser, token: state.token, user: state.user });
      return hasToken && hasUser;
    },
    userName: (state) => state.user ? `${state.user.name} ${state.user.surname}` : '',
    userRole: (state) => state.user?.role || null,
  },

  actions: {
    async register(userData) {
      this.isLoading = true;
      this.error = null;
      
      try {
        const response = await authApi.register(userData);
        
        if (response.data.success) {
          const { token, user } = response.data.data;
          
          // Snimi token i user u localStorage
          localStorage.setItem('token', token);
          localStorage.setItem('user', JSON.stringify(user));
          
          // Koristi $patch za reaktivno ažuriranje stanja
          this.$patch({
            token: token,
            user: user
          });
          
          console.log('Register success - State updated:', { token: this.token, user: this.user });
          
          // Sačekaj malo da se state propagira, zatim preusmeri
          await new Promise(resolve => setTimeout(resolve, 100));
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
        console.log('Full login response:', JSON.stringify(response.data, null, 2));
        
        // Proveri različite formate odgovora
        let token, user;
        
        if (response.data.success && response.data.data) {
          // Format: { success: true, data: { token, user } }
          token = response.data.data.token;
          user = response.data.data.user;
        } else if (response.data.token) {
          // Format: { token, user }
          token = response.data.token;
          user = response.data.user;
        } else {
          console.error('Unexpected response format:', response.data);
          this.error = 'Invalid response format from server';
          return { success: false, error: this.error };
        }
        
        if (token && user) {
          // Snimi token i user u localStorage
          localStorage.setItem('token', token);
          localStorage.setItem('user', JSON.stringify(user));
          
          // Koristi $patch za reaktivno ažuriranje stanja
          this.$patch({
            token: token,
            user: user
          });
          
          console.log('Login success - State updated:', { token: this.token, user: this.user });
          console.log('localStorage check:', {
            token: localStorage.getItem('token'),
            user: localStorage.getItem('user')
          });
          
          // Sačekaj malo da se state propagira, zatim preusmeri
          await new Promise(resolve => setTimeout(resolve, 100));
          router.push('/dashboard');
          return { success: true };
        } else {
          this.error = 'Token or user data missing';
          return { success: false, error: this.error };
        }
      } catch (error) {
        console.error('Login error:', error);
        this.error = error.response?.data?.message || 'Prijava nije uspela';
        return { success: false, error: this.error };
      } finally {
        this.isLoading = false;
      }
    },

    logout() {
      console.log('Logout called - clearing localStorage and state');
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      
      // Koristi $patch za reaktivno ažuriranje
      this.$patch({
        token: null,
        user: null,
        error: null
      });
      
      console.log('After logout - State:', {
        token: this.token,
        user: this.user,
        isAuthenticated: this.isAuthenticated
      });
      console.log('After logout - localStorage:', {
        token: localStorage.getItem('token'),
        user: localStorage.getItem('user')
      });
      
      router.push('/login');
    },

  },
});