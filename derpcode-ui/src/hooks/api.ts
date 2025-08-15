import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useEffect } from 'react';
import { problemsApi, driverTemplatesApi, submissionsApi, articlesApi } from '../services/api';
import { authApi } from '../services/auth';
import { useAuth } from './useAuth';
import type {
  CreateProblemRequest,
  Language,
  UserSubmissionQueryParameters,
  ProblemQueryParameters,
  CreateArticleCommentRequest,
  ArticleCommentQueryParameters,
  JsonPatchDocument
} from '../types/models';
import type { LoginRequest, RegisterRequest } from '../types/auth';

// Query Keys
export const queryKeys = {
  problems: (queryParams?: Partial<ProblemQueryParameters>) => ['problems', queryParams] as const,
  problem: (id: number) => ['problems', id] as const,
  driverTemplates: ['driverTemplates'] as const,
  userSubmissions: (userId: number, problemId?: number) => ['users', userId, 'submissions', problemId] as const,
  problemSubmission: (problemId: number, submissionId: number) =>
    ['problems', problemId, 'submissions', submissionId] as const,
  // Properly typed query parameters
  articleComments: (articleId: number, queryParams?: Partial<ArticleCommentQueryParameters>) =>
    ['articles', articleId, 'comments', queryParams] as const,
  articleCommentReplies: (articleId: number, commentId: number, queryParams?: Partial<ArticleCommentQueryParameters>) =>
    ['articles', articleId, 'comments', commentId, 'replies', queryParams] as const,
  articleCommentQuotedBy: (
    articleId: number,
    commentId: number,
    queryParams?: Partial<ArticleCommentQueryParameters>
  ) => ['articles', articleId, 'comments', commentId, 'quotedBy', queryParams] as const,
  articleComment: (articleId: number, commentId: number) => ['articles', articleId, 'comments', commentId] as const
} as const;

// Problem hooks
export const useProblems = (queryParams?: Partial<ProblemQueryParameters>) => {
  const queryClient = useQueryClient();
  const { isLoading: isAuthLoading } = useAuth();

  const query = useQuery({
    queryKey: queryKeys.problems(queryParams),
    queryFn: () => problemsApi.getProblems(queryParams),
    enabled: !isAuthLoading, // Wait for auth initialization
    staleTime: 15 * 60 * 1000 // 15 minutes
  });

  // Set cache keys for each individual problem when data changes
  useEffect(() => {
    if (query.data) {
      query.data.forEach(problem => {
        queryClient.setQueryData(queryKeys.problem(problem.id), problem);
      });
    }
  }, [query.data, queryClient]);

  return query;
};

export const useProblem = (id: number) => {
  const { isLoading: isAuthLoading } = useAuth();

  return useQuery({
    queryKey: queryKeys.problem(id),
    queryFn: () => problemsApi.getProblem(id),
    enabled: !!id && !isAuthLoading, // Wait for auth initialization and ensure id exists
    staleTime: 15 * 60 * 1000 // 15 minutes
  });
};

export const useCreateProblem = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (problem: CreateProblemRequest) => problemsApi.createProblem(problem),
    onSuccess: newProblem => {
      // Invalidate and refetch problems list
      queryClient.invalidateQueries({ queryKey: ['problems'] });

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
      queryClient.invalidateQueries({ queryKey: ['problems'] });

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
      queryClient.invalidateQueries({ queryKey: ['problems'] });
    }
  });
};

export const useCloneProblem = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (problemId: number) => problemsApi.cloneProblem(problemId),
    onSuccess: newProblem => {
      // Invalidate and refetch problems list
      queryClient.invalidateQueries({ queryKey: ['problems'] });

      // Add the new problem to the cache
      queryClient.setQueryData(queryKeys.problem(newProblem.id), newProblem);
    }
  });
};

