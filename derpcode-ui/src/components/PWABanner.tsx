import { Button, Card, CardBody } from '@heroui/react';
import { XMarkIcon, ArrowDownTrayIcon, ArrowPathIcon } from '@heroicons/react/24/outline';
import { useState } from 'react';
import { usePWA } from '../hooks/usePWA';

/**
 * PWA Banner component that shows installation prompts and update notifications
 */
export function PWABanner(): React.ReactElement | null {
  const { isInstallable, isUpdateAvailable, promptInstall, updateServiceWorker } = usePWA();
  const [isInstallDismissed, setIsInstallDismissed] = useState(false);
  const [isUpdateDismissed, setIsUpdateDismissed] = useState(false);

  const handleInstall = async (): Promise<void> => {
    try {
      await promptInstall();
      setIsInstallDismissed(true);
    } catch (error) {
      console.error('Failed to install PWA:', error);
    }
  };

  const handleUpdate = async (): Promise<void> => {
    try {
      await updateServiceWorker();
      setIsUpdateDismissed(true);
    } catch (error) {
      console.error('Failed to update PWA:', error);
    }
  };

  // Show install banner
  if (isInstallable && !isInstallDismissed) {
    return (
      <Card className="fixed bottom-4 left-4 right-4 z-50 md:left-auto md:right-4 md:w-96">
        <CardBody className="flex flex-row items-center gap-3 p-4">
          <ArrowDownTrayIcon className="h-6 w-6 text-primary" />
          <div className="flex-1">
            <h3 className="text-sm font-semibold">Install DerpCode</h3>
            <p className="text-xs text-gray-500">Get the full app experience with offline access</p>
          </div>
          <div className="flex gap-2">
            <Button
              size="sm"
              color="primary"
              onPress={handleInstall}
              startContent={<ArrowDownTrayIcon className="h-4 w-4" />}
            >
              Install
            </Button>
            <Button size="sm" variant="light" isIconOnly onPress={() => setIsInstallDismissed(true)}>
              <XMarkIcon className="h-4 w-4" />
            </Button>
          </div>
        </CardBody>
      </Card>
    );
  }

  // Show update banner
  if (isUpdateAvailable && !isUpdateDismissed) {
    return (
      <Card className="fixed bottom-4 left-4 right-4 z-50 md:left-auto md:right-4 md:w-96">
        <CardBody className="flex flex-row items-center gap-3 p-4">
          <ArrowPathIcon className="h-6 w-6 text-success" />
          <div className="flex-1">
            <h3 className="text-sm font-semibold">Update Available</h3>
            <p className="text-xs text-gray-500">A new version of DerpCode is ready</p>
          </div>
          <div className="flex gap-2">
            <Button
              size="sm"
              color="success"
              onPress={handleUpdate}
              startContent={<ArrowPathIcon className="h-4 w-4" />}
            >
              Update
            </Button>
            <Button size="sm" variant="light" isIconOnly onPress={() => setIsUpdateDismissed(true)}>
              <XMarkIcon className="h-4 w-4" />
            </Button>
          </div>
        </CardBody>
      </Card>
    );
  }

  return null;
}
