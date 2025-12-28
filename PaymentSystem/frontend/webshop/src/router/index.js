import { createRouter, createWebHistory } from 'vue-router';

const routes = [
  {
    path: '/',
    redirect: '/vehicles'
  },
  {
    path: '/login',
    name: 'Login',
    component: () => import('@/components/LoginView.vue')
  },
  {
    path: '/register',
    name: 'Register',
    component: () => import('@/components/Register.vue')
  },
  {
    path: '/dashboard',
    name: 'Dashboard',
    component: () => import('@/components/Dashboard.vue')
  },
  {
    path: '/profile',
    name: 'Profile',
    component: () => import('@/components/Profile.vue')
  },
  {
    path: '/vehicles',
    name: 'VehicleList',
    component: () => import('@/components/VehicleList.vue')
  },
  {
    path: '/vehicles/:id',
    name: 'VehicleDetail',
    component: () => import('@/components/VehicleDetail.vue')
  },
  {
    path: '/admin/vehicles',
    name: 'AdminVehicles',
    component: () => import('@/components/AdminVehicles.vue')
  }
];

const router = createRouter({
  history: createWebHistory(),
  routes
});

export default router;