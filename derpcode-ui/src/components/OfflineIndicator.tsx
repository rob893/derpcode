import { Chip } from '@heroui/react';
import { SignalSlashIcon } from '@heroicons/react/24/outline';
import { usePWA } from '../hooks/usePWA';

/**
 * Offline indicator component that shows connection status
 */
export function OfflineIndicator(): React.ReactElement | null {
  const { isOnline } = usePWA();

  if (isOnline) {
    return null;
  }

  return (
    <div className="fixed top-4 left-1/2 transform -translate-x-1/2 z-50">
      <Chip color="warning" variant="flat" startContent={<SignalSlashIcon className="h-4 w-4" />}>
        You're offline
      </Chip>
    </div>
  );
}
