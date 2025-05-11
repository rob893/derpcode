export interface Problem {
  id: string;
  name: string;
  description: string;
  difficulty: 'easy' | 'medium' | 'hard';
  expectedOutput: any[];
  tags: string[];
  input: any[];
  drivers: ProblemDriver[];
}

export enum Language {
  CSharp = 'csharp',
  JavaScript = 'javascript',
  TypeScript = 'typescript'
}

export interface ProblemDriver {
  id: string;
  language: Language;
  image: string;
  uiTemplate: string;
  driverCode: string;
}

export interface DriverTemplate {
  id: string;
  language: Language;
  template: string;
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
