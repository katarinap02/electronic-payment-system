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
    proxy: {
      // PSP proxy must be before /api to avoid being caught by /api proxy
      '/api/psp': {
        target: 'https://psp-api:443',
        changeOrigin: true,
        secure: false,
        rewrite: (path) => path.replace(/^\/api\/psp/, '/api')
      },
      '/api': {
        target: 'https://webshop-api:443',
        changeOrigin: true, 
        secure: false,
      }
    }
  }
})
