import { decodeJwtToken, hasAdminRole, hasPremiumUserRole, isTokenExpired } from '../auth';
import { jest } from '@jest/globals';

function base64UrlEncode(obj: unknown): string {
  const json = JSON.stringify(obj);
  return Buffer.from(json).toString('base64').replace(/=/g, '').replace(/\+/g, '-').replace(/\//g, '_');
}

function makeJwt(payload: Record<string, unknown>): string {
  const header = base64UrlEncode({ alg: 'none', typ: 'JWT' });
  const body = base64UrlEncode(payload);
  return `${header}.${body}.`;
}

describe('auth utils', () => {
  test('decodeJwtToken extracts user and roles (string role)', () => {
    const token = makeJwt({
      nameid: 123,
      unique_name: 'alice',
      email: 'alice@example.com',
      email_verified: true,
      role: 'Admin',
      exp: Math.floor(Date.now() / 1000) + 3600,
      iat: 0,
      nbf: 0,
      iss: 'issuer',
      aud: 'aud'
    });

    const user = decodeJwtToken(token);
    expect(user).not.toBeNull();
    expect(user?.id).toBe(123);
    expect(user?.userName).toBe('alice');
    expect(user?.email).toBe('alice@example.com');
    expect(user?.roles).toEqual(['Admin']);
  });

  test('decodeJwtToken extracts roles (array role)', () => {
    const token = makeJwt({
      nameid: 1,
      unique_name: 'bob',
      email: 'bob@example.com',
      email_verified: true,
      role: ['PremiumUser', 'Admin'],
      exp: Math.floor(Date.now() / 1000) + 3600,
      iat: 0,
      nbf: 0,
      iss: 'issuer',
      aud: 'aud'
    });

    const user = decodeJwtToken(token);
    expect(user?.roles).toEqual(['PremiumUser', 'Admin']);
  });

  test('decodeJwtToken returns null on invalid token', () => {
    const errorSpy = jest.spyOn(console, 'error').mockImplementation(() => undefined);

    const user = decodeJwtToken('not-a-jwt');
    expect(user).toBeNull();

    errorSpy.mockRestore();
  });

  test('role helpers behave correctly', () => {
    expect(hasAdminRole(null)).toBe(false);
    expect(hasPremiumUserRole(null)).toBe(false);

    const admin = { id: 1, userName: 'a', email: 'a@a.com', roles: ['Admin'] };
    expect(hasAdminRole(admin)).toBe(true);
    expect(hasPremiumUserRole(admin)).toBe(false);

    const premium = { id: 2, userName: 'p', email: 'p@p.com', roles: ['PremiumUser'] };
    expect(hasAdminRole(premium)).toBe(false);
    expect(hasPremiumUserRole(premium)).toBe(true);
  });

  test('isTokenExpired detects expiration', () => {
    jest.useFakeTimers();
    jest.setSystemTime(new Date('2025-01-01T00:00:00.000Z'));

    const now = Math.floor(Date.now() / 1000);
    const expiredToken = makeJwt({
      nameid: 1,
      unique_name: 'x',
      email: 'x@x.com',
      email_verified: true,
      exp: now - 1,
      iat: 0,
      nbf: 0,
      iss: 'issuer',
      aud: 'aud'
    });

    const validToken = makeJwt({
      nameid: 1,
      unique_name: 'x',
      email: 'x@x.com',
      email_verified: true,
      exp: now + 3600,
      iat: 0,
      nbf: 0,
      iss: 'issuer',
      aud: 'aud'
    });

    expect(isTokenExpired(expiredToken)).toBe(true);
    expect(isTokenExpired(validToken)).toBe(false);

    jest.useRealTimers();
  });
});
