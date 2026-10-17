<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRouter } from 'vue-router';
import { useToastStore } from '../stores/toastStore';
import apiClient from '../api/client';
import { 
  Truck, 
  Scale, 
  UserPlus, 
  ArrowLeft, 
  PlusCircle, 
  FileCheck, 
  CheckCircle2, 
  Clock, 
  DollarSign, 
  Layers,
  X
} from 'lucide-vue-next';

const router = useRouter();
const toast = useToastStore();

interface Supplier {
  id: number;
  fullName: string;
  phoneNumber: string;
  supplierType: string;
  village: string | null;
  district: string | null;
}

interface InwardRecord {
  id: number;
  lotNumber: string;
  supplierName: string;
  supplierPhone: string;
  vehicleNumber: string;
  originMandi: string;
  bagsCount: number;
  grossWeightKg: number;
  tareWeightKg: number;
  netWeightKg: number;
  moisturePercentage: number | null;
  outturnLbs: number | null;
  ratePerQuintal: number;
  totalProcurementCost: number;
  time: string;
}

const suppliers = ref<Supplier[]>([]);
const recentInwards = ref<InwardRecord[]>([]);
const isLoading = ref(false);
const isSubmitting = ref(false);

// Inward Form
const supplierId = ref<number | null>(null);
const vehicleNumber = ref('');
const originMandi = ref('Palasa APMC');
const bagsCount = ref<number>(80);
const grossWeightKg = ref<number | null>(null);
const tareWeightKg = ref<number | null>(null);
const ratePerQuintal = ref<number>(12800);
const moisturePercentage = ref<number>(8.5);
const outturnLbs = ref<number>(50.0);
const driverName = ref('');
const driverPhone = ref('');

// Farmer Modal
const isFarmerModalOpen = ref(false);
const newFarmerName = ref('');
const newFarmerPhone = ref('');
const newFarmerVillage = ref('');
const newFarmerDistrict = ref('Srikakulam');

// Computed Auto-Calculations
const netWeightKg = computed(() => {
  if (!grossWeightKg.value || !tareWeightKg.value) return 0;
  const net = grossWeightKg.value - tareWeightKg.value;
  return net > 0 ? Number(net.toFixed(3)) : 0;
});

const netQuintals = computed(() => {
  return Number((netWeightKg.value / 100).toFixed(2));
});

const calculatedTotalCost = computed(() => {
  if (netWeightKg.value <= 0 || !ratePerQuintal.value) return 0;
  return Math.round((netWeightKg.value / 100) * ratePerQuintal.value);
});

const loadInitialData = async () => {
  try {
    isLoading.value = true;
    const [suppRes, inwRes] = await Promise.all([
      apiClient.get('/erp/Procurement/suppliers'),
      apiClient.get('/erp/Procurement/recent-inwards')
    ]);

    if (suppRes.data.success) {
      suppliers.value = suppRes.data.data;
      if (suppliers.value.length > 0) supplierId.value = suppliers.value[0].id;
    }
    if (inwRes.data.success) {
      recentInwards.value = inwRes.data.data;
    }
  } catch (err: any) {
    toast.add({
      severity: 'error',
      summary: 'Data Fetch Error',
      detail: 'Procurement data load cheyadam lo error vachindi.'
    });
  } finally {
    isLoading.value = false;
  }
};

const handleCreateFarmer = async () => {
  if (!newFarmerName.value.trim() || !newFarmerPhone.value.trim()) {
    toast.add({
      severity: 'warn',
      summary: 'Missing Details',
      detail: 'Farmer name mariyu phone number enter cheyandi.'
    });
    return;
  }

  try {
    const res = await apiClient.post('/erp/Procurement/suppliers', {
      fullName: newFarmerName.value,
      phoneNumber: newFarmerPhone.value,
      supplierType: 'LOCAL_FARMER',
      village: newFarmerVillage.value,
      district: newFarmerDistrict.value,
      state: 'Andhra Pradesh'
    });

    if (res.data.success) {
      toast.add({
        severity: 'success',
        summary: 'Farmer Registered',
        detail: `${res.data.data.fullName} successfully on-boarded.`
      });
      suppliers.value.unshift(res.data.data);
      supplierId.value = res.data.data.id;

      // Reset
      newFarmerName.value = '';
      newFarmerPhone.value = '';
      newFarmerVillage.value = '';
      isFarmerModalOpen.value = false;
    }
  } catch (err: any) {
    toast.add({
      severity: 'error',
      summary: 'Registration Failed',
      detail: err.response?.data?.message || 'Error creating farmer profile.'
    });
  }
};

