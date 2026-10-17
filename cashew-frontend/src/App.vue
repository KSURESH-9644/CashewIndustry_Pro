<script setup lang="ts">
import { RouterView } from 'vue-router';
import GlobalToast from './components/common/GlobalToast.vue';
</script>

<template>
  <div class="min-h-screen bg-[#faf8f5] text-slate-800 antialiased selection:bg-amber-200">
    <GlobalToast />
    <RouterView />
  </div>
</template>