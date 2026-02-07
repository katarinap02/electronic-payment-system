import { createRouter, createWebHistory } from 'vue-router';
import CryptoPaymentView from '../views/CryptoPaymentView.vue';

const routes = [
  {
    path: '/crypto-payment/:cryptoPaymentId',
    name: 'CryptoPayment',
    component: CryptoPaymentView,
    props: true
  },
  {
    path: '/',
    redirect: '/crypto-payment/demo'
  }
];

const router = createRouter({
  history: createWebHistory(),
  routes
});

export default router;
