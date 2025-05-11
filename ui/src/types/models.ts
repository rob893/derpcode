export interface Problem {
  id: string;
  name: string;
  expectedOutput: string;
  input: string;
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

export interface SubmissionResult {
  pass: boolean;
}
