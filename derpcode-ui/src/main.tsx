import React from 'react';
import ReactDOM from 'react-dom/client';
import { HashRouter as Router } from 'react-router';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ReactQueryDevtools } from '@tanstack/react-query-devtools';
import { HeroUIProvider } from '@heroui/react';
import App from './App.tsx';
import './index.css';
import { GitHubCallbackPage } from './pages/GitHubCallbackPage.tsx';
import { AuthProvider } from './contexts/AuthContext.tsx';

// Create a client
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: (failureCount, error) => {
        // Don't retry on 404s
        if (error && typeof error === 'object' && 'status' in error && error.status === 404) {
          return false;
        }
        // Retry up to 2 times for other errors
        return failureCount < 2;
      },
      refetchOnWindowFocus: false
    },
    mutations: {
      retry: false
    }
  }
});

if (window.location.pathname.includes('auth/github/callback')) {
  ReactDOM.createRoot(document.getElementById('root')!).render(
    <React.StrictMode>
      <HeroUIProvider>
        <QueryClientProvider client={queryClient}>
          <AuthProvider>
            <GitHubCallbackPage />
          </AuthProvider>
          <ReactQueryDevtools initialIsOpen={false} />
        </QueryClientProvider>
      </HeroUIProvider>
    </React.StrictMode>
  );
} else {
  ReactDOM.createRoot(document.getElementById('root')!).render(
    <React.StrictMode>
      <HeroUIProvider>
        <QueryClientProvider client={queryClient}>
          <Router>
            <App />
            <ReactQueryDevtools initialIsOpen={false} />
          </Router>
        </QueryClientProvider>
      </HeroUIProvider>
    </React.StrictMode>
  );
}
