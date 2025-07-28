import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router';
import { Card, CardBody, CardHeader, Input, Button, Chip } from '@heroui/react';
import { CheckCircleIcon } from '@heroicons/react/24/outline';
import { ApiErrorDisplay } from '../components/ApiErrorDisplay';
import { userApi } from '../services/user';

export function ForgotPasswordPage() {
  const [email, setEmail] = useState('');
  const [error, setError] = useState<Error | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [isSubmitted, setIsSubmitted] = useState(false);
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setIsLoading(true);

    try {
      await userApi.forgotPassword({ email });
      setIsSubmitted(true);
    } catch {
      // According to security best practices, we should always show success
      // even if the email doesn't exist to prevent user enumeration
      setIsSubmitted(true);
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
                Email Sent
              </div>
            </Chip>
          </CardHeader>

          <CardBody className="px-8 pb-8 text-center">
            <h2 className="text-2xl font-bold text-foreground mb-4">Check Your Email</h2>
            <p className="text-default-600 mb-6 leading-relaxed">
              If an account with that email address exists, we've sent you a password reset link. Please check your
              email and click the link to reset your password.
            </p>
            <p className="text-sm text-default-500 mb-8">
              Didn't receive an email? Check your spam folder or try again in a few minutes.
            </p>

            <div className="space-y-3">
              <Button
                color="primary"
                size="lg"
                variant="solid"
                className="w-full font-semibold"
                onPress={() => navigate('/login')}
              >
                Back to Sign In
              </Button>
              <Button
                variant="light"
                size="lg"
                className="w-full"
                onPress={() => {
                  setIsSubmitted(false);
                  setEmail('');
                }}
              >
                Try Different Email
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
          <h2 className="text-2xl font-semibold text-foreground mb-2">Forgot Password</h2>
          <p className="text-default-600 text-center">
            Enter your email address and we'll send you a link to reset your password.
          </p>
        </CardHeader>

        <CardBody className="px-8 pb-8">
          <form onSubmit={handleSubmit} className="space-y-6">
            {error && <ApiErrorDisplay error={error} title="Reset Request Failed" showDetails={true} />}

            <Input
              label="Email Address"
              type="email"
              value={email}
              onChange={e => setEmail(e.target.value)}
              isRequired
              isDisabled={isLoading}
              placeholder="Enter your email address"
              autoComplete="email"
              variant="bordered"
              color="primary"
              size="lg"
              description="We'll send a password reset link to this email address"
            />

            <Button
              type="submit"
              color="primary"
              size="lg"
              className="w-full font-semibold"
              isLoading={isLoading}
              isDisabled={!email}
            >
              {isLoading ? 'Sending Reset Link...' : 'Send Reset Link'}
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
