<script setup>
import { useRouter } from 'vue-router';
import { useAuthStore } from './stores/auth';
import { computed, ref, onMounted } from 'vue';

const router = useRouter();
const authStore = useAuthStore();

const showMenuModal = ref(false);

const isAdmin = computed(() => authStore.user?.role === 'Admin');

const toggleMenu = () => {
  showMenuModal.value = !showMenuModal.value;
};

const handleLogout = () => {
  authStore.logout();
  showMenuModal.value = false;
};

const navigateTo = (path) => {
  router.push(path);
  showMenuModal.value = false;
};

onMounted(() => {
  console.log('Auth state:', {
    isAuthenticated: authStore.isAuthenticated,
    user: authStore.user,
    token: authStore.token
  });
});
</script>

<template>
  <div id="app">
    <!-- Fixed Navbar na vrhu -->
    <nav class="navbar">
      <div class="navbar-container">
        <!-- Logo levo -->
        <div class="navbar-brand">
          <router-link to="/vehicles">Rent a Car</router-link>
        </div>
        
        <!-- Navigacioni linkovi u centru -->
        <div class="navbar-links">
          <router-link to="/vehicles" class="nav-link">Vehicles</router-link>
          <router-link v-if="authStore.isAuthenticated" to="/dashboard" class="nav-link">Dashboard</router-link>
          <router-link v-if="authStore.isAuthenticated" to="/rentals" class="nav-link">Moje Rezervacije</router-link>
          <router-link v-if="authStore.isAuthenticated" to="/profile" class="nav-link">Profil</router-link>
          <router-link v-if="isAdmin" to="/admin/vehicles" class="nav-link">Admin</router-link>
        </div>

        <!-- Hamburger meni desno -->
        <button class="hamburger-btn" @click="toggleMenu">
          <span></span>
          <span></span>
          <span></span>
        </button>
      </div>
    </nav>

    <!-- Modal za autentifikaciju -->
    <transition name="fade">
      <div v-if="showMenuModal" class="modal-overlay" @click="showMenuModal = false">
        <div class="modal-box" @click.stop>
          <div class="modal-header">
            <h3>Menu</h3>
            <button class="close-btn" @click="showMenuModal = false">&times;</button>
          </div>
          <div class="modal-body">
            <!-- Ako je ulogovan -->
            <div v-if="authStore.isAuthenticated" class="auth-section">
              <p class="user-name">{{ authStore.userName }}</p>
              <button @click="handleLogout" class="btn btn-logout">Logout</button>
            </div>
            <!-- Ako nije ulogovan -->
            <div v-else class="auth-section">
              <button @click="navigateTo('/login')" class="btn btn-login">Login</button>
              <button @click="navigateTo('/register')" class="btn btn-register">Register</button>
            </div>
          </div>
        </div>
      </div>
    </transition>
    
    <!-- Main sadržaj ispod navbara -->
    <main class="main-container">
      <router-view/>
    </main>
  </div>
</template>

<style>
* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
}

#app {
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  color: #2c3e50;
  min-height: 100vh;
  background: #f9fafb;
}

/* Fixed Navbar - 80px visok, fiksan na vrhu */
.navbar {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  width: 100%;
  height: 80px;
  background: white;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  z-index: 1000;
}

.navbar-container {
  max-width: 1400px;
  margin: 0 auto;
  height: 100%;
  padding: 0 24px;
  display: flex;
  align-items: center;
  justify-content: space-between;
}

/* Logo - levo */
.navbar-brand {
  flex-shrink: 0;
}

.navbar-brand a {
  font-size: 24px;
  font-weight: 700;
  color: #4f46e5;
  text-decoration: none;
  letter-spacing: -0.5px;
}

.navbar-brand a:hover {
  color: #4338ca;
}

/* Navigacioni linkovi - centar */
.navbar-links {
  display: flex;
  align-items: center;
  gap: 12px;
  flex: 1;
  justify-content: center;
}

.nav-link {
  padding: 10px 20px;
  text-decoration: none;
  color: #4b5563;
  font-weight: 500;
  font-size: 15px;
  border-radius: 8px;
  transition: all 0.2s;
  white-space: nowrap;
}

.nav-link:hover {
  background: #f3f4f6;
  color: #1f2937;
}

.nav-link.router-link-active {
  color: #4f46e5;
  background: #e0e7ff;
}

/* Hamburger button - desno */
.hamburger-btn {
  display: flex;
  flex-direction: column;
  justify-content: space-around;
  width: 32px;
  height: 24px;
  background: none;
  border: none;
  cursor: pointer;
  padding: 0;
  flex-shrink: 0;
}

.hamburger-btn span {
  width: 100%;
  height: 3px;
  background-color: #4f46e5;
  border-radius: 2px;
  transition: all 0.3s;
}

.hamburger-btn:hover span {
  background-color: #4338ca;
}

/* Main container - ispod navbara */
.main-container {
  margin-top: 80px;
  min-height: calc(100vh - 80px);
  padding: 0;
}

/* Modal */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  justify-content: center;
  align-items: flex-start;
  padding-top: 100px;
  z-index: 2000;
}

.modal-box {
  background: white;
  border-radius: 12px;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
  max-width: 400px;
  width: 90%;
  animation: slideDown 0.3s ease-out;
}

@keyframes slideDown {
  from {
    opacity: 0;
    transform: translateY(-20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 20px 24px;
  border-bottom: 1px solid #e5e7eb;
}

.modal-header h3 {
  font-size: 20px;
  font-weight: 600;
  color: #1f2937;
  margin: 0;
}

.close-btn {
  background: none;
  border: none;
  font-size: 32px;
  color: #9ca3af;
  cursor: pointer;
  line-height: 1;
  padding: 0;
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 6px;
  transition: all 0.2s;
}

.close-btn:hover {
  background: #f3f4f6;
  color: #4b5563;
}

.modal-body {
  padding: 24px;
}

.auth-section {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.user-name {
  font-size: 16px;
  font-weight: 600;
  color: #1f2937;
  text-align: center;
  padding: 12px;
  background: #f0f9ff;
  border-radius: 8px;
  border-left: 4px solid #4f46e5;
  margin-bottom: 8px;
}

.btn {
  padding: 14px 24px;
  font-size: 16px;
  font-weight: 600;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.2s;
  width: 100%;
}

.btn-login {
  background: #f3f4f6;
  color: #1f2937;
}

.btn-login:hover {
  background: #e5e7eb;
}

.btn-register {
  background: #4f46e5;
  color: white;
}

.btn-register:hover {
  background: #4338ca;
}

.btn-logout {
  background: #ef4444;
  color: white;
}

.btn-logout:hover {
  background: #dc2626;
}

/* Fade transition */
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.3s;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}

/* Responsive */
@media (max-width: 768px) {
  .navbar-container {
    padding: 0 16px;
  }

  .navbar-brand a {
    font-size: 20px;
  }

  .navbar-links {
    gap: 4px;
  }

  .nav-link {
    padding: 8px 12px;
    font-size: 14px;
  }
}

@media (max-width: 480px) {
  .navbar-links {
    display: none;
  }
}
</style>