import React, { useState } from 'react';
import { Link, useNavigate, useLocation } from 'react-router';
import { Card, CardBody, CardHeader, Input, Button, Divider } from '@heroui/react';
import { ApiErrorDisplay } from '../components/ApiErrorDisplay';
import { useAuth } from '../hooks/useAuth';

export function LoginPage() {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<Error | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  const { login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  const from = location.state?.from?.pathname || '/';

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setIsLoading(true);

    try {
      await login({ username, password });
      navigate(from, { replace: true });
    } catch (err) {
      setError(err instanceof Error ? err : new Error('Login failed'));
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-background to-content1 flex items-center justify-center p-4">
      <Card className="w-full max-w-md shadow-2xl">
        <CardHeader className="flex flex-col items-center pb-6 pt-8">
          <h1 className="text-4xl font-bold text-primary mb-2">DerpCode</h1>
          <h2 className="text-2xl font-semibold text-foreground mb-2">Sign In</h2>
          <p className="text-default-600 text-center">Welcome back! Please sign in to your account.</p>
        </CardHeader>

        <CardBody className="px-8 pb-8">
          <form onSubmit={handleSubmit} className="space-y-6">
            {error && <ApiErrorDisplay error={error} title="Login Failed" showDetails={true} />}

            <Input
              label="Username"
              value={username}
              onChange={e => setUsername(e.target.value)}
              isRequired
              isDisabled={isLoading}
              placeholder="Enter your username"
              autoComplete="username"
              variant="bordered"
              color="primary"
              size="lg"
            />

            <Input
              label="Password"
              type="password"
              value={password}
              onChange={e => setPassword(e.target.value)}
              isRequired
              isDisabled={isLoading}
              placeholder="Enter your password"
              autoComplete="current-password"
              variant="bordered"
              color="primary"
              size="lg"
            />

            <Button
              type="submit"
              color="primary"
              size="lg"
              className="w-full font-semibold"
              isLoading={isLoading}
              isDisabled={!username || !password}
            >
              {isLoading ? 'Signing In...' : 'Sign In'}
            </Button>
          </form>

          <Divider className="my-6" />

          <div className="text-center">
            <p className="text-default-600">
              Don't have an account?{' '}
              <Link to="/register" className="text-primary hover:text-primary-600 font-medium transition-colors">
                Sign up
              </Link>
            </p>
          </div>
        </CardBody>
      </Card>
    </div>
  );
}
