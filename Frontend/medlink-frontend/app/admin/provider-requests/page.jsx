'use client';

import { useCallback, useEffect, useMemo, useState } from 'react';
import { useRouter } from 'next/navigation';
import API from '../../../api/axios';
import { decodeJwt, getClaim, getRoles } from '@/lib/auth';
import { getToken, getStoredUserName } from '@/lib/session';

/* ---------- Configuration ---------- */

const API_ORIGIN = 'https://localhost:44302';
const PAGE_SIZE = 5;
const STATUS_TABS = ['Pending', 'Approved', 'Rejected'];
const AVATAR_COLORS = [
  'bg-[#1f6bff]',
  'bg-[#10b9a5]',
  'bg-[#8b5cf6]',
  'bg-[#e8824d]',
  'bg-[#0ea5e9]',
];

/* ---------- Icons ---------- */

function MoleculeLogo({ className = 'h-10 w-10' }) {
  return (
    <svg className={className} viewBox="0 0 48 48" fill="none" aria-hidden="true">
      <path
        d="M24 24L12 10M24 24l12-14M24 24L12 38M24 24l12 14"
        stroke="#14b8a6"
        strokeWidth="2.6"
        strokeLinecap="round"
      />
      <circle cx="24" cy="24" r="5" fill="#14b8a6" />
      <circle cx="12" cy="10" r="4" fill="#5eead4" stroke="#14b8a6" strokeWidth="2" />
      <circle cx="36" cy="10" r="4" fill="#5eead4" stroke="#14b8a6" strokeWidth="2" />
      <circle cx="12" cy="38" r="4" fill="#5eead4" stroke="#14b8a6" strokeWidth="2" />
      <circle cx="36" cy="38" r="4" fill="#5eead4" stroke="#14b8a6" strokeWidth="2" />
    </svg>
  );
}

function SearchIcon({ className = 'h-5 w-5' }) {
  return (
    <svg className={className} fill="none" stroke="currentColor" strokeWidth="2" viewBox="0 0 24 24" aria-hidden="true">
      <circle cx="11" cy="11" r="7" />
      <path d="M21 21l-4.3-4.3" strokeLinecap="round" />
    </svg>
  );
}

function LicenseIcon({ className = 'h-10 w-10' }) {
  return (
    <svg className={className} viewBox="0 0 40 48" fill="none" aria-hidden="true">
      <rect x="1" y="1" width="38" height="46" rx="8" fill="#f8fbff" stroke="#dbeafe" strokeWidth="1.5" />
      <path d="M10 15h16M10 21h20M10 27h14" stroke="#bfdbfe" strokeWidth="2" strokeLinecap="round" />
      <circle cx="12" cy="37" r="4" stroke="#60a5fa" strokeWidth="2" />
      <path d="M12 35v2l1.5 1" stroke="#60a5fa" strokeWidth="1.5" strokeLinecap="round" />
    </svg>
  );
}

function ChevronIcon({ direction = 'left', className = 'h-4 w-4' }) {
  return (
    <svg className={className} fill="none" stroke="currentColor" strokeWidth="2" viewBox="0 0 24 24" aria-hidden="true">
      {direction === 'left' ? (
        <path d="M15 18l-6-6 6-6" strokeLinecap="round" strokeLinejoin="round" />
      ) : (
        <path d="M9 18l6-6-6-6" strokeLinecap="round" strokeLinejoin="round" />
      )}
    </svg>
  );
}

/* ---------- Helpers ---------- */

function getInitials(name) {
  const cleaned = String(name || '')
    .replace(/^dr\.?\s+/i, '')
    .trim();
  const parts = cleaned.split(/[\s._-]+/).filter(Boolean);
  if (!parts.length) return 'A';
  return `${parts[0][0] || ''}${parts[1]?.[0] || ''}`.toUpperCase();
}

function formatDate(value) {
  if (!value) return '—';
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return '—';
  return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
}

