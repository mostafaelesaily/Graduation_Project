'use client';

import { useEffect } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';

const services = [
  { icon: '🩺', title: 'Doctors', desc: 'Find specialists' },
  { icon: '🏥', title: 'Hospitals', desc: 'Explore hospitals' },
  { icon: '🦷', title: 'Clinics', desc: 'Nearby clinics' },
  { icon: '🧪', title: 'Laboratories', desc: 'Book lab tests' },
  { icon: '☢️', title: 'Radiology', desc: 'Imaging centers' },
  { icon: '📍', title: 'Nearby', desc: 'Around your location' },
];

const steps = [
  { icon: '🔍', title: 'Search', desc: 'Use AI or filters to find the right service.' },
  { icon: '📅', title: 'Book', desc: 'Choose a provider and pick a convenient time.' },
  { icon: '💙', title: 'Get Care', desc: 'Attend your appointment and manage your records.' },
];

const navLinks = [
  { label: 'Home', href: '#home' },
  { label: 'Services', href: '#services' },
  { label: 'How It Works', href: '#how-it-works' },
  { label: 'About', href: '#about' },
];

function Logo() {
  return (
    <span className="flex items-center gap-2 text-2xl font-bold text-blue-900">
      <span className="text-blue-600">💙</span> MedLink
    </span>
  );
}