const handleRecordInward = async () => {
  if (!supplierId.value) {
    toast.add({ severity: 'warn', summary: 'Select Farmer', detail: 'Dayachesi farmer ni select cheyandi.' });
    return;
  }
  if (!vehicleNumber.value.trim()) {
    toast.add({ severity: 'warn', summary: 'Missing Vehicle', detail: 'Vehicle / Lorry number enter cheyandi.' });
    return;
  }
  if (netWeightKg.value <= 0) {
    toast.add({ severity: 'warn', summary: 'Invalid Weights', detail: 'Gross weight must be higher than Tare weight.' });
    return;
  }

  try {
    isSubmitting.value = true;
    const res = await apiClient.post('/erp/Procurement/record-inward', {
      supplierId: supplierId.value,
      originMandi: originMandi.value,
      vehicleNumber: vehicleNumber.value,
      driverName: driverName.value,
      driverPhone: driverPhone.value,
      bagsCount: bagsCount.value,
      grossWeightKg: grossWeightKg.value,
      tareWeightKg: tareWeightKg.value,
      moisturePercentage: moisturePercentage.value,
      outturnLbs: outturnLbs.value,
      ratePerQuintal: ratePerQuintal.value
    });

    if (res.data.success) {
      toast.add({
        severity: 'success',
        summary: 'Inward Recorded',
        detail: `Lot ${res.data.data.lotNumber}: ${res.data.data.netWeightKg} Kg (${res.data.data.netWeightQuintals} Quintals) registered. Total: ₹${res.data.data.totalProcurementCost}.`
      });

      // Refresh list
      const inwRes = await apiClient.get('/erp/Procurement/recent-inwards');
      if (inwRes.data.success) recentInwards.value = inwRes.data.data;

      // Reset weights
      vehicleNumber.value = '';
      grossWeightKg.value = null;
      tareWeightKg.value = null;
    }
  } catch (err: any) {
    toast.add({
      severity: 'error',
      summary: 'Entry Failed',
      detail: err.response?.data?.message || 'Failed to record procurement.'
    });
  } finally {
    isSubmitting.value = false;
  }
};

onMounted(() => {
  loadInitialData();
});
</script>

