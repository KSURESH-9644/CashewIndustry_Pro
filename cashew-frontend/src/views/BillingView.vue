<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRouter } from 'vue-router';
import { useToastStore } from '../stores/toastStore';
import apiClient from '../api/client';
import { 
  FileText, 
  ArrowLeft, 
  Plus, 
  Trash2, 
  Printer, 
  Truck, 
  CheckCircle2, 
  ShieldCheck, 
  Download,
  Building2
} from 'lucide-vue-next';

const router = useRouter();
const toast = useToastStore();

interface WholesaleCustomer {
  id: number;
  businessName: string;
  contactPerson: string;
  phone: string;
  gstNumber: string | null;
  city: string;
  state: string;
  shippingAddress: string;
}

interface Grade {
  id: number;
  gradeCode: string;
  gradeName: string;
  currentWholesalePricePerKg: number;
}

interface InvoiceRow {
  gradeId: number;
  quantityTins: number;
  unitPricePerKg: number;
}

const customers = ref<WholesaleCustomer[]>([]);
const availableGrades = ref<Grade[]>([]);
const recentInvoices = ref<any[]>([]);
const isLoading = ref(false);
const isSubmitting = ref(false);

// Form
const selectedCustomerId = ref<number | null>(null);
const businessName = ref('');
const contactPerson = ref('');
const phone = ref('');
const gstNumber = ref('');
const shippingAddress = ref('Visakhapatnam Main Road');
const destinationState = ref('Andhra Pradesh');

// Logistics / E-Way Bill
const vehicleNumber = ref('AP30 TX 9876');
const transporterName = ref('Sri Balaji Roadlines');
const approxDistanceKm = ref<number>(180);

// Line Items
const invoiceItems = ref<InvoiceRow[]>([
  { gradeId: 3, quantityTins: 5, unitPricePerKg: 840 }, // W-240 (5 tins = 50 kg)
  { gradeId: 5, quantityTins: 3, unitPricePerKg: 640 }  // JH Splits (3 tins = 30 kg)
]);

// Last Generated Invoice Preview
const lastInvoice = ref<any>(null);

const subTotal = computed(() => {
  return invoiceItems.value.reduce((sum, item) => sum + (item.quantityTins * 10 * item.unitPricePerKg), 0);
});

const isInterstate = computed(() => {
  return destinationState.value !== 'Andhra Pradesh';
});

const taxAmount = computed(() => {
  return Math.round(subTotal.value * 0.05);
});

const grandTotal = computed(() => {
  return subTotal.value + taxAmount.value;
});

const loadBillingData = async () => {
  try {
    isLoading.value = true;
    const [custRes, gradesRes, invRes] = await Promise.all([
      apiClient.get('/erp/Billing/wholesale-customers'),
      apiClient.get('/store/Products/grades'),
      apiClient.get('/erp/Billing/recent-invoices')
    ]);

    if (custRes.data.success) customers.value = custRes.data.data;
    if (gradesRes.data.success) availableGrades.value = gradesRes.data.data;
    if (invRes.data.success) recentInvoices.value = invRes.data.data;
  } catch (err: any) {
    toast.add({
      severity: 'error',
      summary: 'Data Error',
      detail: 'Billing metadata load cheyadam lo error vachindi.'
    });
  } finally {
    isLoading.value = false;
  }
};

const handleCustomerSelect = () => {
  const cust = customers.value.find(c => c.id === selectedCustomerId.value);
  if (cust) {
    businessName.value = cust.businessName || cust.contactPerson;
    contactPerson.value = cust.contactPerson;
    phone.value = cust.phone;
    gstNumber.value = cust.gstNumber || '';
    shippingAddress.value = cust.shippingAddress;
    destinationState.value = cust.state;
  }
};

const addItemRow = () => {
  invoiceItems.value.push({ gradeId: 1, quantityTins: 1, unitPricePerKg: 1050 });
};

const removeItemRow = (index: number) => {
  if (invoiceItems.value.length > 1) {
    invoiceItems.value.splice(index, 1);
  }
};

