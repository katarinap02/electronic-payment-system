import { fileURLToPath, URL } from 'node:url'
import fs from 'fs'
import path from 'path'

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
    https: {
      cert: fs.readFileSync(path.resolve(__dirname, './certs/localhost+2.pem')),
      key: fs.readFileSync(path.resolve(__dirname, './certs/localhost+2-key.pem'))
    },
    port: 5173,
    host: true,
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
