<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRouter } from 'vue-router';
import { useToastStore } from '../stores/toastStore';
import apiClient from '../api/client';
import { 
  Layers, 
  ArrowLeft, 
  Scale, 
  CheckCircle2, 
  AlertTriangle, 
  TrendingUp, 
  Boxes, 
  PackageCheck,
  Archive,
  Send
} from 'lucide-vue-next';

const router = useRouter();
const toast = useToastStore();

interface Batch {
  id: number;
  batchNumber: string;
  rawWeightInputKg: number;
  rawBagsUsed: number;
  status: string;
  boilingDate: string;
}

interface StockItem {
  id: number;
  gradeId: number;
  gradeCode: string;
  gradeName: string;
  category: string;
  retailPrice: number;
  wholesalePrice: number;
  packageType: string;
  totalQuantityCount: number;
  totalWeightInKg: number;
  estimatedValuation: number;
}

const batches = ref<Batch[]>([]);
const stockMatrix = ref<StockItem[]>([]);
const isLoading = ref(false);
const isSubmitting = ref(false);

// Form Inputs
const selectedBatchId = ref<number | null>(null);
const rawInputWeightKg = ref<number>(1600.0);

// Grade Weights (Kg + Grams)
const w180Kg = ref<number | null>(25.500);
const w210Kg = ref<number | null>(48.250);
const w240Kg = ref<number | null>(120.000);
const w320Kg = ref<number | null>(145.400);
const jhKg = ref<number | null>(42.150);
const lwpKg = ref<number | null>(18.200);

const totalKernelOutputKg = computed(() => {
  return Number((
    (w180Kg.value || 0) +
    (w210Kg.value || 0) +
    (w240Kg.value || 0) +
    (w320Kg.value || 0) +
    (jhKg.value || 0) +
    (lwpKg.value || 0)
  ).toFixed(3));
});

// Live Recovery Yield % Formula
const recoveryYieldPercentage = computed(() => {
  if (!rawInputWeightKg.value || rawInputWeightKg.value <= 0 || totalKernelOutputKg.value <= 0) return 0;
  return Number(((totalKernelOutputKg.value / rawInputWeightKg.value) * 100).toFixed(2));
});

const loadGradingData = async () => {
  try {
    isLoading.value = true;
    const [batchesRes, stockRes] = await Promise.all([
      apiClient.get('/erp/GradingStock/batches'),
      apiClient.get('/erp/GradingStock/stock-matrix')
    ]);

    if (batchesRes.data.success) {
      batches.value = batchesRes.data.data;
      if (batches.value.length > 0) {
        selectedBatchId.value = batches.value[0].id;
        rawInputWeightKg.value = batches.value[0].rawWeightInputKg;
      }
    }
    if (stockRes.data.success) {
      stockMatrix.value = stockRes.data.data;
    }
  } catch (err: any) {
    toast.add({
      severity: 'error',
      summary: 'Data Fetch Error',
      detail: 'Grading batches & stock matrix load cheyadam lo error vachindi.'
    });
  } finally {
    isLoading.value = false;
  }
};

const handleRecordGrading = async () => {
  if (totalKernelOutputKg.value <= 0) {
    toast.add({
      severity: 'warn',
      summary: 'Validation Alert',
      detail: 'At least oka grade ki valid weight enter cheyandi.'
    });
    return;
  }

  const gradedOutputs = [
    { gradeId: 1, weightKg: w180Kg.value || 0 }, // W-180
    { gradeId: 2, weightKg: w210Kg.value || 0 }, // W-210
    { gradeId: 3, weightKg: w240Kg.value || 0 }, // W-240
    { gradeId: 4, weightKg: w320Kg.value || 0 }, // W-320
    { gradeId: 5, weightKg: jhKg.value || 0 },   // JH
    { gradeId: 6, weightKg: lwpKg.value || 0 },  // LWP
  ].filter(g => g.weightKg > 0);

  try {
    isSubmitting.value = true;
    const res = await apiClient.post('/erp/GradingStock/record-grading', {
      boilingBatchId: selectedBatchId.value || 1,
      rawNutInputWeightKg: rawInputWeightKg.value,
      gradedOutputs,
      notes: 'Borma cycle dried and graded'
    });

    if (res.data.success) {
      toast.add({
        severity: 'success',
        summary: 'Grading Committed',
        detail: `Yield: ${res.data.data.recoveryYieldPercentage}% (${res.data.data.totalKernelOutputKg} Kg kernel). Finished warehouse stock updated.`,
        life: 5500
      });

      // Refresh stock matrix
      const stockRes = await apiClient.get('/erp/GradingStock/stock-matrix');
      if (stockRes.data.success) stockMatrix.value = stockRes.data.data;
    }
  } catch (err: any) {
    toast.add({
      severity: 'error',
      summary: 'Commit Failed',
      detail: err.response?.data?.message || 'Grading batch save cheyadam lo error.'
    });
  } finally {
    isSubmitting.value = false;
  }
};

