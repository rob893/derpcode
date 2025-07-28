import { useEffect, useState, useRef } from 'react';
import { useNavigate } from 'react-router';
import { Card, CardBody, CardHeader, Button, Spinner, Chip } from '@heroui/react';
import { CheckCircleIcon, XCircleIcon } from '@heroicons/react/24/outline';
import { useAuth } from '../hooks/useAuth';
import { handleOAuthCallbackFromUrl, type OAuthProvider } from '../utils/oauthUtils';

interface ProviderConfig {
  name: string;
  icon: React.JSX.Element;
  color: 'primary' | 'secondary' | 'success' | 'warning' | 'danger';
}

const providerConfigs: Record<OAuthProvider, ProviderConfig> = {
  github: {
    name: 'GitHub',
    color: 'primary',
    icon: (
      <svg className="w-8 h-8 text-foreground" fill="currentColor" viewBox="0 0 24 24" aria-hidden="true">
        <path d="M12 0c-6.626 0-12 5.373-12 12 0 5.302 3.438 9.8 8.207 11.387.599.111.793-.261.793-.577v-2.234c-3.338.726-4.033-1.416-4.033-1.416-.546-1.387-1.333-1.756-1.333-1.756-1.089-.745.083-.729.083-.729 1.205.084 1.839 1.237 1.839 1.237 1.07 1.834 2.807 1.304 3.492.997.107-.775.418-1.305.762-1.604-2.665-.305-5.467-1.334-5.467-5.931 0-1.311.469-2.381 1.236-3.221-.124-.303-.535-1.524.117-3.176 0 0 1.008-.322 3.301 1.23.957-.266 1.983-.399 3.003-.404 1.02.005 2.047.138 3.006.404 2.291-1.552 3.297-1.23 3.297-1.23.653 1.653.242 2.874.118 3.176.77.84 1.235 1.911 1.235 3.221 0 4.609-2.807 5.624-5.479 5.921.43.372.823 1.102.823 2.222v3.293c0 .319.192.694.801.576 4.765-1.589 8.199-6.086 8.199-11.386 0-6.627-5.373-12-12-12z" />
      </svg>
    )
  },
  google: {
    name: 'Google',
    color: 'secondary',
    icon: (
      <svg className="w-8 h-8" viewBox="0 0 24 24">
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
    )
  }
};

