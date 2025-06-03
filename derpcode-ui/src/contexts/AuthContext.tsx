import { createContext, useReducer, useEffect, useCallback, type ReactNode, useMemo } from 'react';
import { authApi, getAccessToken, clearAccessToken } from '../services/auth';
import { decodeJwtToken } from '../utils/auth';
import type { AuthState, User, LoginRequest, RegisterRequest } from '../types/auth';

interface AuthContextType extends AuthState {
  login: (credentials: Omit<LoginRequest, 'deviceId'>) => Promise<void>;
  loginWithGitHub: (code: string) => Promise<void>;
  loginWithGoogle: (idToken: string) => Promise<void>;
  register: (userData: Omit<RegisterRequest, 'deviceId'>) => Promise<void>;
  logout: () => Promise<void>;
  checkAuth: () => Promise<void>;
}

type AuthAction =
  | { type: 'SET_LOADING'; payload: boolean }
  | { type: 'SET_USER'; payload: User }
  | { type: 'CLEAR_USER' }
  | { type: 'SET_AUTH_STATE'; payload: { user: User | null; isAuthenticated: boolean } };

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
    case 'SET_AUTH_STATE':
      return {
        ...state,
        user: action.payload.user,
        isAuthenticated: action.payload.isAuthenticated,
        isLoading: false
      };
    default:
      return state;
  }
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export { AuthContext };

export function AuthProvider({ children }: { children: ReactNode }) {
  const [state, dispatch] = useReducer(authReducer, initialState);

  const login = useCallback(async (credentials: Omit<LoginRequest, 'deviceId'>) => {
    dispatch({ type: 'SET_LOADING', payload: true });
    try {
      const response = await authApi.login(credentials);

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
    } catch (error) {
      dispatch({ type: 'CLEAR_USER' });
      throw error;
    }
  }, []);

  const loginWithGitHub = useCallback(async (code: string) => {
    dispatch({ type: 'SET_LOADING', payload: true });
    try {
      const response = await authApi.loginWithGitHub(code);

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
    } catch (error) {
      dispatch({ type: 'CLEAR_USER' });
      throw error;
    }
  }, []);

  const loginWithGoogle = useCallback(async (code: string) => {
    dispatch({ type: 'SET_LOADING', payload: true });
    try {
      const response = await authApi.loginWithGoogle(code);

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
    } catch (error) {
      dispatch({ type: 'CLEAR_USER' });
      throw error;
    }
  }, []);

  const register = useCallback(async (userData: Omit<RegisterRequest, 'deviceId'>) => {
    dispatch({ type: 'SET_LOADING', payload: true });
    try {
      const response = await authApi.register(userData);

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
    } catch (error) {
      dispatch({ type: 'CLEAR_USER' });
      throw error;
    }
  }, []);

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
    const token = getAccessToken();

    if (!token) {
      // Don't attempt refresh token during OAuth flows or on auth-related pages
      const currentPath = window.location.hash.replace('#', '');
      const isAuthFlow =
        currentPath.includes('/auth/') || currentPath.includes('/login') || currentPath.includes('/register');

      if (!isAuthFlow) {
        try {
          await authApi.refreshToken();

          const newToken = getAccessToken();

          if (newToken) {
            const user = decodeJwtToken(newToken);

            if (user) {
              dispatch({ type: 'SET_USER', payload: user });
            } else {
              dispatch({ type: 'SET_AUTH_STATE', payload: { user: null, isAuthenticated: true } });
            }
          }

          return;
        } catch {
          console.log('No valid refresh token available');
        }
      }

      dispatch({ type: 'CLEAR_USER' });
      return;
    }

    // If we have a token, decode it to get user information
    const user = decodeJwtToken(token);
    if (user) {
      dispatch({ type: 'SET_USER', payload: user });
    } else {
      // Token is invalid or expired
      dispatch({ type: 'CLEAR_USER' });
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
