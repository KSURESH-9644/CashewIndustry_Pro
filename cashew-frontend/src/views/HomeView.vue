<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import apiClient from '../api/client';
import { useCartStore } from '../stores/cartStore';

import CartDrawer from '../components/storefront/CartDrawer.vue';
import BulkOrderModal from '../components/storefront/BulkOrderModal.vue';
import VoiceChatBot from '../components/ai/VoiceChatBot.vue';

import { 
  ShoppingBag, 
  Sparkles, 
  Globe, 
  ShieldCheck, 
  Truck, 
  Award, 
  Star,
  Quote,
  Menu,
  X,
  Lock,
  TrendingUp, 
  TrendingDown, 
  Minus 
} from 'lucide-vue-next';

const { t, locale } = useI18n();
const cart = useCartStore();

interface Grade {
  id: number;
  gradeCode: string;
  gradeName: string;
  gradeCategory: string;
  standardDescription: string | null;
  currentRetailPricePerKg: number;
  currentWholesalePricePerKg: number;
  minimumWholesaleQtyKg: number;
  imageUrl: string | null;
}

interface TickerItem {
  id: number;
  gradeCode: string;
  gradeName: string;
  mandiMarketName: string;
  pricePerKg: number;
  priceTrend: string | null;
}

const grades = ref<Grade[]>([]);
const tickerItems = ref<TickerItem[]>([]);
const isLoading = ref(true);
const isBulkModalOpen = ref(false);
const isMobileMenuOpen = ref(false);

const changeLanguage = (lang: string) => {
  locale.value = lang;
  localStorage.setItem('preferred_language', lang);
};

const fetchStorefrontData = async () => {
  try {
    isLoading.value = true;
    const [gradesRes, tickerRes] = await Promise.all([
      apiClient.get('/store/Products/grades'),
      apiClient.get('/store/Products/live-ticker'),
    ]);
    if (gradesRes.data.success) {
      grades.value = gradesRes.data.data;
    }
    if (tickerRes.data.success) {
      tickerItems.value = tickerRes.data.data;
    }
  } catch (error) {
    console.error('Failed to load storefront data:', error);
  } finally {
    isLoading.value = false;
  }
};

onMounted(() => {
  fetchStorefrontData();
});
</script>

