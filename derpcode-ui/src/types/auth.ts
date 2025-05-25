// Auth types
export interface User {
  id: string;
  userName: string;
  email: string;
  firstName?: string;
  lastName?: string;
}

export interface LoginRequest {
  username: string;
  password: string;
  deviceId: string;
}

export interface RegisterRequest {
  userName: string;
  password: string;
  firstName?: string;
  lastName?: string;
  email: string;
  deviceId: string;
}

export interface LoginResponse {
  token: string;
  user: User;
}

export interface RefreshTokenRequest {
  deviceId: string;
}

export interface RefreshTokenResponse {
  token: string;
}

export interface AuthState {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
}
