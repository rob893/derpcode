import { Language } from '../types/models';
import { UITheme, type UserPreferencesDto } from '../types/userPreferences';

const LOCAL_STORAGE_USER_PREFERENCES_PREFIX = 'derpcode_user_preferences';

function getUserPreferencesStorageKey(userId: number): string {
  return `${LOCAL_STORAGE_USER_PREFERENCES_PREFIX}:${userId}`;
}

export function buildDefaultUserPreferences(userId: number): UserPreferencesDto {
  return {
    id: 0,
    userId,
    lastUpdated: new Date().toISOString(),
    preferences: {
      uiPreference: {
        uiTheme: UITheme.Dark,
        pageSize: 5
      },
      codePreference: {
        defaultLanguage: Language.JavaScript
      },
      editorPreference: {
        enableFlameEffects: true
      }
    }
  };
}

export function loadUserPreferencesFromLocalStorage(userId: number): UserPreferencesDto | null {
  try {
    const key = getUserPreferencesStorageKey(userId);
    const stored = localStorage.getItem(key);
    if (!stored) return null;

    return JSON.parse(stored) as UserPreferencesDto;
  } catch (error) {
    console.warn('Failed to load user preferences from local storage:', error);

    try {
      const key = getUserPreferencesStorageKey(userId);
      localStorage.removeItem(key);
    } catch {
      // ignore
    }

    return null;
  }
}

export function saveUserPreferencesToLocalStorage(userId: number, dto: UserPreferencesDto): void {
  try {
    const key = getUserPreferencesStorageKey(userId);
    localStorage.setItem(key, JSON.stringify(dto));
  } catch (error) {
    console.warn('Failed to save user preferences to local storage:', error);
  }
}
