import { defineStore } from 'pinia';
import { ref, computed } from 'vue';

export interface CartItem {
  id: number;
  gradeCode: string;
  gradeName: string;
  pricePerKg: number;
  quantityKg: number;
}

export const useCartStore = defineStore('cart', () => {
  const items = ref<CartItem[]>([]);
  const isDrawerOpen = ref(false);

  const totalItemsCount = computed(() => 
    items.value.reduce((sum, item) => sum + item.quantityKg, 0)
  );

  const subTotal = computed(() => 
    items.value.reduce((sum, item) => sum + item.pricePerKg * item.quantityKg, 0)
  );

  const addToCart = (product: { id: number; gradeCode: string; gradeName: string; currentRetailPricePerKg: number }, qtyKg = 1) => {
    const existing = items.value.find(i => i.id === product.id);
    if (existing) {
      existing.quantityKg += qtyKg;
    } else {
      items.value.push({
        id: product.id,
        gradeCode: product.gradeCode,
        gradeName: product.gradeName,
        pricePerKg: product.currentRetailPricePerKg,
        quantityKg: qtyKg
      });
    }
    isDrawerOpen.value = true;
  };

  const updateQuantity = (id: number, delta: number) => {
    const item = items.value.find(i => i.id === id);
    if (!item) return;
    item.quantityKg += delta;
    if (item.quantityKg <= 0) {
      items.value = items.value.filter(i => i.id !== id);
    }
  };

  const removeItem = (id: number) => {
    items.value = items.value.filter(i => i.id !== id);
  };

  const clearCart = () => {
    items.value = [];
  };

  return {
    items,
    isDrawerOpen,
    totalItemsCount,
    subTotal,
    addToCart,
    updateQuantity,
    removeItem,
    clearCart
  };
});