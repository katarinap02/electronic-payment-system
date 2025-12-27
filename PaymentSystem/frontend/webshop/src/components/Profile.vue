<template>
  <div class="profile-container">
    <h2 class="profile-title">Moj Profil</h2>
    
    <div v-if="authStore.isAuthenticated && authStore.user" class="profile-card">
      <div class="profile-header">
        <div class="avatar">
          {{ authStore.user.name?.charAt(0)}}{{ authStore.user.surname?.charAt(0) }}
        </div>
        <div class="header-info">
          <h3>{{ authStore.user.name }} {{ authStore.user.surname }}</h3>
          <span class="role-badge" :class="authStore.user.role?.toLowerCase()">
            {{ authStore.user.role }}
          </span>
        </div>
      </div>

      <div class="profile-details">
        <div class="detail-row">
          <span class="detail-label">ID:</span>
          <span class="detail-value">{{ authStore.user.id }}</span>
        </div>
        
        <div class="detail-row">
          <span class="detail-label">Email:</span>
          <span class="detail-value">{{ authStore.user.email }}</span>
        </div>
        
        <div class="detail-row">
          <span class="detail-label">Ime:</span>
          <span class="detail-value">{{ authStore.user.name }}</span>
        </div>
        
        <div class="detail-row">
          <span class="detail-label">Prezime:</span>
          <span class="detail-value">{{ authStore.user.surname }}</span>
        </div>
        
        <div class="detail-row">
          <span class="detail-label">Uloga:</span>
          <span class="detail-value">{{ authStore.user.role }}</span>
        </div>
      </div>

      <div class="profile-actions">
        <button @click="goToDashboard" class="btn btn-primary">
          Nazad na Dashboard
        </button>
        <button @click="handleLogout" class="btn btn-danger">
          Odjavi se
        </button>
      </div>
    </div>

    <div v-else class="error-message">
      <p>Niste ulogovani. Molimo prijavite se.</p>
      <button @click="goToLogin" class="btn btn-primary">
        Prijavi se
      </button>
    </div>
  </div>
</template>

<script setup>
import { useAuthStore } from '@/stores/auth';
import { useRouter } from 'vue-router';

const authStore = useAuthStore();
const router = useRouter();

const handleLogout = () => {
  authStore.logout();
};

const goToLogin = () => {
  router.push('/login');
};

const goToDashboard = () => {
  router.push('/dashboard');
};
</script>

<style scoped>
.profile-container {
  max-width: 700px;
  margin: 50px auto;
  padding: 30px;
}

.profile-title {
  color: #2c3e50;
  font-size: 2.5rem;
  margin-bottom: 30px;
  text-align: center;
  font-weight: 600;
}

.profile-card {
  background-color: white;
  border-radius: 16px;
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.1);
  overflow: hidden;
}

.profile-header {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  padding: 40px 30px;
  display: flex;
  align-items: center;
  gap: 25px;
  color: white;
}

.avatar {
  width: 80px;
  height: 80px;
  border-radius: 50%;
  background-color: rgba(255, 255, 255, 0.3);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 2rem;
  font-weight: bold;
  border: 3px solid white;
  text-transform: uppercase;
}

.header-info h3 {
  margin: 0 0 10px 0;
  font-size: 1.8rem;
  font-weight: 600;
}

.role-badge {
  display: inline-block;
  padding: 6px 16px;
  border-radius: 20px;
  font-size: 0.85rem;
  font-weight: 500;
  background-color: rgba(255, 255, 255, 0.2);
  backdrop-filter: blur(10px);
}

.role-badge.customer {
  background-color: rgba(66, 185, 131, 0.9);
}

.role-badge.admin {
  background-color: rgba(231, 76, 60, 0.9);
}

.profile-details {
  padding: 30px;
}

.detail-row {
  display: flex;
  justify-content: space-between;
  padding: 18px 0;
  border-bottom: 1px solid #e8e8e8;
}

.detail-row:last-child {
  border-bottom: none;
}

.detail-label {
  font-weight: 600;
  color: #5a6c7d;
  font-size: 1rem;
}

.detail-value {
  color: #2c3e50;
  font-size: 1rem;
  text-align: right;
}

.profile-actions {
  padding: 20px 30px 30px;
  display: flex;
  gap: 15px;
  justify-content: center;
}

.btn {
  padding: 12px 28px;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  font-size: 1rem;
  font-weight: 500;
  transition: all 0.3s ease;
  min-width: 150px;
}

.btn-primary {
  background: linear-gradient(135deg, #667eea, #764ba2);
  color: white;
  box-shadow: 0 4px 12px rgba(102, 126, 234, 0.4);
}

.btn-primary:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 16px rgba(102, 126, 234, 0.5);
}

.btn-danger {
  background: linear-gradient(135deg, #dc3545, #c82333);
  color: white;
  box-shadow: 0 4px 12px rgba(220, 53, 69, 0.4);
}

.btn-danger:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 16px rgba(220, 53, 69, 0.5);
}

.btn:active {
  transform: translateY(0);
}

.error-message {
  background-color: #fff3cd;
  border: 1px solid #ffc107;
  border-radius: 12px;
  padding: 40px;
  text-align: center;
}

.error-message p {
  color: #856404;
  font-size: 1.2rem;
  margin-bottom: 20px;
}

/* Responsive design */
@media (max-width: 768px) {
  .profile-container {
    padding: 20px 15px;
  }

  .profile-header {
    flex-direction: column;
    text-align: center;
    padding: 30px 20px;
  }

  .header-info h3 {
    font-size: 1.5rem;
  }

  .avatar {
    width: 70px;
    height: 70px;
    font-size: 1.6rem;
  }

  .detail-row {
    flex-direction: column;
    gap: 8px;
  }

  .detail-value {
    text-align: left;
  }

  .profile-actions {
    flex-direction: column;
  }

  .btn {
    width: 100%;
  }
}
</style>
