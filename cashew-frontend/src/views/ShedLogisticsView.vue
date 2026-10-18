<script setup lang="ts">
import { ref, onMounted, nextTick } from 'vue';
import { useRouter } from 'vue-router';
import { useToastStore } from '../stores/toastStore';
import apiClient from '../api/client';
import L from 'leaflet';
import 'leaflet/dist/leaflet.css';
import { 
  MapPin, 
  ArrowLeft, 
  Truck, 
  Building2, 
  Users, 
  Scale, 
  CheckCircle2, 
  Clock, 
  Navigation,
  Phone
} from 'lucide-vue-next';

const router = useRouter();
const toast = useToastStore();

const mapContainer = ref<HTMLDivElement | null>(null);
let map: L.Map | null = null;

const locations = ref<any[]>([]);
const activeTransits = ref<any[]>([]);
const selectedLocation = ref<any>(null);
const isLoading = ref(true);

const loadLogisticsData = async () => {
  try {
    isLoading.value = true;
    const [locRes, transRes] = await Promise.all([
      apiClient.get('/erp/ShedLogistics/map-locations'),
      apiClient.get('/erp/ShedLogistics/active-transits')
    ]);

    if (locRes.data.success) {
      locations.value = locRes.data.data;
      if (locations.value.length > 0) selectedLocation.value = locations.value[0];
    }
    if (transRes.data.success) {
      activeTransits.value = transRes.data.data;
    }

    await nextTick();
    initLeafletMap();
  } catch (err: any) {
    toast.add({
      severity: 'error',
      summary: 'Data Error',
      detail: 'Map & logistics data load cheyadam lo error vachindi.'
    });
  } finally {
    isLoading.value = false;
  }
};

const initLeafletMap = () => {
  if (!mapContainer.value || map) return;

  // Center on Palasa, Srikakulam
  map = L.map(mapContainer.value).setView([18.775, 84.425], 11);

  // OpenStreetMap Tile Layer
  L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
    attribution: '© OpenStreetMap contributors',
    maxZoom: 18,
  }).addTo(map);

  // Add Markers for Factory & Sheds
  locations.value.forEach((loc) => {
    const isHq = loc.type === 'HQ_FACTORY';
    
    // Custom SVG / HTML Pin
    const customIcon = L.divIcon({
      className: 'custom-map-pin',
      html: `
        <div style="
          background: ${isHq ? '#78350f' : '#d97706'};
          color: white;
          width: 34px;
          height: 34px;
          border-radius: 50%;
          display: flex;
          align-items: center;
          justify-content: center;
          font-weight: bold;
          font-size: 11px;
          border: 3px solid white;
          box-shadow: 0 4px 10px rgba(0,0,0,0.3);
          cursor: pointer;
        ">
          ${isHq ? 'HQ' : 'SH'}
        </div>
      `,
      iconSize: [34, 34],
      iconAnchor: [17, 17]
    });

    const marker = L.marker([Number(loc.latitude), Number(loc.longitude)], { icon: customIcon }).addTo(map!);

    marker.bindPopup(`
      <div style="font-family: inherit; font-size: 12px; min-width: 180px;">
        <p style="font-weight: bold; color: #78350f; margin: 0 0 4px 0;">${loc.name}</p>
        <p style="margin: 0; color: #64748b;">${loc.village}, ${loc.mandal}</p>
        <p style="margin: 4px 0 0 0; font-weight: 600; color: #047857;">Cut Today: ${loc.todayCutKg} Kg</p>
      </div>
    `);

    marker.on('click', () => {
      selectedLocation.value = loc;
    });
  });
};

const focusOnLocation = (loc: any) => {
  selectedLocation.value = loc;
  if (map) {
    map.flyTo([Number(loc.latitude), Number(loc.longitude)], 13, { duration: 1.2 });
  }
};

