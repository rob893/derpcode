import { Card, CardBody, CardHeader, Button } from '@heroui/react';
import { ArrowDownTrayIcon } from '@heroicons/react/24/outline';
import { usePWA } from '../hooks/usePWA';

export function PreferencesSection() {
  const { isInstallable, promptInstall } = usePWA();

  const handleInstall = async (): Promise<void> => {
    try {
      await promptInstall();
    } catch (error) {
      console.error('Failed to install PWA:', error);
    }
  };

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-foreground mb-2">Preferences</h1>
        <p className="text-default-500">Customize your DerpCode experience</p>
      </div>

      {isInstallable && (
        <Card>
          <CardHeader>
            <h2 className="text-lg font-semibold">App Installation</h2>
          </CardHeader>
          <CardBody className="space-y-3">
            <p className="text-default-500">Install DerpCode directly on your device!</p>
            <Button
              color="primary"
              onPress={handleInstall}
              startContent={<ArrowDownTrayIcon className="h-4 w-4" />}
              className="w-fit"
              isDisabled={!isInstallable}
            >
              {isInstallable ? 'Install' : 'Install Not Available'}
            </Button>
          </CardBody>
        </Card>
      )}

      <Card>
        <CardHeader>
          <h2 className="text-lg font-semibold">User Preferences</h2>
        </CardHeader>
        <CardBody>
          <p className="text-default-500">Preferences settings will be available in a future update.</p>
        </CardBody>
      </Card>
    </div>
  );
}
