import { defineConfig } from 'vite';
import { plugin as elm } from 'vite-plugin-elm';

export default defineConfig({
  base: './',
  publicDir: 'www',
  plugins: [elm()],
  server: {
        port: 5173,
        proxy: {
            '/health_check': {
                target: 'http://localhost:8080'
            }
        }
    }
});
