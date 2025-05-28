// API Error types matching the ProblemDetailsWithErrors structure from the API
export interface ProblemDetailsError {
  type: string;
  title: string;
  status: number;
  detail: string;
  instance: string;
  extensions: {
    correlationId: string;
    errors: string[];
    traceId: string;
  };
}

// Enhanced error class that preserves ProblemDetails information
export class ApiError extends Error {
  public readonly status: number;
  public readonly statusText: string;
  public readonly correlationId?: string;
  public readonly errors: string[];
  public readonly traceId?: string;
  public readonly type?: string;
  public readonly instance?: string;

  constructor(message: string, status: number, statusText: string, problemDetails?: ProblemDetailsError) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.statusText = statusText;

    if (problemDetails) {
      this.correlationId = problemDetails.extensions?.correlationId;
      this.errors = problemDetails.extensions?.errors || [];
      this.traceId = problemDetails.extensions?.traceId;
      this.type = problemDetails.type;
      this.instance = problemDetails.instance;
    } else {
      this.errors = [];
    }
  }

  // Get the most relevant error message to display
  get displayMessage(): string {
    if (this.errors.length > 0) {
      return this.errors[0];
    }
    return this.message;
  }

  // Get all error messages
  get allErrors(): string[] {
    if (this.errors.length > 0) {
      return this.errors;
    }
    return [this.message];
  }
}
