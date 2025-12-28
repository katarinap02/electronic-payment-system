<template>
  <div class="login">
    <h2>Login</h2>
    
    <div v-if="error" class="error">
      {{ error }}
    </div>
    
    <form @submit.prevent="handleLogin">
      <div>
        <label>Email:</label>
        <input v-model="form.email" type="email" required />
      </div>
      
      <div>
        <label>Password:</label>
        <input v-model="form.password" type="password" required />
      </div>
      
      <button type="submit" :disabled="isLoading">
        {{ isLoading ? 'Login...' : 'Login' }}
      </button>
      
      <p>
        Don't have account? <router-link to="/register">Register</router-link>
      </p>
    </form>
  </div>
</template>

<script>
import { useAuthStore } from '@/stores/auth';

export default {
  data() {
    return {
      form: {
        email: '',
        password: ''
      },
      error: '',
      isLoading: false
    };
  },
  
  methods: {
    async handleLogin() {
      this.isLoading = true;
      this.error = '';
      
      try {
        const authStore = useAuthStore();
        const result = await authStore.login({
          email: this.form.email,
          password: this.form.password
        });
        
        if (result.success) {
          this.$router.push('/dashboard');
        } else {
          this.error = result.error || 'Login failed';
        }
      } catch (err) {
        this.error = 'Error, try again.';
        console.error('Login error:', err);
      } finally {
        this.isLoading = false;
      }
    }
  }
};
</script>

<style scoped>
.login {
  max-width: 400px;
  min-width: 250px;
  padding: 20px;
  border: 1px solid #ccc;
  border-radius: 5px;

   position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
}

h2 {
  text-align: center;
  margin-bottom: 20px;
}

form > div {
  margin-bottom: 15px;
}

label {
  display: block;
  margin-bottom: 5px;
  font-weight: bold;
}

input {
  width: 100%;
  padding: 8px;
  border: 1px solid #ddd;
  border-radius: 4px;
  box-sizing: border-box;
}

.error {
  background-color: #ffe6e6;
  color: #d00;
  padding: 10px;
  border-radius: 4px;
  margin-bottom: 15px;
  border: 1px solid #ff9999;
}

button {
  width: 100%;
  padding: 10px;
  background-color: #28a745;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 16px;
}

button:disabled {
  background-color: #ccc;
  cursor: not-allowed;
}

button:hover:not(:disabled) {
  background-color: #218838;
}

p {
  text-align: center;
  margin-top: 15px;
}

a {
  color: #007bff;
  text-decoration: none;
}

a:hover {
  text-decoration: underline;
}
</style>