import {
  buildDefaultUserPreferences,
  loadUserPreferencesFromLocalStorage,
  saveUserPreferencesToLocalStorage
} from '../userPreferencesStorage';

describe('userPreferencesStorage', () => {
  const userId = 123;

  beforeEach(() => {
    localStorage.clear();
  });

  test('save/load roundtrip', () => {
    const defaults = buildDefaultUserPreferences(userId);
    saveUserPreferencesToLocalStorage(userId, defaults);

    const loaded = loadUserPreferencesFromLocalStorage(userId);

    expect(loaded).not.toBeNull();
    expect(loaded!.userId).toBe(userId);
    expect(loaded!.preferences.uiPreference.pageSize).toBe(5);
  });

  test('corrupted JSON returns null and clears key', () => {
    const key = `derpcode_user_preferences:${userId}`;
    localStorage.setItem(key, '{not-json');

    const loaded = loadUserPreferencesFromLocalStorage(userId);
    expect(loaded).toBeNull();
    expect(localStorage.getItem(key)).toBeNull();
  });
});
