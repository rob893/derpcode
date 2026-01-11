import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  problemsApi,
  driverTemplatesApi,
  submissionsApi,
  articlesApi,
  tagsApi,
  userFavoritesApi
} from '../services/api';
import { authApi } from '../services/auth';
import { useAuth } from './useAuth';
import type {
  CreateProblemRequest,
  Language,
  UserSubmissionQueryParameters,
  ProblemQueryParameters,
  CreateArticleCommentRequest,
  ArticleCommentQueryParameters,
  JsonPatchDocument,
  CursorPaginationQueryParameters,
  UserFavoriteProblem
} from '../types/models';
import type { LoginRequest, RegisterRequest } from '../types/auth';

// Query Keys
export const queryKeys = {
  problemsLimited: (queryParams?: Partial<ProblemQueryParameters>) => ['problems', 'limited', queryParams] as const,
  problemsCount: ['problems', 'count'] as const,
  problem: (id: number) => ['problems', id] as const,
  driverTemplates: ['driverTemplates'] as const,
  userSubmissions: (userId: number, problemId?: number) => ['users', userId, 'submissions', problemId] as const,
  userFavoriteProblems: (userId: number) => ['users', userId, 'favoriteProblems'] as const,
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
  articleComment: (articleId: number, commentId: number) => ['articles', articleId, 'comments', commentId] as const,
  tags: (queryParams?: Partial<CursorPaginationQueryParameters>) => ['tags', queryParams] as const,
  tag: (id: number) => ['tags', id] as const
} as const;

// Favorites hooks
export const useUserFavoriteProblems = (userId?: number) => {
  const { isLoading: isAuthLoading, isAuthenticated } = useAuth();

  return useQuery({
    queryKey: queryKeys.userFavoriteProblems(userId || 0),
    queryFn: () => userFavoritesApi.getFavoriteProblemsForUser(userId!),
    enabled: !!userId && isAuthenticated && !isAuthLoading,
    staleTime: 5 * 60 * 1000
  });
};

export const useFavoriteProblemForUser = (userId: number) => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (problemId: number) => userFavoritesApi.favoriteProblemForUser(userId, problemId),
    onMutate: async problemId => {
      await queryClient.cancelQueries({ queryKey: queryKeys.userFavoriteProblems(userId) });

      const previous = queryClient.getQueryData<UserFavoriteProblem[]>(queryKeys.userFavoriteProblems(userId));

      const alreadyExists = previous?.some(f => f.problemId === problemId) ?? false;
      if (!alreadyExists) {
        const optimistic: UserFavoriteProblem = {
          userId,
          problemId,
          createdAt: new Date().toISOString()
        };

        queryClient.setQueryData<UserFavoriteProblem[]>(queryKeys.userFavoriteProblems(userId), [
          ...(previous ?? []),
          optimistic
        ]);
      }

      return { previous };
    },
    onError: (_err, _problemId, context) => {
      if (context?.previous) {
        queryClient.setQueryData(queryKeys.userFavoriteProblems(userId), context.previous);
      }
    },
    onSuccess: favorite => {
      queryClient.setQueryData<UserFavoriteProblem[]>(queryKeys.userFavoriteProblems(userId), current => {
        const existing = current || [];
        const withoutDup = existing.filter(f => f.problemId !== favorite.problemId);
        return [...withoutDup, favorite];
      });
    }
  });
};

export const useUnfavoriteProblemForUser = (userId: number) => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (problemId: number) => userFavoritesApi.unfavoriteProblemForUser(userId, problemId),
    onMutate: async problemId => {
      await queryClient.cancelQueries({ queryKey: queryKeys.userFavoriteProblems(userId) });

      const previous = queryClient.getQueryData<UserFavoriteProblem[]>(queryKeys.userFavoriteProblems(userId));

      if (previous) {
        queryClient.setQueryData<UserFavoriteProblem[]>(
          queryKeys.userFavoriteProblems(userId),
          previous.filter(f => f.problemId !== problemId)
        );
      }

      return { previous };
    },
    onError: (_err, _problemId, context) => {
      if (context?.previous) {
        queryClient.setQueryData(queryKeys.userFavoriteProblems(userId), context.previous);
      }
    }
  });
};

// Problem hooks
export const useProblemsCount = () => {
  return useQuery({
    queryKey: queryKeys.problemsCount,
    queryFn: () => problemsApi.getProblemsCount(),
    staleTime: 30 * 60 * 1000 // 30 minutes
  });
};

export const useProblemsLimitedPaginated = (queryParams?: Partial<ProblemQueryParameters>) => {
  const { isLoading: isAuthLoading } = useAuth();

  return useQuery({
    queryKey: queryKeys.problemsLimited(queryParams),
    queryFn: () => problemsApi.getProblemsLimited(queryParams),
    enabled: !isAuthLoading, // Wait for auth initialization
    staleTime: 30 * 60 * 1000, // 30 minutes for paginated data
    select: data => ({
      problems: data.nodes || data.edges?.map(edge => edge.node) || [],
      pageInfo: data.pageInfo,
      totalCount: data.pageInfo.totalCount ?? 0
    })
  });
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
      // Invalidate and refetch problems lists
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
      // Invalidate and refetch problems lists
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
      // Invalidate and refetch problems lists
      queryClient.invalidateQueries({ queryKey: ['problems'] });
    }
  });
};

export const useCloneProblem = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (problemId: number) => problemsApi.cloneProblem(problemId),
    onSuccess: newProblem => {
      // Invalidate and refetch problems lists
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
      // Invalidate and refetch problems lists
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

// Tag hooks
export const useTags = (queryParams?: Partial<CursorPaginationQueryParameters>) => {
  return useQuery({
    queryKey: queryKeys.tags(queryParams),
    queryFn: () => tagsApi.getTags(queryParams),
    staleTime: 60 * 60 * 1000, // 1 hour - tags don't change frequently
    select: data => ({
      tags: data.nodes || [],
      pageInfo: data.pageInfo,
      totalCount: data.pageInfo.totalCount ?? 0
    })
  });
};

export const useAllTags = () => {
  return useQuery({
    queryKey: ['tags', 'all'],
    queryFn: async () => {
      const allTags = [];
      let hasMore = true;
      let afterCursor: string | undefined = undefined;

      // Fetch tags in batches until we have all of them
      while (hasMore) {
        const response = await tagsApi.getTags({ first: 100, after: afterCursor });
        const tags = response.nodes || [];
        allTags.push(...tags);

        hasMore = response.pageInfo?.hasNextPage || false;
        afterCursor = response.pageInfo?.endCursor || undefined;
      }

      return allTags;
    },
    staleTime: 60 * 60 * 1000 // 1 hour - tags don't change frequently
  });
};

export const useTag = (id: number) => {
  return useQuery({
    queryKey: queryKeys.tag(id),
    queryFn: () => tagsApi.getTag(id),
    enabled: !!id,
    staleTime: 60 * 60 * 1000 // 1 hour
  });
};
