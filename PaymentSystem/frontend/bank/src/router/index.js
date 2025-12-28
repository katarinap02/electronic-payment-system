import { createRouter, createWebHistory } from 'vue-router'

const routes = [
  {
    path: '/',
    component: () => import('@/components/HelloWorld.vue')
  },
  {
    path: '/payment/:paymentId',
    name: 'PaymentForm',
    component: () => import('@/components/PaymentForm.vue')
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

export default router