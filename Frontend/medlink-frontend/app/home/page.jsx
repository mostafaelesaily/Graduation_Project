'use client';

import React, { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';

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

function BellIcon({ className = 'h-5 w-5' }) {
  return (
    <svg className={className} fill="none" stroke="currentColor" strokeWidth="1.8" viewBox="0 0 24 24" aria-hidden="true">
      <path d="M18 8a6 6 0 1 0-12 0c0 7-3 9-3 9h18s-3-2-3-9" strokeLinecap="round" strokeLinejoin="round" />
      <path d="M13.7 21a2 2 0 0 1-3.4 0" strokeLinecap="round" strokeLinejoin="round" />
    </svg>
  );
}

function SparkleIcon({ className = 'h-3 w-3' }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="currentColor" aria-hidden="true">
      <path d="M12 2.5l1.8 5.2 5.2 1.8-5.2 1.8L12 16.5l-1.8-5.2L5 9.5l5.2-1.8L12 2.5z" />
      <path d="M18.5 15l.9 2.6 2.6.9-2.6.9-.9 2.6-.9-2.6-2.6-.9 2.6-.9.9-2.6z" />
    </svg>
  );
}

function StarIcon({ className = 'h-3.5 w-3.5' }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="currentColor" aria-hidden="true">
      <path d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z" />
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

function StethoscopeIcon({ className = 'h-6 w-6' }) {
  return (
    <svg className={className} fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" viewBox="0 0 24 24" aria-hidden="true">
      <path d="M5 3v5a4 4 0 0 0 8 0V3" />
      <path d="M4 3h2M12 3h2" />
      <path d="M9 12v2a5 5 0 0 0 10 0v-1.2" />
      <circle cx="19" cy="10" r="2.2" />
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

function ClinicIcon({ className = 'h-6 w-6' }) {
  return (
    <svg className={className} fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" viewBox="0 0 24 24" aria-hidden="true">
      <path d="M3 10.5L12 4l9 6.5" />
      <path d="M5 10v10h14V10" />
      <path d="M12 13v4M10 15h4" />
      <path d="M9 20v-3.5h6V20" />
    </svg>
  );
}

function LabIcon({ className = 'h-6 w-6' }) {
  return (
    <svg className={className} fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" viewBox="0 0 24 24" aria-hidden="true">
      <path d="M9 3h6" />
      <path d="M10 3v6l-5 9a2 2 0 0 0 1.7 3h10.6a2 2 0 0 0 1.7-3l-5-9V3" />
      <path d="M7.2 15h9.6" />
    </svg>
  );
}

function RadiologyIcon({ className = 'h-6 w-6' }) {
  return (
    <svg className={className} fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" viewBox="0 0 24 24" aria-hidden="true">
      <path d="M4 8V6a2 2 0 0 1 2-2h2M16 4h2a2 2 0 0 1 2 2v2M20 16v2a2 2 0 0 1-2 2h-2M8 20H6a2 2 0 0 1-2-2v-2" />
      <circle cx="12" cy="12" r="3.2" />
    </svg>
  );
}

function PinIcon({ className = 'h-6 w-6' }) {
  return (
    <svg className={className} fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" viewBox="0 0 24 24" aria-hidden="true">
      <path d="M12 21s-7-5.5-7-11a7 7 0 1 1 14 0c0 5.5-7 11-7 11z" />
      <circle cx="12" cy="10" r="2.5" />
    </svg>
  );
}

/* ---------- Data ---------- */

const navLinks = [
  { label: 'Home', href: '#home', active: true },
  { label: 'Services', href: '#services' },
  { label: 'Appointments', href: '#appointments' },
  { label: 'Records', href: '#records' },
];

const services = [
  { icon: StethoscopeIcon, title: 'Doctors', desc: 'Find specialists' },
  { icon: HospitalIcon, title: 'Hospitals', desc: 'Explore hospitals' },
  { icon: ClinicIcon, title: 'Clinics', desc: 'Nearby clinics' },
  { icon: LabIcon, title: 'Laboratories', desc: 'Book lab tests' },
  { icon: RadiologyIcon, title: 'Radiology', desc: 'Imaging centers' },
  { icon: PinIcon, title: 'Nearby', desc: 'Around your location' },
];

const doctors = [
  { name: 'Dr. Sara Ahmed', specialty: 'Dermatologist', rating: '4.9', reviews: 120, distance: '2.5 km', price: 800, initials: 'SA', gradient: 'from-rose-400 to-pink-600' },
  { name: 'Dr. Omar Hassan', specialty: 'Pediatrician', rating: '4.8', reviews: 95, distance: '4.1 km', price: 600, initials: 'OH', gradient: 'from-sky-400 to-blue-600' },
  { name: 'Dr. Nour Ali', specialty: 'Cardiologist', rating: '4.7', reviews: 80, distance: '5.3 km', price: 950, initials: 'NA', gradient: 'from-teal-400 to-emerald-600' },
];

const searchCopy = {
  en: {
    greeting: { morning: 'Good morning', afternoon: 'Good afternoon', evening: 'Good evening' },
    subtitle: 'How can we help you today?',
    searchPlaceholder: 'Search doctors, services, or symptoms, or ask AI Health Assistant...',
    aiPlaceholder: 'Ask the AI Health Assistant about your symptoms...',
    searchLabel: 'Search doctors, services, or symptoms',
    searchShort: 'Search...',
    toggleAi: 'Toggle AI',
    search: 'Search',
    searchStatus: (term) => `Searching for “${term}”...`,
    aiStatus: (term) => `Asking the AI Health Assistant about “${term}”...`,
  },
  ar: {
    greeting: { morning: 'صباح الخير', afternoon: 'مساء الخير', evening: 'مساء الخير' },
    subtitle: 'كيف يمكننا مساعدتك اليوم؟',
    searchPlaceholder: 'ابحث عن الأطباء أو الخدمات أو الأعراض، أو اسأل المساعد الصحي الذكي...',
    aiPlaceholder: 'اسأل المساعد الصحي الذكي عن الأعراض التي تشعر بها...',
    searchLabel: 'البحث عن الأطباء أو الخدمات أو الأعراض',
    searchShort: 'بحث...',
    toggleAi: 'تفعيل الذكاء الاصطناعي',
    search: 'بحث',
    searchStatus: (term) => `جارٍ البحث عن «${term}»...`,
    aiStatus: (term) => `جارٍ سؤال المساعد الصحي الذكي عن «${term}»...`,
  },
};

/* ---------- Page ---------- */

export default function Home() {
  const [period, setPeriod] = useState('morning');
  const [userName, setUserName] = useState('Mona');
  const [lang, setLang] = useState('en');
  const [searchTerm, setSearchTerm] = useState('');
  const [aiEnabled, setAiEnabled] = useState(false);
  const [submittedSearch, setSubmittedSearch] = useState(null);

  const t = searchCopy[lang];
  const router = useRouter();

  useEffect(() => {
    if (!localStorage.getItem('token')) {
      router.replace('/landing');
      return;
    }
    const hour = new Date().getHours();
    setPeriod(hour < 12 ? 'morning' : hour < 18 ? 'afternoon' : 'evening');
    const savedName = localStorage.getItem('userName');
    if (savedName) setUserName(savedName);
  }, []);

  useEffect(() => {
    const syncLanguage = () => {
      setLang(document.documentElement.lang?.toLowerCase().startsWith('ar') ? 'ar' : 'en');
    };

    syncLanguage();
    const observer = new MutationObserver(syncLanguage);
    observer.observe(document.documentElement, { attributes: true, attributeFilter: ['lang', 'dir'] });

    return () => observer.disconnect();
  }, []);

  const handleSearch = (e) => {
    e.preventDefault();
    const term = searchTerm.trim();
    setSubmittedSearch(term ? { term, ai: aiEnabled } : null);
  };

  const toggleAi = () => {
    setAiEnabled((enabled) => !enabled);
    setSubmittedSearch(null);
  };

  const handleNavSearch = (e) => {
    e.preventDefault();
    const term = searchTerm.trim();
    setSubmittedSearch(term ? { term, ai: aiEnabled } : null);
    document.getElementById('home-search')?.scrollIntoView({ behavior: 'smooth', block: 'center' });
  };

  return (
    <div id="home" className="min-h-screen bg-white font-sans text-slate-900">
      {/* ---------- Header ---------- */}
      <header className="sticky top-0 z-20 border-b border-slate-100 bg-white/90 backdrop-blur">
        <div className="mx-auto flex max-w-6xl items-center justify-between gap-4 px-6 py-3.5">
          <div className="flex items-center gap-10">
            <span className="flex items-center gap-2 text-xl font-extrabold text-slate-900">
              <LogoIcon className="h-7 w-7 text-blue-600" />
              Med<span className="text-blue-600">Link</span>
            </span>
            <nav className="hidden items-center gap-7 text-sm font-medium md:flex">
              {navLinks.map((l) => (
                <a
                  key={l.label}
                  href={l.href}
                  className={`transition hover:text-blue-600 ${l.active ? 'font-semibold text-blue-600' : 'text-slate-500'}`}
                >
                  {l.label}
                </a>
              ))}
            </nav>
          </div>

          <div className="flex min-w-0 flex-1 items-center justify-end gap-3 sm:gap-4">
            <form
              onSubmit={handleNavSearch}
              role="search"
              className="hidden min-w-0 max-w-[200px] flex-1 items-center gap-2 rounded-full border border-slate-200 bg-white px-3.5 py-2 shadow-sm focus-within:border-blue-300 sm:flex lg:max-w-[280px]"
            >
              <SearchIcon className="h-4 w-4 shrink-0 text-slate-400" />
              <input
                type="text"
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                placeholder={t.searchShort}
                aria-label={t.searchLabel}
                className="min-w-0 flex-1 bg-transparent text-xs text-slate-700 placeholder:text-slate-400 focus:outline-none sm:text-sm"
              />
            </form>
            <button type="button" className="relative shrink-0 text-slate-500 transition hover:text-blue-600">
              <BellIcon />
              <span className="absolute -right-0.5 -top-0.5 h-2 w-2 rounded-full bg-blue-600 ring-2 ring-white" />
            </button>
            <span className="flex h-9 w-9 items-center justify-center rounded-full bg-gradient-to-br from-blue-400 to-blue-600 text-sm font-bold text-white ring-2 ring-blue-100">
              {userName.charAt(0).toUpperCase()}
            </span>
          </div>
        </div>
      </header>

      <main>
        {/* ---------- Greeting + Search ---------- */}
        <section className="bg-[radial-gradient(125%_145%_at_88%_-10%,#c7dfff_0%,#dcecff_32%,#edf5ff_62%,#ffffff_100%)]">
          <div className="mx-auto max-w-6xl px-6 pt-9 pb-10 text-start">
            <h1 className="text-3xl font-extrabold tracking-tight text-[#0b2545] sm:text-[2.6rem] sm:leading-tight">
              {t.greeting[period]}, {userName} <span className="align-middle">👋</span>
            </h1>
            <p className="mt-2 text-lg text-[#173a63] sm:text-[1.35rem]">{t.subtitle}</p>

            <form
              id="home-search"
              onSubmit={handleSearch}
              role="search"
              className="mt-7 flex w-full scroll-mt-24 flex-col gap-2 rounded-[28px] border border-white/70 bg-white/95 p-2 shadow-[0_10px_35px_-12px_rgba(37,99,235,0.45)] backdrop-blur transition focus-within:border-blue-200 focus-within:shadow-[0_14px_40px_-12px_rgba(37,99,235,0.55)] sm:flex-row sm:items-center sm:gap-3 sm:rounded-full sm:py-2 sm:ps-6 sm:pe-2"
            >
              <div className="flex min-w-0 flex-1 items-center gap-3 px-2 py-1 sm:px-0 sm:py-0">
                <SearchIcon className="h-[18px] w-[18px] shrink-0 text-slate-400 sm:h-5 sm:w-5" />
                <input
                  type="text"
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  placeholder={aiEnabled ? t.aiPlaceholder : t.searchPlaceholder}
                  aria-label={t.searchLabel}
                  className="min-w-0 flex-1 bg-transparent py-1.5 text-sm text-slate-700 placeholder:text-slate-500 focus:outline-none sm:text-[15px]"
                />
              </div>

              <div className="flex items-center justify-between gap-2 border-t border-slate-100 pt-2 sm:justify-end sm:border-0 sm:pt-0">
                <button
                  type="button"
                  onClick={toggleAi}
                  aria-pressed={aiEnabled}
                  aria-label={t.toggleAi}
                  title={t.toggleAi}
                  className={`flex h-11 shrink-0 items-center gap-2.5 rounded-full px-3 text-[13px] font-semibold transition focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-blue-500 focus-visible:ring-offset-2 ${
                    aiEnabled
                      ? 'bg-blue-50 text-blue-700 ring-1 ring-blue-200'
                      : 'text-slate-800 shadow-sm ring-1 ring-slate-200/70 hover:bg-slate-50'
                  }`}
                >
                  <span className="relative flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-gradient-to-br from-white to-sky-100 shadow-sm ring-1 ring-blue-200">
                    <span className="text-[11px] font-extrabold tracking-tight text-blue-600">AI</span>
                    <SparkleIcon className="absolute -end-1 -top-1 h-3 w-3 text-sky-500" />
                  </span>
                  <span aria-hidden="true" className="h-7 w-px bg-slate-200" />
                  <span className="whitespace-nowrap">{t.toggleAi}</span>
                </button>

                <button
                  type="submit"
                  className="h-11 shrink-0 rounded-full bg-[#1f6bff] px-7 text-[15px] font-semibold text-white shadow-[0_8px_20px_-6px_rgba(37,99,235,0.65)] transition hover:bg-blue-700 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-blue-500 focus-visible:ring-offset-2"
                >
                  {t.search}
                </button>
              </div>
            </form>

            {submittedSearch && (
              <p role="status" aria-live="polite" className="mt-2 px-4 text-xs text-[#31527a]">
                {submittedSearch.ai ? t.aiStatus(submittedSearch.term) : t.searchStatus(submittedSearch.term)}
              </p>
            )}
          </div>
        </section>

        <div className="mx-auto max-w-6xl px-6 pb-8">
        {/* ---------- Our Services ---------- */}
        <section id="services" className="mt-10">
          <div className="flex items-center justify-between">
            <h2 className="text-lg font-bold text-slate-900">Our Services</h2>
            <a href="#services" className="text-xs font-semibold text-blue-600 hover:underline">
              View all →
            </a>
          </div>
          <div className="mt-4 grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-6">
            {services.map((s) => (
              <button
                key={s.title}
                type="button"
                className="rounded-2xl border border-slate-200 bg-white p-4 text-left shadow-sm transition hover:-translate-y-0.5 hover:border-blue-200 hover:shadow-md"
              >
                <s.icon className="h-6 w-6 text-blue-600" />
                <p className="mt-3 text-sm font-bold text-slate-900">{s.title}</p>
                <p className="mt-0.5 text-[11px] text-slate-500">{s.desc}</p>
              </button>
            ))}
          </div>
        </section>

        {/* ---------- Recommended + Upcoming Appointment ---------- */}
        <section className="mt-10 grid grid-cols-1 gap-8 lg:grid-cols-4">
          {/* Recommended */}
          <div className="lg:col-span-3">
            <div className="flex items-center justify-between">
              <h2 className="text-lg font-bold text-slate-900">Recommended for you</h2>
              <a href="#home" className="text-xs font-semibold text-blue-600 hover:underline">
                View all →
              </a>
            </div>
            <div className="mt-4 grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-3">
              {doctors.map((d) => (
                <div
                  key={d.name}
                  className="rounded-2xl border border-slate-200 bg-white p-4 shadow-sm transition hover:shadow-md"
                >
                  <div className="flex items-center gap-3">
                    <span className={`flex h-12 w-12 shrink-0 items-center justify-center rounded-full bg-gradient-to-br ${d.gradient} text-sm font-bold text-white`}>
                      {d.initials}
                    </span>
                    <div className="min-w-0">
                      <p className="truncate text-sm font-bold text-slate-900">{d.name}</p>
                      <p className="text-xs text-slate-500">{d.specialty}</p>
                    </div>
                  </div>
                  <div className="mt-3 flex flex-wrap items-center gap-x-2 gap-y-1 text-xs text-slate-500">
                    <StarIcon className="h-3.5 w-3.5 text-amber-400" />
                    <span>
                      <span className="font-bold text-slate-800">{d.rating}</span> ({d.reviews} reviews)
                    </span>
                    <span className="text-slate-300">•</span>
                    <span>{d.distance}</span>
                  </div>
                  <div className="mt-4 flex items-center justify-between">
                    <span className="text-sm font-extrabold text-blue-600">{d.price} EGP</span>
                    <button
                      type="button"
                      className="rounded-full bg-blue-600 px-4 py-1.5 text-xs font-bold text-white shadow-md shadow-blue-500/20 transition hover:bg-blue-700"
                    >
                      Book Now
                    </button>
                  </div>
                </div>
              ))}
            </div>
          </div>

          {/* Upcoming Appointment */}
          <div id="appointments">
            <div className="flex items-center justify-between gap-2">
              <h2 className="whitespace-nowrap text-base font-bold text-slate-900">Upcoming Appointment</h2>
              <a href="#appointments" className="whitespace-nowrap text-xs font-semibold text-blue-600 hover:underline">
                View all →
              </a>
            </div>
            <div className="mt-4 rounded-2xl border border-slate-200 bg-white p-4 shadow-sm">
              <div className="flex items-center gap-3">
                <span className="flex h-12 w-12 shrink-0 items-center justify-center rounded-full bg-gradient-to-br from-rose-400 to-pink-600 text-sm font-bold text-white">
                  SA
                </span>
                <div className="min-w-0">
                  <p className="truncate text-sm font-bold text-slate-900">Dr. Sara Ahmed</p>
                  <p className="text-xs text-slate-500">Dermatologist</p>
                </div>
              </div>
              <div className="mt-3 flex items-center gap-2 rounded-xl bg-sky-50 px-3 py-2 text-xs font-medium text-slate-700">
                <CalendarIcon className="h-4 w-4 shrink-0 text-blue-600" />
                Oct 30, 2024 • 10:00 AM
              </div>
              <button
                type="button"
                className="mt-4 w-full rounded-full bg-blue-600 py-2.5 text-xs font-bold text-white shadow-md shadow-blue-500/20 transition hover:bg-blue-700"
              >
                View Details
              </button>
            </div>
          </div>
        </section>

        {/* Placeholder anchor for the Records nav item */}
        <span id="records" className="block h-0 scroll-mt-24" />
        </div>
      </main>
    </div>
  );
}
