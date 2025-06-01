// Auth types
export interface User {
  id: number;
  userName: string;
  email: string;
  roles: string[];
}

export interface LoginRequest {
  username: string;
  password: string;
  deviceId: string;
}

export interface RegisterRequest {
  userName: string;
  password: string;
  email: string;
  deviceId: string;
}

export interface GitHubLoginRequest {
  code: string;
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
