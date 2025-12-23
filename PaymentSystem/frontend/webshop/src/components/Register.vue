<template>
  <div class="register">
    <h2>Register</h2>
    
    <div v-if="error" class="error">
      {{ error }}
    </div>
    
    <form @submit.prevent="handleRegister">
      <div>
        <label>Email:</label>
        <input v-model="form.email" type="email" required />
      </div>
      
      <div>
        <label>Name:</label>
        <input v-model="form.name" type="text" required />
      </div>
      
      <div>
        <label>Surname:</label>
        <input v-model="form.surname" type="text" required />
      </div>
      
      <div>
        <label>Password:</label>
        <input v-model="form.password" type="password" required />
        <div class="password-hint">
          Password must contain: at least 8 characters, one uppercase letter, one number, and one special character.
        </div>
      </div>
      
      <div>
        <label>Confirm password:</label>
        <input v-model="form.confirmPassword" type="password" required />
        <div v-if="form.password && form.confirmPassword && !passwordsMatch" class="mismatch">
          Password doesn't match!
        </div>
      </div>
      
      <button type="submit" :disabled="isLoading || !passwordsMatch">
        {{ isLoading ? 'Register...' : 'Register' }}
      </button>
      
      <p>
        Already have account? <router-link to="/login">Login</router-link>
      </p>
    </form>
  </div>
</template>

<script>
export default {
  data() {
    return {
      form: {
        email: '',
        name: '',
        surname: '',
        password: '',
        confirmPassword: ''
      },
      error: '',
      isLoading: false
    };
  },
  
  computed: {
    passwordsMatch() {
      return this.form.password === this.form.confirmPassword;
    }
  },
  
  methods: {
    async handleRegister() {
      if (!this.passwordsMatch) {
        this.error = 'Passwords do not match!';
        return;
      }
      
      this.isLoading = true;
      this.error = '';
      
      try {
        const requestData = {
          Email: this.form.email,
          Password: this.form.password,
          Name: this.form.name,
          Surname: this.form.surname
        };
        
        console.log('Sending:', JSON.stringify(requestData, null, 2));
        
        const response = await fetch('/api/auth/register', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(requestData)
        });
        
        console.log('Response status:', response.status);
        
        if (!response.ok) {
          const errorText = await response.text();
          console.error('Backend error:', errorText);
          
          try {
            const errorData = JSON.parse(errorText);
            this.error = errorData.message || `Error (${response.status}): ${errorText}`;
          } catch {
            this.error = `Error (${response.status}): ${errorText}`;
          }
          return;
        }
        
        const data = await response.json();
        console.log('Successful response:', data);
        
        if (data.success || response.status === 200) {
          alert('Successfully registered! You can now login.');
          this.$router.push('/login');
        } else {
          this.error = data.message || 'Registration failed';
        }
      } catch (err) {
        console.error('Network error:', err);
        this.error = 'An error occurred. Please try again.';
      } finally {
        this.isLoading = false;
      }
    }
  }
};
</script>

<style scoped>
.register {
  max-width: 400px;
  min-width: 400px;
  padding: 25px;
  border: 1px solid #ccc;
  border-radius: 8px;
  background-color: white;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
}

h2 {
  text-align: center;
  margin-bottom: 20px;
  color: #2c3e50;
}

form > div {
  margin-bottom: 18px;
}

label {
  display: block;
  margin-bottom: 6px;
  font-weight: 500;
  color: #555;
}

input {
  width: 100%;
  padding: 10px 12px;
  border: 1px solid #ddd;
  border-radius: 4px;
  box-sizing: border-box;
  font-size: 16px;
  transition: border-color 0.3s;
}

input:focus {
  outline: none;
  border-color: #42b983;
  box-shadow: 0 0 0 2px rgba(66, 185, 131, 0.1);
}

.password-hint {
  font-size: 12px;
  color: #666;
  margin-top: 5px;
  padding: 6px 8px;
  background-color: #f8f9fa;
  border-radius: 4px;
  border-left: 3px solid #42b983;
}

.error {
  background-color: #ffe6e6;
  color: #d00;
  padding: 12px;
  border-radius: 4px;
  margin-bottom: 15px;
  border: 1px solid #ff9999;
  text-align: center;
}

.mismatch {
  color: #d00;
  font-size: 14px;
  margin-top: 5px;
  font-weight: 500;
}

button {
  width: 100%;
  padding: 12px;
  background-color: #42b983;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 16px;
  font-weight: 500;
  transition: background-color 0.3s;
}

button:hover:not(:disabled) {
  background-color: #3aa876;
}

button:disabled {
  background-color: #ccc;
  cursor: not-allowed;
}

p {
  text-align: center;
  margin-top: 20px;
  color: #666;
}

a {
  color: #42b983;
  text-decoration: none;
  font-weight: 500;
}

a:hover {
  text-decoration: underline;
  color: #3aa876;
}

</style>