// User-related types beyond basic auth
export interface LinkedAccount {
  id: string;
  linkedAccountType: 'Google' | 'GitHub';
  userId: number;
}

export interface UserDto {
  id: number;
  userName: string;
  email: string;
  created: string;
  roles: string[];
  linkedAccounts: LinkedAccount[];
}

export interface ConfirmEmailRequest {
  email: string;
  token: string;
}

export interface ForgotPasswordRequest {
  email: string;
}

export interface ResetPasswordRequest {
  email: string;
  token: string;
  password: string;
}
