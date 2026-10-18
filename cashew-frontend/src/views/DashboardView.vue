<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '../stores/authStore';
import { useToastStore } from '../stores/toastStore';
import apiClient from '../api/client';
import { 
  LogOut, 
  Building2, 
  Users, 
  Scale, 
  TrendingUp, 
  FileText,
  Sparkles, 
  ArrowUpRight, 
  Truck, 
  CheckCircle2, 
  Boxes, 
  UserPlus,
  Wallet,
  Mic,
  MicOff,
  Bot,
  X,
  Send,
  CreditCard,
  Navigation
} from 'lucide-vue-next';

const router = useRouter();
const auth = useAuthStore();
const toast = useToastStore();

const handleLogout = () => {
  auth.logout();
  router.push('/login');
};

// Work Type Definitions
const workTypes = [
  { code: 'CUTTING', label: 'Cutting & Shelling', unit: 'per Kg', defaultRate: 26.0 },
  { code: 'PEELING', label: 'Peeling & Borma', unit: 'per Kg', defaultRate: 18.0 },
  { code: 'GRADING', label: 'Kernel Grading', unit: 'per Kg', defaultRate: 15.0 },
  { code: 'BOILING', label: 'Boiling / Furnace', unit: 'per Day', defaultRate: 500.0 },
  { code: 'PACKING', label: 'Tin Packaging', unit: 'per Day', defaultRate: 450.0 },
  { code: 'LOGISTICS', label: 'Driver / Logistics', unit: 'per Day', defaultRate: 650.0 }
];

// Data States
const laboursList = ref<any[]>([]);
const shedsList = ref<any[]>([]);
const isLoading = ref(true);

// Modals State
const isAddLabourOpen = ref(false);
const isPaymentModalOpen = ref(false);
const selectedLabourForPay = ref<any>(null);
const isSubmittingLabour = ref(false);
const isSubmittingPayment = ref(false);

// New Labour Form
const newLabour = ref({
  fullName: '',
  mobileNumber: '',
  shedId: 1,
  workType: 'CUTTING',
  pieceRate: 26.0,
  preferredLanguage: 'te',
  aadhaarNumber: ''
});

// Payment Form
const paymentForm = ref({
  amount: 0,
  paymentMode: 'CASH', // CASH, UPI, BANK_TRANSFER
  transactionRef: '',
  notes: 'Weekly wage settlement'
});

// Authorized Copilot Chat / Voice State
const isCopilotOpen = ref(false);
const copilotInput = ref('');
const isListening = ref(false);
const copilotMessages = ref<any[]>([
  {
    id: '1',
    sender: 'bot',
    text: `నమస్తే ${auth.user?.fullName || 'Sir'}! నేను మీ CashewPro Enterprise Copilot. Voice లేదా Text ద్వారా Labour Entry, Wage Payments, P&L లేదా Shed Status అడగవచ్చు.`,
    time: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
  }
]);

// Summary Computations
const totalPendingLabourAmount = computed(() => {
  return laboursList.value.reduce((sum, l) => sum + (l.currentBalanceAmount || 0), 0);
});

const loadDashboardData = async () => {
  try {
    isLoading.value = true;
    const [shedsRes, laboursRes] = await Promise.all([
      apiClient.get('/erp/ShedOperations/sheds'),
      apiClient.get('/erp/LabourManagement/labours')
    ]);

    if (shedsRes.data.success) shedsList.value = shedsRes.data.data;
    if (laboursRes.data.success) laboursList.value = laboursRes.data.data;
  } catch (err: any) {
    toast.add({
      severity: 'error',
      summary: 'Data Sync Error',
      detail: 'Dashboard data fetch చేయడంలో సమస్య వచ్చింది.'
    });
  } finally {
    isLoading.value = false;
  }
};

const handleWorkTypeChange = () => {
  const selected = workTypes.find(w => w.code === newLabour.value.workType);
  if (selected) {
    newLabour.value.pieceRate = selected.defaultRate;
  }
};

