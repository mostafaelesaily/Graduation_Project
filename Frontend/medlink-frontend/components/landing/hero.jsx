import Image from "next/image";
import Link from "next/link";
import { ArrowRightIcon, CheckCircleIcon, SparkleIcon } from "./icons";

const trustPoints = ["Verified Providers", "Easy Booking", "Secure & Private"];

export default function Hero() {
  return (
    <section
      id="home"
      className="relative scroll-mt-24 overflow-hidden bg-gradient-to-b from-[#f8fbfe] via-[#f4f9fd] to-white"
    >
      {/* soft brand glow behind the visual */}
      <div
        aria-hidden="true"
        className="pointer-events-none absolute -right-24 top-10 hidden h-[520px] w-[520px] rounded-full bg-[radial-gradient(circle_at_center,rgba(24,120,232,0.12),rgba(24,120,232,0))] lg:block"
      />

      <div className="mx-auto grid max-w-[1186px] items-center gap-12 px-5 pb-12 pt-10 sm:px-8 sm:pt-12 lg:grid-cols-[1.03fr_1fr] lg:gap-8 lg:px-[50px] lg:pb-2 lg:pt-9">
        <div className="max-w-[560px]">
          <h1 className="text-[36px] font-bold leading-[1.07] tracking-[-0.03em] text-ink sm:text-[46px] lg:text-[56px]">
            Your Health
            <br />
            <span className="text-brand">Connected</span>
          </h1>

          <p className="mt-7 max-w-[470px] text-[15.5px] leading-[1.65] text-muted sm:text-[16px] lg:mt-8">
            Find the right doctors, book appointments, access hospitals, labs and
            more &mdash; all in one place.
          </p>

          <p className="mt-3 flex items-start gap-2 text-[13.5px] leading-[1.6] text-muted sm:text-[14px]">
            <SparkleIcon className="mt-[3px] h-4 w-4 shrink-0 text-brand" />
            <span>Powered by AI to help you make better health decisions.</span>
          </p>

          <div className="mt-7 flex flex-wrap items-center gap-3 sm:gap-4">
            <Link
              href="/register"
              className="inline-flex h-[46px] items-center justify-center gap-2 rounded-full bg-brand px-6 text-[14.5px] font-semibold text-white shadow-[0_14px_30px_-12px_rgba(24,120,232,0.9)] transition-all hover:bg-brand-dark hover:shadow-[0_18px_34px_-12px_rgba(24,120,232,0.9)]"
            >
              Get Started
              <ArrowRightIcon className="h-4 w-4" />
            </Link>

            <a
              href="#services"
              className="inline-flex h-[46px] items-center justify-center rounded-full border border-brand-line bg-white px-6 text-[14.5px] font-semibold text-ink-soft shadow-[0_6px_16px_-10px_rgba(13,32,91,0.35)] transition-colors hover:border-[#c8dffa] hover:bg-[#f7fbff]"
            >
              Explore Services
            </a>
          </div>

          <ul className="mt-5 flex flex-wrap items-center gap-x-5 gap-y-2 text-[12.5px] text-muted">
            {trustPoints.map((point) => (
              <li key={point} className="flex items-center gap-1.5">
                <CheckCircleIcon className="h-4 w-4 text-brand" />
                {point}
              </li>
            ))}
          </ul>
        </div>

        <div className="relative flex justify-center lg:-mr-12 lg:justify-end">
          <div
            aria-hidden="true"
            className="absolute inset-x-4 bottom-0 top-6 rounded-[36px] bg-[radial-gradient(circle_at_60%_40%,#e7f2fe,#f8fbfe_70%)] lg:inset-x-0"
          />
          <Image
            src="/landing/hero-doctor.webp"
            alt="Doctor smiling while using the MedLink app on a tablet"
            width={1080}
            height={912}
            priority
            sizes="(max-width: 1024px) 88vw, 540px"
            className="relative z-10 h-auto w-full max-w-[540px]"
          />
        </div>
      </div>
    </section>
  );
}
