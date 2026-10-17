import { defineStore } from 'pinia';
import { ref } from 'vue';

export interface ToastMessage {
  id: number;
  severity: 'success' | 'warn' | 'error' | 'info';
  summary: string;
  detail: string;
}

export const useToastStore = defineStore('toast', () => {
  const toasts = ref<ToastMessage[]>([]);

  const add = (options: { severity: 'success' | 'warn' | 'error' | 'info'; summary: string; detail: string; life?: number }) => {
    const id = Date.now();
    toasts.value.push({ id, ...options });

    // Auto dismiss after given time (default 4 seconds)
    setTimeout(() => {
      remove(id);
    }, options.life || 4000);
  };

  const remove = (id: number) => {
    toasts.value = toasts.value.filter((t) => t.id !== id);
  };

  return { toasts, add, remove };
});