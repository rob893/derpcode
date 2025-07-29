import { useCallback, useEffect, useRef } from 'react';
import { Language } from '../types/models';
import { saveCodeToLocalStorage } from '../utils/localStorageUtils';

/**
 * Custom hook for debounced auto-save functionality
 * @param code - Current code content
 * @param userId - User ID (null for non-authenticated users)
 * @param problemId - Problem ID
 * @param language - Programming language
 * @param delay - Delay in milliseconds (default: 3000ms)
 */
export function useAutoSave(
  code: string,
  userId: number | null,
  problemId: number,
  language: Language,
  delay: number = 3000
) {
  const timeoutRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const lastSavedCodeRef = useRef<string>('');

  const saveCode = useCallback(() => {
    if (code && code !== lastSavedCodeRef.current) {
      saveCodeToLocalStorage(userId, problemId, language, code);
      lastSavedCodeRef.current = code;
      // TODO: Implement logger and move to that
      // console.log('Code auto-saved to local storage');
    }
  }, [code, userId, problemId, language]);

  useEffect(() => {
    // Clear existing timeout
    if (timeoutRef.current) {
      clearTimeout(timeoutRef.current);
    }

    // Only set timeout if code has changed and is not empty
    if (code && code !== lastSavedCodeRef.current) {
      timeoutRef.current = setTimeout(() => {
        saveCode();
      }, delay);
    }

    // Cleanup function
    return () => {
      if (timeoutRef.current) {
        clearTimeout(timeoutRef.current);
      }
    };
  }, [code, saveCode, delay]);

  // Cleanup on unmount
  useEffect(() => {
    return () => {
      if (timeoutRef.current) {
        clearTimeout(timeoutRef.current);
      }
    };
  }, []);

  // Force save function for immediate saves
  const forceSave = useCallback(() => {
    if (timeoutRef.current) {
      clearTimeout(timeoutRef.current);
    }
    saveCode();
  }, [saveCode]);

  return { forceSave };
}
