import { ApiError } from './api';
import type { LoginRequest, RegisterRequest, LoginResponse, RefreshTokenResponse } from '../types/auth';

const API_BASE_URL = import.meta.env.VITE_DERPCODE_API_BASE_URL;

// Storage keys
const STORAGE_KEYS = {
  DEVICE_ID: 'device_id'
} as const;

let accessToken: string | null = null;
let cachedDeviceId: string | null = null;

// Generate or get device ID
export function getDeviceId(): string {
  let deviceId = cachedDeviceId ?? localStorage.getItem(STORAGE_KEYS.DEVICE_ID);

  if (!deviceId) {
    deviceId = crypto.randomUUID();
    localStorage.setItem(STORAGE_KEYS.DEVICE_ID, deviceId);
  }

  cachedDeviceId = deviceId;

  return deviceId;
}

// Token management
export function getAccessToken(): string | null {
  return accessToken;
}

export function setAccessToken(token: string | null): void {
  accessToken = token;
}

export function clearAccessToken(): void {
  accessToken = null;
}

// Enhanced fetch with automatic auth handling
export async function authenticatedFetch(url: string, options: RequestInit = {}): Promise<Response> {
  const token = getAccessToken();

  const authHeaders = token ? { Authorization: `Bearer ${token}` } : {};

  const response = await fetch(url, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      ...authHeaders,
      ...options.headers
    } as any,
    credentials: 'include' // Include cookies for refresh token
  });

  // Check for token expiration
  if (response.status === 401 && response.headers.get('X-Token-Expired')) {
    // Try to refresh the token
    const refreshSuccess = await tryRefreshToken();

    if (refreshSuccess) {
      // Retry the original request with the new token
      const newToken = getAccessToken();
      const retryResponse = await fetch(url, {
        ...options,
        headers: {
          'Content-Type': 'application/json',
          ...(newToken ? { Authorization: `Bearer ${newToken}` } : {}),
          ...options.headers
        },
        credentials: 'include'
      });

      return retryResponse;
    } else {
      // Refresh failed, redirect to login
      clearAccessToken();
      window.location.href = '/login';
      throw new ApiError('Authentication required', 401, 'Unauthorized');
    }
  }

  return response;
}

async function tryRefreshToken(): Promise<boolean> {
  try {
    const deviceId = getDeviceId();
    const response = await fetch(`${API_BASE_URL}/api/v1/auth/refreshToken`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      credentials: 'include',
      body: JSON.stringify({ deviceId })
    });

    if (response.ok) {
      const data: RefreshTokenResponse = await response.json();
      setAccessToken(data.token);
      return true;
    }
  } catch (error) {
    console.error('Token refresh failed:', error);
  }

  return false;
}

export const authApi = {
  login: async (credentials: Omit<LoginRequest, 'deviceId'>): Promise<LoginResponse> => {
    const deviceId = getDeviceId();
    const response = await fetch(`${API_BASE_URL}/api/v1/auth/login`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      credentials: 'include',
      body: JSON.stringify({
        ...credentials,
        deviceId
      })
    });

    if (!response.ok) {
      const errorText = await response.text();
      throw new ApiError(errorText || 'Login failed', response.status, response.statusText);
    }

    const data: LoginResponse = await response.json();
    setAccessToken(data.token);
    return data;
  },

  register: async (userData: Omit<RegisterRequest, 'deviceId'>): Promise<LoginResponse> => {
    const deviceId = getDeviceId();
    const response = await fetch(`${API_BASE_URL}/api/v1/auth/register`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      credentials: 'include',
      body: JSON.stringify({
        ...userData,
        deviceId
      })
    });

    if (!response.ok) {
      const errorText = await response.text();
      throw new ApiError(errorText || 'Registration failed', response.status, response.statusText);
    }

    const data: LoginResponse = await response.json();
    setAccessToken(data.token);
    return data;
  },

  logout: async (): Promise<void> => {
    // Clear the access token
    clearAccessToken();

    // Clear the refresh token cookie by making a request to logout endpoint (if it exists)
    // For now, we'll just clear client-side state
    try {
      await fetch(`${API_BASE_URL}/api/v1/auth/logout`, {
        method: 'POST',
        credentials: 'include'
      });
    } catch {
      // Ignore logout endpoint errors
      console.warn('Logout endpoint not available or failed');
    }
  },

  refreshToken: async (): Promise<RefreshTokenResponse> => {
    const deviceId = getDeviceId();
    const response = await fetch(`${API_BASE_URL}/api/v1/auth/refreshToken`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      credentials: 'include',
      body: JSON.stringify({ deviceId })
    });

    if (!response.ok) {
      throw new ApiError('Token refresh failed', response.status, response.statusText);
    }

    const data: RefreshTokenResponse = await response.json();
    setAccessToken(data.token);
    return data;
  }
};
