import apiClient from './axiosConfig';
import type { JsonPatchDocument } from '../types/models';
import type { UserPreferencesDto } from '../types/userPreferences';

export const userPreferencesApi = {
  async getUserPreferences(userId: number): Promise<UserPreferencesDto> {
    const response = await apiClient.get<UserPreferencesDto>(`/api/v1/users/${userId}/preferences`);
    return response.data;
  },

  async patchUserPreferences(
    userId: number,
    preferencesId: number,
    patchDocument: JsonPatchDocument
  ): Promise<UserPreferencesDto> {
    const response = await apiClient.patch<UserPreferencesDto>(
      `/api/v1/users/${userId}/preferences/${preferencesId}`,
      patchDocument,
      {
        headers: {
          'Content-Type': 'application/json-patch+json'
        }
      }
    );

    return response.data;
  }
};
