'use client';

import React, { Suspense, useCallback, useEffect, useState } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import API from '@/api/axios';
/* ---------- Icons ---------- */

function LogoIcon({ className = 'h-6 w-6' }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="currentColor" aria-hidden="true">
      <path d="M12 21.35l-1.45-1.32C5.4 15.36 2 12.28 2 8.5 2 5.42 4.42 3 7.5 3c1.74 0 3.41.81 4.5 2.09C13.09 3.81 14.76 3 16.5 3 19.58 3 22 5.42 22 8.5c0 3.78-3.4 6.86-8.55 11.54L12 21.35z" />
      <path d="M5.5 9.2h2.6l1.2-2.1 1.9 4 1.3-1.9h4" fill="none" stroke="#ffffff" strokeWidth="1.6" strokeLinecap="round" strokeLinejoin="round" />
    </svg>
  );
}

function SearchIcon({ className = 'h-4 w-4' }) {
  return (
    <svg className={className} fill="none" stroke="currentColor" strokeWidth="2" viewBox="0 0 24 24" aria-hidden="true">
      <circle cx="11" cy="11" r="7" />
      <path d="M21 21l-4.3-4.3" strokeLinecap="round" />
    </svg>
  );
}

function PinIcon({ className = 'h-4 w-4' }) {
  return (
    <svg className={className} fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" viewBox="0 0 24 24" aria-hidden="true">
      <path d="M12 21s-7-5.5-7-11a7 7 0 1 1 14 0c0 5.5-7 11-7 11z" />
      <circle cx="12" cy="10" r="2.5" />
    </svg>
  );
}

function PhoneIcon({ className = 'h-4 w-4' }) {
  return (
    <svg className={className} fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" viewBox="0 0 24 24" aria-hidden="true">
      <path d="M22 16.9v3a2 2 0 0 1-2.2 2 19.8 19.8 0 0 1-8.6-3.1 19.5 19.5 0 0 1-6-6A19.8 19.8 0 0 1 2.1 4.2 2 2 0 0 1 4.1 2h3a2 2 0 0 1 2 1.7c.1 1 .4 1.9.7 2.8a2 2 0 0 1-.5 2.1L8.1 9.9a16 16 0 0 0 6 6l1.3-1.3a2 2 0 0 1 2.1-.4c.9.3 1.8.6 2.8.7a2 2 0 0 1 1.7 2z" />
    </svg>
  );
}

function CalendarIcon({ className = 'h-4 w-4' }) {
  return (
    <svg className={className} fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" viewBox="0 0 24 24" aria-hidden="true">
      <rect x="3" y="5" width="18" height="16" rx="2" />
      <path d="M16 3v4M8 3v4M3 10h18" />
    </svg>
  );
}

function HospitalIcon({ className = 'h-6 w-6' }) {
  return (
    <svg className={className} fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" viewBox="0 0 24 24" aria-hidden="true">
      <path d="M4 21V9l8-5 8 5v12" />
      <path d="M9 21v-5h6v5" />
      <path d="M12 7.5v3M10.5 9h3" />
      <path d="M2 21h20" />
    </svg>
  );
}

/* ---------- Settings ---------- */

 const LOCATIONS = [
  { label: 'Cairo, EG', lat: 30.0444, lng: 31.2357 },
  { label: 'Giza, EG', lat: 30.0131, lng: 31.2089 },
  { label: 'Alexandria, EG', lat: 31.2001, lng: 29.9187 },
];
const RADIUS_METERS = 20000; 
const PAGE_SIZE = 10;

/* ---------- Provider card ---------- */

