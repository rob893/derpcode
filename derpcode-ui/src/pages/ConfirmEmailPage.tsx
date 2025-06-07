import { useEffect, useRef, useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router';
import { Card, CardBody, CardHeader, Button, Spinner } from '@heroui/react';
import { CheckCircleIcon, XCircleIcon } from '@heroicons/react/24/outline';
import { authApi } from '../services/auth';
import { ApiErrorDisplay } from '../components/ApiErrorDisplay';

export function ConfirmEmailPage() {
  const [searchParams] = useSearchParams();
  const [status, setStatus] = useState<'loading' | 'success' | 'error'>('loading');
  const [error, setError] = useState<Error | null>(null);
  const hasConfirmed = useRef(false);
  const navigate = useNavigate();

  const email = searchParams.get('email');
  const token = searchParams.get('token');

  useEffect(() => {
    const confirmEmail = async () => {
      if (!email || !token) {
        setError(new Error('Missing email or token in the URL. Please check the confirmation link.'));
        setStatus('error');
        return;
      }

      if (hasConfirmed.current) return;
      hasConfirmed.current = true;

      try {
        await authApi.confirmEmail({ email, token });
        setStatus('success');
      } catch (err) {
        setError(err instanceof Error ? err : new Error('Failed to confirm email. Please try again.'));
        setStatus('error');
      }
    };

    confirmEmail();
  }, [email, token]);

  const handleContinueToProblems = () => {
    navigate('/problems');
  };

  const handleReturnToLogin = () => {
    navigate('/login');
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-background via-content1 to-background flex items-center justify-center p-4">
      <Card className="w-full max-w-md shadow-2xl border-divider">
        <CardHeader className="flex flex-col items-center pb-6 pt-8">
          {/* DerpCode Logo/Title */}
          <div className="flex items-center gap-3 mb-6">
            <img src="/favicon.ico" alt="DerpCode Logo" className="w-10 h-10" />
            <h1 className="text-3xl font-bold text-primary">DerpCode</h1>
          </div>

          {/* Status Icon */}
          <div className="mb-4 p-3 rounded-full bg-content2 border border-divider">
            {status === 'loading' && <Spinner size="lg" color="primary" />}
            {status === 'success' && <CheckCircleIcon className="w-8 h-8 text-success" />}
            {status === 'error' && <XCircleIcon className="w-8 h-8 text-danger" />}
          </div>
        </CardHeader>

        <CardBody className="px-8 pb-8 text-center">
          {status === 'loading' && (
            <div className="space-y-3">
              <h2 className="text-2xl font-bold text-foreground">Confirming Email...</h2>
              <p className="text-default-600 text-lg">Please wait while we confirm your email address.</p>
            </div>
          )}

          {status === 'success' && (
            <div className="space-y-6">
              <div className="space-y-3">
                <h2 className="text-2xl font-bold text-success">Email Confirmed! 🎉</h2>
                <p className="text-default-600 text-lg">
                  Awesome! Your email has been successfully confirmed. You're all set to start derping around with code!
                </p>
              </div>
              <Button
                color="primary"
                size="lg"
                variant="solid"
                className="w-full font-semibold"
                onPress={handleContinueToProblems}
              >
                Continue to Problems
              </Button>
            </div>
          )}

          {status === 'error' && (
            <div className="space-y-6">
              <div className="space-y-3">
                <h2 className="text-2xl font-bold text-danger">Email Confirmation Failed</h2>
                <p className="text-default-600 text-lg">
                  Oops! Something went wrong while confirming your email. This might happen if the link is expired or
                  has already been used.
                </p>
              </div>

              {error && (
                <div className="p-4 bg-danger/10 border border-danger/20 rounded-lg">
                  <ApiErrorDisplay error={error} title="" showDetails={false} />
                </div>
              )}

              <div className="space-y-3">
                <Button
                  color="primary"
                  size="lg"
                  variant="solid"
                  className="w-full font-semibold"
                  onPress={handleReturnToLogin}
                >
                  Return to Login
                </Button>
                <p className="text-sm text-default-500">Need a new confirmation email? Try signing in to resend it.</p>
              </div>
            </div>
          )}
        </CardBody>
      </Card>
    </div>
  );
}
