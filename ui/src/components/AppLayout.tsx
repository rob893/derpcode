import React from 'react';
import { AppHeader } from './AppHeader';

interface AppLayoutProps {
  children: React.ReactNode;
}

export function AppLayout({ children }: AppLayoutProps) {
  return (
    <div className="app-layout">
      <AppHeader />
      <main className="app-main">
        <div className="container">{children}</div>
      </main>
    </div>
  );
}