export function OAuthCallbackPage({ provider }: { provider: OAuthProvider }) {
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [step, setStep] = useState<'processing' | 'exchanging' | 'authenticating' | 'success' | 'error'>('processing');
  const { loginWithGitHub, loginWithGoogle } = useAuth();
  const navigate = useNavigate();
  const hasProcessed = useRef(false);

  const currentProvider = provider as OAuthProvider;
  const providerConfig = providerConfigs[currentProvider];

  useEffect(() => {
    // Prevent multiple executions
    if (hasProcessed.current) return;
    hasProcessed.current = true;

    const processCallback = async () => {
      try {
        setStep('processing');

        let result: { code?: string; error?: string; errorDescription?: string } | null = null;

        if (currentProvider === 'github') {
          result = handleOAuthCallbackFromUrl('github');
        } else if (currentProvider === 'google') {
          result = handleOAuthCallbackFromUrl('google');
        } else {
          setError(`Unsupported OAuth provider: ${provider}`);
          setStep('error');
          return;
        }

        if (!result) {
          setError('Invalid callback parameters');
          setStep('error');
          return;
        }

        if (result.error) {
          setError(result.errorDescription || result.error);
          setStep('error');
          return;
        }

        if (!result.code) {
          setError('No authorization code received');
          setStep('error');
          return;
        }

        setStep('exchanging');
        await new Promise(resolve => setTimeout(resolve, 500)); // Brief pause for UX

        setStep('authenticating');

        if (currentProvider === 'github') {
          await loginWithGitHub(result.code);
        } else if (currentProvider === 'google') {
          await loginWithGoogle(result.code);
        }

        setStep('success');
        await new Promise(resolve => setTimeout(resolve, 800)); // Show success state

        navigate('/', { replace: true });
      } catch (err) {
        setError(err instanceof Error ? err.message : `${providerConfig?.name || 'OAuth'} login failed`);
        setStep('error');
      } finally {
        setIsLoading(false);
      }
    };

    processCallback();
  }, [loginWithGitHub, loginWithGoogle, navigate, currentProvider, provider, providerConfig?.name]);

  const getStepInfo = () => {
    const providerName = providerConfig?.name || 'OAuth';

    switch (step) {
      case 'processing':
        return {
          title: 'Processing Callback',
          description: `Validating ${providerName} authorization...`,
          icon: <Spinner size="lg" color="primary" />,
          color: 'primary' as const
        };
      case 'exchanging':
        return {
          title: 'Exchanging Tokens',
          description: 'Securely exchanging authorization code...',
          icon: <Spinner size="lg" color="secondary" />,
          color: 'secondary' as const
        };
      case 'authenticating':
        return {
          title: 'Authenticating',
          description: 'Completing your login to DerpCode...',
          icon: <Spinner size="lg" color="primary" />,
          color: 'primary' as const
        };
      case 'success':
        return {
          title: 'Login Successful!',
          description: 'Welcome to DerpCode! Redirecting you now...',
          icon: <CheckCircleIcon className="w-16 h-16 text-success" />,
          color: 'success' as const
        };
      case 'error':
        return {
          title: 'Login Failed',
          description: `Something went wrong during the ${providerName} login process.`,
          icon: <XCircleIcon className="w-16 h-16 text-danger" />,
          color: 'danger' as const
        };
      default:
        return {
          title: 'Processing...',
          description: 'Please wait...',
          icon: <Spinner size="lg" color="primary" />,
          color: 'primary' as const
        };
    }
  };

  const stepInfo = getStepInfo();

  if (!providerConfig) {
    return (
      <div className="min-h-screen bg-linear-to-br from-background via-content1 to-background flex items-center justify-center p-4">
        <Card className="w-full max-w-md shadow-2xl border-divider">
          <CardBody className="px-8 py-8 text-center">
            <XCircleIcon className="w-16 h-16 text-danger mx-auto mb-4" />
            <h2 className="text-2xl font-bold text-foreground mb-2">Invalid Provider</h2>
            <p className="text-default-600 mb-6">Unsupported OAuth provider: {provider}</p>
            <Button
              color="primary"
              size="lg"
              variant="solid"
              className="w-full font-semibold"
              onPress={() => navigate('/login', { replace: true })}
            >
              Return to Login
            </Button>
          </CardBody>
        </Card>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-linear-to-br from-background via-content1 to-background flex items-center justify-center p-4">
      <Card className="w-full max-w-md shadow-2xl border-divider">
        <CardHeader className="flex flex-col items-center pb-6 pt-8">
          {/* DerpCode Logo/Title */}
          <div className="flex items-center gap-3 mb-6">
            <img src="/favicon.ico" alt="DerpCode Logo" className="w-10 h-10" />
            <h1 className="text-3xl font-bold text-primary">DerpCode</h1>
          </div>

          {/* Provider Icon */}
          <div className="mb-4 p-3 rounded-full bg-content2 border border-divider">{providerConfig.icon}</div>

          <Chip color={stepInfo.color} variant="flat" size="md" className="font-medium mb-2">
            {providerConfig.name} Authentication
          </Chip>
        </CardHeader>

        <CardBody className="px-8 pb-8 text-center">
          {/* Status Icon */}
          <div className="flex justify-center mb-6">{stepInfo.icon}</div>

          {/* Status Text */}
          <div className="space-y-3 mb-6">
            <h2 className="text-2xl font-bold text-foreground">{stepInfo.title}</h2>
            <p className="text-default-600 text-lg">{stepInfo.description}</p>
          </div>

          {/* Progress Steps */}
          {isLoading && step !== 'error' && (
            <div className="flex justify-center items-center space-x-2 mb-6">
              <div
                className={`w-2 h-2 rounded-full ${['processing', 'exchanging', 'authenticating', 'success'].includes(step) ? 'bg-primary' : 'bg-content3'}`}
              />
              <div
                className={`w-2 h-2 rounded-full ${['exchanging', 'authenticating', 'success'].includes(step) ? 'bg-primary' : 'bg-content3'}`}
              />
              <div
                className={`w-2 h-2 rounded-full ${['authenticating', 'success'].includes(step) ? 'bg-primary' : 'bg-content3'}`}
              />
              <div className={`w-2 h-2 rounded-full ${step === 'success' ? 'bg-success' : 'bg-content3'}`} />
            </div>
          )}

          {/* Error Details */}
          {error && step === 'error' && (
            <div className="mb-6 p-4 bg-danger/10 border border-danger/20 rounded-lg">
              <p className="text-danger text-sm font-medium">{error}</p>
            </div>
          )}

          {/* Action Button for Error State */}
          {step === 'error' && (
            <Button
              color="primary"
              size="lg"
              variant="solid"
              className="w-full font-semibold"
              onPress={() => navigate('/login', { replace: true })}
            >
              Return to Login
            </Button>
          )}

          {/* Loading State Message */}
          {isLoading && step !== 'error' && <p className="text-sm text-default-500">This may take a few moments...</p>}
        </CardBody>
      </Card>
    </div>
  );
}
