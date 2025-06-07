import { ApiError } from '../types/errors';

export function getErrorMessage(error: unknown, defaultErrorMessage?: string): string {
  if (error instanceof ApiError) {
    return error.allErrors.length > 0 ? error.allErrors.join(', ') : error.displayMessage || 'An API error occurred';
  } else if (error instanceof Error) {
    return error.message;
  } else if (typeof error === 'string') {
    return error;
  } else if (error && typeof error === 'object' && 'message' in error) {
    return (error as { message: string }).message;
  }
  return defaultErrorMessage ?? 'An unknown error occurred';
}
