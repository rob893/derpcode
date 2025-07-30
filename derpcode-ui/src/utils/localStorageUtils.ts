import { Language } from '../types/models';
import { isLanguage } from './typeGuards';

// Constants for local storage operations
const LOCAL_STORAGE_AUTOSAVE_PREFIX = 'derpcode_autosave';

const LOCAL_STORAGE_RECENT_LANG_PREFIX = 'derpcode_recent_language';

// Interface for auto-save data
export interface AutoSaveData {
  code: string;
  timestamp: number;
  language: Language;
}

/**
 * Generates a local storage key for auto-saved code
 * Format: {userId}:{problemId}:{language}
 * @param userId - User ID (use empty string for non-authenticated users)
 * @param problemId - Problem ID
 * @param language - Programming language
 * @returns Formatted local storage key
 */
export function generateProblemCodeStorageKey(
  userId: number | string | null,
  problemId: number,
  language: Language
): string {
  const userIdStr = userId?.toString() || '';
  return `${LOCAL_STORAGE_AUTOSAVE_PREFIX}:${userIdStr}:${problemId}:${language}`;
}

export function generateRecentLanguageStorageKey(userId: number | string | null): string {
  const userIdStr = userId?.toString() || '';
  return `${LOCAL_STORAGE_RECENT_LANG_PREFIX}:${userIdStr}`;
}

export function loadRecentLanguageFromLocalStorage(userId: number | string | null): Language | null {
  try {
    const key = generateRecentLanguageStorageKey(userId);
    const stored = localStorage.getItem(key);

    if (!stored) return null;

    return isLanguage(stored) ? stored : null;
  } catch (error) {
    console.warn('Failed to load code from local storage:', error);
    return null;
  }
}

/**
 * Saves code to local storage with timestamp
 * @param userId - User ID (use null for non-authenticated users)
 * @param problemId - Problem ID
 * @param language - Programming language
 * @param code - Code to save
 */
export function saveCodeToLocalStorage(
  userId: number | string | null,
  problemId: number,
  language: Language,
  code: string
): void {
  try {
    const key = generateProblemCodeStorageKey(userId, problemId, language);
    const data: AutoSaveData = {
      code,
      timestamp: Date.now(),
      language
    };
    localStorage.setItem(key, JSON.stringify(data));
    const recentLangKey = generateRecentLanguageStorageKey(userId);
    localStorage.setItem(recentLangKey, language);
  } catch (error) {
    console.warn('Failed to save code to local storage:', error);
  }
}

/**
 * Loads code from local storage
 * @param userId - User ID (use null for non-authenticated users)
 * @param problemId - Problem ID
 * @param language - Programming language
 * @returns Saved code or null if not found
 */
export function loadCodeFromLocalStorage(
  userId: number | string | null,
  problemId: number,
  language: Language
): string | null {
  try {
    const key = generateProblemCodeStorageKey(userId, problemId, language);
    const stored = localStorage.getItem(key);
    if (!stored) return null;

    const data: AutoSaveData = JSON.parse(stored);
    return data.code;
  } catch (error) {
    console.warn('Failed to load code from local storage:', error);
    return null;
  }
}

/**
 * Checks for saved code with priority logic:
 * 1. Authenticated user's saved code takes priority
 * 2. Falls back to non-authenticated user's saved code if authenticated code doesn't exist
 * @param authenticatedUserId - Current authenticated user ID (null if not authenticated)
 * @param problemId - Problem ID
 * @param language - Programming language
 * @returns Object containing the saved code and whether it came from authenticated user
 */
export function loadCodeWithPriority(
  authenticatedUserId: number | null,
  problemId: number,
  language: Language
): { code: string | null; isFromAuthenticatedUser: boolean } {
  // First, try to load authenticated user's code if user is logged in
  if (authenticatedUserId) {
    const authenticatedCode = loadCodeFromLocalStorage(authenticatedUserId, problemId, language);
    if (authenticatedCode) {
      return { code: authenticatedCode, isFromAuthenticatedUser: true };
    }
  }

  // If no authenticated code exists, try to load non-authenticated code
  const nonAuthenticatedCode = loadCodeFromLocalStorage(null, problemId, language);
  return { code: nonAuthenticatedCode, isFromAuthenticatedUser: false };
}

/**
 * Removes saved code from local storage
 * @param userId - User ID (use null for non-authenticated users)
 * @param problemId - Problem ID
 * @param language - Programming language
 */
export function removeCodeFromLocalStorage(
  userId: number | string | null,
  problemId: number,
  language: Language
): void {
  try {
    const key = generateProblemCodeStorageKey(userId, problemId, language);
    localStorage.removeItem(key);
  } catch (error) {
    console.warn('Failed to remove code from local storage:', error);
  }
}

/**
 * Gets all auto-save keys for a specific problem
 * @param problemId - Problem ID
 * @returns Array of storage keys that match the problem
 */
export function getAutoSaveKeysForProblem(problemId: number): string[] {
  try {
    const keys: string[] = [];
    for (let i = 0; i < localStorage.length; i++) {
      const key = localStorage.key(i);
      if (key && key.startsWith(`${LOCAL_STORAGE_AUTOSAVE_PREFIX}:`) && key.includes(`:${problemId}:`)) {
        keys.push(key);
      }
    }
    return keys;
  } catch (error) {
    console.warn('Failed to get auto-save keys:', error);
    return [];
  }
}

/**
 * Cleans up old auto-save data (older than specified days)
 * @param daysToKeep - Number of days to keep auto-save data (default: 30)
 */
export function cleanupOldAutoSaveData(daysToKeep: number = 30): void {
  try {
    const cutoffTime = Date.now() - daysToKeep * 24 * 60 * 60 * 1000;
    const keysToRemove: string[] = [];

    for (let i = 0; i < localStorage.length; i++) {
      const key = localStorage.key(i);
      if (key && key.startsWith(`${LOCAL_STORAGE_AUTOSAVE_PREFIX}:`)) {
        try {
          const stored = localStorage.getItem(key);
          if (stored) {
            const data: AutoSaveData = JSON.parse(stored);
            if (data.timestamp < cutoffTime) {
              keysToRemove.push(key);
            }
          }
        } catch {
          // If we can't parse the data, it's probably corrupted, so remove it
          keysToRemove.push(key);
        }
      }
    }

    keysToRemove.forEach(key => localStorage.removeItem(key));

    if (keysToRemove.length > 0) {
      console.log(`Cleaned up ${keysToRemove.length} old auto-save entries`);
    }
  } catch (error) {
    console.warn('Failed to cleanup old auto-save data:', error);
  }
}
