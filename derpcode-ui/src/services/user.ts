import apiClient from './axiosConfig';
import type { UserDto } from '../types/user';

export const userApi = {
  async getUserById(id: number): Promise<UserDto> {
    const response = await apiClient.get<UserDto>(`/api/v1/users/${id}`);
    return response.data;
  },

  async deleteUser(id: number): Promise<void> {
    await apiClient.delete(`/api/v1/users/${id}`);
  }
};