function ProviderCard({ p }) {
  const mapsUrl = `https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(
    `${p.serviceName} ${p.address}`
  )}`;

  return (
    <div className="rounded-2xl border border-slate-100 bg-white p-3 shadow-sm transition hover:shadow-md">
      <div className="flex gap-3">
        <div className="flex h-24 w-32 shrink-0 items-center justify-center rounded-xl bg-gradient-to-br from-sky-100 to-blue-200 text-blue-500">
          <HospitalIcon className="h-10 w-10" />
        </div>
        <div className="min-w-0 flex-1">
          <p className="text-base font-bold leading-tight text-slate-900">{p.serviceName}</p>
        </div>
      </div>

      <ul className="mt-3 space-y-1.5 text-sm text-slate-700">
        <li className="flex items-center gap-2">
          <PinIcon className="h-4 w-4 shrink-0 text-blue-600" />
          {p.address}
        </li>
        {p.phoneNumber && (
          <li className="flex items-center gap-2">
            <PhoneIcon className="h-4 w-4 shrink-0 text-blue-600" />
            <a href={`tel:${p.phoneNumber}`} className="hover:underline">{p.phoneNumber}</a>
          </li>
        )}
      </ul>

      <div className="mt-3 flex gap-2">
        {/* صفحة الحجز لسه مش موجودة: الزرار ده هنربطه بعدين */}
        <button
          type="button"
          className="flex flex-1 items-center justify-center gap-2 rounded-full bg-blue-600 py-2.5 text-sm font-semibold text-white transition hover:bg-blue-700"
        >
          <CalendarIcon className="h-4 w-4" />
          Book Appointment
        </button>
        <a
          href={mapsUrl}
          target="_blank"
          rel="noopener noreferrer"
          className="flex items-center justify-center gap-2 rounded-full border border-slate-300 px-4 py-2.5 text-sm font-semibold text-slate-600 transition hover:bg-slate-50"
        >
          <PinIcon className="h-4 w-4 text-blue-600" />
          Directions
        </a>
      </div>
    </div>
  );
}

/* ---------- Page content ---------- */

