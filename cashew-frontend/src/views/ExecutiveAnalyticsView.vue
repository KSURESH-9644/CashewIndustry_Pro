<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '../stores/authStore';
import { useToastStore } from '../stores/toastStore';
import apiClient from '../api/client';
import { 
  TrendingUp, 
  ArrowLeft, 
  DollarSign, 
  Building2, 
  Scale, 
  Truck, 
  AlertTriangle, 
  CheckCircle2, 
  ShieldCheck, 
  Sparkles,
  PieChart,
  Wallet,
  Activity,
  ArrowUpRight
} from 'lucide-vue-next';

const router = useRouter();
const auth = useAuthStore();
const toast = useToastStore();

const overview = ref<any>(null);
const shedMatrix = ref<any[]>([]);
const isLoading = ref(true);

const loadExecutiveData = async () => {
  try {
    isLoading.value = true;
    const [finRes, shedRes] = await Promise.all([
      apiClient.get('/erp/ExecutiveAnalytics/financial-overview'),
      apiClient.get('/erp/ExecutiveAnalytics/shed-efficiency-matrix')
    ]);

    if (finRes.data.success) overview.value = finRes.data.data;
    if (shedRes.data.success) shedMatrix.value = shedRes.data.data;
  } catch (err: any) {
    toast.add({
      severity: 'error',
      summary: 'Analytics Error',
      detail: 'Executive metrics load cheyadam lo error vachindi.'
    });
  } finally {
    isLoading.value = false;
  }
};

onMounted(() => {
  loadExecutiveData();
});
</script>

