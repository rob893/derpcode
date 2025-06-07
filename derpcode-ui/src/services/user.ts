import apiClient from './axiosConfig';
import type { ConfirmEmailRequest, ForgotPasswordRequest, ResetPasswordRequest, UserDto } from '../types/user';

export const userApi = {
  async getUserById(id: number): Promise<UserDto> {
    const response = await apiClient.get<UserDto>(`/api/v1/users/${id}`);
    return response.data;
  },

  async deleteUser(id: number): Promise<void> {
    await apiClient.delete(`/api/v1/users/${id}`);
  },

  async deleteLinkedAccount(userId: number, linkedAccountType: string): Promise<void> {
    await apiClient.delete(`/api/v1/users/${userId}/linkedAccounts/${linkedAccountType}`);
  },

  async confirmEmail(request: ConfirmEmailRequest): Promise<void> {
    await apiClient.post('/api/v1/users/confirmEmail', request);
  },

  async forgotPassword(request: ForgotPasswordRequest): Promise<void> {
    await apiClient.post('/api/v1/users/forgotPassword', request);
  },

  async resetPassword(request: ResetPasswordRequest): Promise<void> {
    await apiClient.post('/api/v1/users/resetPassword', request);
  }
};
