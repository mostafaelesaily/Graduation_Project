'use client';

import { useCallback, useEffect, useMemo, useState } from 'react';
import { useRouter } from 'next/navigation';
import API from '@/api/axios';
import { decodeToken, getRoles, getUserName } from '@/lib/auth';
import { BookingStatus, bookingStatusLabel } from '@/lib/enums';

const PAGE_SIZE = 5;
const TABS = ['Upcoming', 'Today', 'Past', 'Cancelled'];
const DAYS = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];

const STATUS_STYLES = {
  [BookingStatus.Pending]: 'bg-amber-50 text-amber-700',
  [BookingStatus.Confirmed]: 'bg-emerald-50 text-emerald-700',
  [BookingStatus.Cancelled]: 'bg-red-50 text-red-600',
  [BookingStatus.Completed]: 'bg-sky-50 text-sky-700',
  [BookingStatus.Rejected]: 'bg-red-50 text-red-600',
  [BookingStatus.Expired]: 'bg-slate-100 text-slate-500',
};

/* ---------- Helpers ---------- */

const isSameDay = (a, b) => a.toDateString() === b.toDateString();

const timeLabel = (d) =>
  d.toLocaleTimeString('en-US', { hour: 'numeric', minute: '2-digit' });

function dayLabel(date) {
  const now = new Date();
  const tomorrow = new Date(now);
  tomorrow.setDate(now.getDate() + 1);

  if (isSameDay(date, now)) return 'Today';
  if (isSameDay(date, tomorrow)) return 'Tomorrow';

  return date.toLocaleDateString('en-US', {
    month: 'short',
    day: 'numeric',
  });
}

function spanLabel(span) {
  const [h, m] = String(span).split(':').map(Number);
  if (Number.isNaN(h)) return '';

  const d = new Date();
  d.setHours(h, m || 0, 0, 0);

  return timeLabel(d);
}

function matchesTab(b, tab) {
  const date = new Date(b.bookingDate);
  const now = new Date();

  const active =
    b.status === BookingStatus.Pending ||
    b.status === BookingStatus.Confirmed;

  const dead =
    b.status === BookingStatus.Cancelled ||
    b.status === BookingStatus.Rejected;

  if (tab === 'Upcoming') return active && date >= now;
  if (tab === 'Today') return active && isSameDay(date, now);
  if (tab === 'Past') return !dead && date < now;

  return dead;
}

const patientName = (b) =>
  b.patientName ?? `Patient #${String(b.userId).slice(0, 4)}`;

function normalizeAvailability(data) {
  return Array.isArray(data) ? data : data?.intervals ?? [];
}

function createDraft(availability) {
  return availability.map((slot, index) => ({
    localId: `${slot.dayOfWeek}-${index}-${Math.random()}`,
    dayOfWeek: Number(slot.dayOfWeek),
    startTime: String(slot.startTime).slice(0, 5),
    endTime: String(slot.endTime).slice(0, 5),
  }));
}

/* ---------- Page ---------- */

