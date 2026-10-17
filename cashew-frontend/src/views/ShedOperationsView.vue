<script setup lang="ts">
import { ref, onMounted, computed, watch } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '../stores/authStore';
import { useToastStore } from '../stores/toastStore';
import apiClient from '../api/client';
import { 
  Building2, 
  Scale, 
  Send, 
  ArrowLeft, 
  CheckCircle2, 
  Clock,
  Edit2,
  Trash2,
  XCircle,
  MessageSquare,
  Smartphone,
  Globe
} from 'lucide-vue-next';

const router = useRouter();
const auth = useAuthStore();
const toast = useToastStore();

interface Shed {
  id: number;
  shedCode: string;
  shedName: string;
  villageName: string;
  dailyCuttingTargetKg: number;
  labourCount: number;
}

interface Labour {
  id: number;
  labourCode: string;
  fullName: string;
  mobileNumber: string;
  preferredLanguage: string;
  pieceRatePerKg: number;
  currentBalanceAmount: number;
}

interface Entry {
  id: number;
  labourId: number;
  labourName: string;
  labourCode: string;
  rawSeedsIssuedKg: number;
  kernelCutWeightKg: number;
  uncutOrWasteWeightKg: number;
  totalEarnings: number;
  calculatedRatePerKg: number;
  time: string;
}

const sheds = ref<Shed[]>([]);
const selectedShedId = ref<number>(1);
const labours = ref<Labour[]>([]);
const todayEntries = ref<Entry[]>([]);

// Form State
const editingEntryId = ref<number | null>(null);
const selectedLabourId = ref<number | null>(null);
const rawIssuedKg = ref<number>(15.0);
const kernelCutKg = ref<number | null>(null);
const wasteKg = ref<number>(0.5);

// Checkboxes default checked
const sendWhatsApp = ref<boolean>(true);
const sendSms = ref<boolean>(true);
const messageLanguage = ref<string>('te'); // 'te', 'en', 'hi'

const isLoading = ref(false);
const isSubmitting = ref(false);
const statusMessage = ref('');
const dispatchedDetails = ref<any>(null);

const selectedLabour = computed(() => 
  labours.value.find(l => l.id === selectedLabourId.value)
);

// Worker maragane vaari preferred language auto-sync avthundi
watch(selectedLabour, (lab) => {
  if (lab && lab.preferredLanguage) {
    messageLanguage.value = lab.preferredLanguage;
  }
});

const estimatedEarning = computed(() => {
  if (!kernelCutKg.value || !selectedLabour.value) return 0;
  return Math.round(kernelCutKg.value * selectedLabour.value.pieceRatePerKg);
});

const loadShedData = async () => {
  try {
    isLoading.value = true;
    const [shedsRes, laboursRes, entriesRes] = await Promise.all([
      apiClient.get('/erp/ShedOperations/sheds'),
      apiClient.get(`/erp/ShedOperations/sheds/${selectedShedId.value}/labours`),
      apiClient.get(`/erp/ShedOperations/sheds/${selectedShedId.value}/today-entries`),
    ]);

    if (shedsRes.data.success) sheds.value = shedsRes.data.data;
    if (laboursRes.data.success) {
      labours.value = laboursRes.data.data;
      if (labours.value.length > 0 && !selectedLabourId.value) {
        selectedLabourId.value = labours.value[0].id;
        messageLanguage.value = labours.value[0].preferredLanguage || 'te';
      }
    }
    if (entriesRes.data.success) todayEntries.value = entriesRes.data.data;
  } catch (error) {
    console.error('Failed to load shed data:', error);
    toast.add({
      severity: 'error',
      summary: 'Data Fetch Error',
      detail: 'Shed data load cheyadam lo samasya vachindi. Network check cheyandi.',
      life: 4000
    });
  } finally {
    isLoading.value = false;
  }
};

const handleEditEntry = (entry: Entry) => {
  editingEntryId.value = entry.id;
  selectedLabourId.value = entry.labourId;
  rawIssuedKg.value = entry.rawSeedsIssuedKg;
  kernelCutKg.value = entry.kernelCutWeightKg;
  wasteKg.value = entry.uncutOrWasteWeightKg;
  statusMessage.value = '';
  dispatchedDetails.value = null;

  toast.add({
    severity: 'info',
    summary: 'Edit Mode Active',
    detail: `${entry.labourName} record (#${entry.id}) editing kosam load aindi.`,
    life: 3000
  });

  window.scrollTo({ top: 0, behavior: 'smooth' });
};