<template>
  <div class="min-h-screen bg-slate-50 flex flex-col selection:bg-amber-200">
    <!-- Header -->
    <header class="bg-amber-950 text-white px-4 sm:px-6 py-3.5 flex items-center justify-between border-b border-amber-900 shadow sticky top-0 z-40">
      <div class="flex items-center gap-3">
        <router-link to="/erp/dashboard" class="p-2 rounded-xl bg-amber-900/60 hover:bg-amber-800 text-amber-200 transition">
          <ArrowLeft class="w-4 h-4" />
        </router-link>
        <div>
          <h1 class="text-sm sm:text-base font-bold font-serif leading-tight">Executive P&L & Factory Command Cockpit</h1>
          <p class="text-[10px] sm:text-[11px] text-amber-300 font-mono">Financial Real-time Ledger • Yield Efficiency Heatmap • Valuation</p>
        </div>
      </div>

      <div class="flex items-center gap-2">
        <span class="px-3 py-1 rounded-full text-xs font-bold bg-amber-900 border border-amber-700 text-amber-200">
          Director Access
        </span>
      </div>
    </header>

    <!-- Main Content -->
    <main class="flex-1 max-w-7xl w-full mx-auto p-4 sm:p-6 lg:p-8 space-y-6">
      
      <!-- Top High-Level Financial Health Indicators -->
      <div v-if="overview" class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 sm:gap-6">
        
        <!-- Net Factory Valuation -->
        <div class="bg-gradient-to-br from-amber-950 to-amber-900 text-white p-5 rounded-3xl shadow-md border border-amber-800 flex flex-col justify-between">
          <div class="flex items-center justify-between opacity-80 mb-2">
            <span class="text-xs font-bold uppercase tracking-wider">Ground Stock Net Value</span>
            <Wallet class="w-5 h-5 text-amber-300" />
          </div>
          <div>
            <p class="text-3xl font-black font-mono text-amber-300">
              ₹{{ (overview.inventory.marketValuation || 0).toLocaleString('en-IN') }}
            </p>
            <p class="text-[11px] text-amber-200/80 mt-1">
              {{ (overview.inventory.totalWeightKg || 0).toFixed(1) }} Kg graded stock ready
            </p>
          </div>
        </div>

        <!-- Realized B2B Sales -->
        <div class="bg-white p-5 rounded-3xl border border-slate-200/80 shadow-sm flex flex-col justify-between">
          <div class="flex items-center justify-between text-slate-500 mb-2">
            <span class="text-xs font-bold uppercase tracking-wider">B2B Sales Realized</span>
            <TrendingUp class="w-5 h-5 text-emerald-600" />
          </div>
          <div>
            <p class="text-2xl font-black text-slate-900 font-mono">
              ₹{{ (overview.sales.totalRevenue || 0).toLocaleString('en-IN') }}
            </p>
            <p class="text-[11px] text-emerald-700 font-semibold mt-1">
              GST Collected: ₹{{ (overview.sales.taxCollected || 0).toLocaleString('en-IN') }}
            </p>
          </div>
        </div>

        <!-- Raw Seeds Procurement Spend -->
        <div class="bg-white p-5 rounded-3xl border border-slate-200/80 shadow-sm flex flex-col justify-between">
          <div class="flex items-center justify-between text-slate-500 mb-2">
            <span class="text-xs font-bold uppercase tracking-wider">Raw Seeds Spend</span>
            <Truck class="w-5 h-5 text-amber-600" />
          </div>
          <div>
            <p class="text-2xl font-black text-slate-900 font-mono">
              ₹{{ (overview.procurement.totalSpend || 0).toLocaleString('en-IN') }}
            </p>
            <p class="text-[11px] text-slate-500 mt-1">
              {{ ((overview.procurement.totalKg || 0) / 1000).toFixed(2) }} Tons Inward
            </p>
          </div>
        </div>

        <!-- Labour Wages & Outstanding -->
        <div class="bg-white p-5 rounded-3xl border border-slate-200/80 shadow-sm flex flex-col justify-between">
          <div class="flex items-center justify-between text-slate-500 mb-2">
            <span class="text-xs font-bold uppercase tracking-wider">Labour Liability</span>
            <Activity class="w-5 h-5 text-indigo-600" />
          </div>
          <div>
            <p class="text-2xl font-black text-slate-900 font-mono">
              ₹{{ (overview.labourWages.totalPendingBalance || 0).toLocaleString('en-IN') }}
            </p>
            <p class="text-[11px] text-indigo-700 font-semibold mt-1">
              Total Wages: ₹{{ (overview.labourWages.totalEarned || 0).toLocaleString('en-IN') }}
            </p>
          </div>
        </div>

      </div>

      <!-- Village Sheds Efficiency & Yield Heatmap -->
      <div class="bg-white rounded-3xl p-5 sm:p-6 border border-amber-100 shadow-sm space-y-4">
        <div class="flex flex-col sm:flex-row sm:items-center justify-between pb-3.5 border-b border-slate-100 gap-2">
          <div>
            <h3 class="text-base font-bold text-slate-900 flex items-center gap-2">
              <Building2 class="w-5 h-5 text-amber-700" />
              <span>Village Sheds Yield & Efficiency Heatmap</span>
            </h3>
            <p class="text-xs text-slate-500 mt-0.5">
              Live recovery ratio comparisons (Target: 22% - 25% kernel recovery from raw seed)
            </p>
          </div>

          <div class="flex items-center gap-3 text-xs">
            <span class="inline-flex items-center gap-1 text-emerald-700 font-bold">
              <span class="w-2 h-2 rounded-full bg-emerald-500"></span> Optimal (≥23%)
            </span>
            <span class="inline-flex items-center gap-1 text-amber-700 font-bold">
              <span class="w-2 h-2 rounded-full bg-amber-500"></span> Normal (21-23%)
            </span>
            <span class="inline-flex items-center gap-1 text-rose-700 font-bold">
              <span class="w-2 h-2 rounded-full bg-rose-500"></span> Check (&lt;21%)
            </span>
          </div>
        </div>

        <!-- Sheds Efficiency Table -->
        <div class="overflow-x-auto">
          <table class="w-full text-left text-xs">
            <thead>
              <tr class="text-slate-400 uppercase text-[10px] tracking-wider border-b border-slate-100">
                <th class="pb-3">Village Shed</th>
                <th class="pb-3">Supervisor</th>
                <th class="pb-3">Active Cutters</th>
                <th class="pb-3 text-amber-950 font-bold">Output (Kg)</th>
                <th class="pb-3">Shell Waste (Kg)</th>
                <th class="pb-3">Kernel Yield %</th>
                <th class="pb-3 text-right">Status</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-100">
              <tr v-for="s in shedMatrix" :key="s.id" class="hover:bg-amber-50/30 transition">
                <td class="py-3.5 font-bold text-slate-900">
                  {{ s.shedName }}
                  <span class="block text-[10px] font-normal text-slate-500">{{ s.villageName }} ({{ s.mandal }})</span>
                </td>
                <td class="py-3.5 text-slate-600 font-medium">{{ s.supervisorName }}</td>
                <td class="py-3.5 text-slate-800 font-bold">{{ s.activeWorkers }} Workers</td>
                <td class="py-3.5 font-mono font-bold text-amber-950">{{ s.totalCutKg.toFixed(3) }}</td>
                <td class="py-3.5 font-mono text-slate-500">{{ s.totalWastageKg.toFixed(3) }}</td>
                <td class="py-3.5 font-mono font-black text-sm" :class="{
                  'text-emerald-700': s.yieldRatio >= 23.0,
                  'text-amber-700': s.yieldRatio >= 21.0 && s.yieldRatio < 23.0,
                  'text-rose-700': s.yieldRatio < 21.0
                }">
                  {{ s.yieldRatio }}%
                </td>
                <td class="py-3.5 text-right">
                  <span 
                    v-if="s.status === 'HIGH_EFFICIENCY'"
                    class="inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-[10px] font-bold bg-emerald-100 text-emerald-800"
                  >
                    <CheckCircle2 class="w-3 h-3" /> Optimal
                  </span>
                  <span 
                    v-else-if="s.status === 'NORMAL'"
                    class="inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-[10px] font-bold bg-amber-100 text-amber-800"
                  >
                    Normal
                  </span>
                  <span 
                    v-else
                    class="inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-[10px] font-bold bg-rose-100 text-rose-800"
                  >
                    <AlertTriangle class="w-3 h-3" /> Check Shelling
                  </span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- Quick Desk Navigation Cards -->
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 sm:gap-6 pt-2">
        <router-link to="/erp/shed-operations" class="p-4 rounded-2xl bg-white border border-slate-200 hover:border-amber-300 shadow-sm transition flex items-center justify-between">
          <div>
            <p class="text-xs font-bold text-slate-800">Daily Shelling Desk</p>
            <p class="text-[11px] text-slate-400">Worker wage slips</p>
          </div>
          <ArrowUpRight class="w-4 h-4 text-amber-700" />
        </router-link>

        <router-link to="/erp/procurement" class="p-4 rounded-2xl bg-white border border-slate-200 hover:border-amber-300 shadow-sm transition flex items-center justify-between">
          <div>
            <p class="text-xs font-bold text-slate-800">RCN Procurement</p>
            <p class="text-[11px] text-slate-400">Weighbridge inward</p>
          </div>
          <ArrowUpRight class="w-4 h-4 text-amber-700" />
        </router-link>

        <router-link to="/erp/grading-stock" class="p-4 rounded-2xl bg-white border border-slate-200 hover:border-amber-300 shadow-sm transition flex items-center justify-between">
          <div>
            <p class="text-xs font-bold text-slate-800">Grading & Stock</p>
            <p class="text-[11px] text-slate-400">Tins & recovery</p>
          </div>
          <ArrowUpRight class="w-4 h-4 text-indigo-700" />
        </router-link>

        <router-link to="/erp/billing" class="p-4 rounded-2xl bg-white border border-slate-200 hover:border-amber-300 shadow-sm transition flex items-center justify-between">
          <div>
            <p class="text-xs font-bold text-slate-800">GST Billing</p>
            <p class="text-[11px] text-slate-400">E-Way bills & dispatch</p>
          </div>
          <ArrowUpRight class="w-4 h-4 text-emerald-700" />
        </router-link>
      </div>

    </main>

    <!-- Footer -->
    <footer class="p-4 text-center text-slate-400 text-xs border-t border-slate-200 bg-white">
      CashewPro Executive Suite • Palasa Corporate Office
    </footer>
  </div>
</template>