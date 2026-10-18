import { createRouter, createWebHistory } from 'vue-router';
import HomeView from '../views/HomeView.vue';

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/',
      name: 'home',
      component: HomeView,
    },
    {
      path: '/login',
      name: 'login',
      component: () => import('../views/LoginView.vue'),
    },
    {
      path: '/erp/dashboard',
      name: 'dashboard',
      component: () => import('../views/DashboardView.vue'),
    },
    {
      path: '/erp/shed-operations',
      name: 'shed-operations',
      component: () => import('../views/ShedOperationsView.vue'),
    },
    {
      path: '/erp/procurement',
      name: 'procurement',
      component: () => import('../views/ProcurementView.vue'),
    },
    {
      path: '/erp/grading-stock',
      name: 'grading-stock',
      component: () => import('../views/GradingStockView.vue'),
    },
    {
      path: '/erp/billing',
      name: 'billing',
      component: () => import('../views/BillingView.vue'),
    },
    {
      path: '/erp/analytics',
      name: 'executive-analytics',
      component: () => import('../views/ExecutiveAnalyticsView.vue'),
    },
    {
      path: '/erp/shed-logistics',
      name: 'shed-logistics',
      component: () => import('../views/ShedLogisticsView.vue'),
    }
  ],
});

export default router;