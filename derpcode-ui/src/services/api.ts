import {
  type Problem,
  type CursorPaginatedResponse,
  type DriverTemplate,
  type CreateProblemRequest,
  type CreateProblemValidationResponse,
  type Language,
  type ProblemSubmission,
  type UserSubmissionQueryParameters,
  type ArticleComment,
  type CreateArticleCommentRequest,
  type ArticleCommentQueryParameters
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

  async validateProblem(problem: CreateProblemRequest): Promise<CreateProblemValidationResponse> {
    const response = await apiClient.post<CreateProblemValidationResponse>('/api/v1/problems/validate', problem);
    return response.data;
  },

  async createProblem(problem: CreateProblemRequest): Promise<Problem> {
    const response = await apiClient.post<Problem>('/api/v1/problems', problem);
    return response.data;
  },

  async updateProblem(problemId: number, problem: CreateProblemRequest): Promise<Problem> {
    const response = await apiClient.put<Problem>(`/api/v1/problems/${problemId}`, problem);
    return response.data;
  },

  async deleteProblem(problemId: number): Promise<void> {
    await apiClient.delete(`/api/v1/problems/${problemId}`);
  },

  async cloneProblem(problemId: number): Promise<Problem> {
    const response = await apiClient.post<Problem>(`/api/v1/problems/${problemId}/clone`);
    return response.data;
  },

  async submitSolution(problemId: number, userCode: string, language: Language): Promise<ProblemSubmission> {
    const response = await apiClient.post<ProblemSubmission>(`/api/v1/problems/${problemId}/submissions`, {
      userCode,
      language
    });
    return response.data;
  },

  async runSolution(problemId: number, userCode: string, language: Language): Promise<ProblemSubmission> {
    const response = await apiClient.post<ProblemSubmission>(`/api/v1/problems/${problemId}/run`, {
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

export const submissionsApi = {
  async getUserSubmissionsForProblem(
    userId: number,
    problemId: number,
    queryParams?: Partial<UserSubmissionQueryParameters>
  ): Promise<CursorPaginatedResponse<ProblemSubmission>> {
    const params = new URLSearchParams();
    params.append('problemId', problemId.toString());

    if (queryParams?.first) params.append('first', queryParams.first.toString());
    if (queryParams?.last) params.append('last', queryParams.last.toString());
    if (queryParams?.after) params.append('after', queryParams.after);
    if (queryParams?.before) params.append('before', queryParams.before);
    if (queryParams?.includeTotal) params.append('includeTotal', queryParams.includeTotal.toString());
    if (queryParams?.includeNodes) params.append('includeNodes', queryParams.includeNodes.toString());
    if (queryParams?.includeEdges) params.append('includeEdges', queryParams.includeEdges.toString());

    const response = await apiClient.get<CursorPaginatedResponse<ProblemSubmission>>(
      `/api/v1/users/${userId}/submissions?${params.toString()}`
    );
    return response.data;
  },

  async getProblemSubmission(problemId: number, submissionId: number): Promise<ProblemSubmission> {
    const response = await apiClient.get<ProblemSubmission>(
      `/api/v1/problems/${problemId}/submissions/${submissionId}`
    );
    return response.data;
  }
};

export const articlesApi = {
  async getArticleComments(
    articleId: number,
    queryParams?: Partial<ArticleCommentQueryParameters>
  ): Promise<CursorPaginatedResponse<ArticleComment>> {
    const params = new URLSearchParams();

    if (queryParams?.first) params.append('first', queryParams.first.toString());
    if (queryParams?.last) params.append('last', queryParams.last.toString());
    if (queryParams?.after) params.append('after', queryParams.after);
    if (queryParams?.before) params.append('before', queryParams.before);
    if (queryParams?.includeTotal) params.append('includeTotal', queryParams.includeTotal.toString());
    if (queryParams?.includeNodes) params.append('includeNodes', queryParams.includeNodes.toString());
    if (queryParams?.includeEdges) params.append('includeEdges', queryParams.includeEdges.toString());

    const response = await apiClient.get<CursorPaginatedResponse<ArticleComment>>(
      `/api/v1/articles/${articleId}/comments?${params.toString()}`
    );
    return response.data;
  },

  async getArticleCommentReplies(
    articleId: number,
    commentId: number,
    queryParams?: Partial<ArticleCommentQueryParameters>
  ): Promise<CursorPaginatedResponse<ArticleComment>> {
    const params = new URLSearchParams();

    if (queryParams?.first) params.append('first', queryParams.first.toString());
    if (queryParams?.last) params.append('last', queryParams.last.toString());
    if (queryParams?.after) params.append('after', queryParams.after);
    if (queryParams?.before) params.append('before', queryParams.before);
    if (queryParams?.includeTotal) params.append('includeTotal', queryParams.includeTotal.toString());
    if (queryParams?.includeNodes) params.append('includeNodes', queryParams.includeNodes.toString());
    if (queryParams?.includeEdges) params.append('includeEdges', queryParams.includeEdges.toString());

    const response = await apiClient.get<CursorPaginatedResponse<ArticleComment>>(
      `/api/v1/articles/${articleId}/comments/${commentId}/replies?${params.toString()}`
    );
    return response.data;
  },

  async getArticleCommentQuotedBy(
    articleId: number,
    commentId: number,
    queryParams?: Partial<ArticleCommentQueryParameters>
  ): Promise<CursorPaginatedResponse<ArticleComment>> {
    const params = new URLSearchParams();

    if (queryParams?.first) params.append('first', queryParams.first.toString());
    if (queryParams?.last) params.append('last', queryParams.last.toString());
    if (queryParams?.after) params.append('after', queryParams.after);
    if (queryParams?.before) params.append('before', queryParams.before);
    if (queryParams?.includeTotal) params.append('includeTotal', queryParams.includeTotal.toString());
    if (queryParams?.includeNodes) params.append('includeNodes', queryParams.includeNodes.toString());
    if (queryParams?.includeEdges) params.append('includeEdges', queryParams.includeEdges.toString());

    const response = await apiClient.get<CursorPaginatedResponse<ArticleComment>>(
      `/api/v1/articles/${articleId}/comments/${commentId}/quotedBy?${params.toString()}`
    );
    return response.data;
  },

  async createArticleComment(articleId: number, comment: CreateArticleCommentRequest): Promise<ArticleComment> {
    const response = await apiClient.post<ArticleComment>(`/api/v1/articles/${articleId}/comments`, comment);
    return response.data;
  },

  async getArticleComment(articleId: number, commentId: number): Promise<ArticleComment> {
    const response = await apiClient.get<ArticleComment>(`/api/v1/articles/${articleId}/comments/${commentId}`);
    return response.data;
  }
};
