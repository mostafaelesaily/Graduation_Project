import Image from "next/image";
import Link from "next/link";

export default function CtaSection() {
  return (
    <section id="about" className="scroll-mt-24 bg-white">
      <div className="mx-auto max-w-[1246px] px-5 pb-5 pt-4 sm:px-8 lg:px-[50px]">
        <div className="relative overflow-hidden rounded-[26px] bg-[#f1f7fe] px-6 py-8 text-center sm:px-10 lg:flex lg:h-[142px] lg:items-center lg:rounded-[28px] lg:px-0 lg:py-0 lg:pl-[482px] lg:pr-10 lg:text-left">
          <Image
            src="/landing/cta-team.webp"
            alt=""
            aria-hidden="true"
            width={856}
            height={288}
            sizes="(max-width: 1024px) 0px, 428px"
            className="pointer-events-none absolute bottom-0 left-0 hidden h-full w-auto lg:block"
            style={{ maskImage: "linear-gradient(to right, #000 88%, transparent 100%)" }}
          />

          <div className="lg:max-w-[460px]">
            <h2 className="text-[23px] font-bold leading-[1.2] tracking-[-0.02em] text-ink sm:text-[26px]">
              Your Health, Our Priority
            </h2>
            <p className="mt-1 text-[13px] leading-[1.45] text-muted sm:text-[13.5px]">
              Join thousands of users who trust MedLink for their healthcare needs.
            </p>
            <Link
              href="/register"
              className="mt-3 inline-flex h-9 items-center justify-center rounded-full bg-brand px-6 text-[14px] font-semibold text-white shadow-[0_12px_26px_-14px_rgba(24,120,232,0.95)] transition-colors hover:bg-brand-dark"
            >
              Get Started
            </Link>
          </div>
        </div>
      </div>
    </section>
  );
}
