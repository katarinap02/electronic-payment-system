<template>
  <div class="dashboard-container">
    <h2 class="dashboard-title">Dashboard</h2>
    <p class="welcome-message">Welcome to your account!</p>
    
    <div v-if="userEmail" class="user-info">
      <p><strong>Logged in as:</strong> {{ userEmail }}</p>
    </div>
    
    <button @click="logout" class="logout-btn">
      Logout
    </button>
  </div>
</template>

<script>
export default {
  name: 'Dashboard',
  
  data() {
    return {
      userEmail: ''
    };
  },
  
  mounted() {
    // Get user info from localStorage if available
    const user = localStorage.getItem('user');
    if (user) {
      try {
        const userData = JSON.parse(user);
        this.userEmail = userData.name + ' ' + userData.surname || '';
      } catch (e) {
        console.error('Error parsing user data:', e);
      }
    }
  },
  
  methods: {
    logout() {
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      this.$router.push('/login');
    }
  }
};
</script>

<style scoped>
.dashboard-container {
  max-width: 500px;
  padding: 40px 30px;
  background-color: #f8f9fa;
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08);
  text-align: center;
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
}

.dashboard-title {
  color: #2c3e50;
  font-size: 2.2rem;
  margin-bottom: 20px;
  font-weight: 600;
  border-bottom: 2px solid #42b983;
  padding-bottom: 10px;
  display: inline-block;
}

.welcome-message {
  color: #5a6c7d;
  font-size: 1.2rem;
  margin-bottom: 30px;
  line-height: 1.5;
}

.user-info {
  background-color: white;
  padding: 20px;
  border-radius: 8px;
  margin: 25px 0;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
  border-left: 4px solid #42b983;
}

.user-info p {
  margin: 0;
  color: #34495e;
  font-size: 1.1rem;
}

.user-info strong {
  color: #2c3e50;
}

.logout-btn {
  padding: 12px 28px;
  background: linear-gradient(135deg, #dc3545, #c82333);
  color: white;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-size: 1rem;
  font-weight: 500;
  transition: all 0.3s ease;
  box-shadow: 0 3px 8px rgba(220, 53, 69, 0.25);
  margin-top: 20px;
  min-width: 150px;
}

.logout-btn:hover {
  background: linear-gradient(135deg, #c82333, #b21f2d);
  transform: translateY(-2px);
  box-shadow: 0 5px 12px rgba(220, 53, 69, 0.3);
}

.logout-btn:active {
  transform: translateY(0);
}

/* Responsive design */
@media (max-width: 600px) {
  .dashboard-container {
    margin: 30px 20px;
    padding: 30px 20px;
  }
  
  .dashboard-title {
    font-size: 1.8rem;
  }
  
  .welcome-message {
    font-size: 1.1rem;
  }
}
</style>