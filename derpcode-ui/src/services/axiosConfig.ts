import axios, { AxiosError, type AxiosRequestConfig } from 'axios';
import { getDeviceId, getAccessToken, setAccessToken, clearAccessToken } from './auth';

const API_BASE_URL = import.meta.env.VITE_DERPCODE_API_BASE_URL;

export class ApiError extends Error {
  constructor(
    message: string,
    public status: number,
    public statusText: string
  ) {
    super(message);
    this.name = 'ApiError';
  }
}

export const apiClient = axios.create({
  baseURL: API_BASE_URL,
  withCredentials: true, // Include cookies for refresh token
  headers: {
    'Content-Type': 'application/json'
  }
});

// CSRF Token management for Double Submit Cookie pattern
function getCsrfTokenFromCookie(): string | null {
  const match = document.cookie.match(/csrf_token=([^;]+)/);
  return match ? decodeURIComponent(match[1]) : null;
}

apiClient.interceptors.request.use(
  config => {
    const token = getAccessToken();

    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }

    if (config.url?.includes('/refreshToken')) {
      const csrfToken = getCsrfTokenFromCookie();
      if (csrfToken) {
        config.headers['X-CSRF-Token'] = csrfToken;
      }
    }

    return config;
  },
  error => {
    return Promise.reject(error);
  }
);

// Flag to prevent multiple refresh token requests
let isRefreshing = false;
let failedQueue: Array<{
  resolve: (value?: any) => void;
  reject: (error?: any) => void;
}> = [];

const processQueue = (error: any, token: string | null = null) => {
  failedQueue.forEach(({ resolve, reject }) => {
    if (error) {
      reject(error);
    } else {
      resolve(token);
    }
  });

  failedQueue = [];
};

apiClient.interceptors.response.use(
  response => {
    return response;
  },
  async (error: AxiosError) => {
    const originalRequest = error.config as AxiosRequestConfig & { _retry?: boolean };

    if (error.response) {
      const apiError = new ApiError(
        `HTTP ${error.response.status}: ${error.response.statusText}`,
        error.response.status,
        error.response.statusText
      );

      if (
        error.response.status === 401 &&
        (error.response.headers?.['x-token-expired'] || error.response.headers?.['X-Token-Expired']) &&
        !originalRequest._retry
      ) {
        if (isRefreshing) {
          // If already refreshing, queue this request
          return new Promise((resolve, reject) => {
            failedQueue.push({ resolve, reject });
          })
            .then(token => {
              if (originalRequest.headers) {
                originalRequest.headers.Authorization = `Bearer ${token}`;
              }
              return apiClient(originalRequest);
            })
            .catch(err => {
              return Promise.reject(err);
            });
        }

        originalRequest._retry = true;
        isRefreshing = true;

        try {
          const refreshed = await refreshToken();

          if (refreshed) {
            const newToken = getAccessToken();
            processQueue(null, newToken);

            if (originalRequest.headers) {
              originalRequest.headers.Authorization = `Bearer ${newToken}`;
            }

            return apiClient(originalRequest);
          } else {
            processQueue(apiError, null);
            clearAccessToken();

            return Promise.reject(apiError);
          }
        } catch (refreshError) {
          processQueue(refreshError, null);
          clearAccessToken();

          return Promise.reject(refreshError);
        } finally {
          isRefreshing = false;
        }
      }

      return Promise.reject(apiError);
    }

    // Network error or other error without response
    const networkError = new ApiError(error.message || 'Network error', 0, 'Network Error');

    return Promise.reject(networkError);
  }
);

async function refreshToken(): Promise<boolean> {
  try {
    const deviceId = getDeviceId();
    const csrfToken = getCsrfTokenFromCookie();

    if (!csrfToken) {
      console.error('CSRF token not found in cookie');
      return false;
    }

    const response = await axios.post(
      `${API_BASE_URL}/api/v1/auth/refreshToken`,
      { deviceId },
      {
        headers: {
          'Content-Type': 'application/json',
          'X-CSRF-Token': csrfToken
        },
        withCredentials: true
      }
    );

    if (response.status === 200 && response.data.token) {
      setAccessToken(response.data.token);
      return true;
    }
  } catch (error) {
    console.error('Token refresh failed:', error);
  }

  return false;
}

export default apiClient;
