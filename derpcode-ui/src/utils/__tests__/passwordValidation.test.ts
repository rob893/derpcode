import { getPasswordRequirementsDescription, validatePassword } from '../passwordValidation';

describe('passwordValidation', () => {
  test('validatePassword returns errors for missing requirements', () => {
    const result = validatePassword('abc');
    expect(result.isValid).toBe(false);
    expect(result.errors).toEqual(
      expect.arrayContaining([
        'Password must be at least 8 characters long',
        'Password must contain at least 1 number',
        'Password must contain at least 1 special character'
      ])
    );
  });

  test('validatePassword accepts valid password', () => {
    const result = validatePassword('abc123!!');
    expect(result.isValid).toBe(true);
    expect(result.errors).toEqual([]);
  });

  test('getPasswordRequirementsDescription is stable', () => {
    expect(getPasswordRequirementsDescription()).toBe('8+ characters, 1+ number, 1+ special character');
  });
});