onMounted(() => {
  loadLogisticsData();
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
          <h1 class="text-sm sm:text-base font-bold font-serif leading-tight">Multi-Shed Village Map & Logistics Fleet Desk</h1>
          <p class="text-[10px] sm:text-[11px] text-amber-300 font-mono">Palasa Coastal Belt • Live Shed Pinpoints • Bag Transit Tracking</p>
        </div>
      </div>

      <div class="flex items-center gap-2">
        <span class="px-2.5 py-1 rounded-full text-xs font-bold bg-amber-900 border border-amber-700 text-amber-200">
          {{ locations.length }} Active Pins
        </span>
      </div>
    </header>

    <!-- Main Workspace -->
    <main class="flex-1 max-w-7xl w-full mx-auto p-4 sm:p-6 grid grid-cols-1 lg:grid-cols-12 gap-6">
      
      <!-- Left Column: Interactive Map & Live Unit Details (8 Cols) -->
      <div class="lg:col-span-8 space-y-6 flex flex-col justify-between">
        
        <!-- Interactive Leaflet Map Container -->
        <div class="bg-white rounded-3xl p-3 border border-amber-100 shadow-sm overflow-hidden">
          <div class="flex items-center justify-between px-3 py-2 border-b border-slate-100 mb-2">
            <h2 class="text-sm font-bold text-slate-900 flex items-center gap-2">
              <Navigation class="w-4 h-4 text-amber-600" />
              <span>Interactive Village Sheds Geo-Tracker</span>
            </h2>
            <span class="text-[11px] text-slate-400 font-mono">OpenStreetMap Live Engine</span>
          </div>

          <div ref="mapContainer" class="w-full h-[420px] rounded-2xl z-0"></div>
        </div>

        <!-- Selected Shed / Factory Status Details Card -->
        <div v-if="selectedLocation" class="bg-white rounded-3xl p-5 sm:p-6 border border-amber-100 shadow-sm">
          <div class="flex flex-col sm:flex-row sm:items-center justify-between pb-3.5 border-b border-slate-100 gap-3">
            <div>
              <div class="flex items-center gap-2 mb-1">
                <span 
                  :class="selectedLocation.type === 'HQ_FACTORY' ? 'bg-amber-950 text-amber-200' : 'bg-amber-100 text-amber-900'"
                  class="px-2.5 py-0.5 rounded-full text-[10px] font-black tracking-wider uppercase font-mono"
                >
                  {{ selectedLocation.type === 'HQ_FACTORY' ? 'Main Factory HQ' : 'Village Shed' }}
                </span>
                <span class="text-xs text-slate-400">• {{ selectedLocation.village }}, {{ selectedLocation.mandal }}</span>
              </div>
              <h3 class="text-lg font-bold text-slate-900 font-serif">{{ selectedLocation.name }}</h3>
            </div>

            <div class="flex items-center gap-2">
              <a 
                :href="`tel:${selectedLocation.phone}`"
                class="inline-flex items-center gap-1.5 px-3 py-1.5 bg-amber-50 hover:bg-amber-100 text-amber-900 rounded-xl text-xs font-bold border border-amber-200 transition"
              >
                <Phone class="w-3.5 h-3.5 text-amber-700" />
                <span>Call Supervisor</span>
              </a>
            </div>
          </div>

          <!-- Metrics for this unit -->
          <div class="grid grid-cols-2 sm:grid-cols-4 gap-4 pt-4">
            <div class="bg-slate-50 p-3 rounded-2xl border border-slate-100">
              <p class="text-[11px] text-slate-500 font-medium">Supervisor</p>
              <p class="text-xs font-bold text-slate-900 mt-0.5">{{ selectedLocation.supervisor }}</p>
            </div>
            <div class="bg-slate-50 p-3 rounded-2xl border border-slate-100">
              <p class="text-[11px] text-slate-500 font-medium">Active Workforce</p>
              <p class="text-xs font-bold text-indigo-700 mt-0.5">{{ selectedLocation.activeWorkers }} Workers</p>
            </div>
            <div class="bg-slate-50 p-3 rounded-2xl border border-slate-100">
              <p class="text-[11px] text-slate-500 font-medium">Daily Target</p>
              <p class="text-xs font-bold text-slate-700 mt-0.5 font-mono">{{ Number(selectedLocation.targetKg).toFixed(1) }} Kg</p>
            </div>
            <div class="bg-emerald-50/70 p-3 rounded-2xl border border-emerald-100">
              <p class="text-[11px] text-emerald-800 font-medium">Today's Shelling</p>
              <p class="text-sm font-black text-emerald-900 mt-0.5 font-mono">{{ Number(selectedLocation.todayCutKg).toFixed(1) }} Kg</p>
            </div>
          </div>
        </div>

      </div>

      <!-- Right Column: Units List & Bags Transit Tracker (4 Cols) -->
      <div class="lg:col-span-4 space-y-6 flex flex-col justify-between">
        
        <!-- Units Quick Focus List -->
        <div class="bg-white rounded-3xl p-5 border border-amber-100 shadow-sm">
          <h3 class="text-xs font-bold text-slate-400 uppercase tracking-wider mb-3">
            Village Units ({ { locations.length } })
          </h3>

          <div class="space-y-2">
            <div 
              v-for="loc in locations" 
              :key="loc.id"
              @click="focusOnLocation(loc)"
              :class="selectedLocation?.id === loc.id ? 'bg-amber-100/70 border-amber-300' : 'bg-slate-50 hover:bg-amber-50/50 border-slate-100'"
              class="p-3 rounded-2xl border cursor-pointer transition flex items-center justify-between"
            >
              <div>
                <p class="text-xs font-bold text-slate-900">{{ loc.name }}</p>
                <p class="text-[10px] text-slate-400">{{ loc.village }} • {{ loc.activeWorkers }} Workers</p>
              </div>
              <span class="text-xs font-bold font-mono text-emerald-700">
                {{ Number(loc.todayCutKg).toFixed(0) }} Kg
              </span>
            </div>
          </div>
        </div>

        <!-- Bags & Fleet Transit Tracker -->
        <div class="bg-white rounded-3xl p-5 border border-amber-100 shadow-sm">
          <div class="flex items-center justify-between pb-3 border-b border-slate-100 mb-3">
            <h3 class="text-xs font-bold text-slate-900 flex items-center gap-1.5">
              <Truck class="w-4 h-4 text-amber-600" />
              <span>Bag Transit Logistics</span>
            </h3>
            <span class="px-2 py-0.5 rounded text-[10px] font-bold bg-amber-100 text-amber-900">
              Live Fleet
            </span>
          </div>

          <div class="space-y-3">
            <div 
              v-for="tr in activeTransits" 
              :key="tr.tripId"
              class="p-3.5 rounded-2xl border border-slate-100 bg-slate-50 space-y-2"
            >
              <div class="flex items-center justify-between text-xs">
                <span class="font-bold text-slate-800">{{ tr.vehicleNo }}</span>
                <span 
                  :class="tr.status === 'IN_TRANSIT' ? 'bg-amber-100 text-amber-900' : 'bg-emerald-100 text-emerald-900'"
                  class="px-2 py-0.5 rounded text-[10px] font-bold font-mono"
                >
                  {{ tr.status === 'IN_TRANSIT' ? 'In Transit' : 'Delivered' }}
                </span>
              </div>

              <div class="text-[11px] text-slate-600 space-y-0.5">
                <p><span class="text-slate-400">Route:</span> {{ tr.origin }} → {{ tr.destination }}</p>
                <p><span class="text-slate-400">Cargo:</span> {{ tr.cargoType }} ({{ tr.bagsCount }} Bags • {{ tr.weightKg }} Kg)</p>
                <p><span class="text-slate-400">Driver:</span> {{ tr.driverName }} ({{ tr.driverPhone }})</p>
              </div>

              <div class="pt-1.5 border-t border-slate-200/60 flex justify-between text-[10px] text-slate-500 font-mono">
                <span>Dep: {{ tr.departureTime }}</span>
                <span class="text-amber-800 font-bold">ETA: {{ tr.eta }}</span>
              </div>
            </div>
          </div>
        </div>

      </div>

    </main>

    <!-- Footer -->
    <footer class="p-4 text-center text-slate-400 text-xs border-t border-slate-200 bg-white">
      CashewPro Multi-Shed ERP Suite • Srikakulam District Operations
    </footer>
  </div>
</template>

<style>
/* Leaflet map popup style clean overrides */
.leaflet-popup-content-wrapper {
  border-radius: 16px !important;
  box-shadow: 0 10px 25px -5px rgba(0, 0, 0, 0.15) !important;
  border: 1px solid #fef3c7 !important;
}
.leaflet-popup-tip {
  background: white !important;
}
</style>