export const useToggleProblemPublished = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ problemId, isPublished }: { problemId: number; isPublished: boolean }) => {
      const patchDocument: JsonPatchDocument = [
        {
          op: 'replace' as const,
          path: '/isPublished',
          value: isPublished
        }
      ];
      return problemsApi.patchProblem(problemId, patchDocument);
    },
    onSuccess: updatedProblem => {
      // Invalidate and refetch problems list
      queryClient.invalidateQueries({ queryKey: ['problems'] });

      // Update the specific problem cache
      queryClient.setQueryData(queryKeys.problem(updatedProblem.id), updatedProblem);
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
  const { isLoading: isAuthLoading } = useAuth();

  return useQuery({
    queryKey: queryKeys.driverTemplates,
    queryFn: driverTemplatesApi.getDriverTemplates,
    enabled: !isAuthLoading, // Wait for auth initialization
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
  const { isLoading: isAuthLoading } = useAuth();

  return useQuery({
    queryKey: queryKeys.userSubmissions(userId, problemId),
    queryFn: () => submissionsApi.getUserSubmissionsForProblem(userId, problemId, queryParams),
    enabled: !!userId && !!problemId && !isAuthLoading, // Wait for auth initialization
    staleTime: 30 * 60 * 1000 // 30 minutes
  });
};

export const useProblemSubmission = (problemId: number, submissionId: number) => {
  const { isLoading: isAuthLoading } = useAuth();

  return useQuery({
    queryKey: queryKeys.problemSubmission(problemId, submissionId),
    queryFn: () => submissionsApi.getProblemSubmission(problemId, submissionId),
    enabled: !!problemId && !!submissionId && !isAuthLoading, // Wait for auth initialization
    staleTime: 30 * 60 * 1000 // 30 minutes (submissions don't change)
  });
};

// Article comment hooks
export const useArticleComments = (articleId: number, queryParams?: Partial<ArticleCommentQueryParameters>) => {
  const { isLoading: isAuthLoading } = useAuth();

  return useQuery({
    queryKey: queryKeys.articleComments(articleId, queryParams),
    queryFn: () => articlesApi.getArticleComments(articleId, queryParams),
    enabled: !!articleId && !isAuthLoading,
    staleTime: 5 * 60 * 1000 // 5 minutes (comments may change more frequently)
  });
};

export const useArticleCommentReplies = (
  articleId: number,
  commentId: number,
  queryParams?: Partial<ArticleCommentQueryParameters>,
  options?: { enabled?: boolean }
) => {
  const { isLoading: isAuthLoading } = useAuth();

  return useQuery({
    queryKey: queryKeys.articleCommentReplies(articleId, commentId, queryParams),
    queryFn: () => articlesApi.getArticleCommentReplies(articleId, commentId, queryParams),
    enabled: !!articleId && !!commentId && !isAuthLoading && (options?.enabled ?? true),
    staleTime: 5 * 60 * 1000 // 5 minutes
  });
};

export const useArticleCommentQuotedBy = (
  articleId: number,
  commentId: number,
  queryParams?: Partial<ArticleCommentQueryParameters>
) => {
  const { isLoading: isAuthLoading } = useAuth();

  return useQuery({
    queryKey: queryKeys.articleCommentQuotedBy(articleId, commentId, queryParams),
    queryFn: () => articlesApi.getArticleCommentQuotedBy(articleId, commentId, queryParams),
    enabled: !!articleId && !!commentId && !isAuthLoading,
    staleTime: 5 * 60 * 1000 // 5 minutes
  });
};

export const useArticleComment = (articleId: number, commentId: number) => {
  const { isLoading: isAuthLoading } = useAuth();

  return useQuery({
    queryKey: queryKeys.articleComment(articleId, commentId),
    queryFn: () => articlesApi.getArticleComment(articleId, commentId),
    enabled: !!articleId && !!commentId && !isAuthLoading,
    staleTime: 10 * 60 * 1000 // 10 minutes (individual comments change less frequently)
  });
};

export const useCreateArticleComment = (articleId: number) => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (comment: CreateArticleCommentRequest) => articlesApi.createArticleComment(articleId, comment),
    onSuccess: newComment => {
      // Invalidate all article comments queries for this article (all pagination states)
      queryClient.invalidateQueries({
        queryKey: ['articles', articleId, 'comments'],
        exact: false // This will match all queries that start with this pattern
      });

      // If this is a reply, also invalidate the parent comment's replies (all pagination states)
      if (newComment.parentCommentId) {
        queryClient.invalidateQueries({
          queryKey: ['articles', articleId, 'comments', newComment.parentCommentId, 'replies'],
          exact: false
        });
      }

      // Set the new comment in cache
      queryClient.setQueryData(queryKeys.articleComment(articleId, newComment.id), newComment);
    }
  });
};
