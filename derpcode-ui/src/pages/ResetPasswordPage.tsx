import React, { useState, useEffect } from 'react';
import { useNavigate, useSearchParams, Link } from 'react-router';
import { Card, CardBody, CardHeader, Input, Button, Chip } from '@heroui/react';
import { CheckCircleIcon, XCircleIcon } from '@heroicons/react/24/outline';
import { ApiErrorDisplay } from '../components/ApiErrorDisplay';
import {
  validatePassword,
  getPasswordRequirementsDescription,
  type PasswordValidationResult
} from '../utils/passwordValidation';
import { userApi } from '../services/user';

export function ResetPasswordPage() {
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [error, setError] = useState<Error | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [isSubmitted, setIsSubmitted] = useState(false);
  const [passwordValidation, setPasswordValidation] = useState<PasswordValidationResult>({
    isValid: false,
    errors: []
  });

  const [searchParams] = useSearchParams();
  const navigate = useNavigate();

  const token = searchParams.get('token');
  const email = searchParams.get('email');

  useEffect(() => {
    if (password) {
      const validation = validatePassword(password);
      setPasswordValidation(validation);
    }
  }, [password]);

  // Redirect if required parameters are missing
  useEffect(() => {
    if (!token || !email) {
      navigate('/forgot-password', { replace: true });
    }
  }, [token, email, navigate]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    // Validate password requirements
    const passwordValidationResult = validatePassword(password);
    if (!passwordValidationResult.isValid) {
      setError(new Error(passwordValidationResult.errors.join(', ')));
      return;
    }

    if (password !== confirmPassword) {
      setError(new Error('Passwords do not match'));
      return;
    }

    if (!token || !email) {
      setError(new Error('Invalid reset link'));
      return;
    }

    setIsLoading(true);

    try {
      await userApi.resetPassword({
        email,
        token,
        password
      });
      setIsSubmitted(true);
    } catch {
      // Show generic error message for security
      setError(new Error('Failed to reset password. The reset link may be invalid or expired.'));
    } finally {
      setIsLoading(false);
    }
  };

  if (isSubmitted) {
    return (
      <div className="min-h-screen bg-linear-to-br from-background to-content1 flex items-center justify-center p-4">
        <Card className="w-full max-w-md shadow-2xl">
          <CardHeader className="flex flex-col items-center pb-6 pt-8">
            <div className="flex items-center gap-3 mb-6">
              <img src="/favicon.ico" alt="DerpCode Logo" className="w-10 h-10" />
              <h1 className="text-3xl font-bold text-primary">DerpCode</h1>
            </div>
            <Chip color="success" variant="flat" size="lg" className="font-medium mb-2">
              <div className="flex items-center gap-2">
                <CheckCircleIcon className="w-5 h-5" />
                Password Reset Successful
              </div>
            </Chip>
          </CardHeader>

          <CardBody className="px-8 pb-8 text-center">
            <h2 className="text-2xl font-bold text-foreground mb-4">Password Updated!</h2>
            <p className="text-default-600 mb-8 leading-relaxed">
              Your password has been successfully updated. You can now sign in with your new password.
            </p>

            <Button
              color="primary"
              size="lg"
              variant="solid"
              className="w-full font-semibold"
              onPress={() => navigate('/login')}
            >
              Sign In
            </Button>
          </CardBody>
        </Card>
      </div>
    );
  }

  // Show error state if required parameters are missing
  if (!token || !email) {
    return (
      <div className="min-h-screen bg-linear-to-br from-background to-content1 flex items-center justify-center p-4">
        <Card className="w-full max-w-md shadow-2xl">
          <CardHeader className="flex flex-col items-center pb-6 pt-8">
            <div className="flex items-center gap-3 mb-6">
              <img src="/favicon.ico" alt="DerpCode Logo" className="w-10 h-10" />
              <h1 className="text-3xl font-bold text-primary">DerpCode</h1>
            </div>
            <Chip color="danger" variant="flat" size="lg" className="font-medium mb-2">
              <div className="flex items-center gap-2">
                <XCircleIcon className="w-5 h-5" />
                Invalid Reset Link
              </div>
            </Chip>
          </CardHeader>

          <CardBody className="px-8 pb-8 text-center">
            <h2 className="text-2xl font-bold text-foreground mb-4">Link Invalid or Expired</h2>
            <p className="text-default-600 mb-8 leading-relaxed">
              This password reset link is invalid or has expired. Please request a new password reset link.
            </p>

            <div className="space-y-3">
              <Button
                color="primary"
                size="lg"
                variant="solid"
                className="w-full font-semibold"
                onPress={() => navigate('/forgot-password')}
              >
                Request New Reset Link
              </Button>
              <Button variant="light" size="lg" className="w-full" onPress={() => navigate('/login')}>
                Back to Sign In
              </Button>
            </div>
          </CardBody>
        </Card>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-linear-to-br from-background to-content1 flex items-center justify-center p-4">
      <Card className="w-full max-w-md shadow-2xl">
        <CardHeader className="flex flex-col items-center pb-6 pt-8">
          <div className="flex items-center gap-3 mb-6">
            <img src="/favicon.ico" alt="DerpCode Logo" className="w-10 h-10" />
            <h1 className="text-3xl font-bold text-primary">DerpCode</h1>
          </div>
          <h2 className="text-2xl font-semibold text-foreground mb-2">Reset Password</h2>
          <p className="text-default-600 text-center">Enter your new password below.</p>
        </CardHeader>

        <CardBody className="px-8 pb-8">
          <form onSubmit={handleSubmit} className="space-y-4">
            {error && <ApiErrorDisplay error={error} title="Password Reset Failed" showDetails={true} />}

            <div className="mb-4 p-3 bg-default-50 rounded-lg border border-default-200">
              <p className="text-sm text-default-600">
                <strong>Resetting password for:</strong> {email}
              </p>
            </div>

            <Input
              label="New Password"
              name="password"
              type="password"
              value={password}
              onChange={e => setPassword(e.target.value)}
              isRequired
              isDisabled={isLoading}
              placeholder="Enter your new password"
              autoComplete="new-password"
              variant="bordered"
              color={password && !passwordValidation.isValid ? 'danger' : 'primary'}
              description={getPasswordRequirementsDescription()}
              errorMessage={password && !passwordValidation.isValid ? passwordValidation.errors.join(', ') : undefined}
              isInvalid={password.length > 0 && !passwordValidation.isValid}
            />

            <Input
              label="Confirm New Password"
              name="confirmPassword"
              type="password"
              value={confirmPassword}
              onChange={e => setConfirmPassword(e.target.value)}
              isRequired
              isDisabled={isLoading}
              placeholder="Confirm your new password"
              autoComplete="new-password"
              variant="bordered"
              color={confirmPassword && password !== confirmPassword ? 'danger' : 'primary'}
              errorMessage={confirmPassword && password !== confirmPassword ? 'Passwords do not match' : undefined}
              isInvalid={confirmPassword.length > 0 && password !== confirmPassword}
            />

            <Button
              type="submit"
              color="primary"
              size="lg"
              className="w-full font-semibold mt-6"
              isLoading={isLoading}
              isDisabled={!password || !confirmPassword || !passwordValidation.isValid || password !== confirmPassword}
            >
              {isLoading ? 'Updating Password...' : 'Update Password'}
            </Button>
          </form>

          <div className="mt-6 text-center">
            <p className="text-default-600">
              Remember your password?{' '}
              <Link to="/login" className="text-primary hover:text-primary-600 font-medium transition-colors">
                Back to Sign In
              </Link>
            </p>
          </div>
        </CardBody>
      </Card>
    </div>
  );
}
