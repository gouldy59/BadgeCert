import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5000,
    host: '0.0.0.0',
    allowedHosts: [
      'd67df0a2-2d1d-4b80-97b1-0dbf2bbaa229-00-1imjt3grtp1fv.riker.replit.dev',
      'localhost',
      '127.0.0.1',
      '.replit.dev'
    ]
  }
})