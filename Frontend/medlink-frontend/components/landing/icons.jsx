/*
 * Inline SVG icon set for the landing page.
 * All icons share a 24x24 viewBox and inherit `currentColor`, so they can be
 * coloured with Tailwind text utilities.
 */

const stroke = {
  fill: "none",
  stroke: "currentColor",
  strokeWidth: 1.7,
  strokeLinecap: "round",
  strokeLinejoin: "round",
};

function Icon({ children, className = "h-6 w-6" }) {
  return (
    <svg viewBox="0 0 24 24" className={className} {...stroke} aria-hidden="true">
      {children}
    </svg>
  );
}

export function LogoMark({ className = "h-8 w-8" }) {
  return (
    <svg viewBox="0 0 32 32" className={className} aria-hidden="true">
      <path
        d="M16 27.5S3.5 20.2 3.5 12.1A6.9 6.9 0 0 1 16 7.9a6.9 6.9 0 0 1 12.5 4.2C28.5 20.2 16 27.5 16 27.5Z"
        fill="currentColor"
        fillOpacity="0.12"
        stroke="currentColor"
        strokeWidth="2.1"
        strokeLinejoin="round"
      />
      <path
        d="M7.2 14.6h4.1l1.6-3.4 2.5 6.6 1.9-3.2h4.5"
        fill="none"
        stroke="currentColor"
        strokeWidth="1.9"
        strokeLinecap="round"
        strokeLinejoin="round"
      />
    </svg>
  );
}

/* ---------- Services ---------- */

export function DoctorIcon({ className }) {
  return (
    <Icon className={className}>
      <path d="M6 3v5.5a4 4 0 0 0 8 0V3" />
      <path d="M10 12.5v1.8a5.2 5.2 0 0 0 10.4 0v-1.4" />
      <circle cx="20.4" cy="10.7" r="2.1" />
    </Icon>
  );
}

export function HospitalIcon({ className }) {
  return (
    <Icon className={className}>
      <path d="M4.5 20.5V6.2c0-.9.7-1.7 1.7-1.7h11.6c1 0 1.7.8 1.7 1.7v14.3" />
      <path d="M3 20.5h18" />
      <path d="M12 7.6v5.2M9.4 10.2h5.2" />
      <path d="M9.7 20.5v-3.1h4.6v3.1" />
    </Icon>
  );
}

export function ClinicIcon({ className }) {
  return (
    <Icon className={className}>
      <rect x="3.4" y="7.6" width="17.2" height="12.4" rx="2.4" />
      <path d="M8.8 7.6V6.2A2.2 2.2 0 0 1 11 4h2a2.2 2.2 0 0 1 2.2 2.2v1.4" />
      <path d="M12 11.2v5.2M9.4 13.8h5.2" />
    </Icon>
  );
}

export function LabIcon({ className }) {
  return (
    <Icon className={className}>
      <path d="M9.4 3h5.2" />
      <path d="M10.4 3v6.1l-4.7 8.6a2.1 2.1 0 0 0 1.8 3.1h9a2.1 2.1 0 0 0 1.8-3.1l-4.7-8.6V3" />
      <path d="M7.7 15.2h8.6" />
    </Icon>
  );
}

export function RadiologyIcon({ className }) {
  return (
    <Icon className={className}>
      <circle cx="12" cy="12" r="8.6" />
      <circle cx="12" cy="12" r="3.4" />
      <path d="M12 3.4v5.2M12 15.4v5.2M3.4 12h5.2M15.4 12h5.2" />
    </Icon>
  );
}

export function NearbyIcon({ className }) {
  return (
    <Icon className={className}>
      <path d="M12 21s6.8-6.1 6.8-11.2A6.8 6.8 0 0 0 5.2 9.8C5.2 14.9 12 21 12 21Z" />
      <circle cx="12" cy="9.7" r="2.5" />
    </Icon>
  );
}

/* ---------- How it works ---------- */

export function SearchIcon({ className }) {
  return (
    <Icon className={className}>
      <circle cx="11" cy="11" r="7.2" />
      <path d="m20.5 20.5-4.2-4.2" />
    </Icon>
  );
}

export function CalendarIcon({ className }) {
  return (
    <Icon className={className}>
      <rect x="3.5" y="5.2" width="17" height="15.3" rx="2.6" />
      <path d="M8.3 3v4.4M15.7 3v4.4M3.5 10.3h17" />
      <path d="m9.4 15.4 1.9 1.9 3.5-3.6" />
    </Icon>
  );
}

