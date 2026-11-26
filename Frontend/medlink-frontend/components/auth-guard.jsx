'use client';

import { useEffect, useState } from 'react';
import { usePathname, useRouter } from 'next/navigation';
import { getToken, isAdmin, sanitizeNextPath } from '@/lib/session';

/**
 * Client-side guard for protected route segments. `proxy.ts` already blocks
 * direct URL access before rendering; this component re-checks the session
 * (and the Admin role) so client-side navigation is covered too.
 *
 *   <AuthGuard>…</AuthGuard>            // any signed-in user
 *   <AuthGuard requireAdmin>…</AuthGuard> // Admin role only
 */
export default function AuthGuard({ requireAdmin = false, children }) {
  const router = useRouter();
  const pathname = usePathname();
  const [allowed, setAllowed] = useState(false);

  useEffect(() => {
    const token = getToken();

    if (!token) {
      router.replace(`/login?next=${encodeURIComponent(sanitizeNextPath(pathname, '/home'))}`);
      return;
    }

    if (requireAdmin && !isAdmin(token)) {
      router.replace('/home');
      return;
    }

    setAllowed(true);
  }, [pathname, requireAdmin, router]);

  if (!allowed) {
    return (
      <div
        role="status"
        aria-live="polite"
        className="flex min-h-screen items-center justify-center bg-[#eef6fd] px-6"
      >
        <div className="flex items-center gap-3 rounded-full border border-slate-200 bg-white px-6 py-3 text-sm font-semibold text-slate-500 shadow-sm">
          <span className="h-4 w-4 animate-spin rounded-full border-2 border-slate-200 border-t-[#1f6bff]" />
          Checking your session…
        </div>
      </div>
    );
  }

  return children;
}
