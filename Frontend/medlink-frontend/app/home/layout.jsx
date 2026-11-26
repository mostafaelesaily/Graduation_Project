import AuthGuard from '../../components/auth-guard';

// AuthGuard shows a loading state until the session is checked in the
// browser, so the child page is not part of the prefetched payload.
// Opt this segment out of instant-navigation validation.
export const instant = false;

export default function HomeLayout({ children }) {
  return <AuthGuard>{children}</AuthGuard>;
}
