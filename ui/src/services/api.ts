import {
  type Problem,
  type CursorPaginatedResponse,
  type DriverTemplate,
  type CreateProblemRequest,
  type SubmissionResult,
  type Language
} from '../types/models';
import { authenticatedFetch } from './auth';

const API_BASE_URL = import.meta.env.VITE_DERPCODE_API_BASE_URL;

export class ApiError extends Error {
  constructor(
    message: string,
    public status: number,
    public statusText: string
  ) {
    super(message);
    this.name = 'ApiError';
  }
}

async function handleResponse<T>(response: Response): Promise<T> {
  if (!response.ok) {
    const errorMessage = `HTTP ${response.status}: ${response.statusText}`;
    throw new ApiError(errorMessage, response.status, response.statusText);
  }

  try {
    return await response.json();
  } catch (error: unknown) {
    throw new ApiError(
      `Failed to parse JSON response ${(error as Error)?.message}`,
      response.status,
      response.statusText
    );
  }
}

export const problemsApi = {
  // Get all problems
  getProblems: async (): Promise<Problem[]> => {
    const response = await authenticatedFetch(`${API_BASE_URL}/api/v1/problems`);
    const data: CursorPaginatedResponse<Problem> = await handleResponse(response);
    return data.nodes || data.edges?.map(edge => edge.node) || [];
  },

  // Get a specific problem by ID
  getProblem: async (id: number): Promise<Problem> => {
    const response = await authenticatedFetch(`${API_BASE_URL}/api/v1/problems/${id}`);
    return handleResponse(response);
  },

  // Create a new problem
  createProblem: async (problem: CreateProblemRequest): Promise<Problem> => {
    const response = await authenticatedFetch(`${API_BASE_URL}/api/v1/problems`, {
      method: 'POST',
      body: JSON.stringify(problem)
    });
    return handleResponse(response);
  },

  // Submit a solution for a problem
  submitSolution: async (problemId: number, userCode: string, language: Language): Promise<SubmissionResult> => {
    const response = await authenticatedFetch(`${API_BASE_URL}/api/v1/problems/${problemId}/submissions`, {
      method: 'POST',
      body: JSON.stringify({
        userCode,
        language
      })
    });
    return handleResponse(response);
  }
};

export const driverTemplatesApi = {
  // Get all driver templates
  getDriverTemplates: async (): Promise<DriverTemplate[]> => {
    const response = await authenticatedFetch(`${API_BASE_URL}/api/v1/driverTemplates`);
    const data: CursorPaginatedResponse<DriverTemplate> = await handleResponse(response);
    return data.nodes || data.edges?.map(edge => edge.node) || [];
  }
};
