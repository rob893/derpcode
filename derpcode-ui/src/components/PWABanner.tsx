import { Button, Card, CardBody } from '@heroui/react';
import { XMarkIcon, ArrowDownTrayIcon, ArrowPathIcon } from '@heroicons/react/24/outline';
import { useState, useEffect, useRef } from 'react';
import { usePWA } from '../hooks/usePWA';

const WEEK_IN_MS = 7 * 24 * 60 * 60 * 1000; // 1 week in milliseconds
const AUTO_DISMISS_DURATION = 15000; // 10 seconds
const PWA_INSTALL_LAST_SHOWN_KEY = 'pwa-install-last-shown';

/**
 * Safely get item from localStorage
 */
function getFromStorage(key: string): string | null {
  try {
    return localStorage.getItem(key);
  } catch {
    return null;
  }
}

/**
 * Safely set item in localStorage
 */
function setInStorage(key: string, value: string): void {
  try {
    localStorage.setItem(key, value);
  } catch {
    // Ignore localStorage errors
  }
}

/**
 * Circular countdown component for auto-dismiss
 */
interface CountdownProps {
  duration: number;
  onComplete: () => void;
  size?: number;
  strokeWidth?: number;
}

function CircularCountdown({ duration, onComplete, size = 24, strokeWidth = 2 }: CountdownProps): React.ReactElement {
  const [timeLeft, setTimeLeft] = useState(duration);
  const intervalRef = useRef<number | null>(null);

  useEffect(() => {
    intervalRef.current = window.setInterval(() => {
      setTimeLeft(prev => {
        if (prev <= 100) {
          onComplete();
          return 0;
        }
        return prev - 100;
      });
    }, 100);

    return () => {
      if (intervalRef.current) {
        clearInterval(intervalRef.current);
      }
    };
  }, [onComplete]);

  const radius = (size - strokeWidth) / 2;
  const circumference = 2 * Math.PI * radius;
  const strokeDashoffset = circumference - (timeLeft / duration) * circumference;

  return (
    <div className="relative inline-flex items-center justify-center" style={{ width: size, height: size }}>
      <svg className="absolute inset-0 -rotate-90" width={size} height={size} viewBox={`0 0 ${size} ${size}`}>
        <circle
          cx={size / 2}
          cy={size / 2}
          r={radius}
          stroke="currentColor"
          strokeWidth={strokeWidth}
          fill="none"
          className="text-gray-300"
        />
        <circle
          cx={size / 2}
          cy={size / 2}
          r={radius}
          stroke="currentColor"
          strokeWidth={strokeWidth}
          fill="none"
          strokeDasharray={circumference}
          strokeDashoffset={strokeDashoffset}
          className="text-primary transition-all duration-100 ease-linear"
          strokeLinecap="round"
        />
      </svg>
      <XMarkIcon className="h-4 w-4 relative z-10" />
    </div>
  );
}

/**
 * PWA Banner component that shows installation prompts and update notifications
 */
export function PWABanner(): React.ReactElement | null {
  const { isInstallable, isUpdateAvailable, promptInstall, updateServiceWorker, isInstalled } = usePWA();
  const [isInstallDismissed, setIsInstallDismissed] = useState(false);
  const [isUpdateDismissed, setIsUpdateDismissed] = useState(false);
  const [showInstallBanner, setShowInstallBanner] = useState(false);

  // Check if we should show the install banner based on local storage
  useEffect(() => {
    if (!isInstallable || isInstalled) {
      setShowInstallBanner(false);
      return;
    }

    const lastShown = getFromStorage(PWA_INSTALL_LAST_SHOWN_KEY);
    const now = Date.now();

    if (!lastShown) {
      // First time - show the banner
      setShowInstallBanner(true);
      setInStorage(PWA_INSTALL_LAST_SHOWN_KEY, now.toString());
    } else {
      const lastShownTime = parseInt(lastShown, 10);
      const timeSinceLastShown = now - lastShownTime;

      if (timeSinceLastShown >= WEEK_IN_MS) {
        // More than a week has passed - show the banner again
        setShowInstallBanner(true);
        setInStorage(PWA_INSTALL_LAST_SHOWN_KEY, now.toString());
      }
    }
  }, [isInstallable, isInstalled]);

  const handleInstall = async (): Promise<void> => {
    try {
      await promptInstall();
      setIsInstallDismissed(true);
      setShowInstallBanner(false);
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

  const handleInstallDismiss = (): void => {
    setIsInstallDismissed(true);
    setShowInstallBanner(false);
  };

  const handleInstallAutoDismiss = (): void => {
    setShowInstallBanner(false);
  };

  // Show install banner
  if (showInstallBanner && !isInstallDismissed) {
    return (
      <Card className="fixed bottom-4 left-4 right-4 z-50 md:left-auto md:right-4 md:w-96">
        <CardBody className="flex flex-row items-center gap-3 p-4">
          <ArrowDownTrayIcon className="h-6 w-6 text-primary" />
          <div className="flex-1">
            <h3 className="text-sm font-semibold">Install DerpCode</h3>
            <p className="text-xs text-gray-500">Get the full app experience</p>
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
            <Button size="sm" variant="light" isIconOnly onPress={handleInstallDismiss} className="relative">
              <CircularCountdown duration={AUTO_DISMISS_DURATION} onComplete={handleInstallAutoDismiss} />
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