export function CareIcon({ className }) {
  return (
    <Icon className={className}>
      <path d="M12 20.3S4.6 16 4.6 10.3a4.2 4.2 0 0 1 7.4-2.6 4.2 4.2 0 0 1 7.4 2.6c0 5.7-7.4 10-7.4 10Z" />
      <path d="M12 9.1v4.3M9.9 11.2h4.2" />
    </Icon>
  );
}

/* ---------- UI ---------- */

export function CheckIcon({ className = "h-4 w-4" }) {
  return (
    <Icon className={className}>
      <path d="m5 12.6 4.4 4.4L19 6.6" />
    </Icon>
  );
}

export function CheckCircleIcon({ className = "h-4 w-4" }) {
  return (
    <Icon className={className}>
      <circle cx="12" cy="12" r="9" />
      <path d="m8.2 12.3 2.6 2.6 5-5.2" />
    </Icon>
  );
}

export function ArrowRightIcon({ className = "h-4 w-4" }) {
  return (
    <Icon className={className}>
      <path d="M4.5 12h14.4" />
      <path d="m13.4 6.4 5.5 5.6-5.5 5.6" />
    </Icon>
  );
}

export function SparkleIcon({ className = "h-4 w-4" }) {
  return (
    <Icon className={className}>
      <path d="M12 3.4l1.9 5 5 1.9-5 1.9-1.9 5-1.9-5-5-1.9 5-1.9 1.9-5Z" />
      <path d="M18.6 16.2l.8 2 2 .8-2 .8-.8 2-.8-2-2-.8 2-.8.8-2Z" />
    </Icon>
  );
}

export function MenuIcon({ className = "h-6 w-6" }) {
  return (
    <Icon className={className}>
      <path d="M3.8 6.4h16.4M3.8 12h16.4M3.8 17.6h16.4" />
    </Icon>
  );
}

export function CloseIcon({ className = "h-6 w-6" }) {
  return (
    <Icon className={className}>
      <path d="m6 6 12 12M18 6 6 18" />
    </Icon>
  );
}

/* ---------- Social ---------- */

export function FacebookIcon({ className = "h-4 w-4" }) {
  return (
    <svg viewBox="0 0 24 24" className={className} fill="currentColor" aria-hidden="true">
      <path d="M13.5 21v-7.6h2.6l.4-3h-3V8.5c0-.9.3-1.5 1.6-1.5H16.6V4.3c-.3 0-1.3-.1-2.4-.1-2.4 0-4 1.4-4 4.1v2.1H7.6v3h2.6V21h3.3Z" />
    </svg>
  );
}

export function TwitterIcon({ className = "h-4 w-4" }) {
  return (
    <svg viewBox="0 0 24 24" className={className} fill="currentColor" aria-hidden="true">
      <path d="M17.2 3.5h3.1l-6.8 7.8 8 10.6h-6.3l-4.9-6.4-5.6 6.4H1.6l7.3-8.3L1.3 3.5h6.4l4.4 5.8 5.1-5.8Zm-1.1 16.5h1.7L7 5.3H5.2l10.9 14.7Z" />
    </svg>
  );
}

export function LinkedInIcon({ className = "h-4 w-4" }) {
  return (
    <svg viewBox="0 0 24 24" className={className} fill="currentColor" aria-hidden="true">
      <path d="M6.3 9.2H3.2V21h3.1V9.2ZM4.7 3.2a1.8 1.8 0 1 0 0 3.6 1.8 1.8 0 0 0 0-3.6ZM20.8 21h-3.1v-6.2c0-1.5-.5-2.5-1.9-2.5-1 0-1.6.7-1.9 1.4-.1.2-.1.6-.1.9V21H10.7s.1-10.7 0-11.8h3.1v1.7c.4-.6 1.2-1.6 2.9-1.6 2.1 0 3.7 1.4 3.7 4.4V21Z" />
    </svg>
  );
}

export function YouTubeIcon({ className = "h-4 w-4" }) {
  return (
    <svg viewBox="0 0 24 24" className={className} fill="currentColor" aria-hidden="true">
      <path d="M21.6 7.6a2.6 2.6 0 0 0-1.8-1.8C18.2 5.3 12 5.3 12 5.3s-6.2 0-7.8.5A2.6 2.6 0 0 0 2.4 7.6C2 9.2 2 12 2 12s0 2.8.4 4.4a2.6 2.6 0 0 0 1.8 1.8c1.6.5 7.8.5 7.8.5s6.2 0 7.8-.5a2.6 2.6 0 0 0 1.8-1.8c.4-1.6.4-4.4.4-4.4s0-2.8-.4-4.4ZM10.1 15.4V8.6l5.8 3.4-5.8 3.4Z" />
    </svg>
  );
}