export default function ProviderDashboard() {
  const router = useRouter();

  const [name, setName] = useState('');
  const [stats, setStats] = useState(null);
  const [bookings, setBookings] = useState([]);
  const [availability, setAvailability] = useState([]);

  const [tab, setTab] = useState('Upcoming');
  const [page, setPage] = useState(1);
  const [selected, setSelected] = useState([]);

  const [loading, setLoading] = useState(true);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState('');

  // Availability editor
  const [availabilityModal, setAvailabilityModal] = useState(false);
  const [availabilityMode, setAvailabilityMode] = useState('set');
  const [draftAvailability, setDraftAvailability] = useState([]);
  const [availabilityError, setAvailabilityError] = useState('');

  const apiError = (err, fallback) =>
    err.response
      ? err.response.data?.Message ||
        err.response.data?.message ||
        `${fallback} (error ${err.response.status}).`
      : 'Cannot reach the server. Is the backend running?';

  /* ---------- Load dashboard ---------- */

  const loadData = useCallback(async () => {
    setError('');

    try {
      const [s, b, a] = await Promise.all([
        API.get('/Booking/statistics'),
        API.get('/Booking/my-provider', {
          params: { pageNum: 1, pageSize: 100 },
        }),
        API.get('/Provider/availability'),
      ]);

      setStats(s.data);
      setBookings(b.data?.data ?? []);
      setAvailability(normalizeAvailability(a.data));
    } catch (err) {
      setError(apiError(err, 'Could not load your dashboard'));
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    const token = localStorage.getItem('token');
    const claims = token ? decodeToken(token) : null;

    if (!claims) {
      router.replace('/login');
      return;
    }

    if (!getRoles(claims).includes('Provider')) {
      router.replace('/home');
      return;
    }

    setName(getUserName(claims));
    loadData();
  }, [router, loadData]);

  /* ---------- Bookings ---------- */

  const filtered = useMemo(() => {
    const list = bookings.filter((b) => matchesTab(b, tab));
    const asc = tab === 'Upcoming' || tab === 'Today';

    return list.sort((x, y) =>
      asc
        ? new Date(x.bookingDate) - new Date(y.bookingDate)
        : new Date(y.bookingDate) - new Date(x.bookingDate)
    );
  }, [bookings, tab]);

  const totalPages = Math.max(1, Math.ceil(filtered.length / PAGE_SIZE));

  const pageItems = filtered.slice(
    (page - 1) * PAGE_SIZE,
    page * PAGE_SIZE
  );

  const cancellable = (b) =>
    b.status === BookingStatus.Pending ||
    b.status === BookingStatus.Confirmed;

  const changeTab = (t) => {
    setTab(t);
    setPage(1);
    setSelected([]);
  };

  const toggleOne = (id) =>
    setSelected((prev) =>
      prev.includes(id)
        ? prev.filter((x) => x !== id)
        : [...prev, id]
    );

  const allOnPageSelected =
    pageItems.length > 0 &&
    pageItems.every((b) => selected.includes(b.id));

  const toggleAll = () =>
    setSelected(
      allOnPageSelected
        ? []
        : pageItems.filter(cancellable).map((b) => b.id)
    );

  const runAction = async (request, failMsg) => {
    setBusy(true);
    setError('');

    try {
      await request();
      setSelected([]);
      await loadData();
    } catch (err) {
      setError(apiError(err, failMsg));
    } finally {
      setBusy(false);
    }
  };

  const accept = (id) =>
    runAction(
      () => API.patch(`/Booking/${id}/approve`),
      'Could not accept the booking'
    );

  const cancel = (id) => {
    if (!window.confirm('Cancel this booking?')) return;

    runAction(
      () => API.patch(`/Booking/${id}/cancel`),
      'Could not cancel the booking'
    );
  };

  const cancelSelected = () => {
    const ids = bookings
      .filter((b) => selected.includes(b.id) && cancellable(b))
      .map((b) => b.id);

    if (ids.length === 0) return;

    if (!window.confirm(`Cancel ${ids.length} booking(s)?`)) return;

    runAction(
      () => API.patch('/Booking/cancel/bulk', ids),
      'Could not cancel the selected bookings'
    );
  };

  /* ---------- Logout ---------- */

  const logout = async () => {
    try {
      await API.post('/Account/Logout');
    } catch {}

    localStorage.removeItem('token');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('userName');

    router.replace('/login');
  };

 
   const openSetAvailability = () => {
    if (availability.length > 0) return;

    setAvailabilityMode('set');
    setDraftAvailability([]);
    setAvailabilityError('');
    setAvailabilityModal(true);
  };

  // Edit: loads the already-saved working hours into the form.
  const openEditAvailability = () => {
    if (availability.length === 0) return;

    setAvailabilityMode('edit');
    setDraftAvailability(createDraft(availability));
    setAvailabilityError('');
    setAvailabilityModal(true);
  };

  const closeAvailabilityEditor = () => {
    if (busy) return;

    setAvailabilityModal(false);
    setAvailabilityError('');
  };

  const addInterval = (dayIndex) => {
    setDraftAvailability((prev) => [
      ...prev,
      {
        localId: `${dayIndex}-${Date.now()}-${Math.random()}`,
        dayOfWeek: dayIndex,
        startTime: '09:00',
        endTime: '17:00',
      },
    ]);
  };

  const updateInterval = (localId, field, value) => {
    setDraftAvailability((prev) =>
      prev.map((slot) =>
        slot.localId === localId
          ? { ...slot, [field]: value }
          : slot
      )
    );
  };

  const removeInterval = (localId) => {
    setDraftAvailability((prev) =>
      prev.filter((slot) => slot.localId !== localId)
    );
  };

  const saveAvailability = async () => {
    setAvailabilityError('');

    if (draftAvailability.length === 0) {
      const confirmed = window.confirm(
        'You have not added any working hours. Saving will make all days unavailable. Continue?'
      );

      if (!confirmed) return;
    }

    // Validate time ranges.
    for (const slot of draftAvailability) {
      if (!slot.startTime || !slot.endTime) {
        setAvailabilityError('Please enter both start and end times.');
        return;
      }

      if (slot.startTime >= slot.endTime) {
        setAvailabilityError(
          `End time must be after start time on ${DAYS[slot.dayOfWeek]}.`
        );
        return;
      }
    }

    // Convert to backend request format.
    const intervals = draftAvailability.map((slot) => ({
      dayOfWeek: Number(slot.dayOfWeek),
      startTime: `${slot.startTime}:00`,
      endTime: `${slot.endTime}:00`,
    }));

    setBusy(true);

    try {
      await API.put('/Provider/availability', { intervals });

      // Reload saved availability from backend.
      const response = await API.get('/Provider/availability');

      setAvailability(normalizeAvailability(response.data));
      setAvailabilityModal(false);
      setAvailabilityError('');
    } catch (err) {
      setAvailabilityError(
        apiError(err, 'Could not save your availability')
      );
    } finally {
      setBusy(false);
    }
  };

  // Group availability by day (Sunday = 0).
  const byDay = DAYS.map((_, i) =>
    availability.filter((a) => Number(a.dayOfWeek) === i)
  );

  const hasAvailability = availability.length > 0;

  const cards = [
    {
      label: 'Today',
      value: stats?.todayCount,
      color: 'text-blue-600',
    },
    {
      label: 'Next 7 days',
      value: stats?.nextSevenDaysCount,
      color: 'text-slate-900',
    },
    {
      label: 'Pending',
      value: stats?.pendingCount,
      color: 'text-amber-600',
    },
    {
      label: 'Cancelled',
      value: stats?.cancelledCount,
      color: 'text-red-600',
    },
  ];

  /* ---------- UI ---------- */

  return (
    <div className="min-h-screen bg-sky-50 font-sans text-slate-900">

      {/* Header */}
      <header className="border-b border-slate-100 bg-white">
        <div className="mx-auto flex max-w-6xl items-center justify-between px-6 py-3">
          <img
            src="/WhatsApp Image .jpeg"
            alt="MedLink Logo"
            className="h-10 w-auto object-contain"
          />

          <div className="flex items-center gap-3">
            <div className="text-end leading-tight">
              <p className="text-sm font-bold text-slate-900">{name}</p>
              <p className="text-xs text-slate-500">Provider</p>
            </div>

            <span className="flex h-10 w-10 items-center justify-center rounded-full bg-blue-600 text-sm font-bold text-white">
              {name.charAt(0).toUpperCase()}
            </span>

            <button
              type="button"
              onClick={logout}
              className="rounded-full border border-slate-200 px-3 py-1.5 text-xs font-semibold text-slate-600 hover:bg-slate-50"
            >
              Log out
            </button>
          </div>
        </div>
      </header>

      <main className="mx-auto max-w-6xl px-6 py-8">

        {/* Page heading */}
        <div>
          <h1 className="text-3xl font-extrabold text-[#0b2545]">
            My Bookings
          </h1>

          <p className="mt-1 text-sm text-slate-500">
            Manage your appointments and upcoming schedule.
          </p>
        </div>

        {error && (
          <p
            role="alert"
            className="mt-4 rounded-xl bg-red-50 px-4 py-3 text-sm text-red-700"
          >
            {error}
          </p>
        )}

        {/* Stats */}
        <div className="mt-6 grid grid-cols-2 gap-4 lg:grid-cols-4">
          {cards.map((c) => (
            <div
              key={c.label}
              className="rounded-2xl bg-white p-5 shadow-sm"
            >
              <p className="text-xs font-semibold text-slate-500">
                {c.label}
              </p>

              <p className={`mt-1 text-3xl font-extrabold ${c.color}`}>
                {c.value ?? '–'}
              </p>
            </div>
          ))}
        </div>

        {/* Tabs + bulk cancel */}
        <div className="mt-8 flex flex-wrap items-center justify-between gap-3">
          <div className="flex gap-1">
            {TABS.map((t) => (
              <button
                key={t}
                type="button"
                onClick={() => changeTab(t)}
                className={`rounded-full px-4 py-2 text-sm font-semibold transition ${
                  tab === t
                    ? 'bg-blue-600 text-white'
                    : 'text-slate-500 hover:text-blue-600'
                }`}
              >
                {t}
              </button>
            ))}
          </div>

          {selected.length > 0 && (
            <div className="flex items-center overflow-hidden rounded-full bg-[#0b2545] text-sm font-semibold text-white">
              <span className="px-4 py-2">
                {selected.length} selected
              </span>

              <button
                type="button"
                onClick={cancelSelected}
                disabled={busy}
                className="bg-red-500 px-4 py-2 hover:bg-red-600 disabled:opacity-60"
              >
                Cancel selected
              </button>
            </div>
          )}
        </div>

        {/* Bookings table */}
        <div className="mt-4 overflow-x-auto rounded-2xl bg-white shadow-sm">
          <table className="w-full min-w-[720px] text-sm">
            <thead>
              <tr className="bg-sky-50/60 text-start text-xs font-semibold uppercase tracking-wide text-slate-500">
                <th className="w-10 px-4 py-3">
                  <input
                    type="checkbox"
                    checked={allOnPageSelected}
                    onChange={toggleAll}
                    aria-label="Select all"
                  />
                </th>

                <th className="px-2 py-3 text-start">Patient</th>
                <th className="px-2 py-3 text-start">Date &amp; time</th>
                <th className="px-2 py-3 text-start">Status</th>
                <th className="px-2 py-3 text-start">Notes</th>
                <th className="px-4 py-3 text-end">Action</th>
              </tr>
            </thead>

            <tbody>
              {loading && (
                <tr>
                  <td
                    colSpan={6}
                    className="px-4 py-10 text-center text-slate-400"
                  >
                    Loading...
                  </td>
                </tr>
              )}

              {!loading && pageItems.length === 0 && (
                <tr>
                  <td
                    colSpan={6}
                    className="px-4 py-10 text-center text-slate-400"
                  >
                    No bookings here.
                  </td>
                </tr>
              )}

              {pageItems.map((b) => {
                const date = new Date(b.bookingDate);
                const pname = patientName(b);

                return (
                  <tr key={b.id} className="border-t border-slate-100">
                    <td className="px-4 py-3">
                      <input
                        type="checkbox"
                        checked={selected.includes(b.id)}
                        disabled={!cancellable(b)}
                        onChange={() => toggleOne(b.id)}
                        aria-label={`Select ${pname}`}
                      />
                    </td>

                    <td className="px-2 py-3">
                      <div className="flex items-center gap-3">
                        <span className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-blue-600 text-xs font-bold text-white">
                          {pname
                            .replace('Patient #', '')
                            .slice(0, 2)
                            .toUpperCase()}
                        </span>

                        <span className="font-semibold">{pname}</span>
                      </div>
                    </td>

                    <td className="px-2 py-3">
                      <p className="font-semibold">{dayLabel(date)}</p>
                      <p className="text-xs text-slate-500">
                        {timeLabel(date)}
                      </p>
                    </td>

                    <td className="px-2 py-3">
                      <span
                        className={`rounded-full px-3 py-1 text-xs font-semibold ${
                          STATUS_STYLES[b.status] ??
                          'bg-slate-100 text-slate-500'
                        }`}
                      >
                        {bookingStatusLabel(b.status)}
                      </span>
                    </td>

                    <td className="px-2 py-3 text-slate-600">
                      {b.notes || '–'}
                    </td>

                    <td className="px-4 py-3">
                      <div className="flex justify-end gap-2">
                        {b.status === BookingStatus.Pending && (
                          <button
                            type="button"
                            onClick={() => accept(b.id)}
                            disabled={busy}
                            className="rounded-full bg-blue-600 px-4 py-1.5 text-xs font-bold text-white hover:bg-blue-700 disabled:opacity-60"
                          >
                            Accept
                          </button>
                        )}

                        {cancellable(b) && (
                          <button
                            type="button"
                            onClick={() => cancel(b.id)}
                            disabled={busy}
                            className="rounded-full border border-red-300 px-4 py-1.5 text-xs font-bold text-red-600 hover:bg-red-50 disabled:opacity-60"
                          >
                            Cancel
                          </button>
                        )}
                      </div>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>

        {/* Pagination */}
        <div className="mt-4 flex flex-wrap items-center justify-between gap-3 text-sm text-slate-500">
          <span>
            Showing {filtered.length === 0 ? 0 : (page - 1) * PAGE_SIZE + 1}–
            {Math.min(page * PAGE_SIZE, filtered.length)} of {filtered.length}
          </span>

          <div className="flex items-center gap-1">
            <button
              type="button"
              onClick={() => setPage((p) => Math.max(1, p - 1))}
              disabled={page === 1}
              className="rounded-full border border-slate-200 bg-white px-4 py-1.5 font-semibold disabled:opacity-40"
            >
              Previous
            </button>

            {Array.from({ length: totalPages }, (_, i) => i + 1).map((n) => (
              <button
                key={n}
                type="button"
                onClick={() => setPage(n)}
                className={`h-8 w-8 rounded-full text-xs font-bold ${
                  n === page
                    ? 'bg-blue-600 text-white'
                    : 'border border-slate-200 bg-white text-slate-700'
                }`}
              >
                {n}
              </button>
            ))}

            <button
              type="button"
              onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
              disabled={page === totalPages}
              className="rounded-full border border-slate-200 bg-white px-4 py-1.5 font-semibold disabled:opacity-40"
            >
              Next
            </button>
          </div>
        </div>

        {/* Availability section */}
        <section className="mt-8 rounded-2xl bg-white p-6 shadow-sm">
          <div className="flex items-start justify-between gap-4">
            <div>
              <h2 className="text-xl font-bold text-[#0b2545]">
                My Availability
              </h2>

              <p className="text-sm text-slate-500">
                Time slots patients can book with you each week.
              </p>
            </div>

            {!loading && (
              <button
                type="button"
                onClick={
                  hasAvailability
                    ? openEditAvailability
                    : openSetAvailability
                }
                disabled={busy}
                className="rounded-full border border-blue-200 px-4 py-1.5 text-sm font-semibold text-blue-600 hover:bg-blue-50 disabled:opacity-50"
              >
                {hasAvailability ? 'Edit' : 'Set Availability'}
              </button>
            )}
          </div>

          <div className="mt-4 grid grid-cols-2 gap-3 sm:grid-cols-4 lg:grid-cols-7">
            {DAYS.map((day, i) => (
              <div
                key={day}
                className="min-h-[110px] rounded-xl bg-sky-50/70 p-3"
              >
                <p className="text-sm font-bold">{day}</p>

                {byDay[i].length === 0 ? (
                  <p className="mt-3 text-center text-xs text-slate-400">
                    Day off
                  </p>
                ) : (
                  <div className="mt-2 space-y-1.5">
                    {byDay[i].map((slot, k) => (
                      <p
                        key={k}
                        className="rounded-lg bg-sky-100 px-2 py-1 text-center text-[11px] font-semibold text-blue-700"
                      >
                        {spanLabel(slot.startTime)} – {spanLabel(slot.endTime)}
                      </p>
                    ))}
                  </div>
                )}
              </div>
            ))}
          </div>
        </section>
      </main>

      {/* Availability Modal */}
      {availabilityModal && (
        <div
          className="fixed inset-0 z-50 flex items-center justify-center bg-slate-950/50 p-4"
          onMouseDown={(e) => {
            if (e.target === e.currentTarget) closeAvailabilityEditor();
          }}
        >
          <div
            role="dialog"
            aria-modal="true"
            aria-labelledby="availability-title"
            className="max-h-[90vh] w-full max-w-2xl overflow-y-auto rounded-2xl bg-white p-6 shadow-2xl"
          >
            <div className="flex items-start justify-between gap-4">
              <div>
                <h2
                  id="availability-title"
                  className="text-xl font-bold text-[#0b2545]"
                >
                  {availabilityMode === 'set'
                    ? 'Set Your Availability'
                    : 'Edit Availability'}
                </h2>

                <p className="mt-1 text-sm text-slate-500">
                  {availabilityMode === 'set'
                    ? 'Add your first working hours for each day of the week.'
                    : 'Update your saved working hours. You can add, edit, or remove time slots.'}
                </p>
              </div>

              <button
                type="button"
                onClick={closeAvailabilityEditor}
                disabled={busy}
                aria-label="Close"
                className="rounded-full px-3 py-1 text-xl text-slate-400 hover:bg-slate-100"
              >
                ×
              </button>
            </div>

            <div className="mt-6 space-y-4">
              {DAYS.map((day, dayIndex) => {
                const daySlots = draftAvailability.filter(
                  (slot) => slot.dayOfWeek === dayIndex
                );

                return (
                  <div
                    key={day}
                    className="rounded-xl border border-slate-200 p-4"
                  >
                    <div className="flex flex-wrap items-center justify-between gap-3">
                      <div>
                        <p className="font-bold text-slate-800">{day}</p>
                        <p className="text-xs text-slate-500">
                          {daySlots.length === 0
                            ? 'Day off'
                            : `${daySlots.length} working interval(s)`}
                        </p>
                      </div>

                      <button
                        type="button"
                        onClick={() => addInterval(dayIndex)}
                        disabled={busy}
                        className="rounded-full border border-blue-200 px-3 py-1.5 text-xs font-semibold text-blue-600 hover:bg-blue-50 disabled:opacity-50"
                      >
                        + Add time
                      </button>
                    </div>

                    {daySlots.length > 0 && (
                      <div className="mt-3 space-y-3">
                        {daySlots.map((slot) => (
                          <div
                            key={slot.localId}
                            className="grid grid-cols-1 items-end gap-3 sm:grid-cols-[1fr_1fr_auto]"
                          >
                            <label className="block">
                              <span className="mb-1 block text-xs font-semibold text-slate-600">
                                Start time
                              </span>

                              <input
                                type="time"
                                value={slot.startTime}
                                disabled={busy}
                                onChange={(e) =>
                                  updateInterval(
                                    slot.localId,
                                    'startTime',
                                    e.target.value
                                  )
                                }
                                className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-100"
                              />
                            </label>

                            <label className="block">
                              <span className="mb-1 block text-xs font-semibold text-slate-600">
                                End time
                              </span>

                              <input
                                type="time"
                                value={slot.endTime}
                                disabled={busy}
                                onChange={(e) =>
                                  updateInterval(
                                    slot.localId,
                                    'endTime',
                                    e.target.value
                                  )
                                }
                                className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-100"
                              />
                            </label>

                            <button
                              type="button"
                              onClick={() => removeInterval(slot.localId)}
                              disabled={busy}
                              className="rounded-lg border border-red-200 px-3 py-2 text-sm font-semibold text-red-600 hover:bg-red-50 disabled:opacity-50"
                            >
                              Remove
                            </button>
                          </div>
                        ))}
                      </div>
                    )}
                  </div>
                );
              })}
            </div>

            {availabilityError && (
              <p
                role="alert"
                className="mt-4 rounded-xl bg-red-50 px-4 py-3 text-sm text-red-700"
              >
                {availabilityError}
              </p>
            )}

            <div className="mt-6 flex flex-wrap justify-end gap-3 border-t border-slate-100 pt-4">
              <button
                type="button"
                onClick={closeAvailabilityEditor}
                disabled={busy}
                className="rounded-full border border-slate-200 px-5 py-2 text-sm font-semibold text-slate-600 hover:bg-slate-50 disabled:opacity-50"
              >
                Cancel
              </button>

              <button
                type="button"
                onClick={saveAvailability}
                disabled={busy}
                className="rounded-full bg-blue-600 px-6 py-2 text-sm font-bold text-white hover:bg-blue-700 disabled:cursor-wait disabled:opacity-60"
              >
                {busy ? 'Saving...' : 'Save Availability'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
