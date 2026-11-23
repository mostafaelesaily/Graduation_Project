import { NextResponse, type NextRequest } from 'next/server';
import {
  SESSION_COOKIE,
  isAdminToken,
  isTokenValid,
  sanitizeNextPath,
} from './lib/auth';

/** Paths that only signed-out visitors may open. */
const GUEST_ONLY_PATHS = ['/login', '/register'];

/** Paths that require a signed-in user. */
function isProtectedPath(pathname: string) {
  return (
    pathname === '/home' ||
    pathname.startsWith('/home/') ||
    pathname === '/admin' ||
    pathname.startsWith('/admin/')
  );
}

/** Paths reserved for users carrying the Admin role. */
function isAdminPath(pathname: string) {
  return pathname === '/admin' || pathname.startsWith('/admin/');
}

function redirectTo(request: NextRequest, path: string) {
  return NextResponse.redirect(new URL(path, request.url));
}

/**
 * Runs before every matched route so pages cannot be opened by simply typing
 * the URL: protected routes require a valid session cookie, admin routes also
 * require the Admin role, and signed-in users are kept off the auth pages.
 */
export function proxy(request: NextRequest) {
  const { pathname, search } = request.nextUrl;
  const token = request.cookies.get(SESSION_COOKIE)?.value;
  const authenticated = isTokenValid(token);

  if (isProtectedPath(pathname)) {
    if (!authenticated) {
      const next = encodeURIComponent(`${pathname}${search}`);
      return redirectTo(request, `/login?next=${next}`);
    }

    if (isAdminPath(pathname) && !isAdminToken(token)) {
      return redirectTo(request, '/home');
    }

    return NextResponse.next();
  }

  if (authenticated && GUEST_ONLY_PATHS.includes(pathname)) {
    const next = sanitizeNextPath(request.nextUrl.searchParams.get('next'));
    return redirectTo(request, next);
  }

  if (authenticated && pathname === '/landing') {
    return redirectTo(request, '/home');
  }

  return NextResponse.next();
}

export const config = {
  matcher: [
    // Every route except API routes, Next.js internals and static files.
    '/((?!api|_next/static|_next/image|favicon.ico|sitemap.xml|robots.txt).*)',
  ],
};