const cancelEdit = () => {
  editingEntryId.value = null;
  kernelCutKg.value = null;
  rawIssuedKg.value = 15.0;
  wasteKg.value = 0.5;
  statusMessage.value = '';
};

const handleDeleteEntry = async (entry: Entry) => {
  const confirmed = window.confirm(
    `${entry.labourName} yokka ${entry.kernelCutWeightKg.toFixed(3)} Kg cutting entry ni delete cheyali anukuntunnara? Earn chesina wage ₹${entry.totalEarnings} balance nundi rollback avthundi.`
  );
  if (!confirmed) return;

  try {
    const res = await apiClient.delete(`/erp/ShedOperations/entries/${entry.id}`);
    if (res.data.success) {
      if (editingEntryId.value === entry.id) {
        cancelEdit();
      }
      
      const entriesRes = await apiClient.get(`/erp/ShedOperations/sheds/${selectedShedId.value}/today-entries`);
      if (entriesRes.data.success) todayEntries.value = entriesRes.data.data;

      // PrimeVue Danger Toast
      toast.add({
        severity: 'error',
        summary: 'Entry Deleted',
        detail: `${entry.labourName}: ${entry.kernelCutWeightKg.toFixed(3)} Kg record delete aindi. ₹${res.data.rolledBackAmount} balance nundi rollback chesam.`,
        life: 5000
      });
    }
  } catch (err: any) {
    toast.add({
      severity: 'error',
      summary: 'Delete Failed',
      detail: err.response?.data?.message || 'Entry delete cheyadam lo error vachindi.',
      life: 4000
    });
  }
};