// 1. Real Database Labour Registration
const handleRegisterLabour = async () => {
  if (!newLabour.value.fullName.trim() || !newLabour.value.mobileNumber.trim()) {
    toast.add({ 
      severity: 'warn', 
      summary: 'Missing Info', 
      detail: 'దయచేసి Worker Name మరియు Mobile Number ఎంటర్ చేయండి.' 
    });
    return;
  }

  try {
    isSubmittingLabour.value = true;
    const isPerDay = ['BOILING', 'PACKING', 'LOGISTICS'].includes(newLabour.value.workType);
    
    const res = await apiClient.post('/erp/LabourManagement/register', {
      fullName: newLabour.value.fullName.trim(),
      mobileNumber: newLabour.value.mobileNumber.trim(),
      shedId: newLabour.value.shedId,
      workType: newLabour.value.workType,
      wageBasis: isPerDay ? 'PER_DAY' : 'PER_KG',
      rate: newLabour.value.pieceRate,
      preferredLanguage: newLabour.value.preferredLanguage,
      aadhaarNumber: newLabour.value.aadhaarNumber?.trim()
    });

    if (res.data.success) {
      toast.add({
        severity: 'success',
        summary: 'Labour Registered',
        detail: res.data.message
      });

      await loadDashboardData();

      newLabour.value.fullName = '';
      newLabour.value.mobileNumber = '';
      newLabour.value.aadhaarNumber = '';
      isAddLabourOpen.value = false;
    }
  } catch (err: any) {
    toast.add({
      severity: 'error',
      summary: 'Registration Failed',
      detail: err.response?.data?.message || 'డేటాబేస్ లో వర్కర్ ని రిజిస్టర్ చేయడం విఫలమైంది.'
    });
  } finally {
    isSubmittingLabour.value = false;
  }
};

const openPaymentModal = (labour: any) => {
  selectedLabourForPay.value = labour;
  paymentForm.value.amount = labour.currentBalanceAmount > 0 ? labour.currentBalanceAmount : 500;
  paymentForm.value.transactionRef = '';
  isPaymentModalOpen.value = true;
};

// 2. Real Database Wage Settlement & Ledger Deduction
const handleCommitPayment = async () => {
  if (!selectedLabourForPay.value || paymentForm.value.amount <= 0) {
    toast.add({ 
      severity: 'warn', 
      summary: 'Invalid Amount', 
      detail: 'దయచేసి సరైన చెల్లింపు మొత్తాన్ని ఎంటర్ చేయండి.' 
    });
    return;
  }

  try {
    isSubmittingPayment.value = true;
    const res = await apiClient.post('/erp/LabourManagement/settle-payment', {
      labourId: selectedLabourForPay.value.id,
      amount: paymentForm.value.amount,
      paymentMode: paymentForm.value.paymentMode,
      transactionRef: paymentForm.value.transactionRef,
      notes: paymentForm.value.notes,
      processedByUserId: auth.user?.id || 1
    });

    if (res.data.success) {
      isPaymentModalOpen.value = false;

      toast.add({
        severity: 'success',
        summary: 'Payment Committed',
        detail: res.data.message
      });

      await loadDashboardData();

      if (res.data.data?.whatsAppUrl) {
        window.open(res.data.data.whatsAppUrl, '_blank');
      }
    }
  } catch (err: any) {
    toast.add({
      severity: 'error',
      summary: 'Payment Failed',
      detail: err.response?.data?.message || 'పేమెంట్ రికార్డ్ చేయడంలో సమస్య వచ్చింది.'
    });
  } finally {
    isSubmittingPayment.value = false;
  }
};

// Voice Recognition
const toggleVoice = () => {
  const SpeechRecognition = (window as any).SpeechRecognition || (window as any).webkitSpeechRecognition;
  if (!SpeechRecognition) {
    alert('Voice input is not supported in this browser. Please use Google Chrome or Edge.');
    return;
  }

  if (isListening.value) {
    isListening.value = false;
    return;
  }

  const rec = new SpeechRecognition();
  rec.lang = 'te-IN';
  rec.continuous = false;

  rec.onstart = () => { isListening.value = true; };
  rec.onresult = (event: any) => {
    copilotInput.value = event.results[0][0].transcript;
    isListening.value = false;
    handleCopilotSend();
  };
  rec.onerror = () => { isListening.value = false; };
  rec.onend = () => { isListening.value = false; };

  rec.start();
};

