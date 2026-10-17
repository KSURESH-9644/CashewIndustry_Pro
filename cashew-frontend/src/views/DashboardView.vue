<script setup lang="ts">
import { useRouter } from 'vue-router';
import { useAuthStore } from '../stores/authStore';
import { 
  LogOut, 
  Building2, 
  Users, 
  Scale, 
  TrendingUp, 
  Layers, 
  FileText, 
  ShieldAlert, 
  Sparkles,
  ArrowUpRight
} from 'lucide-vue-next';

const router = useRouter();
const auth = useAuthStore();

const handleLogout = () => {
  auth.logout();
  router.push('/login');
};
</script>

<template>
  <div class="min-h-screen bg-slate-50 flex flex-col">
    <!-- Operations Top Navigation -->
    <header class="bg-amber-950 text-white px-6 py-4 flex items-center justify-between border-b border-amber-900 shadow-md">
      <div class="flex items-center gap-3">
        <div class="w-10 h-10 rounded-xl bg-amber-600 flex items-center justify-center text-white font-bold shadow-md">
          <Sparkles class="w-5 h-5" />
        </div>
        <div>
          <h1 class="text-base font-bold font-serif leading-tight">CashewPro ERP Suite</h1>
          <p class="text-[11px] text-amber-300 font-mono tracking-wide">
            {{ auth.user?.roleName || 'Operations Portal' }} • Logged In
          </p>
        </div>
      </div>

      <div class="flex items-center gap-4">
        <div class="text-right hidden sm:block">
          <p class="text-xs font-bold text-white">{{ auth.user?.fullName }}</p>
          <p class="text-[10px] text-amber-300/80">{{ auth.user?.phoneNumber }}</p>
        </div>
        <button 
          @click="handleLogout" 
          class="flex items-center gap-2 px-3 py-1.5 rounded-xl bg-amber-900/80 hover:bg-rose-900 border border-amber-800 text-xs font-semibold transition"
        >
          <LogOut class="w-4 h-4" />
          <span class="hidden sm:inline">Sign Out</span>
        </button>
      </div>
    </header>

    <!-- Main Content Area -->
    <main class="flex-1 max-w-7xl w-full mx-auto p-4 sm:p-6 lg:p-8 space-y-6">
      
      <!-- Welcome Ribbon -->
      <div class="bg-white rounded-3xl p-6 border border-amber-100 shadow-sm flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4">
        <div>
          <span class="px-3 py-1 rounded-full text-xs font-black tracking-wider bg-emerald-100 text-emerald-800 uppercase">
            Live Factory Status
          </span>
          <h2 class="text-2xl font-black text-slate-900 font-serif mt-2">
            Namaste, {{ auth.user?.fullName }}
          </h2>
          <p class="text-xs text-slate-500 mt-1">
            Real-time multi-shed sync active across Palasa and coastal procurement points.
          </p>
        </div>
        <router-link 
          to="/" 
          class="inline-flex items-center gap-1.5 text-xs font-bold text-amber-800 hover:text-amber-900 bg-amber-50 px-4 py-2 rounded-xl border border-amber-200"
        >
          <span>Open Public Store</span>
          <ArrowUpRight class="w-4 h-4" />
        </router-link>
      </div>

      <!-- Quick Metrics Grid -->
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 sm:gap-6">
        
        <div class="bg-white p-5 rounded-2xl border border-slate-200/80 shadow-sm">
          <div class="flex items-center justify-between text-slate-500 mb-3">
            <span class="text-xs font-bold uppercase tracking-wider">Active Sheds</span>
            <Building2 class="w-5 h-5 text-amber-600" />
          </div>
          <p class="text-2xl font-black text-slate-900">8 Units</p>
          <p class="text-[11px] text-emerald-600 font-semibold mt-1">100% operational today</p>
        </div>

        <div class="bg-white p-5 rounded-2xl border border-slate-200/80 shadow-sm">
          <div class="flex items-center justify-between text-slate-500 mb-3">
            <span class="text-xs font-bold uppercase tracking-wider">Active Labour</span>
            <Users class="w-5 h-5 text-indigo-600" />
          </div>
          <p class="text-2xl font-black text-slate-900">246 Cutters</p>
          <p class="text-[11px] text-slate-500 mt-1">Checked-in via Shed supervisors</p>
        </div>

        <div class="bg-white p-5 rounded-2xl border border-slate-200/80 shadow-sm">
          <div class="flex items-center justify-between text-slate-500 mb-3">
            <span class="text-xs font-bold uppercase tracking-wider">Today's Shelling</span>
            <Scale class="w-5 h-5 text-emerald-600" />
          </div>
          <p class="text-2xl font-black text-slate-900">3,420.500 <span class="text-xs font-normal text-slate-500">Kg</span></p>
          <p class="text-[11px] text-emerald-600 font-semibold mt-1">Gram-precision logged</p>
        </div>

        <div class="bg-white p-5 rounded-2xl border border-slate-200/80 shadow-sm">
          <div class="flex items-center justify-between text-slate-500 mb-3">
            <span class="text-xs font-bold uppercase tracking-wider">Kernel Recovery</span>
            <TrendingUp class="w-5 h-5 text-amber-600" />
          </div>
          <p class="text-2xl font-black text-slate-900">24.2 %</p>
          <p class="text-[11px] text-slate-500 mt-1">Expected: 22% - 25%</p>
        </div>

      </div>

    </main>
  </div>
</template>