import { defineConfig, loadEnv } from 'vite';
import react from '@vitejs/plugin-react';
import { VitePWA } from 'vite-plugin-pwa';
import tailwindcss from '@tailwindcss/vite';

// https://vite.dev/config/
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '');

  return {
    plugins: [
      react(),
      tailwindcss(),
      VitePWA({
        registerType: 'autoUpdate',
        includeAssets: ['favicon.ico', 'apple-touch-icon.png', 'masked-icon.svg', 'mstile-150x150.png'],
        manifest: {
          name: env.VITE_PWA_NAME || 'DerpCode - Algorithm Practice Platform',
          short_name: env.VITE_PWA_SHORT_NAME || 'DerpCode',
          description: env.VITE_APP_DESCRIPTION || 'A snarky algorithm practice platform for coding challenges',
          theme_color: '#000000',
          background_color: '#000000',
          display: 'standalone',
          scope: '/',
          start_url: '/',
          icons: [
            {
              src: 'pwa-192x192.png',
              sizes: '192x192',
              type: 'image/png'
            },
            {
              src: 'pwa-512x512.png',
              sizes: '512x512',
              type: 'image/png'
            },
            {
              src: 'pwa-512x512.png',
              sizes: '512x512',
              type: 'image/png',
              purpose: 'any maskable'
            }
          ]
        },
        workbox: {
          globPatterns: ['**/*.{js,css,html,ico,png,svg}'],
          navigateFallback: '/index.html',
          maximumFileSizeToCacheInBytes: 5 * 1024 * 1024, // 5 MB
          navigateFallbackDenylist: [/^\/api/]
          // runtimeCaching: [
          //   {
          //     urlPattern: /^https:\/\/api\.*/i,
          //     handler: 'NetworkFirst',
          //     options: {
          //       cacheName: 'api-cache',
          //       expiration: {
          //         maxEntries: 10,
          //         maxAgeSeconds: 60 * 60 * 24 * 365 // <== 365 days
          //       }
          //     }
          //   }
          // ]
        },
        devOptions: {
          enabled: true,
          suppressWarnings: true
        }
      })
    ],
    css: {
      postcss: './postcss.config.js'
    },
    publicDir: 'public',
    build: {
      rollupOptions: {
        external: [],
        output: {
          // Exclude documentation files from build
          assetFileNames: assetInfo => {
            const name = assetInfo.name || '';
            if (name.includes('README') || name.includes('generate-icons')) {
              return 'excluded/[name].[ext]';
            }
            return 'assets/[name]-[hash].[ext]';
          }
        }
      }
    }
  };
});