function resolveFileUrl(path) {
  if (!path) return null;
  if (/^https?:\/\//i.test(path)) return path;
  return `${API_ORIGIN}/${String(path).replace(/^\/+/, '')}`;
}

function errorMessage(error) {
  if (error?.response?.status === 403) return 'You do not have permission to perform this action.';
  if (error?.response?.status === 401) return 'Your session has expired. Please sign in again.';
  return error?.response?.data?.message || error?.response?.data?.title || 'Something went wrong. Please try again.';
}

/* ---------- Page ---------- */

export default function ProviderRequestsPage() {
  const router = useRouter();

  const [auth, setAuth] = useState(null);
  const [authChecked, setAuthChecked] = useState(false);
  const [status, setStatus] = useState('Pending');
  const [page, setPage] = useState(1);
  const [searchInput, setSearchInput] = useState('');
  const [search, setSearch] = useState('');
  const [result, setResult] = useState(null);
  const [counts, setCounts] = useState({ Pending: 0 });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [forbidden, setForbidden] = useState(false);
  const [busyId, setBusyId] = useState('');
  const [rejectTarget, setRejectTarget] = useState(null);
  const [toast, setToast] = useState(null);
  const [refreshKey, setRefreshKey] = useState(0);

  /* Read the signed-in admin from the JWT. */
  useEffect(() => {
    const token = getToken();
    if (!token) {
      router.replace('/login?next=%2Fadmin%2Fprovider-requests');
      return;
    }

    const claims = decodeJwt(token);
    const roles = getRoles(token);

    setAuth({
      token,
      name:
        getClaim(
          claims,
          'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name',
          'unique_name',
          'name'
        ) ||
        getStoredUserName() ||
        'Admin',
      email:
        getClaim(
          claims,
          'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress',
          'email'
        ) || '',
      isAdmin: roles.includes('Admin'),
    });
    setAuthChecked(true);
  }, [router]);

  /* Debounce the search box. */
  useEffect(() => {
    const timer = setTimeout(() => {
      setSearch(searchInput.trim());
      setPage(1);
    }, 400);
    return () => clearTimeout(timer);
  }, [searchInput]);

  /* Load the current page of provider requests. */
  useEffect(() => {
    if (!auth?.isAdmin) return undefined;

    const controller = new AbortController();
    setLoading(true);
    setError('');

    API.get('/admin/provider-requests', {
      params: {
        pageNum: page,
        pageSize: PAGE_SIZE,
        ...(search ? { search } : {}),
        status,
      },
      headers: { Authorization: `Bearer ${auth.token}` },
      signal: controller.signal,
    })
      .then((response) => {
        const payload = response.data;
        if ((payload?.data?.length ?? 0) === 0 && page > 1) {
          setPage((current) => Math.max(1, current - 1));
          return;
        }
        setResult(payload);
        setForbidden(false);
      })
      .catch((requestError) => {
        if (requestError.code === 'ERR_CANCELED') return;
        if (requestError.response?.status === 401) {
          router.replace('/login?next=%2Fadmin%2Fprovider-requests');
          return;
        }
        if (requestError.response?.status === 403) {
          setForbidden(true);
          setResult(null);
          return;
        }
        setResult(null);
        setError(errorMessage(requestError));
      })
      .finally(() => {
        if (!controller.signal.aborted) setLoading(false);
      });

    return () => controller.abort();
  }, [auth, page, search, status, refreshKey, router]);

  /* Keep the Pending tab counter fresh. */
  useEffect(() => {
    if (!auth?.isAdmin) return undefined;

    let cancelled = false;

    API.get('/admin/provider-requests', {
      params: { pageNum: 1, pageSize: 1, status: 'Pending' },
      headers: { Authorization: `Bearer ${auth.token}` },
    })
      .then((response) => {
        if (!cancelled) {
          setCounts({ Pending: response.data?.totalCount ?? 0 });
        }
      })
      .catch(() => {});

    return () => {
      cancelled = true;
    };
  }, [auth, refreshKey]);

  /* Auto-dismiss toasts. */
  useEffect(() => {
    if (!toast) return undefined;
    const timer = setTimeout(() => setToast(null), 4000);
    return () => clearTimeout(timer);
  }, [toast]);

  const runAction = useCallback(
    async (providerId, action) => {
      if (!auth) return;
      setBusyId(providerId);
      try {
        await API.patch(`/admin/providers/${providerId}/${action}`, null, {
          headers: { Authorization: `Bearer ${auth.token}` },
        });
        setToast({
          type: 'success',
          message: action === 'accept' ? 'Provider approved successfully.' : 'Provider request rejected.',
        });
        setRejectTarget(null);
        setRefreshKey((key) => key + 1);
      } catch (requestError) {
        if (requestError.response?.status === 401) {
          router.replace('/login?next=%2Fadmin%2Fprovider-requests');
          return;
        }
        setToast({ type: 'error', message: errorMessage(requestError) });
      } finally {
        setBusyId('');
      }
    },
    [auth, router]
  );

  const providers = result?.data ?? [];
  const totalCount = result?.totalCount ?? 0;
  const totalPages = result?.totalPages ?? 0;
  const firstRow = totalCount === 0 ? 0 : (page - 1) * PAGE_SIZE + 1;
  const lastRow = Math.min(page * PAGE_SIZE, totalCount);

  const pageNumbers = useMemo(() => {
    const visible = Math.min(totalPages, 5);
    return Array.from({ length: visible }, (_, index) => index + 1);
  }, [totalPages]);

  if (!authChecked) {
    return <div className="min-h-screen bg-[#eef6fd]" />;
  }

  if (auth && !auth.isAdmin) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-[#eef6fd] px-6">
        <div className="w-full max-w-md rounded-3xl border border-slate-200 bg-white p-8 text-center shadow-sm">
          <div className="mx-auto flex h-14 w-14 items-center justify-center rounded-full bg-rose-50 text-2xl">🔒</div>
          <h1 className="mt-5 text-xl font-bold text-slate-900">Admin access required</h1>
          <p className="mt-2 text-sm text-slate-500">
            This page is only available to administrators. Please sign in with an admin account.
          </p>
          <button
            type="button"
            onClick={() => router.replace('/home')}
            className="mt-6 rounded-full bg-[#1f6bff] px-6 py-2.5 text-sm font-semibold text-white shadow-md shadow-blue-500/25 transition hover:bg-blue-700"
          >
            Back to home
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-[#eef6fd] font-sans text-slate-900">
      {/* ---------- Top bar ---------- */}
      <header className="border-b border-slate-200/70 bg-white">
        <div className="mx-auto flex max-w-[1500px] items-center justify-between gap-4 px-4 py-4 sm:px-6 lg:px-10">
          <span className="flex items-center gap-2">
            <MoleculeLogo className="h-10 w-10" />
            <span className="text-2xl font-extrabold tracking-tight text-[#14b8a6] sm:text-[1.7rem]">
              MedLink
            </span>
          </span>

          <div className="flex min-w-0 items-center gap-3">
            <div className="min-w-0 text-right">
              <p className="truncate text-sm font-bold text-slate-900">{auth?.name || 'Admin'}</p>
              {auth?.email && (
                <p className="truncate text-xs text-slate-500">{auth.email}</p>
              )}
            </div>
            <span className="flex h-11 w-11 shrink-0 items-center justify-center rounded-full bg-[#08294d] text-sm font-bold text-white">
              {(auth?.name || 'Admin').replace(/^dr\.?\s+/i, '').charAt(0).toUpperCase() || 'A'}
            </span>
          </div>
        </div>
      </header>

      <main className="mx-auto max-w-[1500px] px-4 pb-10 pt-7 sm:px-6 lg:px-10">
        {/* ---------- Title + search ---------- */}
        <div className="flex flex-col gap-5 lg:flex-row lg:items-start lg:justify-between">
          <div>
            <h1 className="text-3xl font-extrabold tracking-tight text-[#08294d] sm:text-[2.15rem]">
              Provider Requests
            </h1>
            <p className="mt-1.5 text-sm text-slate-500 sm:text-base">
              Review documents and approve new providers.
            </p>
          </div>

          <div className="relative w-full lg:max-w-[430px]">
            <SearchIcon className="pointer-events-none absolute start-4 top-1/2 h-5 w-5 -translate-y-1/2 text-slate-400" />
            <input
              type="search"
              value={searchInput}
              onChange={(event) => setSearchInput(event.target.value)}
              placeholder="Search by name or email"
              aria-label="Search provider requests by name or email"
              className="h-12 w-full rounded-full border border-slate-200 bg-white ps-12 pe-4 text-sm text-slate-700 shadow-sm placeholder:text-slate-400 focus:border-blue-300 focus:outline-none focus:ring-4 focus:ring-blue-100"
            />
          </div>
        </div>

        {/* ---------- Status tabs ---------- */}
        <div className="mt-6 flex items-center gap-2 overflow-x-auto pb-1 sm:gap-4">
          {STATUS_TABS.map((tab) => {
            const active = status === tab;
            return (
              <button
                key={tab}
                type="button"
                onClick={() => {
                  setStatus(tab);
                  setPage(1);
                }}
                className={`shrink-0 rounded-full px-4 py-2 text-sm font-semibold transition sm:px-5 sm:py-2.5 ${
                  active
                    ? 'bg-[#1f6bff] text-white shadow-md shadow-blue-500/25'
                    : 'text-slate-600 hover:bg-white hover:text-slate-900'
                }`}
              >
                {tab}
                {tab === 'Pending' && counts.Pending > 0 ? ` (${counts.Pending})` : ''}
              </button>
            );
          })}
        </div>

        {/* ---------- Table card ---------- */}
        <section className="mt-5 overflow-hidden rounded-2xl border border-slate-200/80 bg-white shadow-[0_1px_3px_rgba(15,42,73,0.06)] sm:rounded-[22px]">
          <div className="overflow-x-auto">
            <table className="w-full min-w-[900px] border-collapse text-left">
              <thead>
                <tr className="bg-[#f7f9fc]">
                  {['Provider', 'Phone', 'License', 'Submitted', 'Actions'].map((label) => (
                    <th
                      key={label}
                      scope="col"
                      className={`px-5 py-4 text-[11px] font-bold uppercase tracking-[0.16em] text-slate-500 sm:px-6 ${
                        label === 'Actions' ? 'text-right' : ''
                      }`}
                    >
                      {label}
                    </th>
                  ))}
                </tr>
              </thead>

              <tbody>
                {loading &&
                  Array.from({ length: PAGE_SIZE }).map((_, index) => (
                    <tr key={`skeleton-${index}`} className="border-t border-slate-100">
                      <td className="px-5 py-5 sm:px-6">
                        <div className="flex animate-pulse items-center gap-3">
                          <span className="h-12 w-12 rounded-full bg-slate-100" />
                          <span className="space-y-2">
                            <span className="block h-3.5 w-36 rounded bg-slate-100" />
                            <span className="block h-3 w-44 rounded bg-slate-100" />
                          </span>
                        </div>
                      </td>
                      <td className="px-5 py-5 sm:px-6"><span className="block h-3.5 w-28 animate-pulse rounded bg-slate-100" /></td>
                      <td className="px-5 py-5 sm:px-6"><span className="block h-11 w-11 animate-pulse rounded-lg bg-slate-100" /></td>
                      <td className="px-5 py-5 sm:px-6"><span className="block h-3.5 w-24 animate-pulse rounded bg-slate-100" /></td>
                      <td className="px-5 py-5 sm:px-6"><span className="ms-auto block h-9 w-40 animate-pulse rounded-full bg-slate-100" /></td>
                    </tr>
                  ))}

                {!loading && !error && !forbidden && providers.length === 0 && (
                  <tr className="border-t border-slate-100">
                    <td colSpan={5} className="px-6 py-16 text-center">
                      <p className="text-sm font-semibold text-slate-700">No provider requests found</p>
                      <p className="mt-1 text-xs text-slate-500">
                        {search ? 'Try a different name or email.' : 'Nothing to review in this tab right now.'}
                      </p>
                    </td>
                  </tr>
                )}

                {!loading && (error || forbidden) && (
                  <tr className="border-t border-slate-100">
                    <td colSpan={5} className="px-6 py-14 text-center">
                      <p className="text-sm font-semibold text-rose-600">
                        {forbidden ? 'You do not have permission to view provider requests.' : error}
                      </p>
                      {!forbidden && (
                        <button
                          type="button"
                          onClick={() => setRefreshKey((key) => key + 1)}
                          className="mt-4 rounded-full border border-slate-200 bg-white px-5 py-2 text-xs font-semibold text-slate-700 shadow-sm transition hover:bg-slate-50"
                        >
                          Try again
                        </button>
                      )}
                    </td>
                  </tr>
                )}

                {!loading &&
                  !error &&
                  !forbidden &&
                  providers.map((provider, index) => {
                    const name = provider.userName || 'Unnamed provider';
                    const licenseUrl = resolveFileUrl(provider.licenseImageUrl);
                    const isBusy = busyId === provider.providerId;

                    return (
                      <tr key={provider.providerId} className="border-t border-slate-100 transition hover:bg-slate-50/60">
                        <td className="px-5 py-4 sm:px-6">
                          <div className="flex items-center gap-3">
                            <span
                              className={`flex h-12 w-12 shrink-0 items-center justify-center rounded-full text-sm font-bold text-white ${AVATAR_COLORS[index % AVATAR_COLORS.length]}`}
                            >
                              {getInitials(name)}
                            </span>
                            <div className="min-w-0">
                              <p className="truncate text-sm font-bold text-slate-900">{name}</p>
                              <p className="mt-0.5 truncate text-xs text-slate-500">{provider.email || '—'}</p>
                            </div>
                          </div>
                        </td>

                        <td className="px-5 py-4 text-sm text-slate-600 sm:px-6">
                          {provider.phoneNumber || '—'}
                        </td>

                        <td className="px-5 py-4 sm:px-6">
                          <div className="flex items-center gap-2.5">
                            <LicenseIcon className="h-12 w-10" />
                            {licenseUrl ? (
                              <a
                                href={licenseUrl}
                                target="_blank"
                                rel="noopener noreferrer"
                                className="text-sm font-semibold text-[#1f6bff] hover:underline"
                              >
                                View
                              </a>
                            ) : (
                              <span className="text-sm text-slate-400">—</span>
                            )}
                          </div>
                        </td>

                        <td className="px-5 py-4 text-sm text-slate-600 sm:px-6">
                          {formatDate(provider.createdAt)}
                        </td>

                        <td className="px-5 py-4 sm:px-6">
                          <div className="flex items-center justify-end gap-3">
                            <button
                              type="button"
                              disabled={isBusy}
                              onClick={() => setRejectTarget(provider)}
                              className="rounded-full border border-[#f0c7c7] bg-white px-5 py-2 text-sm font-semibold text-[#e05c5c] transition hover:bg-rose-50 disabled:cursor-not-allowed disabled:opacity-60"
                            >
                              Reject
                            </button>
                            <button
                              type="button"
                              disabled={isBusy}
                              onClick={() => runAction(provider.providerId, 'accept')}
                              className="rounded-full bg-[#1f6bff] px-6 py-2 text-sm font-semibold text-white shadow-md shadow-blue-500/25 transition hover:bg-blue-700 disabled:cursor-not-allowed disabled:opacity-60"
                            >
                              {isBusy ? 'Working…' : 'Accept'}
                            </button>
                          </div>
                        </td>
                      </tr>
                    );
                  })}
              </tbody>
            </table>
          </div>
        </section>

        {/* ---------- Pagination ---------- */}
        <div className="mt-5 flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
          <p className="text-sm text-slate-500">
            Showing {firstRow}–{lastRow} of {totalCount}
          </p>

          <div className="flex flex-wrap items-center gap-2">
            <button
              type="button"
              disabled={page <= 1 || loading}
              onClick={() => setPage((current) => Math.max(1, current - 1))}
              className="rounded-xl border border-slate-200 bg-white px-4 py-2 text-sm font-medium text-slate-600 shadow-sm transition hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-50"
            >
              Previous
            </button>

            {pageNumbers.map((number) => (
              <button
                key={number}
                type="button"
                onClick={() => setPage(number)}
                aria-current={page === number ? 'page' : undefined}
                className={`h-10 min-w-10 rounded-xl px-3 text-sm font-semibold transition ${
                  page === number
                    ? 'bg-[#1f6bff] text-white shadow-md shadow-blue-500/25'
                    : 'border border-slate-200 bg-white text-slate-600 shadow-sm hover:bg-slate-50'
                }`}
              >
                {number}
              </button>
            ))}

            <button
              type="button"
              disabled={page >= totalPages || loading || totalPages === 0}
              onClick={() => setPage((current) => current + 1)}
              className="rounded-xl border border-slate-200 bg-white px-4 py-2 text-sm font-medium text-slate-600 shadow-sm transition hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-50"
            >
              Next
            </button>
          </div>
        </div>
      </main>

      {/* ---------- Reject confirmation ---------- */}
      {rejectTarget && (
        <div className="fixed inset-0 z-40 flex items-center justify-center bg-slate-900/40 px-4 backdrop-blur-sm">
          <div className="w-full max-w-md rounded-3xl bg-white p-6 shadow-2xl">
            <h2 className="text-lg font-bold text-slate-900">Reject provider request?</h2>
            <p className="mt-2 text-sm leading-6 text-slate-500">
              <span className="font-semibold text-slate-700">{rejectTarget.userName}</span> will not be able
              to join as a provider. You can review the request again later if needed.
            </p>
            <div className="mt-6 flex items-center justify-end gap-3">
              <button
                type="button"
                onClick={() => setRejectTarget(null)}
                className="rounded-full border border-slate-200 bg-white px-5 py-2.5 text-sm font-semibold text-slate-600 transition hover:bg-slate-50"
              >
                Cancel
              </button>
              <button
                type="button"
                onClick={() => runAction(rejectTarget.providerId, 'reject')}
                className="rounded-full bg-[#e05c5c] px-5 py-2.5 text-sm font-semibold text-white shadow-md shadow-rose-500/25 transition hover:bg-rose-600"
              >
                Reject request
              </button>
            </div>
          </div>
        </div>
      )}

      {/* ---------- Toast ---------- */}
      {toast && (
        <div
          role="status"
          className={`fixed bottom-6 end-6 z-50 max-w-sm rounded-2xl px-5 py-3.5 text-sm font-semibold shadow-xl ${
            toast.type === 'success' ? 'bg-[#0f9f8c] text-white' : 'bg-rose-600 text-white'
          }`}
        >
          {toast.message}
        </div>
      )}
    </div>
  );
}
