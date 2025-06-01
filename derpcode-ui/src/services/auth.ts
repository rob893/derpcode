import apiClient from './axiosConfig';
import type { LoginRequest, RegisterRequest, LoginResponse, RefreshTokenResponse } from '../types/auth';

const STORAGE_KEYS = {
  DEVICE_ID: 'device_id'
} as const;

let accessToken: string | null = null;
let cachedDeviceId: string | null = null;

export function getDeviceId(): string {
  let deviceId = cachedDeviceId ?? localStorage.getItem(STORAGE_KEYS.DEVICE_ID);

  if (!deviceId) {
    deviceId = crypto.randomUUID();
    localStorage.setItem(STORAGE_KEYS.DEVICE_ID, deviceId);
  }

  cachedDeviceId = deviceId;

  return deviceId;
}

export function getAccessToken(): string | null {
  return accessToken;
}

export function setAccessToken(token: string | null): void {
  accessToken = token;
}

export function clearAccessToken(): void {
  accessToken = null;
}

export const authApi = {
  async login(credentials: Omit<LoginRequest, 'deviceId'>): Promise<LoginResponse> {
    const deviceId = getDeviceId();
    const response = await apiClient.post<LoginResponse>('/api/v1/auth/login', {
      ...credentials,
      deviceId
    });

    setAccessToken(response.data.token);
    return response.data;
  },

  async loginWithGitHub(code: string): Promise<LoginResponse> {
    const deviceId = getDeviceId();
    const response = await apiClient.post<LoginResponse>('/api/v1/auth/login/github', {
      code,
      deviceId
    });

    setAccessToken(response.data.token);
    return response.data;
  },

  register: async (userData: Omit<RegisterRequest, 'deviceId'>): Promise<LoginResponse> => {
    const deviceId = getDeviceId();
    const response = await apiClient.post<LoginResponse>('/api/v1/auth/register', {
      ...userData,
      deviceId
    });

    setAccessToken(response.data.token);
    return response.data;
  },

  logout: async (): Promise<void> => {
    try {
      await apiClient.post('/api/v1/auth/logout');
    } catch {
      console.warn('Logout endpoint not available or failed');
    } finally {
      clearAccessToken();
    }
  },

  refreshToken: async (): Promise<RefreshTokenResponse> => {
    const deviceId = getDeviceId();
    const response = await apiClient.post<RefreshTokenResponse>('/api/v1/auth/refreshToken', {
      deviceId
    });

    setAccessToken(response.data.token);
    return response.data;
  }
};
