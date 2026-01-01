import { defineConfig, loadEnv } from 'vite';
import { resolve } from 'path';

export default defineConfig(({ mode }) => {
  // Load env file based on `mode` in the current working directory.
  // Set the third parameter to '' to load all envs regardless of the `VITE_` prefix.
  const env = loadEnv(mode, process.cwd());

  return {
    // 1. Multi-page support (Rollup entry points)
    build: {
      rollupOptions: {
        input: {
          main: resolve(__dirname, 'index.html'),
          success: resolve(__dirname, 'success.html'),
        },
      },
    },
    
    // 2. Development Server Configuration
    server: {
      port: 5500,        // Matches your current CORS settings and Docker mapping
      strictPort: true,  // If 5500 is busy, fail instead of picking a random port
      host: true,        // Needed for Docker/Network access
    },

    // 3. Optional: Define global constants
    define: {
      __APP_ENV__: JSON.stringify(env.APP_ENV),
    },
  };
});