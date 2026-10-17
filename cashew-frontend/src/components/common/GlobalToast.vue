<script setup lang="ts">
import { useToastStore } from '../../stores/toastStore';
import { CheckCircle2, AlertTriangle, XCircle, Info, X } from 'lucide-vue-next';

const toastStore = useToastStore();
</script>

<template>
  <div class="fixed top-5 right-5 z-[9999] flex flex-col gap-3 max-w-sm w-full pointer-events-none">
    <TransitionGroup
      enter-active-class="transform ease-out duration-300 transition"
      enter-from-class="translate-y-2 opacity-0 sm:translate-y-0 sm:translate-x-4"
      enter-to-class="translate-y-0 opacity-100 sm:translate-x-0"
      leave-active-class="transition ease-in duration-200"
      leave-from-class="opacity-100"
      leave-to-class="opacity-0 scale-95"
    >
      <div
        v-for="toast in toastStore.toasts"
        :key="toast.id"
        class="pointer-events-auto flex items-start gap-3 p-4 rounded-2xl shadow-xl border bg-white backdrop-blur-md transition-all"
        :class="{
          'border-emerald-200 bg-emerald-50/95 text-emerald-950': toast.severity === 'success',
          'border-amber-200 bg-amber-50/95 text-amber-950': toast.severity === 'warn',
          'border-rose-200 bg-rose-50/95 text-rose-950': toast.severity === 'error',
          'border-blue-200 bg-blue-50/95 text-blue-950': toast.severity === 'info'
        }"
      >
        <!-- Icon -->
        <div class="shrink-0 mt-0.5">
          <CheckCircle2 v-if="toast.severity === 'success'" class="w-5 h-5 text-emerald-600" />
          <AlertTriangle v-else-if="toast.severity === 'warn'" class="w-5 h-5 text-amber-600" />
          <XCircle v-else-if="toast.severity === 'error'" class="w-5 h-5 text-rose-600" />
          <Info v-else class="w-5 h-5 text-blue-600" />
        </div>

        <!-- Text -->
        <div class="flex-1 text-xs">
          <p class="font-bold text-sm tracking-tight mb-0.5">{{ toast.summary }}</p>
          <p class="leading-relaxed opacity-90 font-medium">{{ toast.detail }}</p>
        </div>

        <!-- Close Button -->
        <button
          @click="toastStore.remove(toast.id)"
          class="shrink-0 p-1 rounded-lg text-slate-400 hover:text-slate-700 hover:bg-black/5 transition"
        >
          <X class="w-4 h-4" />
        </button>
      </div>
    </TransitionGroup>
  </div>
</template>