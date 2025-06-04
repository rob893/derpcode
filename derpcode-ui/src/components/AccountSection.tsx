import { useState } from 'react';
import {
  Card,
  CardBody,
  CardHeader,
  Button,
  Chip,
  Modal,
  ModalContent,
  ModalHeader,
  ModalBody,
  ModalFooter,
  useDisclosure
} from '@heroui/react';
import { useDeleteUser, useDeleteLinkedAccount } from '../hooks/useUser';
import type { UserDto } from '../types/user';

interface AccountSectionProps {
  user: UserDto;
}

interface LinkedAccountToUnlink {
  id: string;
  type: string;
}

export function AccountSection({ user }: AccountSectionProps) {
  const deleteUserMutation = useDeleteUser();
  const deleteLinkedAccountMutation = useDeleteLinkedAccount();
  const { isOpen, onOpen, onClose } = useDisclosure();
  const { isOpen: isUnlinkOpen, onOpen: onUnlinkOpen, onClose: onUnlinkClose } = useDisclosure();
  const [isDeleting, setIsDeleting] = useState(false);
  const [isUnlinking, setIsUnlinking] = useState(false);
  const [linkedAccountToUnlink, setLinkedAccountToUnlink] = useState<LinkedAccountToUnlink | null>(null);

  const handleDeleteAccount = async () => {
    try {
      setIsDeleting(true);
      await deleteUserMutation.mutateAsync(user.id);
      onClose();
    } catch (error) {
      console.error('Failed to delete account:', error);
      setIsDeleting(false);
    }
  };

  const handleUnlinkAccount = (linkedAccount: { id: string; linkedAccountType: string }) => {
    setLinkedAccountToUnlink({
      id: linkedAccount.id,
      type: linkedAccount.linkedAccountType
    });
    onUnlinkOpen();
  };

  const handleConfirmUnlink = async () => {
    if (!linkedAccountToUnlink) return;

    try {
      setIsUnlinking(true);
      await deleteLinkedAccountMutation.mutateAsync({
        userId: user.id,
        linkedAccountType: linkedAccountToUnlink.type
      });
      setLinkedAccountToUnlink(null);
      onUnlinkClose();
    } catch (error) {
      console.error('Failed to unlink account:', error);
      setIsUnlinking(false);
    }
  };

  const handleCancelUnlink = () => {
    setLinkedAccountToUnlink(null);
    onUnlinkClose();
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  };

  const getLinkedAccountIcon = (type: string) => {
    switch (type) {
      case 'GitHub':
        return (
          <svg className="w-6 h-6 text-foreground" fill="currentColor" viewBox="0 0 24 24" aria-hidden="true">
            <path d="M12 0c-6.626 0-12 5.373-12 12 0 5.302 3.438 9.8 8.207 11.387.599.111.793-.261.793-.577v-2.234c-3.338.726-4.033-1.416-4.033-1.416-.546-1.387-1.333-1.756-1.333-1.756-1.089-.745.083-.729.083-.729 1.205.084 1.839 1.237 1.839 1.237 1.07 1.834 2.807 1.304 3.492.997.107-.775.418-1.305.762-1.604-2.665-.305-5.467-1.334-5.467-5.931 0-1.311.469-2.381 1.236-3.221-.124-.303-.535-1.524.117-3.176 0 0 1.008-.322 3.301 1.23.957-.266 1.983-.399 3.003-.404 1.02.005 2.047.138 3.006.404 2.291-1.552 3.297-1.23 3.297-1.23.653 1.653.242 2.874.118 3.176.77.84 1.235 1.911 1.235 3.221 0 4.609-2.807 5.624-5.479 5.921.43.372.823 1.102.823 2.222v3.293c0 .319.192.694.801.576 4.765-1.589 8.199-6.086 8.199-11.386 0-6.627-5.373-12-12-12z" />
          </svg>
        );
      case 'Google':
        return (
          <svg className="w-6 h-6" viewBox="0 0 24 24" aria-hidden="true">
            <path
              fill="#4285F4"
              d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z"
            />
            <path
              fill="#34A853"
              d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"
            />
            <path
              fill="#FBBC05"
              d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z"
            />
            <path
              fill="#EA4335"
              d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z"
            />
          </svg>
        );
      default:
        return (
          <svg className="w-6 h-6 text-foreground" fill="currentColor" viewBox="0 0 24 24" aria-hidden="true">
            <path
              fillRule="evenodd"
              d="M19.902 4.098a3.75 3.75 0 00-5.304 0l-4.5 4.5a3.75 3.75 0 001.035 6.037.75.75 0 01-.646 1.353 5.25 5.25 0 01-1.449-8.45l4.5-4.5a5.25 5.25 0 117.424 7.424l-1.757 1.757a.75.75 0 11-1.06-1.06l1.757-1.757a3.75 3.75 0 000-5.304zm-7.804 9.804a3.75 3.75 0 00-1.035-6.037.75.75 0 01.646-1.353 5.25 5.25 0 011.449 8.45l-4.5 4.5a5.25 5.25 0 11-7.424-7.424l1.757-1.757a.75.75 0 111.06 1.06l-1.757 1.757a3.75 3.75 0 105.304 5.304l4.5-4.5z"
              clipRule="evenodd"
            />
          </svg>
        );
    }
  };

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-foreground mb-2">Account Settings</h1>
        <p className="text-default-500">Manage your account information and settings</p>
      </div>

      {/* User Information */}
      <Card>
        <CardHeader>
          <h2 className="text-lg font-semibold">User Information</h2>
        </CardHeader>
        <CardBody className="space-y-4">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="text-sm font-medium text-default-600">Username</label>
              <p className="text-foreground">{user.userName}</p>
            </div>
            <div>
              <label className="text-sm font-medium text-default-600">Email</label>
              <p className="text-foreground">{user.email}</p>
            </div>
            <div>
              <label className="text-sm font-medium text-default-600">Member Since</label>
              <p className="text-foreground">{formatDate(user.created)}</p>
            </div>
          </div>
        </CardBody>
      </Card>

      {/* Linked Accounts */}
      <Card>
        <CardHeader>
          <h2 className="text-lg font-semibold">Linked Accounts</h2>
        </CardHeader>
        <CardBody>
          {user.linkedAccounts.length > 0 ? (
            <div className="space-y-3">
              {user.linkedAccounts.map(account => (
                <div
                  key={account.id}
                  className="flex items-center justify-between p-3 border border-default-200 rounded-lg"
                >
                  <div className="flex items-center gap-3">
                    <div className="flex-shrink-0">{getLinkedAccountIcon(account.linkedAccountType)}</div>
                    <div>
                      <p className="font-medium">{account.linkedAccountType}</p>
                      <p className="text-sm text-default-500">Connected</p>
                    </div>
                  </div>
                  <div className="flex items-center gap-2">
                    <Chip color="success" variant="flat" size="sm">
                      Active
                    </Chip>
                    <Button color="danger" variant="light" size="sm" onPress={() => handleUnlinkAccount(account)}>
                      Unlink
                    </Button>
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <p className="text-default-500">No linked accounts</p>
          )}
        </CardBody>
      </Card>

      {/* Danger Zone */}
      <Card>
        <CardHeader>
          <h2 className="text-lg font-semibold text-danger">Danger Zone</h2>
        </CardHeader>
        <CardBody>
          <div className="flex items-start justify-between">
            <div>
              <h3 className="font-medium text-foreground">Delete Account</h3>
              <p className="text-sm text-default-500 mt-1">
                Permanently delete your account and all associated data. This action cannot be undone.
              </p>
            </div>
            <Button color="danger" variant="bordered" onPress={onOpen} className="ml-4">
              Delete Account
            </Button>
          </div>
        </CardBody>
      </Card>

      {/* Confirmation Modal */}
      <Modal isOpen={isOpen} onClose={onClose} isDismissable={!isDeleting}>
        <ModalContent>
          <ModalHeader>
            <h3 className="text-danger">Confirm Account Deletion</h3>
          </ModalHeader>
          <ModalBody>
            <div className="space-y-4">
              <div className="p-4 bg-danger-50 border border-danger-200 rounded-lg">
                <h4 className="font-semibold text-danger mb-2">⚠️ This action is permanent</h4>
                <p className="text-sm text-danger-700">Deleting your account will permanently remove:</p>
                <ul className="text-sm text-danger-700 mt-2 ml-4 list-disc">
                  <li>Your profile and account information</li>
                  <li>All your problem submissions and progress</li>
                  <li>Any comments or contributions you've made</li>
                  <li>All linked social accounts</li>
                </ul>
              </div>
              <p className="text-foreground">
                Are you absolutely sure you want to delete your account? This cannot be undone.
              </p>
            </div>
          </ModalBody>
          <ModalFooter>
            <Button variant="light" onPress={onClose} isDisabled={isDeleting}>
              Cancel
            </Button>
            <Button color="danger" onPress={handleDeleteAccount} isLoading={isDeleting}>
              {isDeleting ? 'Deleting...' : 'Delete My Account'}
            </Button>
          </ModalFooter>
        </ModalContent>
      </Modal>

      {/* Unlink Account Confirmation Modal */}
      <Modal isOpen={isUnlinkOpen} onClose={onUnlinkClose} isDismissable={!isUnlinking}>
        <ModalContent>
          <ModalHeader>
            <h3 className="text-warning">Confirm Account Unlink</h3>
          </ModalHeader>
          <ModalBody>
            <div className="space-y-4">
              <div className="p-4 bg-warning-50 border border-warning-200 rounded-lg">
                <h4 className="font-semibold text-warning mb-2">⚠️ Are you sure?</h4>
                <p className="text-sm text-warning-700">
                  You are about to unlink your {linkedAccountToUnlink?.type} account.
                </p>
              </div>
              <p className="text-foreground">
                You can link this account again later if needed. This action will not affect your main account.
              </p>
            </div>
          </ModalBody>
          <ModalFooter>
            <Button variant="light" onPress={handleCancelUnlink} isDisabled={isUnlinking}>
              Cancel
            </Button>
            <Button color="warning" onPress={handleConfirmUnlink} isLoading={isUnlinking}>
              {isUnlinking ? 'Unlinking...' : 'Unlink Account'}
            </Button>
          </ModalFooter>
        </ModalContent>
      </Modal>
    </div>
  );
}
