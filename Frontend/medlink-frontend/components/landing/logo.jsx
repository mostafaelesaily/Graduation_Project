import Link from "next/link";
import { LogoMark } from "./icons";

/**
 * MedLink wordmark: brand mark + name.
 * `size` keeps the navbar and footer lockups consistent.
 */
export default function Logo({ size = "md", href = "/landing" }) {
  const isSmall = size === "sm";

  return (
    <Link
      href={href}
      aria-label="MedLink – home"
      className="inline-flex items-center gap-2 text-ink transition-opacity hover:opacity-80"
    >
      <LogoMark
        className={
          isSmall
            ? "h-6 w-6 text-brand"
            : "h-[30px] w-[30px] text-brand sm:h-8 sm:w-8"
        }
      />
      <span
        className={
          isSmall
            ? "text-[18px] font-semibold tracking-[-0.02em]"
            : "text-[20px] font-semibold tracking-[-0.02em] sm:text-[22px]"
        }
      >
        MedLink
      </span>
    </Link>
  );
}
