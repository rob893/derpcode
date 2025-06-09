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
  emailConfirmed: boolean;
  created: string;
  roles: string[];
  linkedAccounts: LinkedAccount[];
  lastLogin?: string;
  lastPasswordChange: string;
  lastEmailChange: string;
  lastUsernameChange: string;
  lastEmailConfirmationSent?: string;
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

export interface UpdatePasswordRequest {
  oldPassword: string;
  newPassword: string;
}

export interface UpdateUsernameRequest {
  newUsername: string;
}
