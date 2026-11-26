const ROLE_CLAIM = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';
const NAME_CLAIM = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name';

// بيفك محتوى الـ token (من غير ما يتحقق من التوقيع، ده شغل الباك إند)
export function decodeToken(token) {
  try {
    const payload = token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/');
    const json = decodeURIComponent(
      atob(payload)
        .split('')
        .map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
        .join('')
    );
    return JSON.parse(json);
  } catch {
    return null;
  }
}

export function getRoles(claims) {
  const r = claims?.[ROLE_CLAIM] ?? claims?.role;
  return Array.isArray(r) ? r : r ? [r] : [];
}

export function getUserName(claims) {
  return claims?.[NAME_CLAIM] ?? claims?.name ?? '';
}

// الأدمن لسه ملوش صفحة، فبيروح للهوم مؤقتًا
export function getHomePath(roles) {
  if (roles.includes('Provider')) return '/provider/dashboard';
  return '/home';
}
