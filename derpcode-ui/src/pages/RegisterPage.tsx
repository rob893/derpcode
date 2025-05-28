import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router';
import { Card, CardBody, CardHeader, Input, Button, Divider } from '@heroui/react';
import { ApiErrorDisplay } from '../components/ApiErrorDisplay';
import { useAuth } from '../hooks/useAuth';

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
