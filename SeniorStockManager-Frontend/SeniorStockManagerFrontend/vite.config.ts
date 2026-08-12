import { defineConfig } from 'vitest/config';
import { loadEnv } from 'vite';
import react from '@vitejs/plugin-react';
import path from 'node:path';

// https://vitejs.dev/config/
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '');

  // Destino do proxy /api em dev — nunca embutido no bundle (só usado pelo
  // servidor de dev do Vite, não pelo cliente axios, que usa caminho relativo).
  // Falha alto e cedo se o valor não for uma URL bem formada, em vez de deixar
  // o proxy silenciosamente não funcionar.
  const apiProxyTarget = env.VITE_API_PROXY_TARGET || 'http://localhost:8080';
  try {
    new URL(apiProxyTarget);
  } catch {
    throw new Error(
      `VITE_API_PROXY_TARGET inválida — precisa ser uma URL completa (ex.: http://localhost:8080), recebido: "${apiProxyTarget}"`
    );
  }

  // §7.1 — caminho-base alvo `/stock`
  // (docs/architecture/senior-portal-contracts.md §1). Default `/` preserva o
  // deploy atual por subdomínio (Caddy ainda roteia por subdomínio, não por
  // caminho — migrar isso é §8.2). Mesmo padrão do portal (§4.1) e do
  // care-web (§6.1).
  const basePath = env.VITE_BASE_PATH || '/';

  return {
    base: basePath,
    plugins: [react()],
    resolve: {
      // Essa opção deve ser a mesma que a definida em tsconfig.app.json
      alias: {
        '@': path.resolve(__dirname, './src'),
      },
    },
    server: {
      proxy: {
        '/api': {
          target: apiProxyTarget,
          changeOrigin: true,
        },
      },
    },
    test: {
      environment: 'jsdom',
      globals: true,
      setupFiles: ['./src/test/setup.ts'],
      coverage: {
        provider: 'v8',
        reporter: ['text', 'lcov'],
        include: ['src/**/*.{ts,tsx}'],
        exclude: ['src/test/**', 'src/main.tsx', 'src/vite-env.d.ts'],
      },
    },
  };
});
