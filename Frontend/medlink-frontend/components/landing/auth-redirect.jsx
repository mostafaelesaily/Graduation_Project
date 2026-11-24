"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { isAuthenticated } from "@/lib/session";

/**
 * Preserves the original landing-page behaviour: signed-in visitors are sent
 * straight to their dashboard instead of seeing the marketing page.
 * `proxy.ts` performs the same check server-side for direct URL visits.
 */
export default function AuthRedirect() {
  const router = useRouter();

  useEffect(() => {
    if (isAuthenticated()) router.replace("/home");
  }, [router]);

  return null;
}
