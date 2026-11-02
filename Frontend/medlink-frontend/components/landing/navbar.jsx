"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import Logo from "./logo";
import { CloseIcon, MenuIcon } from "./icons";

const navLinks = [
  { label: "Home", href: "#home" },
  { label: "Services", href: "#services" },
  { label: "How It Works", href: "#how-it-works" },
  { label: "About", href: "#about" },
];

export default function Navbar() {
  const [open, setOpen] = useState(false);
  const [active, setActive] = useState("#home");

  // Close the mobile sheet when the viewport grows past the breakpoint.
  useEffect(() => {
    const onResize = () => {
      if (window.innerWidth >= 1024) setOpen(false);
    };
    window.addEventListener("resize", onResize);
    return () => window.removeEventListener("resize", onResize);
  }, []);

  return (
    <header className="sticky top-0 z-40 border-b border-[#eef4fb] bg-white/85 backdrop-blur-md">
      <div className="mx-auto flex h-[68px] max-w-[1186px] items-center justify-between gap-6 px-5 sm:px-8 lg:h-[84px] lg:px-5">
        <Logo />

        <nav className="hidden items-center gap-9 text-[15px] font-medium lg:ml-[70px] lg:mr-auto lg:flex">
          {navLinks.map((link) => (
            <a
              key={link.label}
              href={link.href}
              onClick={() => setActive(link.href)}
              className={`transition-colors hover:text-brand ${
                active === link.href ? "text-brand" : "text-muted"
              }`}
            >
              {link.label}
            </a>
          ))}
        </nav>

        <div className="flex items-center gap-4 sm:gap-6">
          <Link
            href="/login"
            className="hidden text-[15px] font-semibold text-ink-soft transition-colors hover:text-brand sm:block"
          >
            Log In
          </Link>

          <Link
            href="/register"
            className="inline-flex h-9 items-center justify-center rounded-full bg-brand px-5 text-[14px] font-semibold text-white shadow-[0_8px_18px_-8px_rgba(24,120,232,0.85)] transition-colors hover:bg-brand-dark"
          >
            Get Started
          </Link>

          <button
            type="button"
            onClick={() => setOpen((value) => !value)}
            aria-expanded={open}
            aria-controls="mobile-nav"
            aria-label={open ? "Close menu" : "Open menu"}
            className="-mr-1 grid h-10 w-10 place-items-center rounded-full text-ink transition-colors hover:bg-brand-soft lg:hidden"
          >
            {open ? <CloseIcon className="h-5 w-5" /> : <MenuIcon className="h-5 w-5" />}
          </button>
        </div>
      </div>

      <div
        id="mobile-nav"
        className={`overflow-hidden border-t border-[#eef4fb] bg-white transition-[max-height] duration-300 lg:hidden ${
          open ? "max-h-80" : "max-h-0 border-t-0"
        }`}
      >
        <nav className="mx-auto flex max-w-[1186px] flex-col px-5 py-2 sm:px-8">
          {navLinks.map((link) => (
            <a
              key={link.label}
              href={link.href}
              onClick={() => {
                setActive(link.href);
                setOpen(false);
              }}
              className="border-b border-[#f1f6fc] py-3 text-[15px] font-medium text-muted last:border-b-0 hover:text-brand"
            >
              {link.label}
            </a>
          ))}
          <Link
            href="/login"
            onClick={() => setOpen(false)}
            className="py-3 text-[15px] font-semibold text-ink-soft sm:hidden"
          >
            Log In
          </Link>
        </nav>
      </div>
    </header>
  );
}
