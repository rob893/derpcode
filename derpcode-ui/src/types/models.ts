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
}

export interface SubmissionResult {
  pass: boolean;
  testCaseCount: number;
  passedTestCases: number;
  failedTestCases: number;
  errorMessage: string;
  executionTimeInMs: number;
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

export interface CreateProblemRequest {
  name: string;
  description: string;
  difficulty: ProblemDifficulty;
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
  submissionResult: SubmissionResult;
}

export interface AdminProblemDto {
  id: number;
  name: string;
  expectedOutput: any[];
  input: any[];
  tags: TagDto[];
  description: string;
  difficulty: ProblemDifficulty;
  drivers: AdminProblemDriverDto[];
  hints: string[];
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
