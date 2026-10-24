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

function SendIcon({ className = 'h-4 w-4' }) {
  return (
    <svg className={className} fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" viewBox="0 0 24 24" aria-hidden="true">
      <path d="M22 2L11 13" />
      <path d="M22 2l-7 20-4-9-9-4 20-7z" />
    </svg>
  );
}

function RobotIcon({ className = 'h-5 w-5' }) {
  return (
    <svg className={className} fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" viewBox="0 0 24 24" aria-hidden="true">
      <rect x="4" y="8" width="16" height="12" rx="3" />
      <path d="M12 4v4" />
      <circle cx="12" cy="3" r="1" fill="currentColor" stroke="none" />
      <path d="M9.5 13v1.5M14.5 13v1.5" />
      <path d="M1.5 12v3M22.5 12v3" />
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

/* ---------- Page ---------- */

export default function Home() {
  const [greeting, setGreeting] = useState('Good morning');
  const [userName, setUserName] = useState('Mona');
  const [searchTerm, setSearchTerm] = useState('');
  const [aiMessage, setAiMessage] = useState('');
  const [aiInput, setAiInput] = useState('');

  const router = useRouter();

  useEffect(() => {
    if (!localStorage.getItem('token')) {
      router.replace('/landing');
      return;
    }
    const hour = new Date().getHours();
    setGreeting(hour < 12 ? 'Good morning' : hour < 18 ? 'Good afternoon' : 'Good evening');
    const savedName = localStorage.getItem('userName');
    if (savedName) setUserName(savedName);
  }, []);

  const handleSearch = (e) => {
    e.preventDefault();
  };

  const handleAiSend = (e) => {
    e.preventDefault();
    if (!aiInput.trim()) return;
    setAiMessage(aiInput.trim());
    setAiInput('');
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

          <div className="flex items-center gap-4">
            <div className="hidden items-center gap-2 rounded-full border border-slate-200 bg-white px-3.5 py-2 shadow-sm lg:flex">
              <SearchIcon className="h-3.5 w-3.5 text-slate-400" />
              <input
                type="text"
                placeholder="Search..."
                className="w-36 bg-transparent text-xs text-slate-700 placeholder-slate-400 focus:outline-none"
              />
            </div>
            <button type="button" className="relative text-slate-500 transition hover:text-blue-600">
              <BellIcon />
              <span className="absolute -right-0.5 -top-0.5 h-2 w-2 rounded-full bg-blue-600 ring-2 ring-white" />
            </button>
            <span className="flex h-9 w-9 items-center justify-center rounded-full bg-gradient-to-br from-blue-400 to-blue-600 text-sm font-bold text-white ring-2 ring-blue-100">
              {userName.charAt(0).toUpperCase()}
            </span>
          </div>
        </div>
      </header>

      <main className="mx-auto max-w-6xl px-6 py-8">
        {/* ---------- Greeting + Search + AI Assistant ---------- */}
        <section className="flex flex-col gap-6 lg:flex-row lg:items-start">
          <div className="flex-1">
            <h1 className="text-3xl font-extrabold tracking-tight text-slate-900">
              {greeting}, {userName} <span className="align-middle">👋</span>
            </h1>
            <p className="mt-1.5 text-sm text-slate-500">How can we help you today?</p>

            <form onSubmit={handleSearch} className="mt-5 flex max-w-xl items-center gap-2 rounded-full border border-slate-200 bg-white p-1.5 pl-5 shadow-sm focus-within:border-blue-300">
              <SearchIcon className="h-4 w-4 shrink-0 text-slate-400" />
              <input
                type="text"
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                placeholder="Search doctors, services, or symptoms..."
                className="min-w-0 flex-1 bg-transparent py-1.5 text-sm text-slate-700 placeholder-slate-400 focus:outline-none"
              />
              <button
                type="submit"
                aria-label="Search"
                className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-blue-600 text-white shadow-md shadow-blue-500/30 transition hover:bg-blue-700"
              >
                <SearchIcon className="h-4 w-4" />
              </button>
            </form>
          </div>

          {/* AI Health Assistant */}
          <div className="w-full shrink-0 rounded-2xl border border-blue-100 bg-gradient-to-br from-blue-50 to-sky-100 p-5 lg:w-[360px]">
            <div className="flex items-center gap-3">
              <span className="flex h-11 w-11 items-center justify-center rounded-xl bg-blue-600 text-white shadow-md shadow-blue-500/30">
                <RobotIcon />
              </span>
              <div>
                <p className="text-sm font-bold text-slate-900">
                  AI Health Assistant <span className="text-xs">✨</span>
                </p>
                <p className="text-[11px] text-slate-500">Describe your symptoms or ask a question.</p>
              </div>
            </div>

            <form onSubmit={handleAiSend} className="mt-4 flex items-center gap-2 rounded-full border border-blue-100 bg-white px-4 py-2 shadow-sm">
              <input
                type="text"
                value={aiInput}
                onChange={(e) => setAiInput(e.target.value)}
                placeholder="Type your message..."
                className="min-w-0 flex-1 bg-transparent text-xs text-slate-700 placeholder-slate-400 focus:outline-none"
              />
              <button
                type="submit"
                aria-label="Send message"
                className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-blue-600 text-white transition hover:bg-blue-700"
              >
                <SendIcon className="h-3.5 w-3.5" />
              </button>
            </form>
            {aiMessage && (
              <p className="mt-3 ml-auto w-fit max-w-[85%] rounded-2xl rounded-tr-sm bg-blue-600 px-3.5 py-2 text-xs text-white shadow-sm">
                {aiMessage}
              </p>
            )}
          </div>
        </section>

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
      </main>
    </div>
  );
}
