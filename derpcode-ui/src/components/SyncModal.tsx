import { useState } from 'react';
import { Modal, ModalContent, ModalHeader, ModalBody, ModalFooter, Button, Link } from '@heroui/react';
import { problemsApi } from '../services/api';

interface SyncModalProps {
  isOpen: boolean;
  onOpenChange: (open: boolean) => void;
}

export function SyncModal({ isOpen, onOpenChange }: SyncModalProps) {
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [prUrl, setPrUrl] = useState<string | null>(null);

  const handleSync = async () => {
    setIsLoading(true);
    setError(null);
    try {
      const result = await problemsApi.syncProblemsToGitHub();
      setPrUrl(result.prUrl);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'An error occurred while syncing problems');
    } finally {
      setIsLoading(false);
    }
  };

  const handleClose = () => {
    setPrUrl(null);
    setError(null);
    onOpenChange(false);
  };

  return (
    <Modal isOpen={isOpen} onOpenChange={onOpenChange} placement="center">
      <ModalContent>
        {onClose => (
          <>
            <ModalHeader className="flex flex-col gap-1">
              <h2 className="text-2xl font-bold text-primary">
                {prUrl ? 'Sync Complete!' : 'Sync Problems to GitHub'}
              </h2>
            </ModalHeader>
            <ModalBody>
              {prUrl ? (
                <div className="space-y-4">
                  <p className="text-default-600">Problems have been successfully synced to GitHub! 🎉</p>
                  <div className="p-3 bg-success-50 border border-success-200 rounded-lg">
                    <p className="text-sm text-success-700 mb-2">Pull Request Created:</p>
                    <Link href={prUrl} isExternal showAnchorIcon className="text-success-600 hover:text-success-800">
                      {prUrl}
                    </Link>
                  </div>
                </div>
              ) : error ? (
                <div className="space-y-4">
                  <p className="text-danger">Oops! Something went wrong while syncing problems:</p>
                  <div className="p-3 bg-danger-50 border border-danger-200 rounded-lg">
                    <p className="text-sm text-danger-700">{error}</p>
                  </div>
                </div>
              ) : (
                <div className="space-y-4">
                  <p className="text-default-600">
                    This will sync all problems from the database to GitHub and create a pull request.
                  </p>
                  <p className="text-warning text-sm">
                    ⚠️ This action will modify the GitHub repository. Are you sure you want to continue?
                  </p>
                </div>
              )}
            </ModalBody>
            <ModalFooter>
              {prUrl ? (
                <Button color="primary" onPress={handleClose}>
                  Close
                </Button>
              ) : (
                <>
                  <Button color="default" variant="light" onPress={onClose} disabled={isLoading}>
                    Cancel
                  </Button>
                  <Button color="primary" onPress={handleSync} isLoading={isLoading} disabled={error !== null}>
                    {error ? 'Retry' : 'Sync Problems'}
                  </Button>
                </>
              )}
            </ModalFooter>
          </>
        )}
      </ModalContent>
    </Modal>
  );
}
