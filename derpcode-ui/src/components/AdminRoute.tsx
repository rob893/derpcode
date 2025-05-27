import { useEffect } from 'react';
import { useNavigate } from 'react-router';
import { useAuth } from '../hooks/useAuth';
import { hasAdminRole } from '../utils/auth';

interface AdminRouteProps {
  children: React.ReactNode;
  redirectTo?: string;
}

export function AdminRoute({ children, redirectTo = '/problems' }: AdminRouteProps) {
  const { user, isAuthenticated, isLoading } = useAuth();
  const navigate = useNavigate();

  useEffect(() => {
    if (!isLoading && isAuthenticated && !hasAdminRole(user)) {
      navigate(redirectTo, { replace: true });
    }
  }, [user, isAuthenticated, isLoading, navigate, redirectTo]);

  // Show loading while checking authentication
  if (isLoading) {
    return (
      <div className="flex justify-center items-center min-h-screen">
        <div className="text-lg">Loading...</div>
      </div>
    );
  }

  // Don't render children if not admin (will redirect via useEffect)
  if (!isAuthenticated || !hasAdminRole(user)) {
    return null;
  }

  return <>{children}</>;
}