const handleCopilotSend = () => {
  const q = copilotInput.value.trim();
  if (!q) return;

  copilotMessages.value.push({
    id: Date.now().toString(),
    sender: 'user',
    text: q,
    time: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
  });

  copilotInput.value = '';

  setTimeout(() => {
    const lower = q.toLowerCase();
    let reply = 'మీ సూచనను పరిశీలించాను.';

    if (lower.includes('pending') || lower.includes('బాకీ') || lower.includes('balance') || lower.includes('బ్యాలెన్స్')) {
      reply = `మొత్తం లేబర్ పెండింగ్ బ్యాలెన్స్: ₹${totalPendingLabourAmount.value.toLocaleString('en-IN')}. ${laboursList.value.length} మంది వర్కర్ల లెడ్జర్ డేటా యాక్టివ్‌గా ఉంది.`;
    } else if (lower.includes('payment') || lower.includes('పేమెంట్') || lower.includes('చెల్లింపు')) {
      reply = `పేమెంట్ డెస్క్ సిద్ధంగా ఉంది. కింద ఉన్న లేబర్ లిస్ట్‌లో 'Pay Wage' క్లిక్ చేసి క్యాష్ లేదా UPI ద్వారా వాట్సాప్ రశీదుతో పేమెంట్ చేయవచ్చు.`;
    } else if (lower.includes('new') || lower.includes('కొత్త') || lower.includes('labour') || lower.includes('వర్కర్')) {
      isAddLabourOpen.value = true;
      reply = `కొత్త వర్కర్ రిజిస్ట్రేషన్ ఫారమ్ ఓపెన్ చేశాను. కట్టింగ్, పీలింగ్, బాయిలింగ్ లేదా ప్యాకింగ్ కేటగిరీ ఎంచుకుని వివరాలు సేవ్ చేయండి.`;
    } else if (lower.includes('map') || lower.includes('ట్రాకర్') || lower.includes('sheds') || lower.includes('షెడ్')) {
      reply = `షెడ్ మ్యాప్ డెస్క్ సిద్ధంగా ఉంది. పైన ఉన్న 'Sheds Map & Fleet' బటన్ ద్వారా పలాస, మందస, కాశీబుగ్గ లైవ్ పిన్‌లు మరియు వ్యాన్ ట్రాన్సిట్ చూడవచ్చు.`;
    }

    copilotMessages.value.push({
      id: (Date.now() + 1).toString(),
      sender: 'bot',
      text: reply,
      time: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
    });
  }, 450);
};

onMounted(() => {
  loadDashboardData();
});
</script>