onMounted(() => {
  loadGradingData();
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
          <h1 class="text-sm sm:text-base font-bold font-serif leading-tight">Finished Goods Grading & Recovery Yield Desk</h1>
          <p class="text-[10px] sm:text-[11px] text-amber-300 font-mono">Borma Chamber Dried • Grade Sorting (Kg+g) • Real-time Stock Matrix</p>
        </div>
      </div>

      <div class="flex items-center gap-2">
        <span class="px-2.5 py-1 rounded-full text-xs font-bold bg-amber-900 border border-amber-700 text-amber-200">
          Target Yield: 22% - 25%
        </span>
      </div>
    </header>

    <!-- Main Workspace -->
    <main class="flex-1 max-w-7xl w-full mx-auto p-4 sm:p-6 grid grid-cols-1 lg:grid-cols-12 gap-6">
      
      <!-- Left Column: Grading Weighment Pad (5 Cols) -->
      <div class="lg:col-span-5 bg-white p-5 sm:p-6 rounded-3xl border border-amber-100 shadow-sm flex flex-col justify-between">
        <div>
          <div class="flex items-center justify-between pb-3.5 border-b border-slate-100 mb-4">
            <h2 class="text-base font-bold text-slate-900 flex items-center gap-2">
              <Scale class="w-5 h-5 text-amber-600" />
              <span>Record Grade-wise Recovery</span>
            </h2>
            <span class="px-2.5 py-0.5 rounded-full text-[10px] font-black bg-amber-100 text-amber-900">
              Gram Precision
            </span>
          </div>

          <form @submit.prevent="handleRecordGrading" class="space-y-4">
            <!-- Select Boiler Batch -->
            <div class="grid grid-cols-2 gap-3">
              <div>
                <label class="block text-xs font-bold text-slate-700 mb-1">Boiling Batch</label>
                <select 
                  v-model="selectedBatchId"
                  class="w-full text-xs p-2.5 rounded-xl border border-slate-200 focus:outline-none focus:border-amber-500 bg-white font-semibold"
                >
                  <option v-for="b in batches" :key="b.id" :value="b.id">
                    {{ b.batchNumber }}
                  </option>
                </select>
              </div>

              <div>
                <label class="block text-xs font-bold text-slate-700 mb-1">RCN Input Weight (Kg)</label>
                <input 
                  v-model.number="rawInputWeightKg"
                  required
                  type="number" 
                  step="0.1" 
                  class="w-full text-xs p-2.5 rounded-xl border border-slate-200 focus:outline-none focus:border-amber-500 font-mono font-bold"
                />
              </div>
            </div>

            <!-- Grade Inputs Grid (Wholes, Splits, Pieces) -->
            <div class="space-y-2.5 bg-amber-50/40 p-4 rounded-2xl border border-amber-100">
              <span class="block text-[11px] font-bold text-slate-700 uppercase tracking-wider">
                Graded Output Kernels (Kg + Grams):
              </span>

              <div class="grid grid-cols-2 gap-2.5">
                <div>
                  <label class="block text-[10px] font-bold text-amber-900 mb-0.5">W-180 (King Jumbo Kg)</label>
                  <input v-model.number="w180Kg" type="number" step="0.005" placeholder="0.000" class="w-full text-xs p-2 rounded-lg border border-slate-200 font-mono" />
                </div>
                <div>
                  <label class="block text-[10px] font-bold text-amber-900 mb-0.5">W-210 (Jumbo Kg)</label>
                  <input v-model.number="w210Kg" type="number" step="0.005" placeholder="0.000" class="w-full text-xs p-2 rounded-lg border border-slate-200 font-mono" />
                </div>
              </div>

              <div class="grid grid-cols-2 gap-2.5">
                <div>
                  <label class="block text-[10px] font-bold text-amber-900 mb-0.5">W-240 (Standard Kg)</label>
                  <input v-model.number="w240Kg" type="number" step="0.005" placeholder="0.000" class="w-full text-xs p-2 rounded-lg border border-slate-200 font-mono" />
                </div>
                <div>
                  <label class="block text-[10px] font-bold text-amber-900 mb-0.5">W-320 (Regular Kg)</label>
                  <input v-model.number="w320Kg" type="number" step="0.005" placeholder="0.000" class="w-full text-xs p-2 rounded-lg border border-slate-200 font-mono" />
                </div>
              </div>

              <div class="grid grid-cols-2 gap-2.5">
                <div>
                  <label class="block text-[10px] font-bold text-slate-700 mb-0.5">JH (Splits / బద్దలు Kg)</label>
                  <input v-model.number="jhKg" type="number" step="0.005" placeholder="0.000" class="w-full text-xs p-2 rounded-lg border border-slate-200 font-mono" />
                </div>
                <div>
                  <label class="block text-[10px] font-bold text-slate-700 mb-0.5">LWP (Large Pieces Kg)</label>
                  <input v-model.number="lwpKg" type="number" step="0.005" placeholder="0.000" class="w-full text-xs p-2 rounded-lg border border-slate-200 font-mono" />
                </div>
              </div>
            </div>

            <!-- Dynamic Yield & Output Meter Card -->
            <div class="bg-slate-900 text-white p-4 rounded-2xl shadow-inner space-y-3">
              <div class="flex items-center justify-between text-xs border-b border-slate-800 pb-2">
                <span class="text-slate-400">Total Kernel Output:</span>
                <span class="font-mono font-bold text-white text-sm">
                  {{ totalKernelOutputKg.toFixed(3) }} Kg
                </span>
              </div>

              <div class="flex items-center justify-between">
                <div>
                  <p class="text-[11px] text-slate-400">Calculated Recovery Yield:</p>
                  <span 
                    :class="recoveryYieldPercentage >= 23.5 
                      ? 'text-emerald-400' 
                      : (recoveryYieldPercentage >= 21.0 ? 'text-amber-400' : 'text-rose-400')"
                    class="text-2xl font-black font-mono"
                  >
                    {{ recoveryYieldPercentage }}%
                  </span>
                </div>

                <div class="text-right">
                  <span 
                    v-if="recoveryYieldPercentage >= 23.5"
                    class="inline-flex items-center gap-1 text-[11px] font-bold px-2.5 py-1 rounded-full bg-emerald-950 text-emerald-400 border border-emerald-800"
                  >
                    <CheckCircle2 class="w-3.5 h-3.5" /> Optimal Yield
                  </span>
                  <span 
                    v-else-if="recoveryYieldPercentage >= 21.0"
                    class="inline-flex items-center gap-1 text-[11px] font-bold px-2.5 py-1 rounded-full bg-amber-950 text-amber-400 border border-amber-800"
                  >
                    <TrendingUp class="w-3.5 h-3.5" /> Acceptable
                  </span>
                  <span 
                    v-else
                    class="inline-flex items-center gap-1 text-[11px] font-bold px-2.5 py-1 rounded-full bg-rose-950 text-rose-400 border border-rose-800"
                  >
                    <AlertTriangle class="w-3.5 h-3.5" /> Low Recovery Alert
                  </span>
                </div>
              </div>
            </div>

            <button 
              type="submit"
              :disabled="isSubmitting || totalKernelOutputKg <= 0"
              class="w-full py-3.5 bg-amber-600 hover:bg-amber-700 disabled:opacity-40 text-white font-bold text-xs rounded-xl shadow-md transition flex items-center justify-center gap-2"
            >
              <PackageCheck class="w-4 h-4" />
              <span>Commit Grading Lot & Update Stock</span>
            </button>
          </form>
        </div>
      </div>

      <!-- Right Column: Live Warehouse Ground Stock Matrix (7 Cols) -->
      <div class="lg:col-span-7 bg-white p-5 sm:p-6 rounded-3xl border border-amber-100 shadow-sm flex flex-col justify-between">
        <div>
          <div class="flex items-center justify-between pb-3.5 border-b border-slate-100 mb-4">
            <div>
              <h2 class="text-base font-bold text-slate-900 flex items-center gap-2">
                <Boxes class="w-5 h-5 text-indigo-600" />
                <span>Warehouse Ground Stock Matrix</span>
              </h2>
              <p class="text-xs text-slate-400 mt-0.5">Real-time packed finished goods available for dispatch</p>
            </div>
            <div class="text-right">
              <span class="text-xs font-bold text-slate-500">Total Stock: </span>
              <span class="text-sm font-black text-amber-950">
                {{ stockMatrix.reduce((sum, s) => sum + s.totalWeightInKg, 0).toFixed(2) }} Kg
              </span>
            </div>
          </div>

          <!-- Stock Matrix Table -->
          <div class="overflow-x-auto">
            <table class="w-full text-left text-xs">
              <thead>
                <tr class="text-slate-400 uppercase text-[10px] tracking-wider border-b border-slate-100">
                  <th class="pb-2.5">Grade</th>
                  <th class="pb-2.5">Category</th>
                  <th class="pb-2.5 text-amber-950 font-bold">Available Kg</th>
                  <th class="pb-2.5">10Kg Tins</th>
                  <th class="pb-2.5 text-right">Wholesale Value</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-slate-100">
                <tr v-for="item in stockMatrix" :key="item.id" class="hover:bg-amber-50/40 transition">
                  <td class="py-3 font-bold text-slate-900">
                    <span class="px-2 py-0.5 rounded bg-amber-100 text-amber-900 mr-1.5 font-mono text-[11px]">
                      {{ item.gradeCode }}
                    </span>
                    {{ item.gradeName }}
                  </td>
                  <td class="py-3 text-slate-500 text-[11px] font-semibold">{{ item.category }}</td>
                  <td class="py-3 font-mono font-bold text-amber-950">{{ item.totalWeightInKg.toFixed(3) }}</td>
                  <td class="py-3 font-mono text-slate-700">
                    <span class="px-2 py-0.5 rounded bg-slate-100 font-bold">{{ item.totalQuantityCount }} Tins</span>
                  </td>
                  <td class="py-3 text-right font-mono font-bold text-emerald-700">
                    ₹{{ item.estimatedValuation.toLocaleString('en-IN') }}
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          <!-- Total Warehouse Valuation Summary Banner -->
          <div class="mt-6 p-4 rounded-2xl bg-amber-50/80 border border-amber-200/80 flex items-center justify-between">
            <div class="flex items-center gap-3">
              <div class="p-2.5 rounded-xl bg-amber-600 text-white">
                <Archive class="w-5 h-5" />
              </div>
              <div>
                <p class="text-xs font-bold text-amber-950">Total Finished Inventory Value</p>
                <p class="text-[11px] text-slate-500">Based on active Mandi wholesale benchmark prices</p>
              </div>
            </div>
            <div class="text-right">
              <p class="text-xl font-black text-amber-950 font-mono">
                ₹{{ stockMatrix.reduce((sum, s) => sum + s.estimatedValuation, 0).toLocaleString('en-IN') }}
              </p>
            </div>
          </div>

        </div>
      </div>

    </main>
  </div>
</template>