<script setup lang="ts">
import { ref } from 'vue';
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
  Sparkles,
  ArrowUpRight,
  Truck,
  CheckCircle2,
  Clock,
  ArrowRight,
  ShieldCheck
} from 'lucide-vue-next';

const router = useRouter();
const auth = useAuthStore();

const handleLogout = () => {
  auth.logout();
  router.push('/login');
};

// Village Sheds Real-Time Progress Matrix
const shedHighlights = ref([
  {
    id: 1,
    name: 'Brahmana Tarla Shed',
    village: 'Brahmana Tarla (Palasa)',
    supervisor: 'Ramesh (Palasa Shed-1)',
    targetKg: 600.000,
    cutKg: 524.350,
    activeWorkers: 18,
    status: 'ACTIVE',
    efficiency: 87.4
  },
  {
    id: 2,
    name: 'Kasibugga Rural Shed',
    village: 'Kasibugga (Palasa)',
    supervisor: 'T. Rama Rao',
    targetKg: 500.000,
    cutKg: 492.100,
    activeWorkers: 15,
    status: 'ACTIVE',
    efficiency: 98.4
  },
  {
    id: 3,
    name: 'Mandasa Coastal Shed',
    village: 'Mandasa Mandal',
    supervisor: 'B. Jagannatham',
    targetKg: 750.000,
    cutKg: 680.400,
    activeWorkers: 22,
    status: 'ACTIVE',
    efficiency: 90.7
  }
]);
</script>