<template>
  <div class="relative min-h-screen flex flex-col justify-between overflow-x-hidden">
    <!-- 1. Top Live Rates Ticker Ribbon (No Scrollbar, Smooth Touch Swipe) -->
    <div class="bg-amber-950 text-amber-200 text-xs py-1.5 px-3 border-b border-amber-900/60 shadow-inner">
      <div class="max-w-7xl mx-auto flex items-center justify-between gap-2">
        <div class="flex items-center gap-1.5 font-bold tracking-wider text-amber-400 shrink-0 uppercase text-[10px]">
          <span class="w-2 h-2 rounded-full bg-emerald-400 animate-pulse"></span>
          <span>LIVE:</span>
        </div>
        
        <div class="flex items-center gap-2 sm:gap-4 overflow-x-auto whitespace-nowrap [scrollbar-width:none] [-ms-overflow-style:none] [&::-webkit-scrollbar]:hidden py-0.5 w-full justify-start sm:justify-end">
          <div 
            v-for="item in tickerItems" 
            :key="item.id" 
            class="flex items-center gap-1 bg-amber-900/50 px-2 py-0.5 rounded text-amber-100 text-[10px] sm:text-xs shrink-0"
          >
            <span class="font-bold text-white">{{ item.gradeCode }}</span>
            <span>₹{{ item.pricePerKg }}</span>
            <TrendingUp v-if="item.priceTrend === 'UP'" class="w-3 h-3 text-emerald-400" />
            <TrendingDown v-else-if="item.priceTrend === 'DOWN'" class="w-3 h-3 text-rose-400" />
            <Minus v-else class="w-3 h-3 text-amber-400" />
          </div>
        </div>
      </div>
    </div>

    <!-- 2. Ultra-Responsive Header Navigation -->
    <header class="sticky top-0 z-40 bg-white/95 backdrop-blur-md border-b border-amber-100 shadow-sm">
      <div class="max-w-7xl mx-auto px-3 sm:px-6 lg:px-8 h-16 sm:h-20 flex items-center justify-between gap-2">
        <!-- Logo -->
        <div class="flex items-center gap-2 shrink min-w-0">
          <div class="w-9 h-9 sm:w-11 sm:h-11 rounded-xl bg-gradient-to-tr from-amber-600 to-amber-400 flex items-center justify-center text-white shadow-md shadow-amber-500/20 shrink-0">
            <Sparkles class="w-5 h-5 sm:w-6 sm:h-6" />
          </div>
          <div class="truncate">
            <h1 class="text-sm sm:text-xl font-black tracking-tight text-amber-950 font-serif leading-tight truncate">
              {{ t('brand_name') }}
            </h1>
            <p class="text-[9px] sm:text-[10px] tracking-wider text-amber-600 uppercase font-semibold hidden md:block">
              Heritage Cashew Exports & Sheds
            </p>
          </div>
        </div>

        <!-- Center Nav Links (Desktop & Tablets Only) -->
        <nav class="hidden md:flex items-center gap-6 lg:gap-8 font-medium text-xs sm:text-sm text-slate-600 shrink-0">
          <a href="#grades" class="hover:text-amber-700 transition">{{ t('nav_grades') }}</a>
          <button @click="isBulkModalOpen = true" class="hover:text-amber-700 transition">
            {{ t('nav_wholesale') }}
          </button>
          <a href="#reviews" class="hover:text-amber-700 transition">{{ t('nav_reviews') }}</a>
        </nav>

        <!-- Right Side Controls (Mobile Optimized) -->
        <div class="flex items-center gap-1.5 sm:gap-2.5 shrink-0">
          
          <!-- Compact Language Selector: Mobile lo Dropdown, Desktop lo Segmented Buttons -->
          <!-- A. Small Screen Dropdown (< sm) -->
          <div class="sm:hidden relative flex items-center bg-amber-50 border border-amber-200 rounded-lg px-1.5 py-1 text-xs">
            <Globe class="w-3.5 h-3.5 text-amber-800 mr-1 shrink-0" />
            <select 
              :value="locale" 
              @change="(e: any) => changeLanguage(e.target.value)"
              class="bg-transparent text-[11px] font-bold text-amber-950 focus:outline-none cursor-pointer"
            >
              <option value="en">EN</option>
              <option value="te">తెలుగు</option>
              <option value="hi">हिंदी</option>
            </select>
          </div>

          <!-- B. Tablet & Desktop Segmented Buttons (>= sm) -->
          <div class="hidden sm:flex items-center bg-amber-50 rounded-xl p-1 border border-amber-200/80 text-xs font-semibold">
            <button 
              @click="changeLanguage('en')" 
              :class="locale === 'en' ? 'bg-amber-600 text-white shadow-sm' : 'text-slate-600 hover:text-amber-900'"
              class="px-2.5 py-1 rounded-lg transition"
            >
              EN
            </button>
            <button 
              @click="changeLanguage('te')" 
              :class="locale === 'te' ? 'bg-amber-600 text-white shadow-sm' : 'text-slate-600 hover:text-amber-900'"
              class="px-2.5 py-1 rounded-lg transition"
            >
              తెలుగు
            </button>
            <button 
              @click="changeLanguage('hi')" 
              :class="locale === 'hi' ? 'bg-amber-600 text-white shadow-sm' : 'text-slate-600 hover:text-amber-900'"
              class="px-2.5 py-1 rounded-lg transition"
            >
              हिंदी
            </button>
          </div>

          <!-- Cart Drawer Button -->
          <button 
            @click="cart.isDrawerOpen = true"
            class="relative p-2 sm:p-2.5 bg-amber-50 rounded-lg sm:rounded-xl text-amber-900 hover:bg-amber-100 transition border border-amber-200 shrink-0"
            title="View Cart"
          >
            <ShoppingBag class="w-4 h-4 sm:w-5 sm:h-5" />
            <span 
              v-if="cart.items.length > 0"
              class="absolute -top-1.5 -right-1.5 w-4 h-4 sm:w-5 sm:h-5 bg-amber-600 text-white rounded-full text-[9px] sm:text-[10px] font-black flex items-center justify-center shadow"
            >
              {{ cart.items.length }}
            </span>
          </button>

          <!-- Staff / Shed Portal (Mobile Lock Icon Button) -->
          <router-link 
            to="/login"
            class="sm:hidden p-2 bg-amber-100 text-amber-950 rounded-lg border border-amber-300 font-bold hover:bg-amber-200 transition shrink-0"
            title="Staff / Shed Portal Login"
          >
            <Lock class="w-4 h-4 text-amber-900" />
          </router-link>

          <!-- Staff / Shed Portal (Desktop Full Button) -->
          <router-link 
            to="/login"
            class="hidden sm:inline-flex items-center gap-1.5 px-3.5 py-2 text-xs font-bold text-amber-900 bg-white border border-amber-300 rounded-xl hover:bg-amber-50 hover:border-amber-400 transition shadow-sm shrink-0"
          >
            <Lock class="w-3.5 h-3.5 text-amber-800" />
            {{ t('btn_staff_login') }}
          </router-link>
        </div>
      </div>

      <!-- Mobile Dropdown Navigation Menu -->
      <div v-if="isMobileMenuOpen" class="md:hidden bg-white border-b border-amber-100 px-4 py-4 space-y-3 shadow-lg">
        <a 
          href="#grades" 
          @click="isMobileMenuOpen = false"
          class="block py-2 text-sm font-semibold text-slate-700 hover:text-amber-700"
        >
          {{ t('nav_grades') }}
        </a>
        <button 
          @click="isBulkModalOpen = true; isMobileMenuOpen = false"
          class="block w-full text-left py-2 text-sm font-semibold text-slate-700 hover:text-amber-700"
        >
          {{ t('nav_wholesale') }}
        </button>
        <a 
          href="#reviews" 
          @click="isMobileMenuOpen = false"
          class="block py-2 text-sm font-semibold text-slate-700 hover:text-amber-700"
        >
          {{ t('nav_reviews') }}
        </a>
        <router-link 
          to="/login" 
          @click="isMobileMenuOpen = false"
          class="flex items-center justify-center gap-2 w-full py-2.5 text-center text-xs font-bold text-amber-900 bg-amber-50 border border-amber-200 rounded-xl mt-2"
        >
          <Lock class="w-3.5 h-3.5" />
          {{ t('btn_staff_login') }}
        </router-link>
      </div>
    </header>

    <!-- 3. Hero Section (Adaptive Grid for Mobile to 4K) -->
    <section class="relative overflow-hidden pt-8 sm:pt-14 pb-14 sm:pb-20 bg-gradient-to-b from-amber-50/70 via-amber-50/20 to-transparent">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="max-w-3xl">
          <div class="inline-flex items-center gap-2 px-3 py-1 sm:py-1.5 rounded-full bg-amber-100 text-amber-900 text-[11px] sm:text-xs font-bold tracking-wide mb-4 sm:mb-6">
            <Award class="w-3.5 h-3.5 sm:w-4 sm:h-4 text-amber-600" />
            {{ t('hero_tagline') }}
          </div>
          
          <h2 class="text-3xl sm:text-5xl lg:text-6xl font-black text-slate-900 tracking-tight leading-[1.15] mb-4 sm:mb-6 font-serif">
            {{ t('hero_title') }}
          </h2>
          
          <p class="text-sm sm:text-base lg:text-lg text-slate-600 leading-relaxed mb-6 sm:mb-8">
            {{ t('hero_subtitle') }}
          </p>

          <div class="flex flex-col sm:flex-row items-stretch sm:items-center gap-3 sm:gap-4">
            <a 
              href="#grades" 
              class="px-5 sm:px-6 py-3 sm:py-3.5 bg-gradient-to-r from-amber-600 to-amber-500 hover:from-amber-700 hover:to-amber-600 text-white font-bold text-sm rounded-xl sm:rounded-2xl shadow-lg shadow-amber-600/25 flex items-center justify-center gap-2 transition"
            >
              <ShoppingBag class="w-4 h-4 sm:w-5 sm:h-5" />
              {{ t('btn_shop_retail') }}
            </a>
            <button 
              @click="isBulkModalOpen = true" 
              class="px-5 sm:px-6 py-3 sm:py-3.5 bg-white hover:bg-amber-50 text-slate-800 font-bold text-sm rounded-xl sm:rounded-2xl border border-slate-200 transition shadow-sm text-center"
            >
              {{ t('btn_request_b2b') }}
            </button>
          </div>

          <!-- Trust Badges (Adaptive 1 col on XS, 3 cols on SM/MD) -->
          <div class="grid grid-cols-1 sm:grid-cols-3 gap-4 sm:gap-6 pt-8 sm:pt-12 mt-8 sm:mt-12 border-t border-amber-200/60">
            <div class="flex items-center gap-3 bg-white/60 sm:bg-transparent p-3 sm:p-0 rounded-xl border sm:border-0 border-amber-100">
              <ShieldCheck class="w-6 h-6 sm:w-7 sm:h-7 text-amber-600 shrink-0" />
              <div class="text-xs">
                <p class="font-bold text-slate-900">100% Organic & Pure</p>
                <p class="text-slate-500 text-[11px]">Zero artificial gloss</p>
              </div>
            </div>
            <div class="flex items-center gap-3 bg-white/60 sm:bg-transparent p-3 sm:p-0 rounded-xl border sm:border-0 border-amber-100">
              <Truck class="w-6 h-6 sm:w-7 sm:h-7 text-amber-600 shrink-0" />
              <div class="text-xs">
                <p class="font-bold text-slate-900">Direct From Mandi</p>
                <p class="text-slate-500 text-[11px]">Farm-gate pricing</p>
              </div>
            </div>
            <div class="flex items-center gap-3 bg-white/60 sm:bg-transparent p-3 sm:p-0 rounded-xl border sm:border-0 border-amber-100">
              <Globe class="w-6 h-6 sm:w-7 sm:h-7 text-amber-600 shrink-0" />
              <div class="text-xs">
                <p class="font-bold text-slate-900">8 Village Sheds</p>
                <p class="text-slate-500 text-[11px]">Women empowered</p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- 4. Product Grades Showcase (1-Col Mobile, 2-Col Tablet, 3-Col Laptop, 4-Col 2XL) -->
    <section id="grades" class="py-12 sm:py-20 max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 w-full">
      <div class="text-center max-w-2xl mx-auto mb-10 sm:mb-16">
        <h3 class="text-2xl sm:text-3xl font-black text-slate-900 tracking-tight font-serif mb-2 sm:mb-3">
          {{ t('section_grades_title') }}
        </h3>
        <p class="text-slate-600 text-xs sm:text-sm px-2">
          {{ t('section_grades_desc') }}
        </p>
      </div>

      <!-- Loading skeleton -->
      <div v-if="isLoading" class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 2xl:grid-cols-4 gap-4 sm:gap-8">
        <div v-for="i in 6" :key="i" class="h-80 bg-white rounded-3xl p-6 border border-slate-100 animate-pulse"></div>
      </div>

      <!-- Responsive Grades Grid -->
      <div v-else class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 2xl:grid-cols-4 gap-4 sm:gap-6 lg:gap-8">
        <div 
          v-for="grade in grades" 
          :key="grade.id"
          class="group bg-white rounded-2xl sm:rounded-3xl p-5 sm:p-6 border border-amber-100 hover:border-amber-300 shadow-sm hover:shadow-xl hover:-translate-y-1 transition duration-300 flex flex-col justify-between"
        >
          <div>
            <!-- Grade Badge -->
            <div class="flex items-center justify-between mb-3 sm:mb-4">
              <span class="px-2.5 sm:px-3.5 py-0.5 sm:py-1 rounded-full text-xs font-black tracking-wider bg-amber-100 text-amber-900">
                {{ grade.gradeCode }}
              </span>
              <span class="text-[10px] sm:text-xs text-slate-400 font-semibold uppercase tracking-wider">
                {{ grade.gradeCategory }}
              </span>
            </div>

            <!-- Title -->
            <h4 class="text-lg sm:text-xl font-bold text-slate-900 mb-2 group-hover:text-amber-700 transition leading-snug">
              {{ grade.gradeName }}
            </h4>

            <p class="text-slate-500 text-xs line-clamp-2 mb-4 sm:mb-6">
              Hygienic steam-boiled kernel, roasted and carefully peeled by village workers.
            </p>

            <!-- Pricing Box -->
            <div class="bg-amber-50/50 rounded-xl sm:rounded-2xl p-3.5 sm:p-4 border border-amber-100/80 mb-4 sm:mb-6">
              <div class="flex items-baseline justify-between mb-1.5">
                <span class="text-xs font-medium text-slate-500">{{ t('retail_price') }}</span>
                <span class="text-xl sm:text-2xl font-black text-amber-950">₹{{ grade.currentRetailPricePerKg }}<span class="text-[10px] sm:text-xs font-normal text-slate-500">/kg</span></span>
              </div>
              <div class="flex items-baseline justify-between pt-2 border-t border-amber-200/50 text-[11px] sm:text-xs">
                <span class="text-slate-600 font-medium">{{ t('wholesale_price') }} (50kg+)</span>
                <span class="font-bold text-emerald-700">₹{{ grade.currentWholesalePricePerKg }}/kg</span>
              </div>
            </div>
          </div>

          <!-- Add to Cart button -->
          <button 
            @click="cart.addToCart(grade, 1)"
            class="w-full py-2.5 sm:py-3 px-4 bg-slate-900 hover:bg-amber-600 text-white font-bold text-xs rounded-xl shadow-md transition flex items-center justify-center gap-2"
          >
            <ShoppingBag class="w-4 h-4" />
            {{ t('btn_add_to_cart') }}
          </button>
        </div>
      </div>
    </section>

    <!-- 5. Reviews Section (Responsive 1 to 3 Columns) -->
    <section id="reviews" class="py-12 sm:py-20 bg-amber-50/40 border-y border-amber-100/80 w-full">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="flex flex-col md:flex-row md:items-end justify-between mb-8 sm:mb-12 gap-4">
          <div>
            <div class="flex items-center gap-1 text-amber-500 mb-1.5">
              <Star v-for="i in 5" :key="i" class="w-3.5 h-3.5 fill-amber-400 text-amber-400" />
              <span class="text-xs font-black text-slate-800 ml-1.5">4.9 / 5.0 Rating</span>
            </div>
            <h3 class="text-2xl sm:text-3xl font-black text-slate-900 font-serif">Verified Customer & B2B Reviews</h3>
            <p class="text-xs text-slate-500 mt-1">Real feedback from sweet houses, wholesalers, and retail buyers.</p>
          </div>
          <button 
            @click="alert('Direct Google Reviews submission modal')"
            class="self-start md:self-auto px-4 py-2 bg-white border border-amber-200 text-amber-900 font-bold text-xs rounded-xl hover:bg-amber-100 transition shadow-sm"
          >
            + Write a Google Review
          </button>
        </div>

        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 sm:gap-6">
          <div class="bg-white p-5 sm:p-6 rounded-2xl sm:rounded-3xl border border-amber-100 shadow-sm relative">
            <Quote class="w-7 h-7 sm:w-8 sm:h-8 text-amber-200 absolute top-4 right-4" />
            <div class="flex items-center gap-1 text-amber-400 mb-2.5">
              <Star v-for="i in 5" :key="i" class="w-3.5 h-3.5 fill-amber-400" />
            </div>
            <p class="text-xs text-slate-600 leading-relaxed mb-4">
              "We ordered 200 kgs of W-240 and Splits for our sweet shops in Vizag. Superb crispiness and timely dispatch with E-way bill. Best Mandi rates!"
            </p>
            <div>
              <p class="text-xs font-bold text-slate-900">Sri Balaji Sweets & Bakery</p>
              <p class="text-[10px] text-slate-400">Wholesale Buyer • Visakhapatnam</p>
            </div>
          </div>

          <div class="bg-white p-5 sm:p-6 rounded-2xl sm:rounded-3xl border border-amber-100 shadow-sm relative">
            <Quote class="w-7 h-7 sm:w-8 sm:h-8 text-amber-200 absolute top-4 right-4" />
            <div class="flex items-center gap-1 text-amber-400 mb-2.5">
              <Star v-for="i in 5" :key="i" class="w-3.5 h-3.5 fill-amber-400" />
            </div>
            <p class="text-xs text-slate-600 leading-relaxed mb-4">
              "King Jumbo W-180 quality is truly king-sized! Perfect for gifting. The aroma and zero moisture feel makes it stand out from supermarket brands."
            </p>
            <div>
              <p class="text-xs font-bold text-slate-900">Ramesh Varma</p>
              <p class="text-[10px] text-slate-400">Retail Customer • Hyderabad</p>
            </div>
          </div>

          <div class="bg-white p-5 sm:p-6 rounded-2xl sm:rounded-3xl border border-amber-100 shadow-sm relative">
            <Quote class="w-7 h-7 sm:w-8 sm:h-8 text-amber-200 absolute top-4 right-4" />
            <div class="flex items-center gap-1 text-amber-400 mb-2.5">
              <Star v-for="i in 5" :key="i" class="w-3.5 h-3.5 fill-amber-400" />
            </div>
            <p class="text-xs text-slate-600 leading-relaxed mb-4">
              "Transparent pricing and great customer support. WhatsApp daily updates on our bulk shipment helped us track trucks smoothly."
            </p>
            <div>
              <p class="text-xs font-bold text-slate-900">Kisan Agro Traders</p>
              <p class="text-[10px] text-slate-400">Distributor • Raipur</p>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- 6. Footer -->
    <footer class="bg-amber-950 text-amber-200 py-8 sm:py-12 border-t border-amber-900 text-center text-xs px-4">
      <p class="font-semibold text-white mb-1.5">CashewPro Industries — Palasa, Andhra Pradesh</p>
      <p class="text-amber-400 text-[11px]">© 2026 CashewPro ERP & Storefront Platform. All Rights Reserved.</p>
    </footer>

    <!-- 7. Responsive Drawers & Modals -->
    <CartDrawer />
    <BulkOrderModal :isOpen="isBulkModalOpen" @close="isBulkModalOpen = false" />
    <VoiceChatBot />
  </div>
</template>