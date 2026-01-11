import { isLanguage } from '../typeGuards';
import { Language } from '../../types/models';

describe('typeGuards', () => {
  test('isLanguage returns true for enum values', () => {
    for (const lang of Object.values(Language)) {
      expect(isLanguage(lang)).toBe(true);
    }
  });

  test('isLanguage returns false for non-language strings', () => {
    expect(isLanguage('NotALanguage')).toBe(false);
  });
});
