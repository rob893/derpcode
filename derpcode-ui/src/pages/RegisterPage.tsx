import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router';
import { Card, CardBody, CardHeader, Input, Button, Divider } from '@heroui/react';
import { ApiErrorDisplay } from '../components/ApiErrorDisplay';
import { useAuth } from '../hooks/useAuth';
import { redirectToGitHubOAuth } from '../utils/githubAuthUtils';

export function RegisterPage() {
  const [formData, setFormData] = useState({
    userName: '',
    email: '',
    password: '',
    confirmPassword: ''
  });
  const [error, setError] = useState<Error | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [passwordValidation, setPasswordValidation] = useState({
    isValid: false,
    errors: [] as string[]
  });

  const { register } = useAuth();
  const navigate = useNavigate();

  const validatePassword = (password: string) => {
    const errors: string[] = [];

    if (password.length < 8) {
      errors.push('Password must be at least 8 characters long');
    }

    if (!/\d/.test(password)) {
      errors.push('Password must contain at least 1 number');
    }

    if (!/[^a-zA-Z0-9]/.test(password)) {
      errors.push('Password must contain at least 1 special character');
    }

    return {
      isValid: errors.length === 0,
      errors
    };
  };

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;

    setFormData({
      ...formData,
      [name]: value
    });

    // Validate password when it changes
    if (name === 'password') {
      const validation = validatePassword(value);
      setPasswordValidation(validation);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    // Validate password requirements
    const passwordValidationResult = validatePassword(formData.password);
    if (!passwordValidationResult.isValid) {
      setError(new Error(passwordValidationResult.errors.join(', ')));
      return;
    }

    if (formData.password !== formData.confirmPassword) {
      setError(new Error('Passwords do not match'));
      return;
    }

    setIsLoading(true);

    try {
      await register({
        userName: formData.userName,
        email: formData.email,
        password: formData.password
      });
      navigate('/', { replace: true });
    } catch (err) {
      setError(err instanceof Error ? err : new Error('Registration failed'));
    } finally {
      setIsLoading(false);
    }
  };

  const handleGitHubLogin = () => {
    try {
      redirectToGitHubOAuth();
    } catch (err) {
      setError(err instanceof Error ? err : new Error('GitHub login setup error'));
    }
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-background to-content1 flex items-center justify-center p-4">
      <Card className="w-full max-w-md shadow-2xl">
        <CardHeader className="flex flex-col items-center pb-6 pt-8">
          <h1 className="text-4xl font-bold text-primary mb-2">DerpCode</h1>
          <h2 className="text-2xl font-semibold text-foreground mb-2">Create Account</h2>
          <p className="text-default-600 text-center">Join DerpCode and start solving problems!</p>
        </CardHeader>

        <CardBody className="px-8 pb-8">
          <form onSubmit={handleSubmit} className="space-y-4">
            {error && <ApiErrorDisplay error={error} title="Registration Failed" showDetails={true} />}

            <Input
              label="Username"
              name="userName"
              value={formData.userName}
              onChange={handleChange}
              isRequired
              isDisabled={isLoading}
              placeholder="Choose a username"
              autoComplete="username"
              variant="bordered"
              color="primary"
            />

            <Input
              label="Email"
              name="email"
              type="email"
              value={formData.email}
              onChange={handleChange}
              isRequired
              isDisabled={isLoading}
              placeholder="Enter your email"
              autoComplete="email"
              variant="bordered"
              color="primary"
            />

            <Input
              label="Password"
              name="password"
              type="password"
              value={formData.password}
              onChange={handleChange}
              isRequired
              isDisabled={isLoading}
              placeholder="Choose a password"
              autoComplete="new-password"
              variant="bordered"
              color={formData.password && !passwordValidation.isValid ? 'danger' : 'primary'}
              description="8+ characters, 1+ number, 1+ special character"
              errorMessage={
                formData.password && !passwordValidation.isValid ? passwordValidation.errors.join(', ') : undefined
              }
              isInvalid={formData.password.length > 0 && !passwordValidation.isValid}
            />

            <Input
              label="Confirm Password"
              name="confirmPassword"
              type="password"
              value={formData.confirmPassword}
              onChange={handleChange}
              isRequired
              isDisabled={isLoading}
              placeholder="Confirm your password"
              autoComplete="new-password"
              variant="bordered"
              color={formData.confirmPassword && formData.password !== formData.confirmPassword ? 'danger' : 'primary'}
              errorMessage={
                formData.confirmPassword && formData.password !== formData.confirmPassword
                  ? 'Passwords do not match'
                  : undefined
              }
              isInvalid={formData.confirmPassword.length > 0 && formData.password !== formData.confirmPassword}
            />

            <Button
              type="submit"
              color="primary"
              size="lg"
              className="w-full font-semibold mt-6"
              isLoading={isLoading}
              isDisabled={
                !formData.userName ||
                !formData.email ||
                !formData.password ||
                !formData.confirmPassword ||
                !passwordValidation.isValid ||
                formData.password !== formData.confirmPassword
              }
            >
              {isLoading ? 'Creating Account...' : 'Create Account'}
            </Button>
          </form>

          <Divider className="my-6" />

          <div className="space-y-4">
            <div className="text-center">
              <p className="text-sm text-default-500 mb-4">or you can sign in with</p>
            </div>
            <Button
              onPress={handleGitHubLogin}
              variant="bordered"
              size="lg"
              className="w-full font-semibold"
              isDisabled={isLoading}
              startContent={
                <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                  <path d="M12 0c-6.626 0-12 5.373-12 12 0 5.302 3.438 9.8 8.207 11.387.599.111.793-.261.793-.577v-2.234c-3.338.726-4.033-1.416-4.033-1.416-.546-1.387-1.333-1.756-1.333-1.756-1.089-.745.083-.729.083-.729 1.205.084 1.839 1.237 1.839 1.237 1.07 1.834 2.807 1.304 3.492.997.107-.775.418-1.305.762-1.604-2.665-.305-5.467-1.334-5.467-5.931 0-1.311.469-2.381 1.236-3.221-.124-.303-.535-1.524.117-3.176 0 0 1.008-.322 3.301 1.23.957-.266 1.983-.399 3.003-.404 1.02.005 2.047.138 3.006.404 2.291-1.552 3.297-1.23 3.297-1.23.653 1.653.242 2.874.118 3.176.77.84 1.235 1.911 1.235 3.221 0 4.609-2.807 5.624-5.479 5.921.43.372.823 1.102.823 2.222v3.293c0 .319.192.694.801.576 4.765-1.589 8.199-6.086 8.199-11.386 0-6.627-5.373-12-12-12z" />
                </svg>
              }
            >
              Continue with GitHub
            </Button>
          </div>

          <Divider className="my-6" />

          <div className="text-center">
            <p className="text-default-600">
              Already have an account?{' '}
              <Link to="/login" className="text-primary hover:text-primary-600 font-medium transition-colors">
                Sign in
              </Link>
            </p>
          </div>
        </CardBody>
      </Card>
    </div>
  );
}
