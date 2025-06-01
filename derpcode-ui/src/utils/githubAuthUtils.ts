/**
 * GitHub OAuth utility functions
 */

const GITHUB_CLIENT_ID = import.meta.env.VITE_GITHUB_CLIENT_ID;
const API_BASE_URL = import.meta.env.VITE_DERPCODE_API_BASE_URL;
const GITHUB_OAUTH_SCOPE = 'read:user user:email';

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
  sessionStorage.setItem('github_oauth_state', state);
}

/**
 * Retrieves and removes OAuth state from sessionStorage
 */
function getAndClearOAuthState(): string | null {
  const state = sessionStorage.getItem('github_oauth_state');
  sessionStorage.removeItem('github_oauth_state');
  return state;
}

/**
 * Redirects to GitHub OAuth authorization page
 */
export function redirectToGitHubOAuth(): void {
  if (!GITHUB_CLIENT_ID) {
    throw new Error('GitHub Client ID not configured. Please set VITE_GITHUB_CLIENT_ID in your environment variables.');
  }

  const state = generateState();
  storeOAuthState(state);

  const params = new URLSearchParams({
    client_id: GITHUB_CLIENT_ID,
    redirect_uri: `${API_BASE_URL}/api/v1/auth/github/callback`,
    scope: GITHUB_OAUTH_SCOPE,
    state
  });

  window.location.href = `https://github.com/login/oauth/authorize?${params.toString()}`;
}

/**
 * Handles GitHub OAuth callback and extracts the authorization code from the current URL
 * This works with hash-based routing by parsing the fragment parameters after the hash
 * @returns Object containing code and any error, or null if invalid
 */
export function handleGitHubCallbackFromUrl(): { code?: string; error?: string; errorDescription?: string } | null {
  // Parse the fragment part after the hash (e.g., #/auth/github/callback?code=xyz&state=abc)
  const hash = window.location.hash;

  // Extract the query string part from the hash fragment
  const queryStringIndex = hash.indexOf('?');
  if (queryStringIndex === -1) {
    return null;
  }

  const queryString = hash.substring(queryStringIndex + 1);
  const searchParams = new URLSearchParams(queryString);
  return handleGitHubCallback(searchParams);
}

/**
 * Handles GitHub OAuth callback and extracts the authorization code
 * @param searchParams - URL search parameters from the callback
 * @returns Object containing code and any error, or null if invalid
 */
export function handleGitHubCallback(
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
