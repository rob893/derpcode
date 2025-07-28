import { useState } from 'react';
import { useCurrentUser } from '../hooks/useUser';
import { AccountSection } from '../components/AccountSection';
import { PreferencesSection } from '../components/PreferencesSection';
import { Card, CardBody } from '@heroui/react';

export function AccountPage() {
  const [activeSection, setActiveSection] = useState<'account' | 'preferences'>('account');
  const { data: user, isLoading, error } = useCurrentUser();

  if (isLoading) {
    return (
      <div className="flex justify-center items-center min-h-96">
        <div className="text-default-500">Loading...</div>
      </div>
    );
  }

  if (error || !user) {
    return (
      <div className="flex justify-center items-center min-h-96">
        <div className="text-danger">Failed to load user information</div>
      </div>
    );
  }

  return (
    <div className="container mx-auto px-4 py-8 max-w-6xl">
      <div className="flex flex-col lg:flex-row gap-6">
        {/* Left sidebar navigation */}
        <div className="lg:w-64 shrink-0">
          <Card>
            <CardBody className="p-0">
              <nav className="space-y-1">
                <button
                  onClick={() => setActiveSection('account')}
                  className={`w-full text-left px-4 py-3 text-sm font-medium transition-colors ${
                    activeSection === 'account'
                      ? 'bg-primary text-primary-foreground'
                      : 'text-foreground hover:bg-default-100'
                  }`}
                >
                  Account
                </button>
                <button
                  onClick={() => setActiveSection('preferences')}
                  className={`w-full text-left px-4 py-3 text-sm font-medium transition-colors ${
                    activeSection === 'preferences'
                      ? 'bg-primary text-primary-foreground'
                      : 'text-foreground hover:bg-default-100'
                  }`}
                >
                  Preferences
                </button>
              </nav>
            </CardBody>
          </Card>
        </div>

        {/* Main content area */}
        <div className="flex-1">
          {activeSection === 'account' && <AccountSection user={user} />}
          {activeSection === 'preferences' && <PreferencesSection />}
        </div>
      </div>
    </div>
  );
}
