<script setup lang="ts">
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
import apiClient from '../api/client';
import { useAuthStore } from '../stores/authStore';
import { 
  Lock, 
  Phone, 
  Sparkles, 
  Eye, 
  EyeOff, 
  ArrowLeft, 
  ShieldCheck, 
  Building2, 
  UserCheck, 
  AlertCircle 
} from 'lucide-vue-next';

const router = useRouter();
const authStore = useAuthStore();
const { locale } = useI18n();

const identifier = ref('');
const password = ref('');
const showPassword = ref(false);
const isLoading = ref(false);
const errorMessage = ref('');

const changeLanguage = (lang: string) => {
  locale.value = lang;
  localStorage.setItem('preferred_language', lang);
};

// 1-Click Quick Demo Accounts
const fillDemo = (type: 'director' | 'shed') => {
  if (type === 'director') {
    identifier.value = '9876543210';
    password.value = 'admin@123';
  } else {
    identifier.value = '9123456780';
    password.value = 'shed@123';
  }
};

const handleLogin = async () => {
  try {
    isLoading.value = true;
    errorMessage.value = '';

    const res = await apiClient.post('/Auth/login', {
      identifier: identifier.value,
      password: password.value,
    });

    if (res.data.success) {
      authStore.setAuth(res.data.data.token, res.data.data.user);
      
      // Auto-set preferred language
      if (res.data.data.user.preferredLanguage) {
        changeLanguage(res.data.data.user.preferredLanguage);
      }

      // Navigate to operations dashboard
      router.push('/erp/dashboard');
    }
  } catch (err: any) {
    if (err.response?.data?.message) {
      errorMessage.value = err.response.data.message;
    } else {
      errorMessage.value = 'Login failed. Please check your network and credentials.';
    }
  } finally {
    isLoading.value = false;
  }
};
</script>

