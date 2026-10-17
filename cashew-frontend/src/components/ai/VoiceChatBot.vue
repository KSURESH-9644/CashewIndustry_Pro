<script setup lang="ts">
import { ref, watch, nextTick } from 'vue';
import { useI18n } from 'vue-i18n';
import { 
  Bot, 
  X, 
  Send, 
  Mic, 
  MicOff, 
  Sparkles, 
  Volume2, 
  VolumeX, 
  ShoppingBag,
  ExternalLink,
  ArrowUpRight
} from 'lucide-vue-next';

const { t, locale } = useI18n();

interface Message {
  id: string;
  sender: 'user' | 'bot';
  text: string;
  time: string;
  action?: {
    type: string;
    payload?: any;
    label?: string;
  };
}

const isOpen = ref(false);
const inputText = ref('');
const isListening = ref(false);
const isSpeakingEnabled = ref(true);
const messagesContainer = ref<HTMLDivElement | null>(null);

const messages = ref<Message[]>([
  {
    id: '1',
    sender: 'bot',
    text: 'నమస్తే! నేను మీ CashewPro AI అసిస్టెంట్ ని. మీకు కావలసిన జీడిపప్పు రకాలు, నేటి ధరలు లేదా ఆర్డర్ల గురించి అడగండి లేదా మాట్లాడండి.',
    time: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
  }
]);

// Switch greeting when language changes
watch(locale, (newLang) => {
  if (messages.value.length === 1) {
    if (newLang === 'te') {
      messages.value[0].text = 'నమస్తే! నేను మీ CashewPro AI అసిస్టెంట్ ని. మీకు కావలసిన జీడిపప్పు రకాలు, నేటి ధరలు లేదా ఆర్డర్ల గురించి అడగండి.';
    } else if (newLang === 'hi') {
      messages.value[0].text = 'नमस्ते! मैं आपका कैश्यूप्रो एआई सहायक हूँ। काजू के प्रकार, ताज़ा भाव या ऑर्डर के बारे में पूछें या बोलें।';
    } else {
      messages.value[0].text = 'Hello! I am your CashewPro AI Assistant. Ask or speak about cashew grades, today\'s market prices, or retail & wholesale booking.';
    }
  }
});

const scrollToBottom = async () => {
  await nextTick();
  if (messagesContainer.value) {
    messagesContainer.value.scrollTop = messagesContainer.value.scrollHeight;
  }
};

// Text-to-Speech (Bot Voice Output)
const speakResponse = (text: string) => {
  if (!isSpeakingEnabled.value || !('speechSynthesis' in window)) return;
  
  window.speechSynthesis.cancel();
  const utterance = new SpeechSynthesisUtterance(text);
  
  if (locale.value === 'te') {
    utterance.lang = 'te-IN';
  } else if (locale.value === 'hi') {
    utterance.lang = 'hi-IN';
  } else {
    utterance.lang = 'en-IN';
  }
  
  utterance.rate = 0.95;
  window.speechSynthesis.speak(utterance);
};

// Web Speech API: Voice Recognition (Telugu, Hindi, English)
let recognition: any = null;

const toggleVoiceRecognition = () => {
  const SpeechRecognition = (window as any).SpeechRecognition || (window as any).webkitSpeechRecognition;
  
  if (!SpeechRecognition) {
    alert('Voice input is not supported in this browser. Please use Google Chrome or Edge.');
    return;
  }

  if (isListening.value) {
    recognition?.stop();
    isListening.value = false;
    return;
  }

  recognition = new SpeechRecognition();
  recognition.continuous = false;
  recognition.interimResults = false;

  // Language based on active switcher
  if (locale.value === 'te') {
    recognition.lang = 'te-IN';
  } else if (locale.value === 'hi') {
    recognition.lang = 'hi-IN';
  } else {
    recognition.lang = 'en-IN';
  }

  recognition.onstart = () => {
    isListening.value = true;
  };

  recognition.onresult = (event: any) => {
    const transcript = event.results[0][0].transcript;
    inputText.value = transcript;
    isListening.value = false;
    handleSendMessage();
  };

  recognition.onerror = () => {
    isListening.value = false;
  };

  recognition.onend = () => {
    isListening.value = false;
  };

  recognition.start();
};

