<script setup lang="ts">
import { ref } from 'vue';
import { X, Building2, Send, CheckCircle2 } from 'lucide-vue-next';

const props = defineProps<{ isOpen: boolean }>();
const emit = defineEmits(['close']);

const form = ref({
  businessName: '',
  contactPerson: '',
  phone: '',
  gstNumber: '',
  deliveryCity: '',
  selectedGrade: 'W-240',
  requiredWeightKg: 100,
  notes: ''
});

const isSubmitted = ref(false);

const submitBulkInquiry = () => {
  // In next step, this connects to Backend POST /api/v1/store/Orders/wholesale-inquiry
  isSubmitted.value = true;
  setTimeout(() => {
    isSubmitted.value = false;
    emit('close');
  }, 2200);
};
</script>

<template>
  <div v-if="isOpen" class="fixed inset-0 z-50 flex items-center justify-center p-4">
    <!-- Backdrop -->
    <div @click="emit('close')" class="fixed inset-0 bg-slate-900/50 backdrop-blur-sm"></div>

    <!-- Modal Box -->
    <div class="relative bg-white rounded-3xl max-w-lg w-full p-6 shadow-2xl border border-amber-100 z-10 overflow-hidden">
      <div class="flex items-center justify-between pb-4 border-b border-amber-100">
        <div class="flex items-center gap-2.5">
          <div class="p-2 rounded-xl bg-amber-100 text-amber-800">
            <Building2 class="w-5 h-5" />
          </div>
          <div>
            <h3 class="font-bold text-slate-900 font-serif">B2B Wholesale / Bulk Quote</h3>
            <p class="text-[11px] text-slate-500">For sweet shops, marts, factories & exporters</p>
          </div>
        </div>
        <button @click="emit('close')" class="p-1.5 rounded-xl text-slate-400 hover:text-slate-700 hover:bg-slate-100">
          <X class="w-5 h-5" />
        </button>
      </div>

      <!-- Success Screen -->
      <div v-if="isSubmitted" class="py-12 text-center space-y-3">
        <CheckCircle2 class="w-14 h-14 text-emerald-500 mx-auto animate-bounce" />
        <h4 class="text-lg font-bold text-slate-900">Quotation Request Received!</h4>
        <p class="text-xs text-slate-500 max-w-xs mx-auto">
          Mee wholesale request register aindi. Mana Sales Team WhatsApp dwara formal invoice & discount rates share chestharu.
        </p>
      </div>

      <!-- Form -->
      <form v-else @submit.prevent="submitBulkInquiry" class="mt-4 space-y-3.5">
        <div class="grid grid-cols-2 gap-3">
          <div>
            <label class="block text-[11px] font-bold text-slate-700 mb-1">Company / Shop Name</label>
            <input v-model="form.businessName" required type="text" placeholder="e.g. Sri Balaji Sweets" class="w-full text-xs p-2.5 rounded-xl border border-slate-200 focus:outline-none focus:border-amber-500" />
          </div>
          <div>
            <label class="block text-[11px] font-bold text-slate-700 mb-1">Contact Person</label>
            <input v-model="form.contactPerson" required type="text" placeholder="Full Name" class="w-full text-xs p-2.5 rounded-xl border border-slate-200 focus:outline-none focus:border-amber-500" />
          </div>
        </div>

        <div class="grid grid-cols-2 gap-3">
          <div>
            <label class="block text-[11px] font-bold text-slate-700 mb-1">Mobile / WhatsApp No</label>
            <input v-model="form.phone" required type="tel" placeholder="10-digit number" class="w-full text-xs p-2.5 rounded-xl border border-slate-200 focus:outline-none focus:border-amber-500" />
          </div>
          <div>
            <label class="block text-[11px] font-bold text-slate-700 mb-1">GST Number (Optional)</label>
            <input v-model="form.gstNumber" type="text" placeholder="22AAAAA0000A1Z5" class="w-full text-xs p-2.5 rounded-xl border border-slate-200 focus:outline-none focus:border-amber-500 uppercase" />
          </div>
        </div>

        <div class="grid grid-cols-2 gap-3">
          <div>
            <label class="block text-[11px] font-bold text-slate-700 mb-1">Selected Grade</label>
            <select v-model="form.selectedGrade" class="w-full text-xs p-2.5 rounded-xl border border-slate-200 focus:outline-none focus:border-amber-500 bg-white">
              <option value="W-180">W-180 (King Jumbo)</option>
              <option value="W-210">W-210 (Jumbo)</option>
              <option value="W-240">W-240 (Standard)</option>
              <option value="W-320">W-320 (Regular)</option>
              <option value="JH">JH (Splits)</option>
              <option value="LWP">LWP (Large Pieces)</option>
            </select>
          </div>
          <div>
            <label class="block text-[11px] font-bold text-slate-700 mb-1">Quantity in Kg (Min 50kg)</label>
            <input v-model.number="form.requiredWeightKg" min="50" step="10" type="number" class="w-full text-xs p-2.5 rounded-xl border border-slate-200 focus:outline-none focus:border-amber-500" />
          </div>
        </div>

        <div>
          <label class="block text-[11px] font-bold text-slate-700 mb-1">Delivery Destination (City / Mandal / State)</label>
          <input v-model="form.deliveryCity" required type="text" placeholder="e.g. Hyderabad, Telangana" class="w-full text-xs p-2.5 rounded-xl border border-slate-200 focus:outline-none focus:border-amber-500" />
        </div>

        <button 
          type="submit"
          class="w-full py-3 bg-slate-900 hover:bg-amber-600 text-white font-bold text-xs rounded-xl shadow-md transition flex items-center justify-center gap-2 mt-4"
        >
          <Send class="w-4 h-4" />
          <span>Submit Wholesale Quotation Request</span>
        </button>
      </form>
    </div>
  </div>
</template>