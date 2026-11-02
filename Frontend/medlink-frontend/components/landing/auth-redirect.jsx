"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";

/**
 * Preserves the original landing-page behaviour: signed-in visitors are sent
 * straight to their dashboard instead of seeing the marketing page.
 */
export default function AuthRedirect() {
  const router = useRouter();

  useEffect(() => {
    try {
      if (localStorage.getItem("token")) router.replace("/home");
    } catch {
      /* localStorage is unavailable (private mode / SSR) — stay on the page */
    }
  }, [router]);

  return null;
}
