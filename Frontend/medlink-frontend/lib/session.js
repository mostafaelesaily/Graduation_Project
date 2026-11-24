/**
 * Browser session storage: the token lives in `localStorage` for the app and
 * is mirrored into a cookie so `proxy.ts` can protect routes before a page is
 * ever rendered.
 */

import {
  SESSION_COOKIE,
  decodeJwt,
  getClaim,
  getRoles,
  isAdminToken,
  isTokenValid,
  sanitizeNextPath,
  tokenExpiresInSeconds,
} from './auth';

const TOKEN_KEY = 'token';
const REFRESH_KEY = 'refreshToken';
const NAME_KEY = 'userName';

export { SESSION_COOKIE, sanitizeNextPath };

function isBrowser() {
  return typeof window !== 'undefined' && typeof document !== 'undefined';
}

/** Returns the stored access token, or null when there is none. */
export function getToken() {
  if (!isBrowser()) return null;
  try {
    return window.localStorage.getItem(TOKEN_KEY);
  } catch {
    return null;
  }
}

function writeSessionCookie(token) {
  if (!isBrowser()) return;
  const maxAge = tokenExpiresInSeconds(token);
  const secure = window.location.protocol === 'https:' ? '; secure' : '';
  document.cookie = `${SESSION_COOKIE}=${encodeURIComponent(
    token
  )}; path=/; max-age=${maxAge}; samesite=lax${secure}`;
}

function removeSessionCookie() {
  if (!isBrowser()) return;
  document.cookie = `${SESSION_COOKIE}=; path=/; max-age=0; samesite=lax`;
}

/**
 * Persists a signed-in session (localStorage + cookie).
 * Accepts `{ accessToken, refreshToken, userName }`.
 */
export function setSession({ accessToken, refreshToken, userName } = {}) {
  if (!isBrowser()) return;

  try {
    if (accessToken) window.localStorage.setItem(TOKEN_KEY, accessToken);
    if (refreshToken) window.localStorage.setItem(REFRESH_KEY, refreshToken);
    if (userName) window.localStorage.setItem(NAME_KEY, userName);
  } catch {
    /* storage unavailable (private mode) — the cookie still carries the session */
  }

  if (accessToken) writeSessionCookie(accessToken);
}

/** Removes every trace of the session from localStorage and the cookie. */
export function clearSession() {
  if (!isBrowser()) return;
  try {
    window.localStorage.removeItem(TOKEN_KEY);
    window.localStorage.removeItem(REFRESH_KEY);
    window.localStorage.removeItem(NAME_KEY);
  } catch {
    /* ignore */
  }
  removeSessionCookie();
}

/** True when a non-expired token is present. */
export function isAuthenticated() {
  return isTokenValid(getToken());
}

/** True when the signed-in user carries the Admin role. */
export function isAdmin() {
  return isAdminToken(getToken());
}

/** Roles of the signed-in user. */
export function getSessionRoles() {
  return getRoles(getToken());
}

/** Reads a claim from the signed-in user's token. */
export function getSessionClaim(...keys) {
  return getClaim(decodeJwt(getToken()), ...keys);
}

/** The signed-in user's display name, falling back to localStorage. */
export function getStoredUserName() {
  if (!isBrowser()) return '';
  try {
    return (
      getSessionClaim(
        'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name',
        'unique_name',
        'name'
      ) || window.localStorage.getItem(NAME_KEY) || ''
    );
  } catch {
    return '';
  }
}
