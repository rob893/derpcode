export enum ArticleType {
  ProblemSolution = 'ProblemSolution',
  News = 'News',
  BlogPost = 'BlogPost',
  Tutorial = 'Tutorial',
  Other = 'Other'
}

export interface ExplanationArticle {
  id: number;
  userId: number;
  title: string;
  content: string;
  upVotes: number;
  downVotes: number;
  createdAt: string;
  updatedAt: string;
  lastEditedById: number;
  type: ArticleType;
  tags: TagDto[];
}

export interface Problem {
  id: number;
  name: string;
  expectedOutput: any[];
  input: any[];
  tags: TagDto[];
  description: string;
  difficulty: ProblemDifficulty;
  drivers: ProblemDriver[];
  hints: string[];
  explanationArticle?: ExplanationArticle;
}

export enum Language {
  CSharp = 'CSharp',
  JavaScript = 'JavaScript',
  TypeScript = 'TypeScript'
}

export enum ProblemDifficulty {
  VeryEasy = 'VeryEasy',
  Easy = 'Easy',
  Medium = 'Medium',
  Hard = 'Hard',
  VeryHard = 'VeryHard'
}

export interface TagDto {
  id: number;
  name: string;
}

export interface ProblemDriver {
  id: number;
  problemId: number;
  language: Language;
  uiTemplate: string;
  image?: string;
  driverCode?: string;
  answer?: string;
}

export interface DriverTemplate {
  id: number;
  language: Language;
  template: string;
  uiTemplate: string;
}

export interface CursorPaginatedResponse<T> {
  edges?: CursorPaginatedResponseEdge<T>[];
  nodes?: T[];
  pageInfo: CursorPaginatedResponsePageInfo;
}

export interface CursorPaginatedResponseEdge<T> {
  cursor: string;
  node: T;
}

export interface CursorPaginatedResponsePageInfo {
  startCursor?: string;
  endCursor?: string;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
  pageCount: number;
  totalCount?: number;
}

export interface CreateExplanationArticleRequest {
  title: string;
  content: string;
}

export interface CreateProblemRequest {
  name: string;
  description: string;
  difficulty: ProblemDifficulty;
  explanationArticle: CreateExplanationArticleRequest;
  expectedOutput: any[];
  input: any[];
  hints: string[];
  tags: CreateTagRequest[];
  drivers: CreateProblemDriverRequest[];
}

export interface CreateTagRequest {
  name: string;
}

export interface CreateProblemDriverRequest {
  uiTemplate: string;
  language: Language;
  image: string;
  driverCode: string;
  answer: string;
}

export interface CreateProblemValidationResponse {
  isValid: boolean;
  errorMessage?: string;
  driverValidations: CreateProblemDriverValidationResponse[];
}

export interface CreateProblemDriverValidationResponse {
  isValid: boolean;
  errorMessage?: string;
  language: Language;
  image: string;
  submissionResult: ProblemSubmission;
}

export interface AdminProblemDriverDto {
  id: number;
  problemId: number;
  language: Language;
  uiTemplate: string;
  image: string;
  driverCode: string;
  answer: string;
}

export interface ProblemSubmission {
  id: number;
  userId: number;
  problemId: number;
  language: Language;
  code: string;
  createdAt: string;
  pass: boolean;
  testCaseCount: number;
  passedTestCases: number;
  failedTestCases: number;
  errorMessage: string;
  executionTimeInMs: number;
  testCaseResults: TestCaseResult[];
}

export interface TestCaseResult {
  id: number;
  input: any;
  expectedOutput: any;
  actualOutput: any;
  pass: boolean;
  errorMessage: string;
}

export interface UserSubmissionQueryParameters {
  problemId?: number;
  after?: string;
  before?: string;
  first?: number;
  last?: number;
  includeTotal?: boolean;
  includeNodes?: boolean;
  includeEdges?: boolean;
}
