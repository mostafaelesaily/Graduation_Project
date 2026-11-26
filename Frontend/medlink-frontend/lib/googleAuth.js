const GOOGLE_CLIENT_ID = process.env.NEXT_PUBLIC_GOOGLE_CLIENT_ID;

let scriptLoadingPromise = null;

function loadGoogleScript() {
  if (typeof window === 'undefined') {
    return Promise.reject(new Error('Google Sign-In is only available in the browser.'));
  }
  if (window.google?.accounts?.id) return Promise.resolve();
  if (scriptLoadingPromise) return scriptLoadingPromise;

  scriptLoadingPromise = new Promise((resolve, reject) => {
    const script = document.createElement('script');
    script.src = 'https://accounts.google.com/gsi/client';
    script.async = true;
    script.defer = true;
    script.onload = () => resolve();
    script.onerror = () => {
      scriptLoadingPromise = null;
      reject(new Error('Failed to load Google Sign-In script.'));
    };
    document.head.appendChild(script);
  });

  return scriptLoadingPromise;
}

/**
 * Renders Google's real "Sign in with Google" button inside the given container
 * and invokes `onToken` with the Google ID token once the user completes sign-in.
 *
 * We use the real rendered button instead of the One Tap `prompt()` flow because
 * One Tap depends on FedCM finding an already-signed-in Google session in the
 * browser; when there is none (or FedCM/third-party cookies are blocked) it fails
 * with "Provider's accounts list is empty" / FedCM NetworkError. The rendered
 * button opens Google's own account chooser, so it works even without an active
 * session. The token should be sent to the backend's POST /Account/GoogleSignIn.
 */
export async function renderGoogleButton(container, onToken, width) {
  if (!GOOGLE_CLIENT_ID) {
    throw new Error('Google sign-in is not configured. Missing NEXT_PUBLIC_GOOGLE_CLIENT_ID.');
  }
  if (!container) return;

  await loadGoogleScript();

  window.google.accounts.id.initialize({
    client_id: GOOGLE_CLIENT_ID,
    callback: (response) => {
      if (response?.credential) {
        onToken(response.credential);
      }
    },
  });

  container.innerHTML = '';
  window.google.accounts.id.renderButton(container, {
    type: 'standard',
    theme: 'outline',
    size: 'large',
    width: width || 300,
  });
}
