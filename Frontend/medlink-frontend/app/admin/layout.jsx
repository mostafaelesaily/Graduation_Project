import AuthGuard from '../../components/auth-guard';

// AuthGuard renders a loading placeholder until the Admin session is
// verified in the browser, so the child page is not part of the prefetched
// payload. Opt this segment out of instant-navigation validation.
export const instant = false;

/** Every /admin route requires a signed-in user with the Admin role. */
export default function AdminLayout({ children }) {
  return <AuthGuard requireAdmin>{children}</AuthGuard>;
}
