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
        target: 'https://psp-lb:443',
        changeOrigin: true, 
        secure: false
      }
    }
  }
})