const handleGenerateInvoice = async () => {
  if (!businessName.value.trim() || !phone.value.trim()) {
    toast.add({ severity: 'warn', summary: 'Missing Info', detail: 'Party name & phone are required.' });
    return;
  }

  try {
    isSubmitting.value = true;
    const res = await apiClient.post('/erp/Billing/generate-invoice', {
      customerId: selectedCustomerId.value,
      businessName: businessName.value,
      contactPerson: contactPerson.value || businessName.value,
      phone: phone.value,
      gstNumber: gstNumber.value,
      shippingAddress: shippingAddress.value,
      destinationState: destinationState.value,
      vehicleNumber: vehicleNumber.value,
      transporterName: transporterName.value,
      approxDistanceKm: approxDistanceKm.value,
      items: invoiceItems.value
    });

    if (res.data.success) {
      lastInvoice.value = res.data.data;
      toast.add({
        severity: 'success',
        summary: 'Invoice & E-Way Bill Generated',
        detail: `Bill #${res.data.data.orderNumber} for ₹${res.data.data.GrandTotal.toLocaleString('en-IN')} created. Stock deducted.`
      });

      // Refresh list
      const invRes = await apiClient.get('/erp/Billing/recent-invoices');
      if (invRes.data.success) recentInvoices.value = invRes.data.data;
    }
  } catch (err: any) {
    toast.add({
      severity: 'error',
      summary: 'Invoice Failed',
      detail: err.response?.data?.message || 'Invoice generate cheyadam lo error.'
    });
  } finally {
    isSubmitting.value = false;
  }
};

