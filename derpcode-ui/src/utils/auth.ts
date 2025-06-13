import { jwtDecode } from 'jwt-decode';
import type { User } from '../types/auth';

interface JwtPayload {
  nameid: number; // user ID
  unique_name: string; // user name
  email: string;
  email_verified: boolean;
  role?: string | string[]; // roles can be single string or array
  exp: number; // expiration time
  iat: number; // issued at
  nbf: number; // not before
  iss: string; // issuer
  aud: string; // audience
}

export function decodeJwtToken(token: string): User | null {
  try {
    const decoded = jwtDecode<JwtPayload>(token);

    // Extract roles - can be single role or array of roles
    let roles: string[] = [];
    if (decoded.role) {
      roles = Array.isArray(decoded.role) ? decoded.role : [decoded.role];
    }

    const user = {
      id: decoded.nameid,
      userName: decoded.unique_name,
      email: decoded.email,
      roles
    };

    return user;
  } catch (error) {
    console.error('Failed to decode JWT token:', error);
    return null;
  }
}

export function hasAdminRole(user: User | null): boolean {
  const isAdmin = user?.roles?.includes('Admin') ?? false;
  return isAdmin;
}

export function hasPremiumUserRole(user: User | null): boolean {
  const isAdmin = user?.roles?.includes('PremiumUser') ?? false;
  return isAdmin;
}

export function isTokenExpired(token: string): boolean {
  try {
    const decoded = jwtDecode<JwtPayload>(token);
    const currentTime = Date.now() / 1000;
    return decoded.exp < currentTime;
  } catch {
    return true;
  }
}
