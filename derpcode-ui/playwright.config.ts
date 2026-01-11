/// <reference types="node" />

import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './e2e',
  fullyParallel: true,
  timeout: 60_000,
  expect: { timeout: 10_000 },
  use: {
    baseURL: 'http://127.0.0.1:5173',
    ignoreHTTPSErrors: true,
    trace: 'retain-on-failure',
    screenshot: 'only-on-failure',
    video: 'retain-on-failure'
  },
  webServer: [
    {
      command:
        'dotnet run --project ../DerpCode.API/DerpCode.API.csproj --urls https://localhost:7059;http://localhost:5170',
      url: 'https://localhost:7059/health',
      ignoreHTTPSErrors: true,
      reuseExistingServer: true,
      timeout: 120_000,
      env: {
        ...process.env,
        ASPNETCORE_ENVIRONMENT: 'Development'
      }
    },
    {
      command: 'npm run dev -- --host 127.0.0.1 --port 5173',
      url: 'http://127.0.0.1:5173',
      env: {
        ...process.env,
        // Playwright runs against local API by default; the repo's `.env` points at prod.
        VITE_DERPCODE_API_BASE_URL: 'https://localhost:7059'
      },
      reuseExistingServer: true,
      timeout: 120_000
    }
  ],
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] }
    }
  ]
});
