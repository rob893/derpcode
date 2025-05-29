import {
  type Problem,
  type CursorPaginatedResponse,
  type DriverTemplate,
  type CreateProblemRequest,
  type CreateProblemValidationResponse,
  type SubmissionResult,
  type Language
} from '../types/models';
import apiClient from './axiosConfig';

export const problemsApi = {
  getProblems: async (): Promise<Problem[]> => {
    const response = await apiClient.get<CursorPaginatedResponse<Problem>>('/api/v1/problems');
    return response.data.nodes || response.data.edges?.map(edge => edge.node) || [];
  },

  getProblem: async (id: number): Promise<Problem> => {
    const response = await apiClient.get<Problem>(`/api/v1/problems/${id}`);
    return response.data;
  },

  validateProblem: async (problem: CreateProblemRequest): Promise<CreateProblemValidationResponse> => {
    const response = await apiClient.post<CreateProblemValidationResponse>('/api/v1/problems/validate', problem);
    return response.data;
  },

  createProblem: async (problem: CreateProblemRequest): Promise<Problem> => {
    const response = await apiClient.post<Problem>('/api/v1/problems', problem);
    return response.data;
  },

  deleteProblem: async (problemId: number): Promise<void> => {
    await apiClient.delete(`/api/v1/problems/${problemId}`);
  },

  submitSolution: async (
    problemId: number,
    userId: number,
    userCode: string,
    language: Language
  ): Promise<SubmissionResult> => {
    const response = await apiClient.post<SubmissionResult>(`/api/v1/users/${userId}/submissions`, {
      problemId,
      userCode,
      language
    });
    return response.data;
  }
};

export const driverTemplatesApi = {
  getDriverTemplates: async (): Promise<DriverTemplate[]> => {
    const response = await apiClient.get<CursorPaginatedResponse<DriverTemplate>>('/api/v1/driverTemplates');
    return response.data.nodes || response.data.edges?.map(edge => edge.node) || [];
  }
};
