import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import fs from 'node:fs'

export default defineConfig({
  plugins: [react()],
  server: {
    https: {
      pfx: fs.readFileSync('./localhost.pfx'),
      passphrase: 'LifeLinkDevCert123',
    },
    host: 'localhost',
    port: 5173,
  },
})