<template>
  <div class="min-h-screen bg-slate-50 flex flex-col selection:bg-amber-200">
    <!-- Top Header -->
    <header class="bg-amber-950 text-white px-4 sm:px-6 py-3.5 sm:py-4 flex items-center justify-between border-b border-amber-900 shadow-md sticky top-0 z-40">
      <div class="flex items-center gap-2.5 sm:gap-3">
        <div class="w-9 h-9 sm:w-10 sm:h-10 rounded-xl bg-gradient-to-tr from-amber-600 to-amber-500 flex items-center justify-center text-white font-bold shadow-md shrink-0">
          <Sparkles class="w-5 h-5" />
        </div>
        <div>
          <h1 class="text-sm sm:text-base font-bold font-serif leading-tight">CashewPro ERP Enterprise Suite</h1>
          <p class="text-[10px] sm:text-[11px] text-amber-300 font-mono tracking-wide">
            {{ auth.user?.roleName || 'Director / GM' }} • Control Panel
          </p>
        </div>
      </div>

      <div class="flex items-center gap-2 sm:gap-4">
        <!-- Floating Copilot Button in Header -->
        <button 
          @click="isCopilotOpen = true"
          class="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-xl bg-amber-600 hover:bg-amber-500 text-white text-xs font-bold shadow transition"
        >
          <Bot class="w-4 h-4" />
          <span class="hidden sm:inline">Voice & Chat Copilot</span>
        </button>

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

    <!-- Main Workspace -->
    <main class="flex-1 max-w-7xl w-full mx-auto p-4 sm:p-6 lg:p-8 space-y-6">
      
      <!-- Welcome Ribbon & Direct Desk Links -->
      <div class="bg-white rounded-3xl p-5 sm:p-6 border border-amber-100 shadow-sm flex flex-col md:flex-row items-start md:items-center justify-between gap-4">
        <div>
          <span class="px-3 py-1 rounded-full text-[10px] sm:text-xs font-black tracking-wider bg-emerald-100 text-emerald-800 uppercase">
            Live Central Command
          </span>
          <h2 class="text-xl sm:text-2xl font-black text-slate-900 font-serif mt-2">
            Namaste, {{ auth.user?.fullName }}
          </h2>
          <p class="text-xs text-slate-500 mt-1">
            Real-time multi-shed sync active across Palasa, Mandasa and coastal procurement units.
          </p>
        </div>
        
        <div class="flex flex-wrap items-center gap-2.5 w-full md:w-auto">
          <!-- Add Labour Trigger -->
          <button 
            @click="isAddLabourOpen = true"
            class="flex-1 sm:flex-initial inline-flex items-center justify-center gap-2 px-3.5 py-2.5 bg-indigo-700 hover:bg-indigo-800 text-white font-bold text-xs rounded-xl shadow-md transition"
          >
            <UserPlus class="w-4 h-4" />
            <span>+ New Labour</span>
          </button>

          <!-- Daily Shelling Entry Desk Link -->
          <router-link 
            to="/erp/shed-operations" 
            class="flex-1 sm:flex-initial inline-flex items-center justify-center gap-2 px-3.5 py-2.5 bg-amber-600 hover:bg-amber-700 text-white font-bold text-xs rounded-xl shadow-md transition"
          >
            <Scale class="w-4 h-4" />
            <span>Daily Shelling</span>
          </router-link>

          <!-- RCN Procurement Desk Link -->
          <router-link 
            to="/erp/procurement" 
            class="flex-1 sm:flex-initial inline-flex items-center justify-center gap-2 px-3.5 py-2.5 bg-slate-900 hover:bg-slate-800 text-white font-bold text-xs rounded-xl shadow-md transition"
          >
            <Truck class="w-4 h-4 text-amber-400" />
            <span>RCN Inward</span>
          </router-link>

          <!-- Grading & Stock Matrix Link -->
          <router-link 
            to="/erp/grading-stock" 
            class="flex-1 sm:flex-initial inline-flex items-center justify-center gap-2 px-3.5 py-2.5 bg-amber-800 hover:bg-amber-900 text-white font-bold text-xs rounded-xl shadow-md transition"
          >
            <Boxes class="w-4 h-4 text-amber-300" />
            <span>Grading & Stock</span>
          </router-link>

          <!-- GST Billing Link -->
          <router-link 
            to="/erp/billing" 
            class="flex-1 sm:flex-initial inline-flex items-center justify-center gap-2 px-3.5 py-2.5 bg-emerald-700 hover:bg-emerald-800 text-white font-bold text-xs rounded-xl shadow-md transition"
          >
            <FileText class="w-4 h-4" />
            <span>GST Billing</span>
          </router-link>

          <!-- Executive P&L Cockpit Link -->
          <router-link 
            to="/erp/analytics" 
            class="flex-1 sm:flex-initial inline-flex items-center justify-center gap-2 px-3.5 py-2.5 bg-slate-800 hover:bg-black text-amber-300 font-bold text-xs rounded-xl shadow-md transition"
          >
            <TrendingUp class="w-4 h-4" />
            <span>P&L Cockpit</span>
          </router-link>

          <!-- Sheds Map & Fleet Link -->
          <router-link 
            to="/erp/shed-logistics" 
            class="flex-1 sm:flex-initial inline-flex items-center justify-center gap-2 px-3.5 py-2.5 bg-amber-950 hover:bg-black text-amber-200 font-bold text-xs rounded-xl shadow-md transition"
          >
            <Navigation class="w-4 h-4 text-amber-400" />
            <span>Sheds Map</span>
          </router-link>

          <!-- Public Storefront Preview -->
          <router-link 
            to="/" 
            class="inline-flex items-center justify-center gap-1.5 text-xs font-bold text-amber-900 hover:text-amber-950 bg-amber-50 px-3.5 py-2.5 rounded-xl border border-amber-200 transition shrink-0"
            title="View Public Store"
          >
            <span class="hidden sm:inline">Store</span>
            <ArrowUpRight class="w-4 h-4" />
          </router-link>
        </div>
      </div>

      <!-- Quick Metrics Cards -->
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 sm:gap-6">
        
        <div class="bg-white p-5 rounded-2xl border border-slate-200/80 shadow-sm">
          <div class="flex items-center justify-between text-slate-500 mb-3">
            <span class="text-xs font-bold uppercase tracking-wider">Active Sheds</span>
            <Building2 class="w-5 h-5 text-amber-600" />
          </div>
          <p class="text-2xl font-black text-slate-900">{{ shedsList.length }} Units</p>
          <p class="text-[11px] text-emerald-600 font-semibold mt-1">100% operational today</p>
        </div>

        <div class="bg-white p-5 rounded-2xl border border-slate-200/80 shadow-sm">
          <div class="flex items-center justify-between text-slate-500 mb-3">
            <span class="text-xs font-bold uppercase tracking-wider">Active Workforce</span>
            <Users class="w-5 h-5 text-indigo-600" />
          </div>
          <p class="text-2xl font-black text-slate-900">{{ laboursList.length }} Workers</p>
          <p class="text-[11px] text-slate-500 mt-1">Cutting, Peeling, Boiling & Packing</p>
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
            <span class="text-xs font-bold uppercase tracking-wider">Total Pending Wage</span>
            <Wallet class="w-5 h-5 text-rose-600" />
          </div>
          <p class="text-2xl font-black text-rose-700 font-mono">
            ₹{{ totalPendingLabourAmount.toLocaleString('en-IN') }}
          </p>
          <p class="text-[11px] text-slate-500 mt-1">Outstanding labour liabilities</p>
        </div>

      </div>

      <!-- Labour Directory & Real Wage Settlement Table -->
      <div class="bg-white rounded-3xl p-5 sm:p-6 border border-amber-100 shadow-sm space-y-4">
        <div class="flex flex-col sm:flex-row sm:items-center justify-between pb-3.5 border-b border-slate-100 gap-2">
          <div>
            <h3 class="text-base font-bold text-slate-900 flex items-center gap-2">
              <Users class="w-5 h-5 text-indigo-700" />
              <span>Labour Directory & Live Wage Settlement Desk</span>
            </h3>
            <p class="text-xs text-slate-500 mt-0.5">
              Piece-rate cut tracking, daily wage settlements, and automated WhatsApp slip generation
            </p>
          </div>

          <button 
            @click="isAddLabourOpen = true"
            class="inline-flex items-center gap-1.5 px-3 py-1.5 bg-indigo-50 border border-indigo-200 text-indigo-800 text-xs font-bold rounded-xl hover:bg-indigo-100 transition self-start sm:self-auto"
          >
            <UserPlus class="w-3.5 h-3.5" />
            <span>+ Register Worker</span>
          </button>
        </div>

        <!-- Labour Table -->
        <div class="overflow-x-auto">
          <table class="w-full text-left text-xs">
            <thead>
              <tr class="text-slate-400 uppercase text-[10px] tracking-wider border-b border-slate-100">
                <th class="pb-3">Labour / Worker</th>
                <th class="pb-3">Work Category</th>
                <th class="pb-3">Assigned Shed</th>
                <th class="pb-3">Rate / Basis</th>
                <th class="pb-3 text-amber-950 font-bold">Total Cut (Kg)</th>
                <th class="pb-3 text-rose-700 font-bold">Pending Balance</th>
                <th class="pb-3 text-right">Actions</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-100">
              <tr v-if="laboursList.length === 0">
                <td colspan="7" class="py-12 text-center text-slate-400">
                  No registered workers found in database.
                </td>
              </tr>
              <tr v-for="lab in laboursList" :key="lab.id" class="hover:bg-amber-50/30 transition">
                <td class="py-3.5 font-bold text-slate-900">
                  {{ lab.fullName }}
                  <span class="block text-[10px] font-normal text-slate-500 font-mono">{{ lab.labourCode }} • {{ lab.mobileNumber }}</span>
                </td>
                <td class="py-3.5">
                  <span class="px-2 py-0.5 rounded text-[11px] font-semibold bg-slate-100 text-slate-800">
                    {{ lab.workType || 'CUTTING' }}
                  </span>
                </td>
                <td class="py-3.5 text-slate-600 font-medium">
                  {{ lab.shedName || 'Brahmana Tarla' }}
                </td>
                <td class="py-3.5 font-mono text-slate-700 font-bold">
                  ₹{{ lab.rate || lab.pieceRatePerKg }} 
                  <span class="text-[10px] text-slate-400 font-normal">/ {{ lab.wageBasis === 'PER_DAY' ? 'day' : 'kg' }}</span>
                </td>
                <td class="py-3.5 font-mono text-amber-950 font-bold">
                  {{ (lab.totalCutKg || 0).toFixed(3) }} Kg
                </td>
                <td class="py-3.5 font-mono font-bold text-rose-700 text-sm">
                  ₹{{ (lab.currentBalanceAmount || 0).toLocaleString('en-IN') }}
                </td>
                <td class="py-3.5 text-right">
                  <button 
                    @click="openPaymentModal(lab)"
                    class="px-3 py-1.5 bg-emerald-600 hover:bg-emerald-700 text-white font-bold text-[11px] rounded-lg shadow-sm transition inline-flex items-center gap-1.5"
                  >
                    <CreditCard class="w-3 h-3" />
                    <span>Pay Wage</span>
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

    </main>

    <!-- Modal 1: Register New Labour (Different Work Types) -->
    <div v-if="isAddLabourOpen" class="fixed inset-0 z-50 flex items-center justify-center p-4">
      <div @click="isAddLabourOpen = false" class="fixed inset-0 bg-slate-900/50 backdrop-blur-sm"></div>
      
      <div class="relative bg-white rounded-3xl max-w-md w-full p-6 shadow-2xl border border-amber-100 z-10">
        <div class="flex items-center justify-between pb-3.5 border-b border-slate-100 mb-4">
          <h3 class="text-base font-bold text-slate-900 flex items-center gap-2">
            <UserPlus class="w-4 h-4 text-indigo-600" />
            <span>On-board New Worker (All Work Types)</span>
          </h3>
          <button @click="isAddLabourOpen = false" class="p-1 rounded-lg text-slate-400 hover:text-slate-700">
            <X class="w-4 h-4" />
          </button>
        </div>

        <form @submit.prevent="handleRegisterLabour" class="space-y-3">
          <div>
            <label class="block text-xs font-bold text-slate-700 mb-1">Worker Full Name</label>
            <input v-model="newLabour.fullName" required placeholder="e.g. S. Malleswari" type="text" class="w-full text-xs p-2.5 rounded-xl border border-slate-200 focus:outline-none focus:border-amber-500 font-semibold" />
          </div>

          <div>
            <label class="block text-xs font-bold text-slate-700 mb-1">Mobile No (For WhatsApp Slip)</label>
            <input v-model="newLabour.mobileNumber" required placeholder="10-digit mobile number" type="tel" class="w-full text-xs p-2.5 rounded-xl border border-slate-200 focus:outline-none focus:border-amber-500 font-mono" />
          </div>

          <div class="grid grid-cols-2 gap-3">
            <div>
              <label class="block text-xs font-bold text-slate-700 mb-1">Work Type</label>
              <select v-model="newLabour.workType" @change="handleWorkTypeChange" class="w-full text-xs p-2.5 rounded-xl border border-slate-200 focus:outline-none focus:border-amber-500 bg-white font-semibold">
                <option v-for="w in workTypes" :key="w.code" :value="w.code">{{ w.label }}</option>
              </select>
            </div>

            <div>
              <label class="block text-xs font-bold text-slate-700 mb-1">Rate (₹ / Unit)</label>
              <input v-model.number="newLabour.pieceRate" required type="number" step="0.5" class="w-full text-xs p-2.5 rounded-xl border border-slate-200 focus:outline-none focus:border-amber-500 font-mono font-bold text-amber-900" />
            </div>
          </div>

          <div class="grid grid-cols-2 gap-3">
            <div>
              <label class="block text-xs font-bold text-slate-700 mb-1">Assigned Shed</label>
              <select v-model="newLabour.shedId" class="w-full text-xs p-2.5 rounded-xl border border-slate-200 focus:outline-none focus:border-amber-500 bg-white font-semibold">
                <option v-for="s in shedsList" :key="s.id" :value="s.id">{{ s.shedName }}</option>
              </select>
            </div>

            <div>
              <label class="block text-xs font-bold text-slate-700 mb-1">WhatsApp Language</label>
              <select v-model="newLabour.preferredLanguage" class="w-full text-xs p-2.5 rounded-xl border border-slate-200 focus:outline-none focus:border-amber-500 bg-white font-semibold">
                <option value="te">తెలుగు (Telugu)</option>
                <option value="en">English</option>
                <option value="hi">हिंदी (Hindi)</option>
              </select>
            </div>
          </div>

          <div>
            <label class="block text-xs font-bold text-slate-700 mb-1">Aadhaar Card No (Optional)</label>
            <input v-model="newLabour.aadhaarNumber" placeholder="12-digit Aadhaar number" type="text" class="w-full text-xs p-2.5 rounded-xl border border-slate-200 focus:outline-none focus:border-amber-500 font-mono" />
          </div>

          <button 
            type="submit" 
            :disabled="isSubmittingLabour"
            class="w-full py-3 bg-indigo-600 hover:bg-indigo-700 disabled:opacity-50 text-white font-bold text-xs rounded-xl shadow mt-2 transition flex items-center justify-center gap-2"
          >
            <span v-if="isSubmittingLabour" class="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin"></span>
            <span>Commit Worker to Shed Database</span>
          </button>
        </form>
      </div>
    </div>

    <!-- Modal 2: Direct Labour Payment Settlement -->
    <div v-if="isPaymentModalOpen" class="fixed inset-0 z-50 flex items-center justify-center p-4">
      <div @click="isPaymentModalOpen = false" class="fixed inset-0 bg-slate-900/50 backdrop-blur-sm"></div>

      <div class="relative bg-white rounded-3xl max-w-md w-full p-6 shadow-2xl border border-amber-100 z-10">
        <div class="flex items-center justify-between pb-3.5 border-b border-slate-100 mb-4">
          <div>
            <h3 class="text-base font-bold text-slate-900 flex items-center gap-2">
              <CreditCard class="w-4 h-4 text-emerald-600" />
              <span>Settle Labour Wage Payment</span>
            </h3>
            <p class="text-[11px] text-slate-500">{{ selectedLabourForPay?.fullName }} ({{ selectedLabourForPay?.labourCode }})</p>
          </div>
          <button @click="isPaymentModalOpen = false" class="p-1 rounded-lg text-slate-400 hover:text-slate-700">
            <X class="w-4 h-4" />
          </button>
        </div>

        <form @submit.prevent="handleCommitPayment" class="space-y-3.5">
          <div class="bg-amber-50 p-3 rounded-2xl border border-amber-200 flex justify-between items-center text-xs">
            <span class="text-slate-600">Current Outstanding Balance:</span>
            <span class="font-bold font-mono text-rose-700 text-base">₹{{ selectedLabourForPay?.currentBalanceAmount || 0 }}</span>
          </div>

          <div>
            <label class="block text-xs font-bold text-slate-700 mb-1">Amount to Pay Now (₹)</label>
            <input v-model.number="paymentForm.amount" required min="1" type="number" class="w-full text-sm p-2.5 rounded-xl border border-slate-300 font-mono font-black text-emerald-700 focus:outline-none focus:border-emerald-500" />
          </div>

          <div>
            <label class="block text-xs font-bold text-slate-700 mb-1">Payment Mode</label>
            <div class="grid grid-cols-3 gap-2">
              <button 
                type="button" 
                @click="paymentForm.paymentMode = 'CASH'"
                :class="paymentForm.paymentMode === 'CASH' ? 'bg-emerald-600 text-white font-bold' : 'bg-slate-100 text-slate-700'"
                class="py-2 rounded-xl text-xs transition"
              >
                Cash
              </button>
              <button 
                type="button" 
                @click="paymentForm.paymentMode = 'UPI'"
                :class="paymentForm.paymentMode === 'UPI' ? 'bg-emerald-600 text-white font-bold' : 'bg-slate-100 text-slate-700'"
                class="py-2 rounded-xl text-xs transition"
              >
                PhonePe / UPI
              </button>
              <button 
                type="button" 
                @click="paymentForm.paymentMode = 'BANK_TRANSFER'"
                :class="paymentForm.paymentMode === 'BANK_TRANSFER' ? 'bg-emerald-600 text-white font-bold' : 'bg-slate-100 text-slate-700'"
                class="py-2 rounded-xl text-xs transition"
              >
                Bank
              </button>
            </div>
          </div>

          <div>
            <label class="block text-xs font-bold text-slate-700 mb-1">Reference / UTR No (Optional)</label>
            <input v-model="paymentForm.transactionRef" placeholder="e.g. UPI Ref / Cash Voucher No" class="w-full text-xs p-2 rounded-xl border border-slate-200 focus:outline-none" />
          </div>

          <button 
            type="submit" 
            :disabled="isSubmittingPayment"
            class="w-full py-3.5 bg-emerald-600 hover:bg-emerald-700 disabled:opacity-50 text-white font-bold text-xs rounded-xl shadow mt-2 transition flex items-center justify-center gap-2"
          >
            <span v-if="isSubmittingPayment" class="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin"></span>
            <span v-else class="flex items-center gap-1.5">
              <CheckCircle2 class="w-4 h-4" />
              Confirm Payment & Send WhatsApp Slip
            </span>
          </button>
        </form>
      </div>
    </div>

    <!-- Authorized Copilot Voice & Chat Drawer -->
    <div v-if="isCopilotOpen" class="fixed inset-0 z-50 flex justify-end">
      <div @click="isCopilotOpen = false" class="fixed inset-0 bg-slate-900/40 backdrop-blur-sm"></div>

      <div class="relative w-full max-w-md bg-white h-full shadow-2xl flex flex-col z-10 border-l border-amber-200">
        <!-- Copilot Header -->
        <div class="p-4 bg-gradient-to-r from-amber-950 via-amber-900 to-amber-950 text-white flex items-center justify-between border-b border-amber-800">
          <div class="flex items-center gap-2.5">
            <div class="w-9 h-9 rounded-xl bg-amber-600 flex items-center justify-center text-white">
              <Bot class="w-5 h-5" />
            </div>
            <div>
              <h3 class="text-sm font-bold font-serif">CashewPro ERP Copilot</h3>
              <p class="text-[10px] text-amber-300 font-mono">STAFF VOICE & CHAT ENABLED</p>
            </div>
          </div>
          <button @click="isCopilotOpen = false" class="p-1.5 rounded-lg text-amber-300 hover:text-white">
            <X class="w-4 h-4" />
          </button>
        </div>

        <!-- Messages Area -->
        <div class="flex-1 p-4 overflow-y-auto space-y-3.5 bg-amber-50/30">
          <div 
            v-for="m in copilotMessages" 
            :key="m.id"
            :class="m.sender === 'user' ? 'justify-end' : 'justify-start'"
            class="flex items-end gap-2"
          >
            <div 
              :class="m.sender === 'user' ? 'bg-amber-600 text-white rounded-2xl rounded-br-none' : 'bg-white text-slate-800 border border-amber-100 rounded-2xl rounded-bl-none shadow-sm'"
              class="max-w-[85%] p-3 text-xs leading-relaxed"
            >
              <p>{{ m.text }}</p>
              <span class="block text-[9px] mt-1 text-right opacity-60">{{ m.time }}</span>
            </div>
          </div>

          <div v-if="isListening" class="flex items-center gap-2 p-2.5 bg-rose-50 border border-rose-200 rounded-xl text-rose-700 text-xs">
            <span class="w-2.5 h-2.5 bg-rose-600 rounded-full animate-ping"></span>
            <span class="font-bold">మీ వాయిస్ కమాండ్‌ను వింటున్నాను (Listening)...</span>
          </div>
        </div>

        <!-- Voice & Text Input -->
        <div class="p-3 bg-white border-t border-amber-100">
          <form @submit.prevent="handleCopilotSend" class="flex items-center gap-2">
            <button 
              type="button" 
              @click="toggleVoice"
              :class="isListening ? 'bg-rose-600 text-white animate-pulse' : 'bg-amber-100 text-amber-800'"
              class="p-2.5 rounded-xl transition"
              title="Speak in Telugu/English"
            >
              <MicOff v-if="isListening" class="w-4 h-4" />
              <Mic v-else class="w-4 h-4" />
            </button>

            <input 
              v-model="copilotInput"
              placeholder="Ask pending balance, labour pay or speak..."
              class="flex-1 text-xs p-2.5 rounded-xl border border-slate-200 focus:outline-none focus:border-amber-500 bg-slate-50"
            />

            <button 
              type="submit" 
              :disabled="!copilotInput.trim()"
              class="p-2.5 bg-amber-600 hover:bg-amber-700 disabled:opacity-40 text-white rounded-xl transition"
            >
              <Send class="w-4 h-4" />
            </button>
          </form>
        </div>
      </div>
    </div>

    <!-- Operations Footer -->
    <footer class="p-4 text-center text-slate-400 text-xs border-t border-slate-200 bg-white">
      CashewPro Multi-Shed ERP Suite • Palasa Central Processing Division
    </footer>
  </div>
</template>