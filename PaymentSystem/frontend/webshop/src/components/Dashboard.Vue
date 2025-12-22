<template>
  <div class="dashboard">
    <h2>Dashboard</h2>
    <p>Dobrodošli!</p>
    <button @click="logout">Odjavi se</button>
  </div>
</template>

<script>
export default {
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
.dashboard {
  max-width: 400px;
  margin: 50px auto;
  padding: 20px;
  text-align: center;
}

button {
  padding: 10px 20px;
  background-color: #dc3545;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  margin-top: 20px;
}

button:hover {
  background-color: #c82333;
}
</style>