// Fast Local AI Engine for Storefront Intents
const processCustomerIntent = (input: string): { reply: string; action?: any } => {
  const lower = input.toLowerCase();

  // Price inquiries
  if (lower.includes('180') || lower.includes('king') || lower.includes('కింగ్') || lower.includes('వంద ఎనభై')) {
    const text = locale.value === 'te' 
      ? 'W-180 King Jumbo హోల్ జీడిపప్పు రిటైల్ ధర కిలో ₹1,180/- మరియు హోల్‌సేల్ (50kg+) ధర ₹1,050/-. ఇది అత్యంత పెద్ద పరిమాణం గల ప్రీమియం రకం.'
      : locale.value === 'hi'
      ? 'W-180 किंग जंबो काजू का खुदरा भाव ₹1,180/किग्रा और थोक भाव ₹1,050/किग्रा है।'
      : 'W-180 King Jumbo Whole Cashews retail at ₹1,180/kg and wholesale (50kg+) at ₹1,050/kg. This is our largest royal grade.';
    return { reply: text, action: { type: 'HIGHLIGHT_GRADE', payload: 'W-180', label: 'View W-180' } };
  }

  if (lower.includes('240') || lower.includes('standard') || lower.includes('స్టాండర్డ్')) {
    const text = locale.value === 'te'
      ? 'W-240 స్టాండర్డ్ హోల్ జీడిపప్పు కిలో ₹950/- (రిటైల్) మరియు హోల్‌సేల్ కిలో ₹840/-. ఇది రెగ్యులర్ వాడకానికి బెస్ట్ క్వాలిటీ.'
      : locale.value === 'hi'
      ? 'W-240 स्टैंडर्ड काजू का खुदरा भाव ₹950/किग्रा और थोक भाव ₹840/किग्रा है।'
      : 'W-240 Standard Whole is priced at ₹950/kg (Retail) and ₹840/kg (Wholesale).';
    return { reply: text, action: { type: 'HIGHLIGHT_GRADE', payload: 'W-240', label: 'View W-240' } };
  }

  if (lower.includes('320') || lower.includes('regular') || lower.includes('రైస్')) {
    const text = locale.value === 'te'
      ? 'W-320 రెగ్యులర్ హోల్ జీడిపప్పు రిటైల్ ₹840/- మరియు హోల్‌సేల్ ₹750/-.'
      : 'W-320 Regular Whole is priced at ₹840/kg (Retail) and ₹750/kg (Wholesale).';
    return { reply: text, action: { type: 'HIGHLIGHT_GRADE', payload: 'W-320', label: 'View W-320' } };
  }

  if (lower.includes('split') || lower.includes('బద్దలు') || lower.includes('jh') || lower.includes('టुकड़ा')) {
    const text = locale.value === 'te'
      ? 'JH (జీడి బద్దలు / Splits) రిటైల్ ధర కిలో ₹720/- మరియు హోల్‌సేల్ ₹640/-. స్వీట్స్ మరియు వంటకాలకు ఉత్తమం.'
      : 'JH Splits are priced at ₹720/kg (Retail) and ₹640/kg (Wholesale).';
    return { reply: text, action: { type: 'HIGHLIGHT_GRADE', payload: 'JH', label: 'View Splits' } };
  }

  // Bulk / Wholesale Order Intent
  if (lower.includes('wholesale') || lower.includes('bulk') || lower.includes('హోల్ సేల్') || lower.includes('తోట') || lower.includes('ఆర్డర్') || lower.includes('order')) {
    const text = locale.value === 'te'
      ? 'హోల్‌సేల్ ఆర్డర్లు కనీసం 50 కిలోల నుండి ప్రారంభమవుతాయి. GST బిల్లింగ్, ట్రాన్స్‌పోర్ట్ మరియు E-Way Bill సదుపాయం కలదు. హోల్‌సేల్ విభాగాన్ని సందర్శించండి.'
      : locale.value === 'hi'
      ? 'थोक ऑर्डर 50 किलो से शुरू होते हैं। जीएसटी बिल और ट्रांसपोर्ट की सुविधा उपलब्ध है।'
      : 'Wholesale orders start from 50kg with GST invoices and direct transport dispatch across India.';
    return { reply: text, action: { type: 'NAVIGATE', payload: '#wholesale', label: 'Go to Wholesale Section' } };
  }

  // General fallback
  const fallback = locale.value === 'te'
    ? 'మీరు అడిగిన ప్రశ్నకు మా ప్రతినిధులు సిద్ధంగా ఉన్నారు. మీరు నిర్దిష్ట గ్రేడ్ (W-180, W-240, W-320, JH) ధరలు లేదా హోల్‌సేల్ కొటేషన్ గురించి అడగవచ్చు.'
    : locale.value === 'hi'
    ? 'आप काजू ग्रेड (W-180, W-240, W-320, JH) के भाव या थोक ऑर्डर के बारे में पूछ सकते हैं।'
    : 'You can ask about specific grades (W-180, W-240, W-320, JH), live Mandi rates, or wholesale quote bookings.';
  
  return { reply: fallback };
};

