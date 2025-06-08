import { useState, useEffect } from 'react';
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
  useDisclosure,
  Input
} from '@heroui/react';
import { CheckCircleIcon, ExclamationTriangleIcon, PencilIcon } from '@heroicons/react/24/outline';
import { useNavigate } from 'react-router';
import { useDeleteUser, useDeleteLinkedAccount, useUpdatePassword, useUpdateUsername } from '../hooks/useUser';
import { useAuth } from '../hooks/useAuth';
import {
  validatePassword,
  getPasswordRequirementsDescription,
  type PasswordValidationResult
} from '../utils/passwordValidation';
import type { UserDto } from '../types/user';
import { ApiErrorDisplay } from './ApiErrorDisplay';
import { userApi } from '../services/user';
import { daysSinceDate, isWithinTimeLimit } from '../utils/dateTimeUtils';

interface AccountSectionProps {
  user: UserDto;
}

interface LinkedAccountToUnlink {
  id: string;
  type: string;
}

const DAYS_TO_WAIT_FOR_USERNAME_CHANGE = 30;
const MINUTES_SINCE_LOGIN_TO_WAIT_FOR_USERNAME_CHANGE = 15;

export function AccountSection({ user }: AccountSectionProps) {
  const { logout } = useAuth();
  const deleteUserMutation = useDeleteUser();
  const deleteLinkedAccountMutation = useDeleteLinkedAccount();
  const updatePasswordMutation = useUpdatePassword();
  const updateUsernameMutation = useUpdateUsername();
  const navigate = useNavigate();
  const { isOpen, onOpen, onClose } = useDisclosure();
  const { isOpen: isUnlinkOpen, onOpen: onUnlinkOpen, onClose: onUnlinkClose } = useDisclosure();
  const { isOpen: isPasswordOpen, onOpen: onPasswordOpen, onClose: onPasswordClose } = useDisclosure();
  const { isOpen: isUsernameOpen, onOpen: onUsernameOpen, onClose: onUsernameClose } = useDisclosure();
  const [isDeleting, setIsDeleting] = useState(false);
  const [isUnlinking, setIsUnlinking] = useState(false);
  const [isUpdatingPassword, setIsUpdatingPassword] = useState(false);
  const [isUpdatingUsername, setIsUpdatingUsername] = useState(false);
  const [linkedAccountToUnlink, setLinkedAccountToUnlink] = useState<LinkedAccountToUnlink | null>(null);
  const [passwordForm, setPasswordForm] = useState({
    oldPassword: '',
    newPassword: '',
    confirmPassword: ''
  });
  const [usernameForm, setUsernameForm] = useState({
    newUsername: ''
  });
  const [passwordError, setPasswordError] = useState<Error | null>(null);
  const [usernameError, setUsernameError] = useState<Error | null>(null);
  const [deleteError, setDeleteError] = useState<Error | null>(null);
  const [unlinkError, setUnlinkError] = useState<Error | null>(null);
  const [passwordValidation, setPasswordValidation] = useState<PasswordValidationResult>({
    isValid: false,
    errors: []
  });
  const [isResendingConfirmation, setIsResendingConfirmation] = useState(false);
  const [confirmationResent, setConfirmationResent] = useState(false);
  const [confirmationError, setConfirmationError] = useState<Error | null>(null);

  // Validate password when it changes
  useEffect(() => {
    if (passwordForm.newPassword) {
      const validation = validatePassword(passwordForm.newPassword);
      setPasswordValidation(validation);
    } else {
      setPasswordValidation({ isValid: false, errors: [] });
    }
  }, [passwordForm.newPassword]);

  const handleDeleteAccount = async () => {
    try {
      setIsDeleting(true);
      setDeleteError(null);
      await deleteUserMutation.mutateAsync(user.id);
      onClose();
    } catch (error) {
      console.error('Failed to delete account:', error);
      setDeleteError(error as Error);
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
      setUnlinkError(null);
      await deleteLinkedAccountMutation.mutateAsync({
        userId: user.id,
        linkedAccountType: linkedAccountToUnlink.type
      });
      setLinkedAccountToUnlink(null);
      onUnlinkClose();
    } catch (error) {
      console.error('Failed to unlink account:', error);
      setUnlinkError(error as Error);
      setIsUnlinking(false);
    }
  };

  const handleCancelUnlink = () => {
    setLinkedAccountToUnlink(null);
    onUnlinkClose();
  };

  const handleUpdatePassword = async () => {
    setPasswordError(null);

    // Validate form
    const errors: string[] = [];
    if (!passwordForm.oldPassword) {
      errors.push('Current password is required');
    }
    if (!passwordForm.newPassword) {
      errors.push('New password is required');
    } else {
      // Use the password validation utility
      const passwordValidationResult = validatePassword(passwordForm.newPassword);
      if (!passwordValidationResult.isValid) {
        errors.push(...passwordValidationResult.errors);
      }
    }
    if (passwordForm.newPassword !== passwordForm.confirmPassword) {
      errors.push('New passwords do not match');
    }

    if (errors.length > 0) {
      setPasswordError(new Error(errors.join('. ')));
      return;
    }

    try {
      setIsUpdatingPassword(true);
      await updatePasswordMutation.mutateAsync({
        userId: user.id,
        request: {
          oldPassword: passwordForm.oldPassword,
          newPassword: passwordForm.newPassword
        }
      });

      // Reset form and close modal on success
      setPasswordForm({ oldPassword: '', newPassword: '', confirmPassword: '' });
      onPasswordClose();
    } catch (error: any) {
      setPasswordError(error as Error);
    } finally {
      setIsUpdatingPassword(false);
    }
  };

  const handlePasswordCancel = () => {
    setPasswordForm({ oldPassword: '', newPassword: '', confirmPassword: '' });
    setPasswordError(null);
    onPasswordClose();
  };

  const handleUpdateUsername = async () => {
    setUsernameError(null);

    // Validate form
    const errors: string[] = [];
    if (!usernameForm.newUsername) {
      errors.push('New username is required');
    } else if (usernameForm.newUsername.trim().length < 3) {
      errors.push('Username must be at least 3 characters long');
    } else if (usernameForm.newUsername === user.userName) {
      errors.push('New username must be different from current username');
    }

    if (errors.length > 0) {
      setUsernameError(new Error(errors.join('. ')));
      return;
    }

    try {
      setIsUpdatingUsername(true);
      await updateUsernameMutation.mutateAsync({
        userId: user.id,
        request: {
          newUsername: usernameForm.newUsername.trim()
        }
      });

      // Reset form and close modal on success
      setUsernameForm({ newUsername: '' });
      onUsernameClose();
    } catch (error: any) {
      setUsernameError(error as Error);
    } finally {
      setIsUpdatingUsername(false);
    }
  };

  const handleUsernameCancel = () => {
    setUsernameForm({ newUsername: '' });
    setUsernameError(null);
    onUsernameClose();
  };

  const isPasswordFormValid = () => {
    const hasOldPassword = passwordForm.oldPassword.trim().length > 0;
    const hasNewPassword = passwordForm.newPassword.trim().length > 0;
    const hasConfirmPassword = passwordForm.confirmPassword.trim().length > 0;
    const isNewPasswordValid = passwordValidation.isValid;
    const passwordsMatch = passwordForm.newPassword === passwordForm.confirmPassword;

    return hasOldPassword && hasNewPassword && hasConfirmPassword && isNewPasswordValid && passwordsMatch;
  };

  const handleResendConfirmation = async () => {
    try {
      setIsResendingConfirmation(true);
      setConfirmationError(null);
      await userApi.resendEmailConfirmation(user.id);
      setConfirmationResent(true);
      // Clear the success message after 5 seconds
      setTimeout(() => setConfirmationResent(false), 5000);
    } catch (error) {
      console.error('Failed to resend confirmation email:', error);
      setConfirmationError(error as Error);
    } finally {
      setIsResendingConfirmation(false);
    }
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
          {/* Email confirmation warning - only show if email is not verified */}
          {!user.emailConfirmed && (
            <div className="bg-warning-50 border border-warning-200 rounded-lg p-4 flex items-start gap-3">
              <ExclamationTriangleIcon className="w-5 h-5 text-warning-600 mt-0.5 flex-shrink-0" />
              <div className="flex-1">
                <p className="text-warning-800 font-medium text-sm">Email not verified</p>
                <p className="text-warning-700 text-sm mt-1">
                  You won't be able to recover your account without a verified email address.
                </p>
                <div className="flex items-center gap-2 mt-3">
                  <Button
                    size="sm"
                    color="warning"
                    variant="flat"
                    onPress={handleResendConfirmation}
                    isLoading={isResendingConfirmation}
                  >
                    Resend Confirmation Email
                  </Button>
                  {confirmationResent && (
                    <Chip size="sm" color="success" variant="flat">
                      Email sent!
                    </Chip>
                  )}
                </div>
                {confirmationError && (
                  <div className="mt-2">
                    <ApiErrorDisplay error={confirmationError} />
                  </div>
                )}
              </div>
            </div>
          )}

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="text-sm font-medium text-default-600">Username</label>
              <div className="flex items-center gap-2">
                <p className="text-foreground">{user.userName}</p>
                <Button
                  color="primary"
                  variant="light"
                  size="sm"
                  isIconOnly
                  onPress={onUsernameOpen}
                  className="p-1"
                  aria-label="Change username"
                >
                  <PencilIcon className="w-4 h-4" />
                </Button>
              </div>
            </div>
            <div>
              <label className="text-sm font-medium text-default-600">Email</label>
              <div className="flex items-center gap-2">
                <p className="text-foreground">{user.email}</p>
                {user.emailConfirmed ? (
                  <div className="flex items-center gap-1">
                    <CheckCircleIcon className="w-4 h-4 text-success-600" />
                    <span className="text-success-600 text-xs font-medium">Verified</span>
                  </div>
                ) : (
                  <div className="flex items-center gap-1">
                    <ExclamationTriangleIcon className="w-4 h-4 text-warning-600" />
                    <span className="text-warning-600 text-xs font-medium">Not verified</span>
                  </div>
                )}
              </div>
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

      {/* Password Update */}
      <Card>
        <CardHeader>
          <h2 className="text-lg font-semibold">Password</h2>
        </CardHeader>
        <CardBody>
          <div className="flex items-start justify-between">
            <div>
              <h3 className="font-medium text-foreground">Update Password</h3>
              <p className="text-sm text-default-500 mt-1">Change your account password to keep your account secure.</p>
            </div>
            <div className="flex flex-col gap-2 ml-4">
              <Button color="primary" variant="bordered" onPress={onPasswordOpen}>
                Change Password
              </Button>
            </div>
          </div>
          <br />
          <div className="flex items-start justify-between">
            <div>
              <h3 className="font-medium text-foreground">Reset Password</h3>
              {!user.emailConfirmed ? (
                <div className="mt-2 p-2 bg-warning-50 border border-warning-200 rounded-md">
                  <div className="flex items-start gap-2">
                    <ExclamationTriangleIcon className="w-4 h-4 text-warning-600 mt-0.5 flex-shrink-0" />
                    <p className="text-warning-700 text-xs">Email verification required to reset password</p>
                  </div>
                </div>
              ) : (
                <p className="text-sm text-default-500 mt-1">Forgot your password? No problem!</p>
              )}
            </div>
            <div className="flex flex-col gap-2 ml-4">
              <Button
                color="secondary"
                variant="bordered"
                onPress={() => navigate('/forgot-password')}
                isDisabled={!user.emailConfirmed}
              >
                Reset Password
              </Button>
            </div>
          </div>
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
              {deleteError && (
                <ApiErrorDisplay error={deleteError} title="Account Deletion Failed" showDetails={true} />
              )}
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
              {unlinkError && <ApiErrorDisplay error={unlinkError} title="Account Unlink Failed" showDetails={true} />}
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

      {/* Password Update Modal */}
      <Modal isOpen={isPasswordOpen} onClose={onPasswordClose} isDismissable={!isUpdatingPassword}>
        <ModalContent>
          <ModalHeader>
            <h3 className="text-foreground">Change Password</h3>
          </ModalHeader>
          <ModalBody>
            <div className="space-y-4">
              {passwordError && (
                <ApiErrorDisplay error={passwordError} title="Password Update Failed" showDetails={true} />
              )}

              <Input
                label="Current Password"
                type="password"
                value={passwordForm.oldPassword}
                onChange={e => setPasswordForm(prev => ({ ...prev, oldPassword: e.target.value }))}
                isRequired
                isDisabled={isUpdatingPassword}
              />

              <Input
                label="New Password"
                type="password"
                value={passwordForm.newPassword}
                onChange={e => setPasswordForm(prev => ({ ...prev, newPassword: e.target.value }))}
                isRequired
                isDisabled={isUpdatingPassword}
                description={getPasswordRequirementsDescription()}
                errorMessage={
                  passwordForm.newPassword && !passwordValidation.isValid
                    ? passwordValidation.errors.join(', ')
                    : undefined
                }
                isInvalid={passwordForm.newPassword.length > 0 && !passwordValidation.isValid}
              />

              <Input
                label="Confirm New Password"
                type="password"
                value={passwordForm.confirmPassword}
                onChange={e => setPasswordForm(prev => ({ ...prev, confirmPassword: e.target.value }))}
                isRequired
                isDisabled={isUpdatingPassword}
                errorMessage={
                  passwordForm.confirmPassword && passwordForm.newPassword !== passwordForm.confirmPassword
                    ? 'Passwords do not match'
                    : undefined
                }
                isInvalid={
                  passwordForm.confirmPassword.length > 0 && passwordForm.newPassword !== passwordForm.confirmPassword
                }
              />
            </div>
          </ModalBody>
          <ModalFooter>
            <Button variant="light" onPress={handlePasswordCancel} isDisabled={isUpdatingPassword}>
              Cancel
            </Button>
            <Button
              color="primary"
              onPress={handleUpdatePassword}
              isLoading={isUpdatingPassword}
              isDisabled={isUpdatingPassword || !isPasswordFormValid()}
            >
              {isUpdatingPassword ? 'Updating...' : 'Update Password'}
            </Button>
          </ModalFooter>
        </ModalContent>
      </Modal>

      {/* Username Update Modal */}
      <Modal isOpen={isUsernameOpen} onClose={onUsernameClose} isDismissable={!isUpdatingUsername}>
        <ModalContent>
          <ModalHeader>
            <h3 className="text-foreground">Change Username</h3>
          </ModalHeader>
          <ModalBody>
            <div className="space-y-4">
              {usernameError && (
                <ApiErrorDisplay error={usernameError} title="Username Update Failed" showDetails={true} />
              )}

              {/* Conditional 30-day restriction warning */}
              {daysSinceDate(user.lastUsernameChange) < DAYS_TO_WAIT_FOR_USERNAME_CHANGE && (
                <div className="p-4 bg-danger-50 border border-danger-200 rounded-lg">
                  <h4 className="font-semibold text-danger mb-2">🚫 Username Change Restricted</h4>
                  <p className="text-sm text-danger-700 mb-2">
                    You changed your username {daysSinceDate(user.lastUsernameChange)} days ago.
                  </p>
                  <p className="text-sm text-danger-700">
                    You must wait {DAYS_TO_WAIT_FOR_USERNAME_CHANGE - daysSinceDate(user.lastUsernameChange)} more days
                    before changing your username again.
                  </p>
                </div>
              )}

              {/* Show general warning if user is eligible to change username */}
              {daysSinceDate(user.lastUsernameChange) >= DAYS_TO_WAIT_FOR_USERNAME_CHANGE && (
                <div className="p-4 bg-warning-50 border border-warning-200 rounded-lg">
                  <h4 className="font-semibold text-warning mb-2">⚠️ Important</h4>
                  <p className="text-sm text-warning-700 mb-2">
                    Username changes are limited to once every {DAYS_TO_WAIT_FOR_USERNAME_CHANGE} days. Choose wisely!
                  </p>
                  <p className="text-sm text-warning-700">
                    Changing your username will affect how you appear in any existing links to your profile.
                  </p>
                </div>
              )}

              {/* Conditional authentication warning */}
              {!isWithinTimeLimit(user.lastLogin, MINUTES_SINCE_LOGIN_TO_WAIT_FOR_USERNAME_CHANGE) && (
                <div className="p-4 bg-warning-50 border border-warning-200 rounded-lg">
                  <h4 className="font-semibold text-warning mb-2">🔐 Security Notice</h4>
                  <p className="text-sm text-warning-700 mb-2">
                    For your security, you must have authenticated within the last{' '}
                    {MINUTES_SINCE_LOGIN_TO_WAIT_FOR_USERNAME_CHANGE} minutes to change your username.
                  </p>
                  <p className="text-sm text-warning-700 mb-2">
                    Please sign out and sign back in again before changing your username.
                  </p>
                  <Button
                    color="primary"
                    onPress={async () => {
                      try {
                        await logout();
                        navigate('/login');
                      } catch (error) {
                        console.error('Logout failed:', error);
                      }
                    }}
                  >
                    Sign Out
                  </Button>
                </div>
              )}

              <Input
                label="New Username"
                type="text"
                value={usernameForm.newUsername}
                onChange={e => setUsernameForm(prev => ({ ...prev, newUsername: e.target.value }))}
                isRequired
                isDisabled={
                  isUpdatingUsername ||
                  daysSinceDate(user.lastUsernameChange) < DAYS_TO_WAIT_FOR_USERNAME_CHANGE ||
                  !isWithinTimeLimit(user.lastLogin, MINUTES_SINCE_LOGIN_TO_WAIT_FOR_USERNAME_CHANGE)
                }
                description="Username must be at least 3 characters long"
                errorMessage={
                  usernameForm.newUsername && usernameForm.newUsername.trim().length < 3
                    ? 'Username must be at least 3 characters long'
                    : usernameForm.newUsername === user.userName
                      ? 'New username must be different from current username'
                      : undefined
                }
                isInvalid={
                  (usernameForm.newUsername.length > 0 && usernameForm.newUsername.trim().length < 3) ||
                  usernameForm.newUsername === user.userName
                }
              />
            </div>
          </ModalBody>
          <ModalFooter>
            <Button variant="light" onPress={handleUsernameCancel} isDisabled={isUpdatingUsername}>
              Cancel
            </Button>
            <Button
              color="primary"
              onPress={handleUpdateUsername}
              isLoading={isUpdatingUsername}
              isDisabled={
                isUpdatingUsername ||
                !usernameForm.newUsername ||
                usernameForm.newUsername.trim().length < 3 ||
                usernameForm.newUsername === user.userName ||
                daysSinceDate(user.lastUsernameChange) < DAYS_TO_WAIT_FOR_USERNAME_CHANGE ||
                !isWithinTimeLimit(user.lastLogin, MINUTES_SINCE_LOGIN_TO_WAIT_FOR_USERNAME_CHANGE)
              }
            >
              {isUpdatingUsername ? 'Updating...' : 'Update Username'}
            </Button>
          </ModalFooter>
        </ModalContent>
      </Modal>
    </div>
  );
}
