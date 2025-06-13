import { Modal, ModalContent, ModalHeader, ModalBody, ModalFooter, Button } from '@heroui/react';

interface ProblemModalsProps {
  // Login Modal
  isLoginOpen: boolean;
  onLoginOpenChange: () => void;
  onNavigateToLogin: () => void;
  onNavigateToRegister: () => void;

  // Delete Modal
  isDeleteOpen: boolean;
  onDeleteOpenChange: () => void;
  problemToDelete: { id: number; name: string } | null;
  onConfirmDelete: () => void;
  onCancelDelete: () => void;
  isDeleting: boolean;

  // Reset Code Modal
  isResetOpen: boolean;
  onResetOpenChange: () => void;
  problemName: string;
  onConfirmReset: () => void;
}

export const ProblemModals = ({
  isLoginOpen,
  onLoginOpenChange,
  onNavigateToLogin,
  onNavigateToRegister,
  isDeleteOpen,
  onDeleteOpenChange,
  problemToDelete,
  onConfirmDelete,
  onCancelDelete,
  isDeleting,
  isResetOpen,
  onResetOpenChange,
  problemName,
  onConfirmReset
}: ProblemModalsProps) => {
  return (
    <>
      {/* Login Modal */}
      <Modal isOpen={isLoginOpen} onOpenChange={onLoginOpenChange} placement="center">
        <ModalContent>
          {onClose => (
            <>
              <ModalHeader className="flex flex-col gap-1">
                <h2 className="text-2xl font-bold text-primary">LOL!!!</h2>
              </ModalHeader>
              <ModalBody>
                <p className="text-default-600">
                  You thought I'd let you use my compute resources without signing in?
                  <br />
                  <br />
                  Either login, sign up, or gtfo 🖕
                  <br />
                  <br />
                  (Your code is saved locally)
                </p>
              </ModalBody>
              <ModalFooter>
                <Button color="default" variant="light" onPress={onClose}>
                  Cancel
                </Button>
                <Button
                  color="primary"
                  variant="ghost"
                  onPress={() => {
                    onClose();
                    onNavigateToLogin();
                  }}
                >
                  Sign In
                </Button>
                <Button
                  color="primary"
                  onPress={() => {
                    onClose();
                    onNavigateToRegister();
                  }}
                >
                  Sign Up
                </Button>
              </ModalFooter>
            </>
          )}
        </ModalContent>
      </Modal>

      {/* Delete Confirmation Modal */}
      <Modal isOpen={isDeleteOpen} onOpenChange={onDeleteOpenChange} placement="center">
        <ModalContent>
          {() => (
            <>
              <ModalHeader className="flex flex-col gap-1">
                <h2 className="text-xl font-bold text-danger">Delete Problem</h2>
              </ModalHeader>
              <ModalBody>
                <p className="text-foreground">
                  Are you sure you want to delete the problem "{problemToDelete?.name}"?
                </p>
                <p className="text-warning text-sm mt-2">
                  This action cannot be undone. All associated data will be permanently removed.
                </p>
              </ModalBody>
              <ModalFooter>
                <Button color="default" variant="light" onPress={onCancelDelete} isDisabled={isDeleting}>
                  Cancel
                </Button>
                <Button color="danger" onPress={onConfirmDelete} isLoading={isDeleting}>
                  {isDeleting ? 'Deleting...' : 'Delete'}
                </Button>
              </ModalFooter>
            </>
          )}
        </ModalContent>
      </Modal>

      {/* Reset Code Confirmation Modal */}
      <Modal isOpen={isResetOpen} onOpenChange={onResetOpenChange} placement="center">
        <ModalContent>
          {() => (
            <>
              <ModalHeader className="flex flex-col gap-1">
                <h2 className="text-xl font-bold text-danger">Reset Code</h2>
              </ModalHeader>
              <ModalBody>
                <p className="text-foreground">
                  Are you sure you want to reset the code for the problem "{problemName}"?
                </p>
                <p className="text-warning text-sm mt-2">This action will discard your current code.</p>
              </ModalBody>
              <ModalFooter>
                <Button color="default" variant="light" onPress={onResetOpenChange}>
                  Cancel
                </Button>
                <Button color="danger" onPress={onConfirmReset}>
                  Reset Code
                </Button>
              </ModalFooter>
            </>
          )}
        </ModalContent>
      </Modal>
    </>
  );
};