function SearchContent() {
  const router = useRouter();
  const params = useSearchParams();
  const q = (params.get('q') || '').trim();

  const [searchTerm, setSearchTerm] = useState(q);
  const [locIndex, setLocIndex] = useState(0);
  const [items, setItems] = useState([]);
  const [page, setPage] = useState(1);
  const [hasNext, setHasNext] = useState(false);
  const [total, setTotal] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const loc = LOCATIONS[locIndex];

  useEffect(() => {
    if (!localStorage.getItem('token')) {
      router.replace('/landing');
    }
  }, [router]);

  useEffect(() => {
    setSearchTerm(q);
  }, [q]);

  const fetchPage = useCallback(
    async (pageNumber, append) => {
        console.log('FETCH PAGE STARTED');
      setLoading(true);
      setError('');
      try {
        const { data } = await API.post('/Search/UnifiedMedicalSearch', {
          keyword: q,
          latitude: loc.lat,
          longitude: loc.lng,
          radiusInMeters: RADIUS_METERS,
          pageNumber,
          pageSize: PAGE_SIZE,
        });
         console.log('SEARCH API RESPONSE:', data);
        const internal = data?.results?.medlinkProviders;
        console.log('ALL RESULTS:', data.results);
        console.log('MEDLINK PROVIDERS:', internal);
        console.log('EXTERNAL PROVIDERS:', data.results.externalProviders);
        const newItems = internal?.items ?? [];

        setItems((prev) => (append ? [...prev, ...newItems] : newItems));
        setPage(internal?.pageNumber ?? pageNumber);
        setHasNext(Boolean(internal?.hasNext));
        setTotal(internal?.totalCount ?? newItems.length);
      } catch (err) {
        if (err.response) {
          setError(`Search failed (error ${err.response.status}).`);
        } else {
          setError('Cannot reach the server. Check that the backend is running.');
        }
        console.error('SEARCH API ERROR:', err.message, err.response?.status, err.response?.data);
      } finally {
        setLoading(false);
      }
    },
    [q, loc]
  );

  // أول ما الكلمة أو الموقع يتغيروا: ابدأ من الصفحة 1
  useEffect(() => {
    fetchPage(1, false);
  }, [fetchPage]);

  const handleSubmit = (e) => {
    e.preventDefault();
    const term = searchTerm.trim();
    router.push(term ? `/search?q=${encodeURIComponent(term)}` : '/search');
  };

  return (
    <div className="min-h-screen bg-sky-50 font-sans text-slate-900">
      <header className="border-b border-slate-100 bg-white">
        <div className="mx-auto flex max-w-6xl items-center justify-between px-6 py-3.5">
          <div className="flex items-center gap-10">
            <span className="flex items-center gap-2 text-xl font-extrabold text-slate-900">
              <LogoIcon className="h-7 w-7 text-blue-600" />
              Med<span className="text-blue-600">Link</span>
            </span>
            <a href="/" className="text-sm font-semibold text-slate-800 hover:text-blue-600">
              Home
            </a>
          </div>
        </div>
      </header>

      <main className="mx-auto max-w-6xl px-6 pb-10 pt-5">
        <form onSubmit={handleSubmit} role="search" className="flex flex-col gap-3 sm:flex-row sm:items-center">
          <label className="flex items-center gap-2 rounded-xl bg-white px-3 py-2.5 shadow-sm">
            <PinIcon className="h-4 w-4 text-blue-600" />
            <select
              value={locIndex}
              onChange={(e) => setLocIndex(Number(e.target.value))}
              className="bg-transparent text-sm font-semibold text-slate-800 focus:outline-none"
              aria-label="Location"
            >
              {LOCATIONS.map((l, i) => (
                <option key={l.label} value={i}>{l.label}</option>
              ))}
            </select>
          </label>

          <div className="flex flex-1 items-center gap-2 rounded-full border border-slate-200 bg-white px-4 py-2.5 shadow-sm focus-within:border-blue-300">
            <SearchIcon className="h-4 w-4 shrink-0 text-slate-400" />
            <input
              type="text"
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              placeholder="Search hospitals, clinics, labs or radiology..."
              aria-label="Search"
              className="min-w-0 flex-1 bg-transparent text-sm text-slate-700 placeholder:text-slate-400 focus:outline-none"
            />
          </div>

          <button
            type="submit"
            className="flex items-center justify-center gap-3 rounded-xl bg-blue-600 px-8 py-2.5 text-base font-semibold text-white shadow-md shadow-blue-500/30 transition hover:bg-blue-700"
          >
            Search
            <SearchIcon className="h-5 w-5" />
          </button>
        </form>

        {q && !error && (
          <p className="mt-4 text-sm text-slate-600">
            {total} result{total === 1 ? '' : 's'} for “<span className="font-semibold text-slate-900">{q}</span>”
          </p>
        )}

        {error && (
          <p role="alert" className="mt-6 rounded-xl bg-red-50 px-4 py-3 text-sm text-red-700">
            {error}
          </p>
        )}

        {!error && !loading && items.length === 0 && (
          <p className="mt-10 text-center text-slate-500">No results found. Try a different search.</p>
        )}

        {items.length > 0 && (
          <section className="mt-5">
            <h2 className="text-center text-xl font-bold text-slate-900">MedLink Partners</h2>
            <div className="mt-3 grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-3">
              {items.map((p) => (
                <ProviderCard key={p.providerId} p={p} />
              ))}
            </div>
          </section>
        )}

        {loading && <p className="mt-6 text-center text-sm text-slate-500">Loading...</p>}

        {hasNext && !loading && (
          <div className="mt-6 flex justify-center">
            <button
              type="button"
              onClick={() => fetchPage(page + 1, true)}
              className="rounded-full border border-blue-300 bg-white px-8 py-2 text-sm font-semibold text-blue-700 transition hover:bg-blue-50"
            >
              View More Results
            </button>
          </div>
        )}
      </main>

      <footer className="bg-sky-100 py-3 text-center text-xs text-slate-500">
        Better Care &nbsp;•&nbsp; Smarter Health &nbsp;•&nbsp; For a Healthier Tomorrow
      </footer>
    </div>
  );
}

export default function SearchPage() {
  return (
    <Suspense fallback={null}>
      <SearchContent />
    </Suspense>
  );
}