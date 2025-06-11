import React from 'react';
import { AppHeader } from '../components/AppHeader';
import { WarningBanner } from '../components/WarningBanner';

interface AppLayoutProps {
  children: React.ReactNode;
}

export function AppLayout({ children }: AppLayoutProps) {
  return (
    <div className="min-h-screen bg-background flex flex-col">
      <AppHeader />
      <WarningBanner />
      <main className="flex-1 py-8">
        <div className="w-full px-4">{children}</div>
      </main>
    </div>
  );
}
