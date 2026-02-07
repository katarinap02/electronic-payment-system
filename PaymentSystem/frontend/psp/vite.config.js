import { fileURLToPath, URL } from 'node:url'

import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import vueDevTools from 'vite-plugin-vue-devtools'

export default defineConfig({
  plugins: [
    vue(),
    vueDevTools(),
  ],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    },
  },
  server: {
    port: 5174,
    host: true,
    watch: {
      usePolling: true,
    },
    hmr: {
      host: 'localhost',
      port: 5174,
    },
    proxy: {
      '/api': {
        target: 'https://psp-api:443',
        changeOrigin: true, 
        secure: false
      }
    }
  }
})
