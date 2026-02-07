import { fileURLToPath, URL } from 'node:url'
import fs from 'fs'
import path from 'path'

import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import vueDevTools from 'vite-plugin-vue-devtools'

// ES modules __dirname equivalent
const __dirname = path.dirname(fileURLToPath(import.meta.url))

// Get PSP API target from environment variable
const getPspTarget = () => {
  const url = process.env.VITE_PSP_API_URL || 'https://psp-lb:443/api'
  return url.replace(/\/api$/, '')
}

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
        target: getPspTarget(),
        changeOrigin: true, 
        secure: false
      }
    }
  }
})