export default function LandingPage() {
  const router = useRouter();

  useEffect(() => {
    if (localStorage.getItem('token')) router.replace('/home');
  }, [router]);

  return (
    <div className="flex flex-col flex-1 bg-white text-slate-900">
      <header className="sticky top-0 z-10 border-b border-blue-50 bg-white/90 backdrop-blur">
        <div className="mx-auto flex max-w-6xl items-center justify-between px-6 py-4">
          <Logo />
          <nav className="hidden gap-8 text-sm font-medium text-slate-700 md:flex">
            {navLinks.map((l) => (
              <a key={l.label} href={l.href} className="hover:text-blue-600">
                {l.label}
              </a>
            ))}
          </nav>
          <div className="flex items-center gap-4 text-sm font-medium">
            <Link href="/login" className="text-blue-700 hover:underline">
              Log In
            </Link>
            <Link href="/register" className="rounded-full bg-blue-600 px-5 py-2 text-white hover:bg-blue-700">
              Get Started
            </Link>
          </div>
        </div>
      </header>

      <main>
        <section id="home" className="bg-gradient-to-br from-blue-50 via-white to-sky-100">
          <div className="mx-auto grid max-w-6xl items-center gap-10 px-6 py-16 md:grid-cols-2">
            <div>
              <span className="inline-block rounded bg-blue-100 px-3 py-1 text-[11px] font-semibold uppercase tracking-wide text-blue-700">
                Smart Healthcare Platform
              </span>
              <h1 className="mt-5 text-5xl font-bold leading-tight text-blue-950 md:text-6xl">
                Your Health
                <br />
                <span className="text-blue-600">Connected</span>
              </h1>
              <p className="mt-5 max-w-md text-lg text-slate-600">
                Find the right doctors, book appointments, access hospitals, labs and more — all in one place.
                Powered by AI to help you make better health decisions.
              </p>
              <div className="mt-8 flex flex-wrap gap-4">
                <Link href="/register" className="rounded-lg bg-blue-600 px-6 py-3 font-semibold text-white shadow hover:bg-blue-700">
                  Get Started →
                </Link>
                <a href="#services" className="rounded-lg border border-blue-100 bg-white px-6 py-3 font-semibold text-blue-900 shadow-sm hover:bg-blue-50">
                  Explore Services
                </a>
              </div>
              <ul className="mt-6 flex flex-wrap gap-5 text-sm text-slate-600">
                {['Verified Providers', 'Easy Booking', 'Secure & Private'].map((t) => (
                  <li key={t}>
                    <span className="text-blue-600">✓</span> {t}
                  </li>
                ))}
              </ul>
            </div>

            <div className="relative mx-auto w-full max-w-md">
              <div className="absolute -right-6 -top-6 h-72 w-72 rounded-full bg-blue-100/70" />
              <div className="relative space-y-4">
                <div className="flex items-center gap-3 rounded-2xl bg-white p-4 shadow-lg">
                  <span className="flex h-10 w-10 items-center justify-center rounded-full bg-blue-600 text-white">★</span>
                  <div className="text-sm">
                    <p className="font-semibold text-blue-950">AI Health Assistant</p>
                    <p className="text-slate-500">How can I help you today?</p>
                  </div>
                </div>
                <div className="rounded-xl bg-white p-3 text-sm text-slate-400 shadow">
                  🔍 Search for doctors, hospitals, labs...
                </div>
                <ul className="w-48 space-y-3 rounded-2xl bg-white p-4 text-sm font-medium text-blue-950 shadow-lg">
                  {['Doctors', 'Hospitals', 'Clinics', 'Laboratories', 'Radiology'].map((s) => (
                    <li key={s}>{s}</li>
                  ))}
                </ul>
              </div>
            </div>
          </div>
        </section>

        <section id="services" className="mx-auto max-w-6xl px-6 py-14">
          <h2 className="text-2xl font-bold text-blue-950">Our Services</h2>
          <p className="mt-1 text-sm text-slate-600">Access a wide range of healthcare services, all in one place.</p>
          <div className="mt-8 grid grid-cols-2 gap-4 md:grid-cols-3 lg:grid-cols-6">
            {services.map((s) => (
              <div key={s.title} className="rounded-xl border border-blue-50 bg-blue-50/40 p-5 text-center shadow-sm transition hover:-translate-y-1 hover:shadow-md">
                <div className="text-3xl">{s.icon}</div>
                <p className="mt-3 font-semibold text-blue-950">{s.title}</p>
                <p className="mt-1 text-xs text-slate-500">{s.desc}</p>
              </div>
            ))}
          </div>
        </section>

        <section id="how-it-works" className="bg-blue-50/60">
          <div className="mx-auto grid max-w-6xl gap-8 px-6 py-12 md:grid-cols-4">
            <div>
              <h2 className="text-2xl font-bold text-blue-950">How MedLink Works</h2>
              <p className="mt-1 text-sm text-slate-600">Simple steps to better healthcare.</p>
            </div>
            {steps.map((s, i) => (
              <div key={s.title} className="flex gap-3">
                <span className="text-2xl">{s.icon}</span>
                <div>
                  <p className="font-semibold text-blue-950">
                    {i + 1}. {s.title}
                  </p>
                  <p className="mt-1 text-sm text-slate-500">{s.desc}</p>
                </div>
              </div>
            ))}
          </div>
        </section>

        <section id="about" className="mx-auto max-w-6xl px-6 py-10">
          <div className="flex flex-col items-center justify-between gap-6 rounded-2xl bg-gradient-to-r from-blue-100 to-sky-50 px-8 py-10 md:flex-row">
            <div>
              <h2 className="text-xl font-bold text-blue-950">Your Health, Our Priority</h2>
              <p className="mt-1 text-sm text-slate-600">Join thousands of users who trust MedLink for their healthcare needs.</p>
            </div>
            <Link href="/register" className="rounded-lg bg-blue-600 px-6 py-3 text-sm font-semibold text-white hover:bg-blue-700">
              Get Started
            </Link>
          </div>
        </section>
      </main>

      <footer className="border-t border-blue-50">
        <div className="mx-auto flex max-w-6xl flex-col items-center justify-between gap-4 px-6 py-6 text-xs text-slate-600 md:flex-row">
          <Logo />
          <nav className="flex gap-8">
            {['About', 'Privacy', 'Terms', 'Contact'].map((l) => (
              <a key={l} href="#" className="hover:text-blue-600">
                {l}
              </a>
            ))}
          </nav>
          
        </div>
      </footer>
    </div>
  );
}