const handleSaveOrUpdateCutting = async () => {
  if (!selectedLabourId.value || !kernelCutKg.value || kernelCutKg.value <= 0) {
    toast.add({
      severity: 'warn',
      summary: 'Validation Alert',
      detail: 'Dayachesi 0 Kg kante ekkuva valid kernel cut weight enter cheyandi.',
      life: 3500
    });
    return;
  }

  try {
    isSubmitting.value = true;
    statusMessage.value = '';
    dispatchedDetails.value = null;

    const res = await apiClient.post('/erp/ShedOperations/save-cutting', {
      entryId: editingEntryId.value,
      shedId: selectedShedId.value,
      labourId: selectedLabourId.value,
      rawIssuedKg: rawIssuedKg.value,
      kernelCutKg: kernelCutKg.value,
      wasteKg: wasteKg.value,
      sendWhatsApp: sendWhatsApp.value,
      sendSms: sendSms.value,
      messageLanguage: messageLanguage.value,
      enteredByUserId: auth.user?.id || 1,
    });

    if (res.data.success) {
      statusMessage.value = res.data.message;
      dispatchedDetails.value = res.data.data;

      // 1. PrimeVue Floating Toast Message
      toast.add({
        severity: res.data.isUpdate ? 'warn' : 'success',
        summary: res.data.isUpdate ? 'Record Updated' : 'Entry Saved & Dispatched',
        detail: `${selectedLabour.value?.fullName}: ${res.data.data.cutWeightKg.toFixed(3)} Kg cut. Wage ₹${res.data.data.totalEarnings} credited. Total balance: ₹${res.data.data.labourBalance}.`,
        life: 5000
      });

      // 2. Refresh Table Log Immediately without page reload
      const entriesRes = await apiClient.get(`/erp/ShedOperations/sheds/${selectedShedId.value}/today-entries`);
      if (entriesRes.data.success) todayEntries.value = entriesRes.data.data;

      // 3. Clear Edit/Form inputs
      cancelEdit();
    }
  } catch (err: any) {
    toast.add({
      severity: 'error',
      summary: 'Action Failed',
      detail: err.response?.data?.message || 'Cutting record commit cheyadam lo error vachindi.',
      life: 4500
    });
  } finally {
    isSubmitting.value = false;
  }
};
onMounted(() => {
  loadShedData();
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
          <h1 class="text-sm sm:text-base font-bold font-serif leading-tight">Daily Shelling & Wage Desk</h1>
          <p class="text-[10px] sm:text-[11px] text-amber-300 font-mono">Gram Precision • Auto Multi-Lang WhatsApp & SMS Dispatch</p>
        </div>
      </div>

      <!-- Shed Switcher -->
      <div class="flex items-center gap-2">
        <Building2 class="w-4 h-4 text-amber-400 hidden sm:block" />
        <select 
          v-model="selectedShedId" 
          @change="loadShedData"
          class="bg-amber-900/80 border border-amber-700 text-white text-xs rounded-xl px-2.5 py-1.5 focus:outline-none font-bold"
        >
          <option v-for="s in sheds" :key="s.id" :value="s.id">
            {{ s.shedName }} ({{ s.villageName }})
          </option>
        </select>
      </div>
    </header>

    <!-- Main Content -->
    <main class="flex-1 max-w-7xl w-full mx-auto p-4 sm:p-6 grid grid-cols-1 lg:grid-cols-12 gap-6">
      
      <!-- Left Column: Fast Entry, Edit & Language Pad (5 Cols) -->
      <div class="lg:col-span-5 bg-white p-5 sm:p-6 rounded-3xl border border-amber-100 shadow-sm flex flex-col justify-between">
        <div>
          <div class="flex items-center justify-between pb-3.5 border-b border-slate-100 mb-4">
            <h2 class="text-base font-bold text-slate-900 flex items-center gap-2">
              <Scale class="w-5 h-5 text-amber-600" />
              <span>{{ editingEntryId ? 'Edit Shelling Record' : 'Record Daily Weight' }}</span>
            </h2>
            <div class="flex items-center gap-2">
              <span v-if="editingEntryId" class="px-2.5 py-0.5 rounded-full text-[10px] font-black bg-amber-500 text-white animate-pulse">
                Editing #{{ editingEntryId }}
              </span>
              <span class="px-2.5 py-0.5 rounded-full text-[10px] font-black bg-amber-100 text-amber-900">
                Gram Precision
              </span>
            </div>
          </div>

          <!-- Success Alert with Full Month & Pending Summary -->
          <div v-if="statusMessage && dispatchedDetails" class="mb-4 p-4 rounded-2xl bg-emerald-50 border border-emerald-200 text-emerald-900 text-xs space-y-2">
            <div class="flex items-center gap-2 font-bold">
              <CheckCircle2 class="w-4 h-4 text-emerald-600 shrink-0" />
              <span>{{ statusMessage }}</span>
            </div>
            
            <div class="bg-white/80 p-2.5 rounded-xl border border-emerald-200 text-[11px] space-y-1 font-mono">
              <p>• Today Cut: <span class="font-bold text-slate-800">{{ dispatchedDetails.cutWeightKg.toFixed(3) }} Kg</span> (₹{{ dispatchedDetails.totalEarnings }})</p>
              <p>• Month Total: <span class="font-bold text-slate-800">{{ dispatchedDetails.monthTotalKg.toFixed(3) }} Kg</span> (₹{{ dispatchedDetails.monthTotalEarnings }})</p>
              <p>• Total Pending Balance: <span class="font-bold text-emerald-700">₹{{ dispatchedDetails.labourBalance }}</span></p>
            </div>

            <div class="flex items-center gap-2 pt-1 text-[10px] text-emerald-700 font-semibold">
              <span class="bg-emerald-100 text-emerald-900 px-2 py-0.5 rounded font-mono uppercase">
                Lang: {{ dispatchedDetails.channels?.language }}
              </span>
              <span v-if="dispatchedDetails.channels?.whatsAppSent" class="inline-flex items-center gap-1 bg-emerald-100 px-2 py-0.5 rounded">✓ WhatsApp Sent</span>
              <span v-if="dispatchedDetails.channels?.smsSent" class="inline-flex items-center gap-1 bg-indigo-100 text-indigo-800 px-2 py-0.5 rounded">✓ SMS Dispatched</span>
            </div>
          </div>

          <form @submit.prevent="handleSaveOrUpdateCutting" class="space-y-4">
            <!-- Select Labour -->
            <div>
              <label class="block text-xs font-bold text-slate-700 mb-1">Select Labour / Worker</label>
              <select 
                v-model="selectedLabourId"
                :disabled="!!editingEntryId"
                class="w-full text-xs p-3 rounded-xl border border-slate-200 focus:outline-none focus:border-amber-500 bg-white font-semibold disabled:bg-slate-100"
              >
                <option v-for="lab in labours" :key="lab.id" :value="lab.id">
                  {{ lab.fullName }} ({{ lab.labourCode }}) — ₹{{ lab.pieceRatePerKg }}/kg [{{ lab.preferredLanguage.toUpperCase() }}]
                </option>
              </select>
            </div>

            <!-- Weight Issued vs Cut Weight (Kg + Grams) -->
            <div class="grid grid-cols-2 gap-3">
              <div>
                <label class="block text-xs font-bold text-slate-700 mb-1">Raw Issued (Kg)</label>
                <input 
                  v-model.number="rawIssuedKg"
                  type="number" 
                  step="0.050" 
                  class="w-full text-xs p-3 rounded-xl border border-slate-200 focus:outline-none focus:border-amber-500 font-mono"
                />
              </div>

              <div>
                <label class="block text-xs font-bold text-amber-900 mb-1">Kernel Cut (Kg + g)</label>
                <input 
                  v-model.number="kernelCutKg"
                  required
                  placeholder="e.g. 14.350"
                  type="number" 
                  step="0.005" 
                  class="w-full text-xs p-3 rounded-xl border-2 border-amber-500 focus:outline-none focus:ring-2 focus:ring-amber-400 font-mono font-bold text-amber-950 bg-amber-50/30"
                />
              </div>
            </div>

            <!-- Wastage Weight -->
            <div>
              <label class="block text-xs font-bold text-slate-700 mb-1">Waste / Shells / Damaged (Kg)</label>
              <input 
                v-model.number="wasteKg"
                type="number" 
                step="0.010" 
                class="w-full text-xs p-3 rounded-xl border border-slate-200 focus:outline-none focus:border-amber-500 font-mono"
              />
            </div>

            <!-- Auto Notification Channels & Language Selector -->
            <div class="bg-slate-50 p-3.5 rounded-2xl border border-slate-200 space-y-3">
              <div class="flex items-center justify-between">
                <span class="text-[11px] font-bold text-slate-700 uppercase tracking-wider">
                  Notification Channels & Language
                </span>
                
                <!-- Language Selector for Message -->
                <div class="flex items-center bg-white border border-slate-300 rounded-lg p-0.5 text-[10px] font-bold">
                  <button 
                    type="button"
                    @click="messageLanguage = 'te'"
                    :class="messageLanguage === 'te' ? 'bg-amber-600 text-white' : 'text-slate-600'"
                    class="px-2 py-0.5 rounded transition"
                  >
                    తెలుగు
                  </button>
                  <button 
                    type="button"
                    @click="messageLanguage = 'en'"
                    :class="messageLanguage === 'en' ? 'bg-amber-600 text-white' : 'text-slate-600'"
                    class="px-2 py-0.5 rounded transition"
                  >
                    EN
                  </button>
                  <button 
                    type="button"
                    @click="messageLanguage = 'hi'"
                    :class="messageLanguage === 'hi' ? 'bg-amber-600 text-white' : 'text-slate-600'"
                    class="px-2 py-0.5 rounded transition"
                  >
                    हिंदी
                  </button>
                </div>
              </div>

              <div class="flex items-center gap-5 text-xs font-semibold text-slate-800">
                <label class="inline-flex items-center gap-2 cursor-pointer">
                  <input 
                    type="checkbox" 
                    v-model="sendWhatsApp"
                    class="w-4 h-4 text-emerald-600 rounded border-slate-300 focus:ring-emerald-500" 
                  />
                  <span class="flex items-center gap-1 text-emerald-800">
                    <MessageSquare class="w-3.5 h-3.5" />
                    WhatsApp
                  </span>
                </label>

                <label class="inline-flex items-center gap-2 cursor-pointer">
                  <input 
                    type="checkbox" 
                    v-model="sendSms"
                    class="w-4 h-4 text-indigo-600 rounded border-slate-300 focus:ring-indigo-500" 
                  />
                  <span class="flex items-center gap-1 text-indigo-800">
                    <Smartphone class="w-3.5 h-3.5" />
                    Text SMS
                  </span>
                </label>
              </div>

              <p class="text-[10px] text-slate-400">
                Message includes today's kgs/wages, current month totals, and balance in selected language.
              </p>
            </div>

            <!-- Instant Calculation Card -->
            <div class="bg-amber-50/70 p-4 rounded-2xl border border-amber-200/80 flex items-center justify-between">
              <div>
                <p class="text-[11px] text-slate-500 font-medium">Calculated Wage for this Entry:</p>
                <p class="text-xs text-slate-700">
                  Rate: ₹{{ selectedLabour?.pieceRatePerKg || 0 }}/kg × {{ kernelCutKg || 0 }} kg
                </p>
              </div>
              <p class="text-2xl font-black text-amber-950">
                ₹{{ estimatedEarning }}
              </p>
            </div>

            <!-- Action Buttons -->
            <div class="flex items-center gap-2">
              <button 
                type="submit"
                :disabled="isSubmitting"
                class="flex-1 py-3.5 bg-slate-900 hover:bg-amber-600 disabled:opacity-50 text-white font-bold text-xs rounded-xl shadow-md transition flex items-center justify-center gap-2"
              >
                <Send class="w-4 h-4" />
                <span>{{ editingEntryId ? 'Update Entry & Resend Message' : 'Record Weight & Auto Send Message' }}</span>
              </button>

              <button 
                v-if="editingEntryId"
                type="button"
                @click="cancelEdit"
                class="p-3.5 bg-slate-100 hover:bg-slate-200 text-slate-600 rounded-xl transition"
                title="Cancel Edit"
              >
                <XCircle class="w-4 h-4" />
              </button>
            </div>
          </form>
        </div>
      </div>

      <!-- Right Column: Today's Live Entries Table with Edit & Delete (7 Cols) -->
      <div class="lg:col-span-7 bg-white p-5 sm:p-6 rounded-3xl border border-amber-100 shadow-sm flex flex-col justify-between">
        <div>
          <div class="flex items-center justify-between pb-3.5 border-b border-slate-100 mb-4">
            <div>
              <h2 class="text-base font-bold text-slate-900 flex items-center gap-2">
                <Clock class="w-5 h-5 text-indigo-600" />
                <span>Today's Shed Shelling Log</span>
              </h2>
              <p class="text-xs text-slate-400 mt-0.5">Real-time entries recorded at this village shed</p>
            </div>
            <div class="text-right">
              <span class="text-xs font-bold text-slate-500">Total Cut: </span>
              <span class="text-sm font-black text-emerald-700">
                {{ todayEntries.reduce((sum, e) => sum + e.kernelCutWeightKg, 0).toFixed(3) }} Kg
              </span>
            </div>
          </div>

          <!-- Entries Table with Edit & Delete Actions -->
          <div class="overflow-x-auto">
            <table class="w-full text-left text-xs">
              <thead>
                <tr class="text-slate-400 uppercase text-[10px] tracking-wider border-b border-slate-100">
                  <th class="pb-2.5">Labour</th>
                  <th class="pb-2.5">Issued (Kg)</th>
                  <th class="pb-2.5 text-amber-900">Kernel Cut (Kg)</th>
                  <th class="pb-2.5">Earned</th>
                  <th class="pb-2.5">Time</th>
                  <th class="pb-2.5 text-right">Actions</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-slate-100">
                <tr v-if="todayEntries.length === 0">
                  <td colspan="6" class="py-12 text-center text-slate-400">
                    No entries logged yet today for this shed.
                  </td>
                </tr>
                <tr 
                  v-for="entry in todayEntries" 
                  :key="entry.id" 
                  :class="editingEntryId === entry.id ? 'bg-amber-100/60' : 'hover:bg-amber-50/40'"
                  class="transition"
                >
                  <td class="py-3 font-bold text-slate-900">
                    {{ entry.labourName }}
                    <span class="block text-[10px] font-normal text-slate-400 font-mono">{{ entry.labourCode }}</span>
                  </td>
                  <td class="py-3 text-slate-600 font-mono">{{ entry.rawSeedsIssuedKg.toFixed(3) }}</td>
                  <td class="py-3 font-bold text-amber-950 font-mono">{{ entry.kernelCutWeightKg.toFixed(3) }}</td>
                  <td class="py-3 font-bold text-emerald-700">₹{{ entry.totalEarnings }}</td>
                  <td class="py-3 text-slate-400 font-mono text-[11px]">{{ entry.time }}</td>
                  <td class="py-3 text-right">
                    <div class="inline-flex items-center gap-1.5">
                      <!-- Edit Button -->
                      <button 
                        @click="handleEditEntry(entry)"
                        class="p-1.5 text-amber-800 bg-amber-50 hover:bg-amber-100 rounded-lg border border-amber-200 transition"
                        title="Edit entry"
                      >
                        <Edit2 class="w-3.5 h-3.5" />
                      </button>
                      
                      <!-- Delete Button -->
                      <button 
                        @click="handleDeleteEntry(entry)"
                        class="p-1.5 text-rose-700 bg-rose-50 hover:bg-rose-100 rounded-lg border border-rose-200 transition"
                        title="Delete entry & rollback wage"
                      >
                        <Trash2 class="w-3.5 h-3.5" />
                      </button>
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>

    </main>
  </div>
</template>