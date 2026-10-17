import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

export default defineConfig({
  plugins: [vue()],
  server: {
    port: 5173,
    strictPort: true, // port 5173 busy ga unte inkoti teeskodu, 5173 lone run chestundi
    proxy: {
      '/api': {
        target: 'http://localhost:5115',
        changeOrigin: true,
        secure: false,
      }
    }
  }
})