<template>
  <div class="min-h-screen bg-slate-50 flex flex-col selection:bg-amber-200">
    <!-- Top Header -->
    <header class="bg-amber-950 text-white px-4 sm:px-6 py-3.5 flex items-center justify-between border-b border-amber-900 shadow sticky top-0 z-40">
      <div class="flex items-center gap-3">
        <router-link to="/erp/dashboard" class="p-2 rounded-xl bg-amber-900/60 hover:bg-amber-800 text-amber-200 transition">
          <ArrowLeft class="w-4 h-4" />
        </router-link>
        <div>
          <h1 class="text-sm sm:text-base font-bold font-serif leading-tight">Raw Cashew Nut (RCN) Inward Weighbridge Desk</h1>
          <p class="text-[10px] sm:text-[11px] text-amber-300 font-mono">Gross/Tare Weighbridge • Moisture & Outturn (Lbs) • Farmer Settlement</p>
        </div>
      </div>

      <button 
        @click="isFarmerModalOpen = true"
        class="inline-flex items-center gap-1.5 px-3 py-1.5 bg-amber-600 hover:bg-amber-700 text-white font-bold text-xs rounded-xl shadow transition"
      >
        <UserPlus class="w-3.5 h-3.5" />
        <span class="hidden sm:inline">+ Register Farmer</span>
      </button>
    </header>

    <!-- Main Workspace -->
    <main class="flex-1 max-w-7xl w-full mx-auto p-4 sm:p-6 grid grid-cols-1 lg:grid-cols-12 gap-6">
      
      <!-- Left Column: Weighbridge Inward Form (5 Cols) -->
      <div class="lg:col-span-5 bg-white p-5 sm:p-6 rounded-3xl border border-amber-100 shadow-sm flex flex-col justify-between">
        <div>
          <div class="flex items-center justify-between pb-3.5 border-b border-slate-100 mb-4">
            <h2 class="text-base font-bold text-slate-900 flex items-center gap-2">
              <Truck class="w-5 h-5 text-amber-600" />
              <span>Weighbridge Entry Slip</span>
            </h2>
            <span class="px-2.5 py-0.5 rounded-full text-[10px] font-black bg-amber-100 text-amber-900">
              Mandi / Gate Inward
            </span>
          </div>

          <form @submit.prevent="handleRecordInward" class="space-y-3.5">
            <!-- Farmer / Vendor Selector -->
            <div>
              <div class="flex items-center justify-between mb-1">
                <label class="text-xs font-bold text-slate-700">Select Farmer / Supplier</label>
                <button 
                  type="button" 
                  @click="isFarmerModalOpen = true" 
                  class="text-[11px] font-bold text-amber-700 hover:underline"
                >
                  + Add New
                </button>
              </div>
              <select 
                v-model="supplierId"
                required
                class="w-full text-xs p-3 rounded-xl border border-slate-200 focus:outline-none focus:border-amber-500 bg-white font-semibold"
              >
                <option v-for="s in suppliers" :key="s.id" :value="s.id">
                  {{ s.fullName }} ({{ s.phoneNumber }}) — {{ s.village || s.district }}
                </option>
              </select>
            </div>

            <!-- Vehicle & Mandi Origin -->
            <div class="grid grid-cols-2 gap-3">
              <div>
                <label class="block text-xs font-bold text-slate-700 mb-1">Vehicle / Truck No</label>
                <input 
                  v-model="vehicleNumber"
                  required
                  placeholder="e.g. AP30 TX 4589"
                  type="text" 
                  class="w-full text-xs p-2.5 rounded-xl border border-slate-200 focus:outline-none focus:border-amber-500 font-mono font-bold uppercase"
                />
              </div>

              <div>
                <label class="block text-xs font-bold text-slate-700 mb-1">Bags Count</label>
                <input 
                  v-model.number="bagsCount"
                  required
                  type="number" 
                  class="w-full text-xs p-2.5 rounded-xl border border-slate-200 focus:outline-none focus:border-amber-500 font-mono"
                />
              </div>
            </div>

            <!-- Weighbridge Gross & Tare (Kg) -->
            <div class="grid grid-cols-2 gap-3 bg-amber-50/50 p-3.5 rounded-2xl border border-amber-200/70">
              <div>
                <label class="block text-[11px] font-bold text-slate-700 mb-1">Gross Weight (Lorry + Seeds Kg)</label>
                <input 
                  v-model.number="grossWeightKg"
                  required
                  placeholder="e.g. 12450.000"
                  type="number" 
                  step="0.050" 
                  class="w-full text-xs p-2.5 rounded-xl border border-slate-300 focus:outline-none focus:border-amber-500 font-mono font-bold"
                />
              </div>

              <div>
                <label class="block text-[11px] font-bold text-slate-700 mb-1">Tare Weight (Empty Lorry Kg)</label>
                <input 
                  v-model.number="tareWeightKg"
                  required
                  placeholder="e.g. 4200.000"
                  type="number" 
                  step="0.050" 
                  class="w-full text-xs p-2.5 rounded-xl border border-slate-300 focus:outline-none focus:border-amber-500 font-mono font-bold"
                />
              </div>
            </div>

            <!-- Quality Check: Outturn & Moisture -->
            <div class="grid grid-cols-2 gap-3">
              <div>
                <label class="block text-xs font-bold text-slate-700 mb-1">Outturn (Lbs / 80kg)</label>
                <input 
                  v-model.number="outturnLbs"
                  type="number" 
                  step="0.5" 
                  placeholder="e.g. 50.5"
                  class="w-full text-xs p-2.5 rounded-xl border border-slate-200 focus:outline-none focus:border-amber-500 font-mono"
                />
              </div>

              <div>
                <label class="block text-xs font-bold text-slate-700 mb-1">Moisture (%)</label>
                <input 
                  v-model.number="moisturePercentage"
                  type="number" 
                  step="0.1" 
                  placeholder="e.g. 8.5"
                  class="w-full text-xs p-2.5 rounded-xl border border-slate-200 focus:outline-none focus:border-amber-500 font-mono"
                />
              </div>
            </div>

            <!-- Pricing: Rate Per Quintal -->
            <div>
              <label class="block text-xs font-bold text-slate-700 mb-1">Mandi Rate Per Quintal (₹)</label>
              <input 
                v-model.number="ratePerQuintal"
                required
                type="number" 
                step="50"
                class="w-full text-xs p-2.5 rounded-xl border border-slate-200 focus:outline-none focus:border-amber-500 font-mono font-bold text-emerald-800"
              />
            </div>

            <!-- Instant Calculation Card -->
            <div class="bg-amber-950 text-white p-4 rounded-2xl shadow-inner space-y-2">
              <div class="flex items-center justify-between text-xs border-b border-amber-900 pb-2">
                <span class="text-amber-300">Net Raw Seeds Weight:</span>
                <span class="font-mono font-bold text-amber-100 text-sm">
                  {{ netWeightKg.toFixed(3) }} Kg <span class="text-xs text-amber-400">({{ netQuintals }} Quintals)</span>
                </span>
              </div>
              <div class="flex items-center justify-between">
                <span class="text-xs text-amber-200">Total Farmer Payout:</span>
                <span class="text-2xl font-black text-amber-400 font-mono">
                  ₹{{ calculatedTotalCost.toLocaleString('en-IN') }}
                </span>
              </div>
            </div>

            <button 
              type="submit"
              :disabled="isSubmitting || netWeightKg <= 0"
              class="w-full py-3.5 bg-slate-900 hover:bg-amber-600 disabled:opacity-40 text-white font-bold text-xs rounded-xl shadow-md transition flex items-center justify-center gap-2"
            >
              <FileCheck class="w-4 h-4" />
              <span>Generate Inward Lot & Commit Stock</span>
            </button>
          </form>
        </div>
      </div>

      <!-- Right Column: Recent Inwards Table (7 Cols) -->
      <div class="lg:col-span-7 bg-white p-5 sm:p-6 rounded-3xl border border-amber-100 shadow-sm flex flex-col justify-between">
        <div>
          <div class="flex items-center justify-between pb-3.5 border-b border-slate-100 mb-4">
            <div>
              <h2 class="text-base font-bold text-slate-900 flex items-center gap-2">
                <Layers class="w-5 h-5 text-indigo-600" />
                <span>Recent Consignments Received</span>
              </h2>
              <p class="text-xs text-slate-400 mt-0.5">Real-time gate weighbridge inward consignments</p>
            </div>
            <div class="text-right">
              <span class="text-xs font-bold text-slate-500">Today's Inward: </span>
              <span class="text-sm font-black text-emerald-700">
                {{ (recentInwards.reduce((sum, r) => sum + r.netWeightKg, 0) / 1000).toFixed(2) }} Tons
              </span>
            </div>
          </div>

          <!-- Inwards Table -->
          <div class="overflow-x-auto">
            <table class="w-full text-left text-xs">
              <thead>
                <tr class="text-slate-400 uppercase text-[10px] tracking-wider border-b border-slate-100">
                  <th class="pb-2.5">Lot & Vehicle</th>
                  <th class="pb-2.5">Farmer / Mandi</th>
                  <th class="pb-2.5">Bags</th>
                  <th class="pb-2.5 text-amber-950 font-bold">Net Kg</th>
                  <th class="pb-2.5">Quality</th>
                  <th class="pb-2.5 text-right">Total Cost</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-slate-100">
                <tr v-if="recentInwards.length === 0">
                  <td colspan="6" class="py-12 text-center text-slate-400">
                    No inward consignments recorded yet.
                  </td>
                </tr>
                <tr v-for="rec in recentInwards" :key="rec.id" class="hover:bg-amber-50/40 transition">
                  <td class="py-3 font-bold text-slate-900">
                    {{ rec.vehicleNumber }}
                    <span class="block text-[10px] font-normal text-slate-400 font-mono">{{ rec.lotNumber }}</span>
                  </td>
                  <td class="py-3">
                    <span class="font-semibold text-slate-800">{{ rec.supplierName }}</span>
                    <span class="block text-[10px] text-slate-400">{{ rec.originMandi }}</span>
                  </td>
                  <td class="py-3 font-mono text-slate-600">{{ rec.bagsCount }}</td>
                  <td class="py-3 font-bold text-amber-950 font-mono">{{ rec.netWeightKg.toFixed(3) }}</td>
                  <td class="py-3 text-[11px] font-mono text-slate-600">
                    <span class="px-1.5 py-0.5 rounded bg-emerald-50 text-emerald-700 font-bold">{{ rec.outturnLbs || 50 }} lbs</span>
                  </td>
                  <td class="py-3 text-right font-black text-emerald-800 font-mono">
                    ₹{{ rec.totalProcurementCost.toLocaleString('en-IN') }}
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>

    </main>

    <!-- Register Farmer Modal Popup -->
    <div v-if="isFarmerModalOpen" class="fixed inset-0 z-50 flex items-center justify-center p-4">
      <div @click="isFarmerModalOpen = false" class="fixed inset-0 bg-slate-900/50 backdrop-blur-sm"></div>
      
      <div class="relative bg-white rounded-3xl max-w-md w-full p-6 shadow-2xl border border-amber-100 z-10">
        <div class="flex items-center justify-between pb-3.5 border-b border-slate-100 mb-4">
          <h3 class="text-base font-bold text-slate-900 flex items-center gap-2">
            <UserPlus class="w-4 h-4 text-amber-600" />
            <span>On-board New Farmer / Supplier</span>
          </h3>
          <button @click="isFarmerModalOpen = false" class="p-1.5 rounded-lg text-slate-400 hover:text-slate-700">
            <X class="w-4 h-4" />
          </button>
        </div>

        <form @submit.prevent="handleCreateFarmer" class="space-y-3">
          <div>
            <label class="block text-xs font-bold text-slate-700 mb-1">Farmer Full Name</label>
            <input v-model="newFarmerName" required type="text" placeholder="e.g. K. Venkata Rao" class="w-full text-xs p-2.5 rounded-xl border border-slate-200 focus:outline-none focus:border-amber-500" />
          </div>

          <div>
            <label class="block text-xs font-bold text-slate-700 mb-1">Mobile Number</label>
            <input v-model="newFarmerPhone" required type="tel" placeholder="10-digit phone number" class="w-full text-xs p-2.5 rounded-xl border border-slate-200 focus:outline-none focus:border-amber-500" />
          </div>

          <div class="grid grid-cols-2 gap-3">
            <div>
              <label class="block text-xs font-bold text-slate-700 mb-1">Village / Mandal</label>
              <input v-model="newFarmerVillage" type="text" placeholder="e.g. Mandasa" class="w-full text-xs p-2.5 rounded-xl border border-slate-200 focus:outline-none focus:border-amber-500" />
            </div>
            <div>
              <label class="block text-xs font-bold text-slate-700 mb-1">District</label>
              <input v-model="newFarmerDistrict" type="text" placeholder="Srikakulam" class="w-full text-xs p-2.5 rounded-xl border border-slate-200 focus:outline-none focus:border-amber-500" />
            </div>
          </div>

          <button 
            type="submit"
            class="w-full py-3 bg-amber-600 hover:bg-amber-700 text-white font-bold text-xs rounded-xl shadow mt-2 transition"
          >
            Save Farmer & Return to Slip
          </button>
        </form>
      </div>
    </div>

  </div>
</template>