onMounted(() => {
  loadBillingData();
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
          <h1 class="text-sm sm:text-base font-bold font-serif leading-tight">GST Tax Invoicing & E-Way Bill Desk</h1>
          <p class="text-[10px] sm:text-[11px] text-amber-300 font-mono">HSN 0801 • Interstate IGST / CGST+SGST (5%) • Automatic Stock Deduction</p>
        </div>
      </div>

      <div class="flex items-center gap-2">
        <span class="px-2.5 py-1 rounded-full text-xs font-bold bg-amber-900 border border-amber-700 text-amber-200">
          GST HSN: 0801
        </span>
      </div>
    </header>

    <!-- Main Workspace -->
    <main class="flex-1 max-w-7xl w-full mx-auto p-4 sm:p-6 grid grid-cols-1 lg:grid-cols-12 gap-6">
      
      <!-- Left Column: Billing & Dispatch Form (7 Cols) -->
      <div class="lg:col-span-7 bg-white p-5 sm:p-6 rounded-3xl border border-amber-100 shadow-sm flex flex-col justify-between">
        <div>
          <div class="flex items-center justify-between pb-3.5 border-b border-slate-100 mb-4">
            <h2 class="text-base font-bold text-slate-900 flex items-center gap-2">
              <FileText class="w-5 h-5 text-emerald-600" />
              <span>Generate Wholesale Tax Invoice</span>
            </h2>
            <span class="px-2.5 py-0.5 rounded-full text-[10px] font-black bg-emerald-100 text-emerald-900">
              B2B / Interstate
            </span>
          </div>

          <form @submit.prevent="handleGenerateInvoice" class="space-y-4">
            
            <!-- Customer Selector -->
            <div>
              <label class="block text-xs font-bold text-slate-700 mb-1">Select Existing Client / Shop</label>
              <select 
                v-model="selectedCustomerId" 
                @change="handleCustomerSelect"
                class="w-full text-xs p-2.5 rounded-xl border border-slate-200 focus:outline-none focus:border-amber-500 bg-white font-semibold"
              >
                <option :value="null">-- New Client / Direct Walk-in --</option>
                <option v-for="c in customers" :key="c.id" :value="c.id">
                  {{ c.businessName || c.contactPerson }} ({{ c.city }}, {{ c.state }})
                </option>
              </select>
            </div>

            <!-- Party Details -->
            <div class="grid grid-cols-2 gap-3">
              <div>
                <label class="block text-xs font-bold text-slate-700 mb-1">Business / Party Name</label>
                <input v-model="businessName" required placeholder="e.g. Sri Balaji Sweets" type="text" class="w-full text-xs p-2.5 rounded-xl border border-slate-200 focus:outline-none focus:border-amber-500" />
              </div>
              <div>
                <label class="block text-xs font-bold text-slate-700 mb-1">Mobile / WhatsApp No</label>
                <input v-model="phone" required placeholder="10-digit number" type="tel" class="w-full text-xs p-2.5 rounded-xl border border-slate-200 focus:outline-none focus:border-amber-500" />
              </div>
            </div>

            <div class="grid grid-cols-2 gap-3">
              <div>
                <label class="block text-xs font-bold text-slate-700 mb-1">Party GSTIN (Optional)</label>
                <input v-model="gstNumber" placeholder="37AAAAA0000A1Z5" type="text" class="w-full text-xs p-2.5 rounded-xl border border-slate-200 focus:outline-none focus:border-amber-500 uppercase font-mono" />
              </div>
              <div>
                <label class="block text-xs font-bold text-slate-700 mb-1">Destination State</label>
                <select v-model="destinationState" class="w-full text-xs p-2.5 rounded-xl border border-slate-200 focus:outline-none focus:border-amber-500 bg-white font-semibold">
                  <option value="Andhra Pradesh">Andhra Pradesh (CGST+SGST 5%)</option>
                  <option value="Telangana">Telangana (IGST 5%)</option>
                  <option value="Odisha">Odisha (IGST 5%)</option>
                  <option value="Maharashtra">Maharashtra (IGST 5%)</option>
                  <option value="Gujarat">Gujarat (IGST 5%)</option>
                  <option value="Delhi">Delhi (IGST 5%)</option>
                </select>
              </div>
            </div>

            <!-- Line Items: Grades & 10Kg Tins -->
            <div class="bg-amber-50/50 p-4 rounded-2xl border border-amber-100 space-y-3">
              <div class="flex items-center justify-between">
                <span class="text-xs font-bold text-amber-950 uppercase tracking-wider">
                  Dispatched Grade Items (1 Tin = 10 Kg):
                </span>
                <button type="button" @click="addItemRow" class="text-xs font-bold text-amber-800 hover:underline flex items-center gap-1">
                  <Plus class="w-3.5 h-3.5" /> Add Grade Item
                </button>
              </div>

              <div v-for="(item, idx) in invoiceItems" :key="idx" class="grid grid-cols-12 gap-2 items-center">
                <div class="col-span-5">
                  <select v-model="item.gradeId" class="w-full text-xs p-2 rounded-lg border border-slate-200 bg-white font-semibold">
                    <option v-for="g in availableGrades" :key="g.id" :value="g.id">
                      {{ g.gradeCode }} - {{ g.gradeName }}
                    </option>
                  </select>
                </div>
                <div class="col-span-3">
                  <input v-model.number="item.quantityTins" type="number" min="1" placeholder="Tins" class="w-full text-xs p-2 rounded-lg border border-slate-200 font-mono" />
                </div>
                <div class="col-span-3">
                  <input v-model.number="item.unitPricePerKg" type="number" step="10" placeholder="₹/kg" class="w-full text-xs p-2 rounded-lg border border-slate-200 font-mono font-bold" />
                </div>
                <div class="col-span-1 text-right">
                  <button type="button" @click="removeItemRow(idx)" class="text-slate-400 hover:text-rose-600">
                    <Trash2 class="w-3.5 h-3.5" />
                  </button>
                </div>
              </div>
            </div>

            <!-- E-Way Bill / Transport Info -->
            <div class="grid grid-cols-3 gap-3 bg-slate-50 p-3.5 rounded-2xl border border-slate-200">
              <div>
                <label class="block text-[11px] font-bold text-slate-700 mb-1">Vehicle No</label>
                <input v-model="vehicleNumber" required type="text" class="w-full text-xs p-2 rounded-lg border border-slate-300 font-mono font-bold uppercase" />
              </div>
              <div>
                <label class="block text-[11px] font-bold text-slate-700 mb-1">Transporter</label>
                <input v-model="transporterName" required type="text" class="w-full text-xs p-2 rounded-lg border border-slate-300" />
              </div>
              <div>
                <label class="block text-[11px] font-bold text-slate-700 mb-1">Distance (Km)</label>
                <input v-model.number="approxDistanceKm" required type="number" class="w-full text-xs p-2 rounded-lg border border-slate-300 font-mono" />
              </div>
            </div>

            <!-- Total Breakdown Card -->
            <div class="bg-amber-950 text-white p-4 rounded-2xl shadow-inner space-y-1.5 font-mono text-xs">
              <div class="flex justify-between text-amber-200">
                <span>Taxable Subtotal:</span>
                <span>₹{{ subTotal.toLocaleString('en-IN') }}</span>
              </div>
              <div class="flex justify-between text-amber-300">
                <span>{{ isInterstate ? 'IGST (5%):' : 'CGST (2.5%) + SGST (2.5%):' }}</span>
                <span>₹{{ taxAmount.toLocaleString('en-IN') }}</span>
              </div>
              <div class="flex justify-between text-base font-bold text-white border-t border-amber-900 pt-2">
                <span class="font-sans">Grand Invoice Total:</span>
                <span class="text-amber-400 text-lg">₹{{ grandTotal.toLocaleString('en-IN') }}</span>
              </div>
            </div>

            <button 
              type="submit"
              :disabled="isSubmitting || subTotal <= 0"
              class="w-full py-3.5 bg-emerald-600 hover:bg-emerald-700 disabled:opacity-40 text-white font-bold text-xs rounded-xl shadow-md transition flex items-center justify-center gap-2"
            >
              <FileText class="w-4 h-4" />
              <span>Generate Tax Invoice & Commit Stock Dispatch</span>
            </button>
          </form>
        </div>
      </div>

      <!-- Right Column: Recent Invoices & E-Way Bills (5 Cols) -->
      <div class="lg:col-span-5 bg-white p-5 sm:p-6 rounded-3xl border border-amber-100 shadow-sm flex flex-col justify-between">
        <div>
          <div class="flex items-center justify-between pb-3.5 border-b border-slate-100 mb-4">
            <h2 class="text-base font-bold text-slate-900 flex items-center gap-2">
              <Truck class="w-5 h-5 text-indigo-600" />
              <span>Recent Dispatches & Bills</span>
            </h2>
            <span class="text-xs font-bold text-slate-500">Live Ledger</span>
          </div>

          <!-- Invoices List -->
          <div class="space-y-3 overflow-y-auto max-h-[560px] pr-1">
            <div v-if="recentInvoices.length === 0" class="py-12 text-center text-slate-400 text-xs">
              No invoices generated yet.
            </div>

            <div 
              v-for="inv in recentInvoices" 
              :key="inv.id" 
              class="p-4 rounded-2xl border border-slate-200 hover:border-amber-300 bg-white transition space-y-2 shadow-sm"
            >
              <div class="flex items-center justify-between">
                <span class="font-mono text-xs font-bold text-slate-900">{{ inv.orderNumber }}</span>
                <span class="px-2 py-0.5 rounded text-[10px] font-bold bg-emerald-100 text-emerald-800">
                  {{ inv.orderStatus }}
                </span>
              </div>

              <div class="flex justify-between items-baseline text-xs">
                <div>
                  <p class="font-bold text-slate-800">{{ inv.customerName }}</p>
                  <p class="text-[11px] text-slate-400">{{ inv.customerState }} • {{ inv.date }}</p>
                </div>
                <p class="text-base font-black text-amber-950 font-mono">
                  ₹{{ inv.grandTotal.toLocaleString('en-IN') }}
                </p>
              </div>
            </div>
          </div>
        </div>

        <!-- Last Invoice Printable Banner -->
        <div v-if="lastInvoice" class="mt-4 p-4 rounded-2xl bg-emerald-50 border border-emerald-200 text-xs text-emerald-950 flex items-center justify-between">
          <div>
            <p class="font-bold">E-Way Bill: {{ lastInvoice.ewayBillNumber }}</p>
            <p class="text-[11px] text-emerald-800">Vehicle: {{ lastInvoice.vehicleNumber }}</p>
          </div>
          <button 
            @click="window.print()" 
            class="px-3 py-1.5 bg-emerald-600 text-white font-bold rounded-xl text-xs flex items-center gap-1.5 hover:bg-emerald-700 shadow"
          >
            <Printer class="w-3.5 h-3.5" /> Print Bill
          </button>
        </div>
      </div>

    </main>
  </div>
</template>