<template>
  <div class="min-h-screen bg-[#faf8f5] flex flex-col justify-between selection:bg-amber-200">
    <!-- Top Bar -->
    <header class="max-w-7xl w-full mx-auto p-4 sm:p-6 flex items-center justify-between">
      <router-link 
        to="/" 
        class="inline-flex items-center gap-2 text-xs sm:text-sm font-bold text-slate-700 hover:text-amber-800 transition bg-white px-3.5 py-2 rounded-xl border border-amber-200/80 shadow-sm"
      >
        <ArrowLeft class="w-4 h-4" />
        <span>Back to Storefront</span>
      </router-link>

      <!-- Language Dropdown -->
      <div class="flex items-center bg-white border border-amber-200 rounded-xl px-2.5 py-1 text-xs shadow-sm">
        <select 
          :value="locale" 
          @change="(e: any) => changeLanguage(e.target.value)"
          class="bg-transparent font-bold text-amber-950 focus:outline-none cursor-pointer"
        >
          <option value="en">English (EN)</option>
          <option value="te">తెలుగు (TE)</option>
          <option value="hi">हिंदी (HI)</option>
        </select>
      </div>
    </header>

    <!-- Main Container -->
    <main class="flex-1 flex items-center justify-center p-4 sm:p-6">
      <div class="max-w-4xl w-full bg-white rounded-3xl sm:rounded-[32px] border border-amber-100 shadow-xl overflow-hidden grid grid-cols-1 md:grid-cols-2">
        
        <!-- Left Side: Visual Branding (Hidden on mobile) -->
        <div class="hidden md:flex flex-col justify-between p-10 bg-gradient-to-br from-amber-950 via-amber-900 to-amber-950 text-white relative">
          <div>
            <div class="w-12 h-12 rounded-2xl bg-amber-500/20 border border-amber-400/30 flex items-center justify-center text-amber-300 mb-6 shadow-inner">
              <Sparkles class="w-6 h-6" />
            </div>
            <h2 class="text-3xl font-black font-serif leading-snug text-amber-100 mb-3">
              CashewPro Operations ERP
            </h2>
            <p class="text-xs text-amber-200/80 leading-relaxed">
              Multi-shed cutting ledger, raw nut inward logistics, gram-accurate labour payments, and export compliance.
            </p>
          </div>

          <!-- Feature Bullets -->
          <div class="space-y-4 my-8">
            <div class="flex items-center gap-3">
              <div class="p-2 rounded-xl bg-amber-800/60 border border-amber-700/50">
                <Building2 class="w-4 h-4 text-amber-300" />
              </div>
              <p class="text-xs font-semibold text-amber-100">8 Distributed Village Sheds Sync</p>
            </div>
            <div class="flex items-center gap-3">
              <UserCheck class="w-4 h-4 text-amber-300" />
              <p class="text-xs font-semibold text-amber-100">Piece-Rate WhatsApp Wage Slips</p>
            </div>
            <div class="flex items-center gap-3">
              <ShieldCheck class="w-4 h-4 text-amber-300" />
              <p class="text-xs font-semibold text-amber-100">Director P&L and GST Analytics</p>
            </div>
          </div>

          <div class="pt-6 border-t border-amber-800/80 text-[11px] text-amber-300/70">
            Authorised personnel only. All access is logged with IP & audit trace.
          </div>
        </div>

        <!-- Right Side: Login Form -->
        <div class="p-6 sm:p-10 flex flex-col justify-center">
          <div class="mb-6">
            <div class="md:hidden w-10 h-10 rounded-xl bg-amber-600 flex items-center justify-center text-white mb-3">
              <Sparkles class="w-5 h-5" />
            </div>
            <h3 class="text-xl sm:text-2xl font-black text-slate-900 font-serif mb-1">
              Staff Portal Login
            </h3>
            <p class="text-xs text-slate-500">
              Sign in with your registered mobile number or email.
            </p>
          </div>

          <!-- Error Alert -->
          <div v-if="errorMessage" class="mb-4 p-3 rounded-xl bg-rose-50 border border-rose-200 text-rose-700 text-xs flex items-center gap-2">
            <AlertCircle class="w-4 h-4 shrink-0 text-rose-600" />
            <span>{{ errorMessage }}</span>
          </div>

          <!-- Login Form -->
          <form @submit.prevent="handleLogin" class="space-y-4">
            <div>
              <label class="block text-xs font-bold text-slate-700 mb-1">Mobile / Email</label>
              <div class="relative">
                <Phone class="w-4 h-4 text-slate-400 absolute left-3.5 top-3" />
                <input 
                  v-model="identifier" 
                  required 
                  type="text" 
                  placeholder="e.g. 9876543210 or email"
                  class="w-full text-xs pl-10 pr-4 py-2.5 rounded-xl border border-slate-200 focus:outline-none focus:border-amber-500 focus:ring-1 focus:ring-amber-500 transition"
                />
              </div>
            </div>

            <div>
              <label class="block text-xs font-bold text-slate-700 mb-1">Password</label>
              <div class="relative">
                <Lock class="w-4 h-4 text-slate-400 absolute left-3.5 top-3" />
                <input 
                  v-model="password" 
                  required 
                  :type="showPassword ? 'text' : 'password'" 
                  placeholder="Enter password"
                  class="w-full text-xs pl-10 pr-10 py-2.5 rounded-xl border border-slate-200 focus:outline-none focus:border-amber-500 focus:ring-1 focus:ring-amber-500 transition"
                />
                <button 
                  type="button" 
                  @click="showPassword = !showPassword"
                  class="absolute right-3.5 top-3 text-slate-400 hover:text-slate-600"
                >
                  <EyeOff v-if="showPassword" class="w-4 h-4" />
                  <Eye v-else class="w-4 h-4" />
                </button>
              </div>
            </div>

            <!-- Submit Button -->
            <button 
              type="submit" 
              :disabled="isLoading"
              class="w-full py-3 bg-slate-900 hover:bg-amber-600 disabled:opacity-50 text-white font-bold text-xs rounded-xl shadow-md transition flex items-center justify-center gap-2"
            >
              <span v-if="isLoading" class="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin"></span>
              <span v-else>Authorize & Sign In</span>
            </button>
          </form>

          <!-- Quick Demo Logins -->
          <div class="mt-8 pt-6 border-t border-slate-100">
            <p class="text-[11px] font-semibold text-slate-400 uppercase tracking-wider mb-2.5">
              Quick Demo Fill:
            </p>
            <div class="flex items-center gap-2">
              <button 
                @click="fillDemo('director')"
                type="button"
                class="px-2.5 py-1.5 bg-amber-50 hover:bg-amber-100 text-amber-900 rounded-lg text-[11px] font-bold border border-amber-200 transition"
              >
                Director
              </button>
              <button 
                @click="fillDemo('shed')"
                type="button"
                class="px-2.5 py-1.5 bg-amber-50 hover:bg-amber-100 text-amber-900 rounded-lg text-[11px] font-bold border border-amber-200 transition"
              >
                Shed-1 Supervisor
              </button>
            </div>
          </div>

        </div>

      </div>
    </main>

    <!-- Footer -->
    <footer class="p-4 text-center text-slate-400 text-[11px]">
      CashewPro Security Gateway • 256-bit Encrypted Session
    </footer>
  </div>
</template>