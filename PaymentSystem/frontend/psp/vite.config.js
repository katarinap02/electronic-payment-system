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
      pfx: fs.readFileSync(path.resolve(__dirname, './certs/frontend-psp.pfx')),
      passphrase: 'dev-cert-2024'
    },
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
