/**
 * Pure authentication helpers shared by the browser session code and the
 * server-side `proxy.ts`. Keep this file free of browser-only globals so it
 * can be imported from both runtimes.
 */

/** Cookie mirrored by the browser so `proxy.ts` can gate routes server-side. */
export const SESSION_COOKIE = 'medlink_session';

/** Claims that may carry the user roles depending on the token issuer. */
const ROLE_CLAIMS = [
  'http://schemas.microsoft.com/ws/2008/06/identity/claims/role',
  'role',
  'roles',
];

/** Reads the first non-empty value among the given claim keys. */
export function getClaim(payload, ...keys) {
  if (!payload) return '';
  for (const key of keys) {
    const value = payload[key];
    if (Array.isArray(value) && value.length > 0) return value[0];
    if (typeof value === 'string' && value) return value;
  }
  return '';
}

function decodeSegment(segment) {
  if (typeof atob !== 'function') return null;
  try {
    const normalized = segment.replace(/-/g, '+').replace(/_/g, '/');
    const padded = normalized.padEnd(Math.ceil(normalized.length / 4) * 4, '=');
    const binary = atob(padded);
    const bytes = Uint8Array.from(binary, (char) => char.charCodeAt(0));
    return new TextDecoder('utf-8').decode(bytes);
  } catch {
    return null;
  }
}

/** Decodes a JWT payload, or returns null when the token is unusable. */
export function decodeJwt(token) {
  if (typeof token !== 'string' || !token) return null;
  const parts = token.split('.');
  if (parts.length < 2) return null;
  const json = decodeSegment(parts[1]);
  if (!json) return null;
  try {
    return JSON.parse(json);
  } catch {
    return null;
  }
}

/** A token is valid when it decodes and has not expired. */
export function isTokenValid(token) {
  const payload = decodeJwt(token);
  if (!payload) return false;
  if (typeof payload.exp === 'number') return payload.exp * 1000 > Date.now();
  // No expiry claim: let the API be the authority instead of locking the UI.
  return true;
}

/** Roles carried by the token (always returned as an array of strings). */
export function getRoles(token) {
  const payload = decodeJwt(token);
  if (!payload) return [];

  const raw = ROLE_CLAIMS.map((key) => payload[key]).find(
    (value) => value !== undefined && value !== null && value !== ''
  );

  if (raw === undefined) return [];
  return (Array.isArray(raw) ? raw : [raw]).filter((value) => typeof value === 'string');
}

/** True when the token belongs to a user with the Admin role. */
export function isAdminToken(token) {
  return getRoles(token).includes('Admin');
}

/** Remaining lifetime of a token in seconds, used for the session cookie. */
export function tokenExpiresInSeconds(token) {
  const payload = decodeJwt(token);
  if (!payload || typeof payload.exp !== 'number') return 8 * 60 * 60;
  const seconds = Math.ceil(payload.exp - Date.now() / 1000);
  return Math.min(Math.max(seconds, 60), 30 * 24 * 60 * 60);
}

/**
 * Only allows same-origin paths such as `/home` or `/admin/provider-requests`.
 * Anything else (external URLs, protocol-relative `//evil.com`) falls back.
 */
export function sanitizeNextPath(rawPath, fallback = '/home') {
  if (typeof rawPath !== 'string') return fallback;
  const path = rawPath.trim();
  if (!path.startsWith('/') || path.startsWith('//')) return fallback;
  if (path.startsWith('/login') || path.startsWith('/register')) return fallback;
  return path;
}
