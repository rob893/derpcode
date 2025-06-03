/**
 * Google OAuth utility functions
 */

const GOOGLE_CLIENT_ID = import.meta.env.VITE_GOOGLE_CLIENT_ID;
const API_BASE_URL = import.meta.env.VITE_DERPCODE_API_BASE_URL;
const GOOGLE_OAUTH_SCOPE = 'openid email profile';

/**
 * Generates a random state string for OAuth security
 */
function generateState(): string {
  return crypto.randomUUID();
}

/**
 * Stores OAuth state in sessionStorage for verification
 */
function storeOAuthState(state: string): void {
  sessionStorage.setItem('google_oauth_state', state);
}

/**
 * Retrieves and removes OAuth state from sessionStorage
 */
function getAndClearOAuthState(): string | null {
  const state = sessionStorage.getItem('google_oauth_state');
  sessionStorage.removeItem('google_oauth_state');
  return state;
}

/**
 * Redirects to Google OAuth authorization page
 */
export function redirectToGoogleOAuth(): void {
  if (!GOOGLE_CLIENT_ID) {
    throw new Error('Google Client ID not configured. Please set VITE_GOOGLE_CLIENT_ID in your environment variables.');
  }

  const state = generateState();
  storeOAuthState(state);

  const params = new URLSearchParams({
    client_id: GOOGLE_CLIENT_ID,
    redirect_uri: `${API_BASE_URL}/api/v1/auth/google/callback`,
    response_type: 'code',
    scope: GOOGLE_OAUTH_SCOPE,
    state,
    access_type: 'offline',
    prompt: 'consent'
  });

  window.location.href = `https://accounts.google.com/o/oauth2/v2/auth?${params.toString()}`;
}

/**
 * Handles Google OAuth callback and extracts the authorization code from the current URL
 * This works with hash-based routing by parsing the fragment parameters after the hash
 * @returns Object containing code and any error, or null if invalid
 */
export function handleGoogleCallbackFromUrl(): { code?: string; error?: string; errorDescription?: string } | null {
  // Parse the fragment part after the hash (e.g., #/auth/google/callback?code=xyz&state=abc)
  const hash = window.location.hash;

  // Extract the query string part from the hash fragment
  const queryStringIndex = hash.indexOf('?');
  if (queryStringIndex === -1) {
    return null;
  }

  const queryString = hash.substring(queryStringIndex + 1);
  const searchParams = new URLSearchParams(queryString);
  return handleGoogleCallback(searchParams);
}

/**
 * Handles Google OAuth callback and extracts the authorization code
 * @param searchParams - URL search parameters from the callback
 * @returns Object containing code and any error, or null if invalid
 */
export function handleGoogleCallback(
  searchParams: URLSearchParams
): { code?: string; error?: string; errorDescription?: string } | null {
  const code = searchParams.get('code');
  const state = searchParams.get('state');
  const error = searchParams.get('error');
  const errorDescription = searchParams.get('error_description');

  // Check for OAuth errors
  if (error) {
    return {
      error,
      errorDescription: errorDescription || undefined
    };
  }

  // Verify state parameter for CSRF protection
  const storedState = getAndClearOAuthState();
  if (!state || state !== storedState) {
    return {
      error: 'invalid_state',
      errorDescription: 'OAuth state verification failed. This may be a CSRF attack.'
    };
  }

  // Return the authorization code if everything is valid
  if (code) {
    return { code };
  }

  return {
    error: 'missing_code',
    errorDescription: 'Authorization code not found in callback.'
  };
}
