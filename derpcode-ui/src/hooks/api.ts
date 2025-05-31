import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { problemsApi, driverTemplatesApi } from '../services/api';
import { authApi } from '../services/auth';
import type { CreateProblemRequest, Language } from '../types/models';
import type { LoginRequest, RegisterRequest } from '../types/auth';

// Query Keys
export const queryKeys = {
  problems: ['problems'] as const,
  problem: (id: number) => ['problems', id] as const,
  adminProblem: (id: number) => ['problems', 'admin', id] as const,
  driverTemplates: ['driverTemplates'] as const
} as const;

// Problem hooks
export const useProblems = () => {
  return useQuery({
    queryKey: queryKeys.problems,
    queryFn: problemsApi.getProblems,
    staleTime: 5 * 60 * 1000 // 5 minutes
  });
};

export const useProblem = (id: number) => {
  return useQuery({
    queryKey: queryKeys.problem(id),
    queryFn: () => problemsApi.getProblem(id),
    enabled: !!id,
    staleTime: 5 * 60 * 1000 // 5 minutes
  });
};

export const useAdminProblem = (id: number) => {
  return useQuery({
    queryKey: queryKeys.adminProblem(id),
    queryFn: () => problemsApi.getAdminProblem(id),
    enabled: !!id,
    staleTime: 5 * 60 * 1000 // 5 minutes
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
      queryClient.setQueryData(queryKeys.adminProblem(updatedProblem.id), updatedProblem);
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

export const useValidateProblem = () => {
  return useMutation({
    mutationFn: (problem: CreateProblemRequest) => problemsApi.validateProblem(problem)
  });
};

export const useSubmitSolution = (problemId: number) => {
  return useMutation({
    mutationFn: ({ userCode, language, userId }: { userCode: string; language: Language; userId: number }) =>
      problemsApi.submitSolution(problemId, userId, userCode, language)
  });
};

// Driver template hooks
export const useDriverTemplates = () => {
  return useQuery({
    queryKey: queryKeys.driverTemplates,
    queryFn: driverTemplatesApi.getDriverTemplates,
    staleTime: 10 * 60 * 1000 // 10 minutes (these change less frequently)
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
