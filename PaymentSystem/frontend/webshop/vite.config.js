import { fileURLToPath, URL } from 'node:url'
import fs from 'fs'
import path from 'path'

import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import vueDevTools from 'vite-plugin-vue-devtools'

// ES modules __dirname equivalent
const __dirname = path.dirname(fileURLToPath(import.meta.url))

// Read environment variables for distributed setup
// Remove /api suffix if present to get base URL for proxy target
const getPspTarget = () => {
  const url = process.env.VITE_PSP_API_URL || 'https://psp-lb:443/api'
  return url.replace(/\/api$/, '')
}

const getWebshopTarget = () => {
  const url = process.env.VITE_WEBSHOP_API_URL || 'https://webshop-api:443/api'
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
    port: 5173,
    host: true,
    proxy: {
      // PSP proxy must be before /api to avoid being caught by /api proxy
      '/api/psp': {
        target: getPspTarget(),
        changeOrigin: true,
        secure: false,
        rewrite: (path) => path.replace(/^\/api\/psp/, '/api')
      },
      '/api': {
        target: getWebshopTarget(),
        changeOrigin: true, 
        secure: false,
        rewrite: (path) => path,
      }
    }
  }
})
