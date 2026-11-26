import Link from "next/link";
import Logo from "./logo";
import {
  FacebookIcon,
  LinkedInIcon,
  TwitterIcon,
  YouTubeIcon,
} from "./icons";

const footerLinks = [
  { label: "About", href: "#about" },
  { label: "Privacy", href: "/privacy" },
  { label: "Terms", href: "/terms" },
  { label: "Contact", href: "/contact" },
];

const socials = [
  { name: "Facebook", href: "#", Icon: FacebookIcon },
  { name: "Twitter", href: "#", Icon: TwitterIcon },
  { name: "LinkedIn", href: "#", Icon: LinkedInIcon },
  { name: "YouTube", href: "#", Icon: YouTubeIcon },
];

export default function SiteFooter() {
  return (
    <footer className="bg-white">
      <div className="mx-auto flex max-w-[1186px] flex-col items-center gap-5 px-5 py-8 sm:px-8 md:flex-row md:justify-between md:gap-8 lg:px-5">
        <Logo size="sm" />

        <nav className="flex flex-wrap items-center justify-center gap-x-7 gap-y-2 text-[13px] font-medium text-muted">
          {footerLinks.map((link) => (
            <Link
              key={link.label}
              href={link.href}
              className="transition-colors hover:text-brand"
            >
              {link.label}
            </Link>
          ))}
        </nav>

        <div className="flex items-center gap-4 text-ink-soft">
          {socials.map(({ name, href, Icon }) => (
            <a
              key={name}
              href={href}
              aria-label={name}
              className="grid h-8 w-8 place-items-center rounded-full transition-colors hover:bg-brand-soft hover:text-brand"
            >
              <Icon className="h-[15px] w-[15px]" />
            </a>
          ))}
        </div>
      </div>
    </footer>
  );
}
