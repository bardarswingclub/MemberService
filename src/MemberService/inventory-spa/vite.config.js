import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
// https://vitejs.dev/config/
export default defineConfig({
    plugins: [react()],
    base: '/',
    build: {
        outDir: '../wwwroot/dist/inventory',
        emptyOutDir: true,
        rollupOptions: {
            output: {
                entryFileNames: 'assets/main.js',
                assetFileNames: 'assets/main[extname]',
            },
        },
    },
    server: {
        allowedHosts: [
            'localhost',
            '127.0.0.1',
            'hyperethical-sincere-sympathizingly.ngrok-free.dev',
        ],
    },
});
