/**
 * Generic OAuth utility functions for multiple providers
 */

const API_BASE_URL = import.meta.env.VITE_DERPCODE_API_BASE_URL;

export type OAuthProvider = 'github' | 'google';

export interface OAuthCallbackResult {
  code?: string;
  error?: string;
  errorDescription?: string;
}

interface OAuthProviderConfig {
  clientId: string;
  authUrl: string;
  scope: string;
  stateKey: string;
  redirectPath: string;
  additionalParams?: Record<string, string>;
}

/**
 * OAuth provider configurations
 */
const OAUTH_PROVIDERS: Record<OAuthProvider, OAuthProviderConfig> = {
  github: {
    clientId: import.meta.env.VITE_GITHUB_CLIENT_ID,
    authUrl: 'https://github.com/login/oauth/authorize',
    scope: 'read:user user:email',
    stateKey: 'github_oauth_state',
    redirectPath: '/api/v1/auth/github/callback'
  },
  google: {
    clientId: import.meta.env.VITE_GOOGLE_CLIENT_ID,
    authUrl: 'https://accounts.google.com/o/oauth2/v2/auth',
    scope: 'openid email profile',
    stateKey: 'google_oauth_state',
    redirectPath: '/api/v1/auth/google/callback',
    additionalParams: {
      response_type: 'code',
      access_type: 'offline',
      prompt: 'consent'
    }
  }
};

/**
 * Generates a random state string for OAuth security
 */
function generateState(): string {
  return crypto.randomUUID();
}

/**
 * Stores OAuth state in sessionStorage for verification
 */
function storeOAuthState(provider: OAuthProvider, state: string): void {
  const config = OAUTH_PROVIDERS[provider];
  sessionStorage.setItem(config.stateKey, state);
}

/**
 * Retrieves and removes OAuth state from sessionStorage
 */
function getAndClearOAuthState(provider: OAuthProvider): string | null {
  const config = OAUTH_PROVIDERS[provider];
  const state = sessionStorage.getItem(config.stateKey);
  sessionStorage.removeItem(config.stateKey);
  return state;
}

/**
 * Redirects to OAuth authorization page for the specified provider
 */
export function redirectToOAuth(provider: OAuthProvider): void {
  const config = OAUTH_PROVIDERS[provider];

  if (!config.clientId) {
    throw new Error(
      `${provider.charAt(0).toUpperCase() + provider.slice(1)} Client ID not configured. ` +
        `Please set VITE_${provider.toUpperCase()}_CLIENT_ID in your environment variables.`
    );
  }

  const state = generateState();
  storeOAuthState(provider, state);

  const params = new URLSearchParams({
    client_id: config.clientId,
    redirect_uri: `${API_BASE_URL}${config.redirectPath}`,
    scope: config.scope,
    state,
    ...config.additionalParams
  });

  window.location.href = `${config.authUrl}?${params.toString()}`;
}

/**
 * Handles OAuth callback and extracts the authorization code from the current URL
 * This works with hash-based routing by parsing the fragment parameters after the hash
 * @param provider - The OAuth provider (github, google, etc.)
 * @returns Object containing code and any error, or null if invalid
 */
export function handleOAuthCallbackFromUrl(provider: OAuthProvider): OAuthCallbackResult | null {
  // Parse the fragment part after the hash (e.g., #/auth/github/callback?code=xyz&state=abc)
  const hash = window.location.hash;

  // Extract the query string part from the hash fragment
  const queryStringIndex = hash.indexOf('?');
  if (queryStringIndex === -1) {
    return null;
  }

  const queryString = hash.substring(queryStringIndex + 1);
  const searchParams = new URLSearchParams(queryString);
  return handleOAuthCallback(provider, searchParams);
}

/**
 * Handles OAuth callback and extracts the authorization code
 * @param provider - The OAuth provider (github, google, etc.)
 * @param searchParams - URL search parameters from the callback
 * @returns Object containing code and any error, or null if invalid
 */
export function handleOAuthCallback(
  provider: OAuthProvider,
  searchParams: URLSearchParams
): OAuthCallbackResult | null {
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
  const storedState = getAndClearOAuthState(provider);
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

// Convenience functions for backwards compatibility and ease of use
export const redirectToGitHubOAuth = () => redirectToOAuth('github');
export const redirectToGoogleOAuth = () => redirectToOAuth('google');
export const handleGitHubCallbackFromUrl = () => handleOAuthCallbackFromUrl('github');
export const handleGoogleCallbackFromUrl = () => handleOAuthCallbackFromUrl('google');
export const handleGitHubCallback = (searchParams: URLSearchParams) => handleOAuthCallback('github', searchParams);
export const handleGoogleCallback = (searchParams: URLSearchParams) => handleOAuthCallback('google', searchParams);
