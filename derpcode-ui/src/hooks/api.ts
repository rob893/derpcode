import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { problemsApi, driverTemplatesApi, submissionsApi } from '../services/api';
import { authApi } from '../services/auth';
import type { CreateProblemRequest, Language, UserSubmissionQueryParameters } from '../types/models';
import type { LoginRequest, RegisterRequest } from '../types/auth';

// Query Keys
export const queryKeys = {
  problems: ['problems'] as const,
  problem: (id: number) => ['problems', id] as const,
  driverTemplates: ['driverTemplates'] as const,
  userSubmissions: (userId: number, problemId?: number) => ['users', userId, 'submissions', problemId] as const,
  problemSubmission: (problemId: number, submissionId: number) =>
    ['problems', problemId, 'submissions', submissionId] as const
} as const;

// Problem hooks
export const useProblems = () => {
  return useQuery({
    queryKey: queryKeys.problems,
    queryFn: problemsApi.getProblems,
    staleTime: 15 * 60 * 1000 // 15 minutes
  });
};

export const useProblem = (id: number) => {
  return useQuery({
    queryKey: queryKeys.problem(id),
    queryFn: () => problemsApi.getProblem(id),
    enabled: !!id,
    staleTime: 15 * 60 * 1000 // 15 minutes
  });
};

export const useCreateProblem = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (problem: CreateProblemRequest) => problemsApi.createProblem(problem),
    onSuccess: newProblem => {
      // Invalidate and refetch problems list
      queryClient.invalidateQueries({ queryKey: queryKeys.problems });

      // Optionally, add the new problem to the cache
      queryClient.setQueryData(queryKeys.problem(newProblem.id), newProblem);
    }
  });
};

export const useUpdateProblem = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ problemId, problem }: { problemId: number; problem: CreateProblemRequest }) =>
      problemsApi.updateProblem(problemId, problem),
    onSuccess: updatedProblem => {
      // Invalidate and refetch problems list
      queryClient.invalidateQueries({ queryKey: queryKeys.problems });

      // Update the specific problem caches
      queryClient.setQueryData(queryKeys.problem(updatedProblem.id), updatedProblem);
    }
  });
};

export const useDeleteProblem = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (problemId: number) => problemsApi.deleteProblem(problemId),
    onSuccess: () => {
      // Invalidate and refetch problems list
      queryClient.invalidateQueries({ queryKey: queryKeys.problems });
    }
  });
};

export const useCloneProblem = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (problemId: number) => problemsApi.cloneProblem(problemId),
    onSuccess: newProblem => {
      // Invalidate and refetch problems list
      queryClient.invalidateQueries({ queryKey: queryKeys.problems });

      // Add the new problem to the cache
      queryClient.setQueryData(queryKeys.problem(newProblem.id), newProblem);
    }
  });
};

export const useValidateProblem = () => {
  return useMutation({
    mutationFn: (problem: CreateProblemRequest) => problemsApi.validateProblem(problem)
  });
};

export const useSubmitSolution = (userId: number, problemId: number) => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ userCode, language }: { userCode: string; language: Language }) =>
      problemsApi.submitSolution(problemId, userCode, language),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.userSubmissions(userId, problemId) });
    }
  });
};

export const useRunSolution = (problemId: number) => {
  return useMutation({
    mutationFn: ({ userCode, language }: { userCode: string; language: Language }) =>
      problemsApi.runSolution(problemId, userCode, language)
  });
};

// Driver template hooks
export const useDriverTemplates = () => {
  return useQuery({
    queryKey: queryKeys.driverTemplates,
    queryFn: driverTemplatesApi.getDriverTemplates,
    staleTime: 30 * 60 * 1000 // 30 minutes (these change less frequently)
  });
};

// Auth hooks
export const useLogin = () => {
  return useMutation({
    mutationFn: (credentials: Omit<LoginRequest, 'deviceId'>) => authApi.login(credentials)
  });
};

export const useRegister = () => {
  return useMutation({
    mutationFn: (userData: Omit<RegisterRequest, 'deviceId'>) => authApi.register(userData)
  });
};

export const useLogout = () => {
  return useMutation({
    mutationFn: () => authApi.logout()
  });
};

export const useRefreshToken = () => {
  return useMutation({
    mutationFn: () => authApi.refreshToken()
  });
};

// Submission hooks
export const useUserSubmissionsForProblem = (
  userId: number,
  problemId: number,
  queryParams?: Partial<UserSubmissionQueryParameters>
) => {
  return useQuery({
    queryKey: queryKeys.userSubmissions(userId, problemId),
    queryFn: () => submissionsApi.getUserSubmissionsForProblem(userId, problemId, queryParams),
    enabled: !!userId && !!problemId,
    staleTime: 30 * 60 * 1000 // 30 minutes
  });
};

export const useProblemSubmission = (problemId: number, submissionId: number) => {
  return useQuery({
    queryKey: queryKeys.problemSubmission(problemId, submissionId),
    queryFn: () => submissionsApi.getProblemSubmission(problemId, submissionId),
    enabled: !!problemId && !!submissionId,
    staleTime: 30 * 60 * 1000 // 30 minutes (submissions don't change)
  });
};