const handleSendMessage = () => {
  const query = inputText.value.trim();
  if (!query) return;

  // Add User Message
  messages.value.push({
    id: Date.now().toString(),
    sender: 'user',
    text: query,
    time: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
  });

  inputText.value = '';
  scrollToBottom();

  // Process Bot Response
  setTimeout(() => {
    const { reply, action } = processCustomerIntent(query);
    messages.value.push({
      id: (Date.now() + 1).toString(),
      sender: 'bot',
      text: reply,
      time: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
      action
    });
    scrollToBottom();
    speakResponse(reply);
  }, 400);
};

const handleActionClick = (action: any) => {
  if (action.type === 'NAVIGATE') {
    const el = document.querySelector(action.payload);
    el?.scrollIntoView({ behavior: 'smooth' });
  } else if (action.type === 'HIGHLIGHT_GRADE') {
    const el = document.querySelector('#grades');
    el?.scrollIntoView({ behavior: 'smooth' });
  }
};
</script>

<template>
  <div class="fixed bottom-6 right-6 z-50">
    <!-- Floating Trigger Button -->
    <button
      v-if="!isOpen"
      @click="isOpen = true"
      class="group relative flex items-center gap-3 px-5 py-3.5 bg-gradient-to-r from-amber-700 via-amber-600 to-amber-500 hover:from-amber-800 hover:to-amber-600 text-white rounded-full shadow-2xl shadow-amber-900/40 hover:scale-105 active:scale-95 transition-all duration-300 border border-amber-300/40"
    >
      <div class="relative">
        <Bot class="w-6 h-6 animate-bounce" />
        <span class="absolute -top-1 -right-1 w-2.5 h-2.5 bg-emerald-400 rounded-full ring-2 ring-white"></span>
      </div>
      <div class="text-left hidden sm:block">
        <p class="text-xs font-black tracking-wider uppercase flex items-center gap-1">
          CashewPro AI
          <Sparkles class="w-3 h-3 text-amber-200" />
        </p>
        <p class="text-[10px] text-amber-100/90 font-medium">Voice & Text Assistant</p>
      </div>
    </button>

    <!-- Expanded Chat Box -->
    <div
      v-else
      class="w-[92vw] sm:w-[420px] h-[580px] bg-white rounded-3xl shadow-2xl border border-amber-200/80 flex flex-col overflow-hidden animate-in fade-in zoom-in-95 duration-200"
    >
      <!-- Chat Header -->
      <div class="bg-gradient-to-r from-amber-950 via-amber-900 to-amber-950 p-4 text-white flex items-center justify-between border-b border-amber-800/60">
        <div class="flex items-center gap-3">
          <div class="w-10 h-10 rounded-2xl bg-amber-600/60 border border-amber-400/30 flex items-center justify-center text-amber-200">
            <Bot class="w-5 h-5" />
          </div>
          <div>
            <h3 class="text-sm font-bold flex items-center gap-1.5 font-serif">
              CashewPro AI Assistant
              <span class="w-2 h-2 rounded-full bg-emerald-400 inline-block"></span>
            </h3>
            <p class="text-[10px] text-amber-300/90 font-mono tracking-wider">TE • EN • HI VOICE ENABLED</p>
          </div>
        </div>
        <div class="flex items-center gap-1">
          <button 
            @click="isSpeakingEnabled = !isSpeakingEnabled"
            class="p-2 text-amber-300 hover:text-white rounded-xl hover:bg-amber-800/40 transition"
            :title="isSpeakingEnabled ? 'Mute Voice Output' : 'Unmute Voice Output'"
          >
            <Volume2 v-if="isSpeakingEnabled" class="w-4 h-4 text-emerald-400" />
            <VolumeX v-else class="w-4 h-4 text-slate-400" />
          </button>
          <button 
            @click="isOpen = false"
            class="p-2 text-amber-300 hover:text-white rounded-xl hover:bg-amber-800/40 transition"
          >
            <X class="w-5 h-5" />
          </button>
        </div>
      </div>

      <!-- Messages Area -->
      <div ref="messagesContainer" class="flex-1 p-4 overflow-y-auto space-y-4 bg-amber-50/30">
        <div
          v-for="msg in messages"
          :key="msg.id"
          :class="msg.sender === 'user' ? 'justify-end' : 'justify-start'"
          class="flex items-end gap-2"
        >
          <div v-if="msg.sender === 'bot'" class="w-7 h-7 rounded-xl bg-amber-600 text-white flex items-center justify-center text-xs shrink-0 shadow-sm">
            <Bot class="w-4 h-4" />
          </div>

          <div
            :class="msg.sender === 'user' 
              ? 'bg-amber-600 text-white rounded-2xl rounded-br-none shadow-md shadow-amber-600/20' 
              : 'bg-white text-slate-800 border border-amber-100 rounded-2xl rounded-bl-none shadow-sm'"
            class="max-w-[80%] p-3.5 text-xs sm:text-[13px] leading-relaxed"
          >
            <p>{{ msg.text }}</p>

            <!-- Action Button inside message -->
            <button
              v-if="msg.action"
              @click="handleActionClick(msg.action)"
              class="mt-2.5 inline-flex items-center gap-1.5 px-3 py-1.5 rounded-lg bg-amber-100 text-amber-900 font-bold text-[11px] hover:bg-amber-200 transition"
            >
              <span>{{ msg.action.label }}</span>
              <ArrowUpRight class="w-3.5 h-3.5" />
            </button>

            <span 
              :class="msg.sender === 'user' ? 'text-amber-200' : 'text-slate-400'"
              class="block text-[9px] mt-1 text-right"
            >
              {{ msg.time }}
            </span>
          </div>
        </div>

        <!-- Listening visual wave -->
        <div v-if="isListening" class="flex items-center gap-2 p-3 bg-rose-50 border border-rose-200 rounded-2xl text-rose-700 text-xs">
          <span class="w-2.5 h-2.5 bg-rose-600 rounded-full animate-ping"></span>
          <span class="font-bold">మీరు మాట్లాడుతున్నారు, వింటున్నాను... (Listening)</span>
        </div>
      </div>

      <!-- Input Area -->
      <div class="p-3 bg-white border-t border-amber-100">
        <form @submit.prevent="handleSendMessage" class="flex items-center gap-2">
          <!-- Voice Button -->
          <button
            type="button"
            @click="toggleVoiceRecognition"
            :class="isListening 
              ? 'bg-rose-600 text-white animate-pulse' 
              : 'bg-amber-100 text-amber-800 hover:bg-amber-200'"
            class="p-3 rounded-2xl transition shadow-sm shrink-0"
            :title="isListening ? 'Stop Mic' : 'Speak in Telugu, English or Hindi'"
          >
            <MicOff v-if="isListening" class="w-4 h-4" />
            <Mic v-else class="w-4 h-4" />
          </button>

          <!-- Input Text -->
          <input
            v-model="inputText"
            type="text"
            :placeholder="t('ai_bot_placeholder')"
            class="flex-1 bg-slate-50 border border-slate-200 rounded-2xl px-4 py-2.5 text-xs text-slate-800 focus:outline-none focus:border-amber-500 focus:bg-white transition"
          />

          <!-- Send Button -->
          <button
            type="submit"
            :disabled="!inputText.trim()"
            class="p-3 bg-amber-600 hover:bg-amber-700 disabled:opacity-40 text-white rounded-2xl transition shadow-sm shrink-0"
          >
            <Send class="w-4 h-4" />
          </button>
        </form>
      </div>
    </div>
  </div>
</template>