import { createI18n } from 'vue-i18n';
import en from './locales/en.json';
import te from './locales/te.json';
import hi from './locales/hi.json';

const savedLang = localStorage.getItem('preferred_language') || 'te';

const i18n = createI18n({
  legacy: false,
  locale: savedLang,
  fallbackLocale: 'en',
  messages: {
    en,
    te,
    hi,
  },
});

export default i18n;