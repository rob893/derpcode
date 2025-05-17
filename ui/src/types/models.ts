export interface Problem {
  id: string;
  name: string;
  expectedOutput: any[];
  input: any[];
  tags: string[];
  description: string;
  difficulty: 'easy' | 'medium' | 'hard';
  drivers: ProblemDriver[];
}

export enum Language {
  CSharp = 'CSharp',
  JavaScript = 'JavaScript',
  TypeScript = 'TypeScript'
}

export interface ProblemDriver {
  id: string;
  language: Language;
  image: string;
  driverCode: string;
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
  id: string;
  language: Language;
  template: string;
  uiTemplate: string;
}
