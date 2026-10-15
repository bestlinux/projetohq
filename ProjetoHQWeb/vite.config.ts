import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    host: true, // Permite acesso pelo celular na mesma rede Wi-Fi (ex: http://192.168.15.3:5173)
    port: 5173,
    proxy: {
      '/api': {
        target: 'https://localhost:9001',
        changeOrigin: true,
        secure: false, // Permite o certificado autoassinado de desenvolvimento do ASP.NET Core
      },
      '/capas': {
        target: 'https://localhost:9001',
        changeOrigin: true,
        secure: false,
      },
      '/desejos-capas': {
        target: 'https://localhost:9001',
        changeOrigin: true,
        secure: false,
      },
      '/frases-imagens': {
        target: 'https://localhost:9001',
        changeOrigin: true,
        secure: false,
      }
    }
  }
})
