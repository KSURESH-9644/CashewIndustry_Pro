<script setup lang="ts">
import { useCartStore } from '../../stores/cartStore';
import { useI18n } from 'vue-i18n';
import { X, Trash2, Plus, Minus, ShoppingBag, ArrowRight } from 'lucide-vue-next';

const cart = useCartStore();
const { t } = useI18n();
</script>

<template>
  <div v-if="cart.isDrawerOpen" class="fixed inset-0 z-50 flex justify-end">
    <!-- Backdrop -->
    <div 
      @click="cart.isDrawerOpen = false" 
      class="fixed inset-0 bg-slate-900/40 backdrop-blur-sm transition-opacity"
    ></div>

    <!-- Drawer Panel -->
    <div class="relative w-full max-w-md bg-white h-full shadow-2xl flex flex-col z-10 border-l border-amber-100">
      <!-- Drawer Header -->
      <div class="p-5 border-b border-amber-100 flex items-center justify-between bg-amber-50/50">
        <div class="flex items-center gap-2">
          <ShoppingBag class="w-5 h-5 text-amber-700" />
          <h3 class="font-bold text-slate-900 font-serif">Mee Cart (Order Summary)</h3>
          <span class="px-2 py-0.5 rounded-full bg-amber-200 text-amber-900 text-xs font-bold">
            {{ cart.items.length }}
          </span>
        </div>
        <button 
          @click="cart.isDrawerOpen = false"
          class="p-1.5 rounded-xl text-slate-400 hover:text-slate-700 hover:bg-white transition"
        >
          <X class="w-5 h-5" />
        </button>
      </div>

      <!-- Cart Items List -->
      <div class="flex-1 overflow-y-auto p-5 space-y-4">
        <div v-if="cart.items.length === 0" class="text-center py-16 text-slate-400">
          <ShoppingBag class="w-12 h-12 mx-auto mb-3 opacity-30" />
          <p class="text-sm">Mee cart lo items emi levu.</p>
        </div>

        <div 
          v-for="item in cart.items" 
          :key="item.id"
          class="flex items-center justify-between p-3.5 rounded-2xl border border-amber-100 bg-white shadow-sm"
        >
          <div>
            <div class="flex items-center gap-2">
              <span class="font-bold text-xs px-2 py-0.5 rounded bg-amber-100 text-amber-900">
                {{ item.gradeCode }}
              </span>
              <p class="font-bold text-xs text-slate-900">{{ item.gradeName }}</p>
            </div>
            <p class="text-xs text-slate-500 mt-1">₹{{ item.pricePerKg }}/kg</p>
          </div>

          <div class="flex items-center gap-3">
            <!-- Quantity Counter -->
            <div class="flex items-center gap-1.5 bg-slate-50 border border-slate-200 rounded-xl px-2 py-1">
              <button @click="cart.updateQuantity(item.id, -1)" class="p-1 text-slate-500 hover:text-slate-800">
                <Minus class="w-3 h-3" />
              </button>
              <span class="text-xs font-bold text-slate-800 w-6 text-center">{{ item.quantityKg }} kg</span>
              <button @click="cart.updateQuantity(item.id, 1)" class="p-1 text-slate-500 hover:text-slate-800">
                <Plus class="w-3 h-3" />
              </button>
            </div>

            <!-- Total Price -->
            <p class="text-xs font-black text-amber-950 w-16 text-right">
              ₹{{ item.pricePerKg * item.quantityKg }}
            </p>

            <!-- Remove Button -->
            <button @click="cart.removeItem(item.id)" class="text-slate-400 hover:text-rose-600 transition">
              <Trash2 class="w-4 h-4" />
            </button>
          </div>
        </div>
      </div>

      <!-- Drawer Footer -->
      <div v-if="cart.items.length > 0" class="p-5 border-t border-amber-100 bg-amber-50/30 space-y-3">
        <div class="flex justify-between items-center text-sm">
          <span class="text-slate-600">Subtotal:</span>
          <span class="text-xl font-black text-amber-950">₹{{ cart.subTotal }}</span>
        </div>
        <p class="text-[11px] text-slate-500">Taxes & delivery will be calculated at checkout</p>
        
        <button 
          @click="alert('Direct Payment Gateway checkout integration')"
          class="w-full py-3.5 bg-amber-600 hover:bg-amber-700 text-white font-bold text-xs rounded-xl shadow-lg shadow-amber-600/25 flex items-center justify-center gap-2 transition"
        >
          <span>Proceed to Checkout</span>
          <ArrowRight class="w-4 h-4" />
        </button>
      </div>
    </div>
  </div>
</template>