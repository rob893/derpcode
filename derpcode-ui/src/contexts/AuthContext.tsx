import { createContext, useReducer, useEffect, useCallback, useRef, type ReactNode, useMemo } from 'react';
import { authApi, getAccessToken, clearAccessToken } from '../services/auth';
import { decodeJwtToken } from '../utils/auth';
import type { AuthState, User, LoginRequest, RegisterRequest } from '../types/auth';

interface AuthContextType extends AuthState {
  login(credentials: Omit<LoginRequest, 'deviceId'>): Promise<void>;
  loginWithGitHub(code: string): Promise<void>;
  loginWithGoogle(code: string): Promise<void>;
  register(userData: Omit<RegisterRequest, 'deviceId'>): Promise<void>;
  logout(): Promise<void>;
  checkAuth(): Promise<void>;
}

type AuthAction =
  | { type: 'SET_LOADING'; payload: boolean }
  | { type: 'SET_USER'; payload: User }
  | { type: 'CLEAR_USER' };

const initialState: AuthState = {
  user: null,
  isAuthenticated: false,
  isLoading: true
};

function authReducer(state: AuthState, action: AuthAction): AuthState {
  switch (action.type) {
    case 'SET_LOADING':
      return { ...state, isLoading: action.payload };
    case 'SET_USER':
      return { ...state, user: action.payload, isAuthenticated: true, isLoading: false };
    case 'CLEAR_USER':
      return { ...state, user: null, isAuthenticated: false, isLoading: false };
    default:
      return state;
  }
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export { AuthContext };

export function AuthProvider({ children }: { children: ReactNode }) {
  const [state, dispatch] = useReducer(authReducer, initialState);

  // Ref to track if we're currently checking auth to prevent duplicate calls
  const isCheckingAuth = useRef(false);

  // Helper function to handle authentication response
  const handleAuthResponse = useCallback((response: { user: User }) => {
    const token = getAccessToken();

    if (token) {
      const userFromToken = decodeJwtToken(token);
      if (userFromToken) {
        dispatch({ type: 'SET_USER', payload: userFromToken });
      } else {
        dispatch({ type: 'SET_USER', payload: response.user });
      }
    } else {
      dispatch({ type: 'SET_USER', payload: response.user });
    }
  }, []);

  const login = useCallback(
    async (credentials: Omit<LoginRequest, 'deviceId'>) => {
      dispatch({ type: 'SET_LOADING', payload: true });
      try {
        const response = await authApi.login(credentials);
        handleAuthResponse(response);
      } catch (error) {
        dispatch({ type: 'CLEAR_USER' });
        throw error;
      }
    },
    [handleAuthResponse]
  );

  const loginWithGitHub = useCallback(
    async (code: string) => {
      dispatch({ type: 'SET_LOADING', payload: true });
      try {
        const response = await authApi.loginWithGitHub(code);
        handleAuthResponse(response);
      } catch (error) {
        dispatch({ type: 'CLEAR_USER' });
        throw error;
      }
    },
    [handleAuthResponse]
  );

  const loginWithGoogle = useCallback(
    async (code: string) => {
      dispatch({ type: 'SET_LOADING', payload: true });
      try {
        const response = await authApi.loginWithGoogle(code);
        handleAuthResponse(response);
      } catch (error) {
        dispatch({ type: 'CLEAR_USER' });
        throw error;
      }
    },
    [handleAuthResponse]
  );

  const register = useCallback(
    async (userData: Omit<RegisterRequest, 'deviceId'>) => {
      dispatch({ type: 'SET_LOADING', payload: true });
      try {
        const response = await authApi.register(userData);
        handleAuthResponse(response);
      } catch (error) {
        dispatch({ type: 'CLEAR_USER' });
        throw error;
      }
    },
    [handleAuthResponse]
  );

  const logout = useCallback(async () => {
    dispatch({ type: 'SET_LOADING', payload: true });
    try {
      await authApi.logout();
    } catch (error) {
      console.error('Logout error:', error);
    } finally {
      clearAccessToken();
      dispatch({ type: 'CLEAR_USER' });
    }
  }, []);

  const checkAuth = useCallback(async () => {
    // Don't attempt refresh token during OAuth flows or on auth-related pages
    const currentPath = window.location.hash.replace('#', '');
    const isAuthFlow =
      currentPath.includes('/auth/') || currentPath.includes('/login') || currentPath.includes('/register');

    // Prevent concurrent auth checks
    if (isCheckingAuth.current || isAuthFlow) {
      return;
    }

    isCheckingAuth.current = true;
    dispatch({ type: 'SET_LOADING', payload: true });

    try {
      let token = getAccessToken();

      if (!token) {
        await authApi.refreshToken();
        token = getAccessToken();
      }

      if (token) {
        const user = decodeJwtToken(token);
        if (user) {
          dispatch({ type: 'SET_USER', payload: user });
        } else {
          dispatch({ type: 'CLEAR_USER' });
        }
      } else {
        dispatch({ type: 'CLEAR_USER' });
      }
    } catch (error) {
      // Refresh failed, clear any existing tokens and set as unauthenticated
      console.log('Token refresh failed on app load:', error);
      clearAccessToken();
      dispatch({ type: 'CLEAR_USER' });
    } finally {
      isCheckingAuth.current = false;
    }
  }, []);

  useEffect(() => {
    checkAuth();
  }, [checkAuth]);

  const value: AuthContextType = useMemo(
    () => ({
      ...state,
      login,
      loginWithGitHub,
      loginWithGoogle,
      register,
      logout,
      checkAuth
    }),
    [state, login, loginWithGitHub, loginWithGoogle, register, logout, checkAuth]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
