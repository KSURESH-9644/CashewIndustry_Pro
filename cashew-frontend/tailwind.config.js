/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{vue,js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        cashew: {
          50: '#fdfbf7',
          100: '#f7f2e7',
          200: '#ede1ca',
          300: '#dfcaa5',
          400: '#ceae7c',
          500: '#b89257', // Golden cashew accent
          600: '#9e7543',
          700: '#7d5935',
          800: '#64472f',
          900: '#523b29',
        }
      }
    },
  },
  plugins: [],
}