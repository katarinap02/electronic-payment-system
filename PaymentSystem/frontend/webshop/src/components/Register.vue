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
        this.error = 'Lozinke se ne podudaraju!';
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
        
        console.log('Šaljem:', JSON.stringify(requestData, null, 2));
        
        const response = await fetch('/api/auth/register', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(requestData)
        });
        
        console.log('Status odgovora:', response.status);
        
        if (!response.ok) {
          const errorText = await response.text();
          console.error('Greška od backend-a:', errorText);
          console.log(errorText)
          
          try {
            const errorData = JSON.parse(errorText);
            this.error = errorData.message || `Greška (${response.status}): ${errorText}`;
          } catch {
            this.error = `Greška (${response.status}): ${errorText}`;
          }
          return;
        }
        
        const data = await response.json();
        console.log('Uspešan odgovor:', data);
        
        if (data.success || response.status === 200) {
          alert('Uspešno ste registrovani! Sada se možete prijaviti.');
          this.$router.push('/login');
        } else {
          this.error = data.message || 'Registracija nije uspela';
        }
      } catch (err) {
        console.error('Mrežna greška:', err);
        this.error = 'Došlo je do greške. Pokušajte ponovo.';
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

.mismatch {
  color: #d00;
  font-size: 14px;
  margin-top: 5px;
}

button {
  width: 100%;
  padding: 10px;
  background-color:  #42b983;;
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
  background-color: #0056b3;
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