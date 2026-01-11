import {
  cleanupOldAutoSaveData,
  generateProblemCodeStorageKey,
  generateRecentLanguageStorageKey,
  getAutoSaveKeysForProblem,
  loadCodeFromLocalStorage,
  loadCodeWithPriority,
  loadRecentLanguageFromLocalStorage,
  removeCodeFromLocalStorage,
  saveCodeToLocalStorage
} from '../localStorageUtils';
import { Language } from '../../types/models';
import { jest } from '@jest/globals';

describe('localStorageUtils', () => {
  beforeEach(() => {
    localStorage.clear();
    sessionStorage.clear();
  });

  test('generateProblemCodeStorageKey formats correctly', () => {
    expect(generateProblemCodeStorageKey(12, 99, Language.CSharp)).toBe('derpcode_autosave:12:99:CSharp');
    expect(generateProblemCodeStorageKey('abc', 99, Language.CSharp)).toBe('derpcode_autosave:abc:99:CSharp');
    expect(generateProblemCodeStorageKey(null, 99, Language.CSharp)).toBe('derpcode_autosave::99:CSharp');
  });

  test('save/load/remove code roundtrip', () => {
    saveCodeToLocalStorage(1, 10, Language.TypeScript, 'code');
    expect(loadCodeFromLocalStorage(1, 10, Language.TypeScript)).toBe('code');

    removeCodeFromLocalStorage(1, 10, Language.TypeScript);
    expect(loadCodeFromLocalStorage(1, 10, Language.TypeScript)).toBeNull();
  });

  test('save stores recent language and loadRecentLanguage validates enum', () => {
    saveCodeToLocalStorage(2, 10, Language.Rust, 'code');
    expect(loadRecentLanguageFromLocalStorage(2)).toBe(Language.Rust);

    const badKey = generateRecentLanguageStorageKey(2);
    localStorage.setItem(badKey, 'NotALanguage');
    expect(loadRecentLanguageFromLocalStorage(2)).toBeNull();
  });

  test('loadCodeWithPriority prefers authenticated user when present', () => {
    saveCodeToLocalStorage(123, 7, Language.Python, 'auth');
    saveCodeToLocalStorage(null, 7, Language.Python, 'anon');

    const preferred = loadCodeWithPriority(123, 7, Language.Python);
    expect(preferred).toEqual({ code: 'auth', isFromAuthenticatedUser: true });

    const fallback = loadCodeWithPriority(999, 7, Language.Python);
    expect(fallback).toEqual({ code: 'anon', isFromAuthenticatedUser: false });
  });

  test('getAutoSaveKeysForProblem returns matching keys', () => {
    saveCodeToLocalStorage(1, 5, Language.Java, 'a');
    saveCodeToLocalStorage(2, 5, Language.CSharp, 'b');
    saveCodeToLocalStorage(3, 6, Language.CSharp, 'c');

    const keys = getAutoSaveKeysForProblem(5);
    expect(keys).toHaveLength(2);
    expect(keys.every(k => k.includes(':5:'))).toBe(true);
  });

  test('cleanupOldAutoSaveData removes old entries and corrupted entries', () => {
    jest.useFakeTimers();
    jest.setSystemTime(new Date('2025-01-31T00:00:00.000Z'));

    // New entry (kept)
    saveCodeToLocalStorage(1, 1, Language.CSharp, 'new');

    // Old entry (removed)
    const oldKey = generateProblemCodeStorageKey(1, 2, Language.CSharp);
    localStorage.setItem(
      oldKey,
      JSON.stringify({ code: 'old', timestamp: Date.now() - 40 * 24 * 60 * 60 * 1000, language: Language.CSharp })
    );

    // Corrupted entry (removed)
    const badKey = generateProblemCodeStorageKey(1, 3, Language.CSharp);
    localStorage.setItem(badKey, '{not-json');

    cleanupOldAutoSaveData(30);

    expect(localStorage.getItem(oldKey)).toBeNull();
    expect(localStorage.getItem(badKey)).toBeNull();

    // The new entry should still exist
    const keptKeys = Object.keys(localStorage).filter(k => k.startsWith('derpcode_autosave:'));
    expect(keptKeys.length).toBeGreaterThanOrEqual(1);

    jest.useRealTimers();
  });
});
