import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  base: process.env.NODE_ENV === 'production' ? '/dist/inventory/' : '/',
  build: {
    outDir: '../wwwroot/dist/inventory',
    emptyOutDir: true,
  },
  server: {
    allowedHosts: [
      'localhost',
      '127.0.0.1',
      'hyperethical-sincere-sympathizingly.ngrok-free.dev',
    ],
  },
})
