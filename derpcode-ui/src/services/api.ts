import {
  type Problem,
  type CursorPaginatedResponse,
  type DriverTemplate,
  type CreateProblemRequest,
  type CreateProblemValidationResponse,
  type SubmissionResult,
  type Language,
  type AdminProblemDto
} from '../types/models';
import apiClient from './axiosConfig';

export const problemsApi = {
  async getProblems(): Promise<Problem[]> {
    const response = await apiClient.get<CursorPaginatedResponse<Problem>>('/api/v1/problems');
    return response.data.nodes || response.data.edges?.map(edge => edge.node) || [];
  },

  async getProblem(id: number): Promise<Problem> {
    const response = await apiClient.get<Problem>(`/api/v1/problems/${id}`);
    return response.data;
  },

  async getAdminProblem(id: number): Promise<AdminProblemDto> {
    const response = await apiClient.get<AdminProblemDto>(`/api/v1/problems/admin/${id}`);
    return response.data;
  },

  async validateProblem(problem: CreateProblemRequest): Promise<CreateProblemValidationResponse> {
    const response = await apiClient.post<CreateProblemValidationResponse>('/api/v1/problems/validate', problem);
    return response.data;
  },

  async createProblem(problem: CreateProblemRequest): Promise<Problem> {
    const response = await apiClient.post<Problem>('/api/v1/problems', problem);
    return response.data;
  },

  async updateProblem(problemId: number, problem: CreateProblemRequest): Promise<AdminProblemDto> {
    const response = await apiClient.put<AdminProblemDto>(`/api/v1/problems/${problemId}`, problem);
    return response.data;
  },

  async deleteProblem(problemId: number): Promise<void> {
    await apiClient.delete(`/api/v1/problems/${problemId}`);
  },

  async cloneProblem(problemId: number): Promise<AdminProblemDto> {
    const response = await apiClient.post<AdminProblemDto>(`/api/v1/problems/${problemId}/clone`);
    return response.data;
  },

  async submitSolution(problemId: number, userCode: string, language: Language): Promise<SubmissionResult> {
    const response = await apiClient.post<SubmissionResult>(`/api/v1/problems/${problemId}/submissions`, {
      userCode,
      language
    });
    return response.data;
  },

  async runSolution(problemId: number, userCode: string, language: Language): Promise<SubmissionResult> {
    const response = await apiClient.post<SubmissionResult>(`/api/v1/problems/${problemId}/run`, {
      userCode,
      language
    });
    return response.data;
  }
};

export const driverTemplatesApi = {
  async getDriverTemplates(): Promise<DriverTemplate[]> {
    const response = await apiClient.get<CursorPaginatedResponse<DriverTemplate>>('/api/v1/driverTemplates');
    return response.data.nodes || response.data.edges?.map(edge => edge.node) || [];
  }
};