<template>
  <div class="min-h-screen bg-slate-50 flex flex-col selection:bg-amber-200">
    <!-- Operations Top Navigation -->
    <header class="bg-amber-950 text-white px-4 sm:px-6 py-3.5 sm:py-4 flex items-center justify-between border-b border-amber-900 shadow-md sticky top-0 z-40">
      <div class="flex items-center gap-2.5 sm:gap-3">
        <div class="w-9 h-9 sm:w-10 sm:h-10 rounded-xl bg-gradient-to-tr from-amber-600 to-amber-500 flex items-center justify-center text-white font-bold shadow-md shrink-0">
          <Sparkles class="w-5 h-5" />
        </div>
        <div>
          <h1 class="text-sm sm:text-base font-bold font-serif leading-tight">CashewPro ERP Suite</h1>
          <p class="text-[10px] sm:text-[11px] text-amber-300 font-mono tracking-wide">
            {{ auth.user?.roleName || 'Executive Portal' }} • Logged In
          </p>
        </div>
      </div>

      <div class="flex items-center gap-3 sm:gap-4">
        <div class="text-right hidden sm:block">
          <p class="text-xs font-bold text-white">{{ auth.user?.fullName }}</p>
          <p class="text-[10px] text-amber-300/80">{{ auth.user?.phoneNumber }}</p>
        </div>
        <button 
          @click="handleLogout" 
          class="flex items-center gap-1.5 sm:gap-2 px-3 py-1.5 rounded-xl bg-amber-900/80 hover:bg-rose-900 border border-amber-800 text-xs font-semibold transition"
          title="Sign Out"
        >
          <LogOut class="w-4 h-4" />
          <span class="hidden sm:inline">Sign Out</span>
        </button>
      </div>
    </header>

    <!-- Main Content Area -->
    <main class="flex-1 max-w-7xl w-full mx-auto p-4 sm:p-6 lg:p-8 space-y-6">
      
      <!-- Welcome Ribbon & Direct Desk Links -->
      <div class="bg-white rounded-3xl p-5 sm:p-6 border border-amber-100 shadow-sm flex flex-col md:flex-row items-start md:items-center justify-between gap-4">
        <div>
          <span class="px-3 py-1 rounded-full text-[10px] sm:text-xs font-black tracking-wider bg-emerald-100 text-emerald-800 uppercase">
            Live Factory Command
          </span>
          <h2 class="text-xl sm:text-2xl font-black text-slate-900 font-serif mt-2">
            Namaste, {{ auth.user?.fullName }}
          </h2>
          <p class="text-xs text-slate-500 mt-1">
            Real-time multi-shed sync active across Palasa, Mandasa and coastal procurement points.
          </p>
        </div>
        
        <div class="flex flex-wrap items-center gap-2.5 w-full md:w-auto">
          <!-- Daily Shelling Entry Desk Link -->
          <router-link 
            to="/erp/shed-operations" 
            class="flex-1 sm:flex-initial inline-flex items-center justify-center gap-2 px-4 py-2.5 bg-amber-600 hover:bg-amber-700 text-white font-bold text-xs rounded-xl shadow-md shadow-amber-600/20 transition"
          >
            <Scale class="w-4 h-4" />
            <span>Daily Shelling Desk</span>
          </router-link>

          <!-- RCN Procurement Desk Link -->
          <router-link 
            to="/erp/procurement" 
            class="flex-1 sm:flex-initial inline-flex items-center justify-center gap-2 px-4 py-2.5 bg-slate-900 hover:bg-slate-800 text-white font-bold text-xs rounded-xl shadow-md transition"
          >
            <Truck class="w-4 h-4 text-amber-400" />
            <span>RCN Inward Desk</span>
          </router-link>

          <!-- Public Storefront Preview -->
          <router-link 
            to="/" 
            class="inline-flex items-center justify-center gap-1.5 text-xs font-bold text-amber-900 hover:text-amber-950 bg-amber-50 px-3.5 py-2.5 rounded-xl border border-amber-200 transition"
            title="View Public Store"
          >
            <span class="hidden sm:inline">Store</span>
            <ArrowUpRight class="w-4 h-4" />
          </router-link>
        </div>
      </div>

      <!-- Quick Operational Metrics Grid -->
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 sm:gap-6">
        
        <div class="bg-white p-5 rounded-2xl border border-slate-200/80 shadow-sm">
          <div class="flex items-center justify-between text-slate-500 mb-3">
            <span class="text-xs font-bold uppercase tracking-wider">Active Sheds</span>
            <Building2 class="w-5 h-5 text-amber-600" />
          </div>
          <p class="text-2xl font-black text-slate-900">3 Units</p>
          <p class="text-[11px] text-emerald-600 font-semibold mt-1">100% operational today</p>
        </div>

        <div class="bg-white p-5 rounded-2xl border border-slate-200/80 shadow-sm">
          <div class="flex items-center justify-between text-slate-500 mb-3">
            <span class="text-xs font-bold uppercase tracking-wider">Active Cutters</span>
            <Users class="w-5 h-5 text-indigo-600" />
          </div>
          <p class="text-2xl font-black text-slate-900">55 Workers</p>
          <p class="text-[11px] text-slate-500 mt-1">Logged via village supervisors</p>
        </div>

        <div class="bg-white p-5 rounded-2xl border border-slate-200/80 shadow-sm">
          <div class="flex items-center justify-between text-slate-500 mb-3">
            <span class="text-xs font-bold uppercase tracking-wider">Today's Shelling</span>
            <Scale class="w-5 h-5 text-emerald-600" />
          </div>
          <p class="text-2xl font-black text-slate-900">1,696.850 <span class="text-xs font-normal text-slate-500">Kg</span></p>
          <p class="text-[11px] text-emerald-600 font-semibold mt-1">Gram precision verified</p>
        </div>

        <div class="bg-white p-5 rounded-2xl border border-slate-200/80 shadow-sm">
          <div class="flex items-center justify-between text-slate-500 mb-3">
            <span class="text-xs font-bold uppercase tracking-wider">Kernel Recovery</span>
            <TrendingUp class="w-5 h-5 text-amber-600" />
          </div>
          <p class="text-2xl font-black text-slate-900">24.2 %</p>
          <p class="text-[11px] text-emerald-600 font-semibold mt-1">Optimal recovery (22% - 25%)</p>
        </div>

      </div>

      <!-- Village Sheds Performance & Progress Matrix -->
      <div class="bg-white rounded-3xl p-5 sm:p-6 border border-amber-100 shadow-sm">
        <div class="flex flex-col sm:flex-row sm:items-center justify-between pb-4 border-b border-slate-100 gap-2 mb-4">
          <div>
            <h3 class="text-base font-bold text-slate-900 flex items-center gap-2">
              <Building2 class="w-5 h-5 text-amber-700" />
              <span>Village Sheds Live Progress</span>
            </h3>
            <p class="text-xs text-slate-500 mt-0.5">Real-time cutting kilograms logged per distributed village unit</p>
          </div>
          
          <router-link 
            to="/erp/shed-operations" 
            class="text-xs font-bold text-amber-700 hover:text-amber-800 flex items-center gap-1 self-start sm:self-auto"
          >
            <span>Open Entry Desk</span>
            <ArrowRight class="w-3.5 h-3.5" />
          </router-link>
        </div>

        <!-- Progress Table -->
        <div class="overflow-x-auto">
          <table class="w-full text-left text-xs">
            <thead>
              <tr class="text-slate-400 uppercase text-[10px] tracking-wider border-b border-slate-100">
                <th class="pb-3">Village Shed</th>
                <th class="pb-3">Supervisor</th>
                <th class="pb-3">Active Cutters</th>
                <th class="pb-3">Target (Kg)</th>
                <th class="pb-3 text-amber-950 font-bold">Cut Output (Kg)</th>
                <th class="pb-3 text-right">Target Achieved</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-100">
              <tr v-for="shed in shedHighlights" :key="shed.id" class="hover:bg-amber-50/30 transition">
                <td class="py-3.5 font-bold text-slate-900">
                  {{ shed.name }}
                  <span class="block text-[10px] font-normal text-slate-500">{{ shed.village }}</span>
                </td>
                <td class="py-3.5 text-slate-600">{{ shed.supervisor }}</td>
                <td class="py-3.5 text-slate-700 font-semibold">{{ shed.activeWorkers }} Workers</td>
                <td class="py-3.5 font-mono text-slate-500">{{ shed.targetKg.toFixed(3) }}</td>
                <td class="py-3.5 font-mono font-bold text-amber-950">{{ shed.cutKg.toFixed(3) }}</td>
                <td class="py-3.5 text-right">
                  <span class="inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-[11px] font-bold bg-emerald-50 text-emerald-700 border border-emerald-200">
                    <CheckCircle2 class="w-3 h-3" />
                    {{ shed.efficiency }}%
                  </span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- Bottom Operational Modules Grid -->
      <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
        
        <!-- Procurement Desk Card -->
        <div class="bg-white p-5 rounded-2xl border border-slate-200/80 shadow-sm flex flex-col justify-between">
          <div>
            <div class="flex items-center gap-2 text-slate-900 font-bold mb-2">
              <Truck class="w-4 h-4 text-amber-600" />
              <span>RCN Procurement Desk</span>
            </div>
            <p class="text-xs text-slate-500 leading-relaxed mb-4">
              Local farmer weighbridge entries, moisture testing (8-10%), outturn (48-52 lbs), and lorry freight slips.
            </p>
          </div>
          <router-link 
            to="/erp/procurement" 
            class="text-xs font-bold text-amber-700 flex items-center gap-1 hover:underline"
          >
            <span>Open Inward Weighbridge Desk →</span>
          </router-link>
        </div>

        <!-- Daily Shelling & Wages Card -->
        <div class="bg-white p-5 rounded-2xl border border-slate-200/80 shadow-sm flex flex-col justify-between">
          <div>
            <div class="flex items-center gap-2 text-slate-900 font-bold mb-2">
              <Scale class="w-4 h-4 text-indigo-600" />
              <span>Shed Cutting & Wages</span>
            </div>
            <p class="text-xs text-slate-500 leading-relaxed mb-4">
              Gram-accurate cutter entries, wage calculations, and automated multi-lingual WhatsApp & SMS wage slip dispatch.
            </p>
          </div>
          <router-link 
            to="/erp/shed-operations" 
            class="text-xs font-bold text-indigo-700 flex items-center gap-1 hover:underline"
          >
            <span>Open Shelling Entry Desk →</span>
          </router-link>
        </div>

        <!-- GST & E-Way Bills Card -->
        <div class="bg-white p-5 rounded-2xl border border-slate-200/80 shadow-sm flex flex-col justify-between">
          <div>
            <div class="flex items-center gap-2 text-slate-900 font-bold mb-2">
              <FileText class="w-4 h-4 text-emerald-600" />
              <span>GST & E-Way Bills Desk</span>
            </div>
            <p class="text-xs text-slate-500 leading-relaxed mb-4">
              HSN 0801 tax invoices, vehicle transport details, wholesale client balances, and interstate dispatch records.
            </p>
          </div>
          <span 
            @click="router.push('/erp/procurement')" 
            class="text-xs font-bold text-emerald-700 flex items-center gap-1 cursor-pointer hover:underline"
          >
            <span>Review Active Invoices →</span>
          </span>
        </div>

      </div>

    </main>

    <!-- Operations Footer -->
    <footer class="p-4 text-center text-slate-400 text-xs border-t border-slate-200 bg-white">
      CashewPro Multi-Shed ERP Suite • Palasa Central Processing Division
    </footer>
  </div>
</template>