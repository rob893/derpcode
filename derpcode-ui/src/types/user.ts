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
