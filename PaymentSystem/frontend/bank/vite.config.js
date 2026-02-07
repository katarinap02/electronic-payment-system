import { fileURLToPath, URL } from 'node:url'
import fs from 'fs'
import path from 'path'

import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import vueDevTools from 'vite-plugin-vue-devtools'

// ES modules __dirname equivalent
const __dirname = path.dirname(fileURLToPath(import.meta.url))

// Get Bank API target from environment variable
const getBankTarget = () => {
  const url = process.env.VITE_BANK_API_URL || 'https://bank-api:443/api'
  return url.replace(/\/api$/, '')
}

// https://vite.dev/config/
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
    port: 5172,
    host: true,
    proxy: {
      '/api': {
        target: getBankTarget(),
        changeOrigin: true, 
        secure: false,
      }
    }
  }
})
