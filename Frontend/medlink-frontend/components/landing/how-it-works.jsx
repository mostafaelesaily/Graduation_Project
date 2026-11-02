import { CalendarIcon, CareIcon, SearchIcon } from "./icons";

const steps = [
  {
    Icon: SearchIcon,
    title: "Search",
    description: "Use AI or filters to find the right service.",
  },
  {
    Icon: CalendarIcon,
    title: "Book",
    description: "Choose a provider and pick a convenient time.",
  },
  {
    Icon: CareIcon,
    title: "Get Care",
    description: "Attend your appointment and manage your records.",
  },
];

export default function HowItWorks() {
  return (
    <section id="how-it-works" className="scroll-mt-24 bg-canvas">
      <div className="mx-auto grid max-w-[1186px] gap-9 px-5 py-10 sm:px-8 sm:py-12 lg:grid-cols-[minmax(0,1fr)_minmax(0,2.15fr)] lg:items-center lg:gap-16 lg:px-[50px] lg:py-[26px]">
        <div>
          <h2 className="text-[25px] font-bold tracking-[-0.02em] text-ink sm:text-[28px]">
            How MedLink Works
          </h2>
          <p className="mt-2 text-[14.5px] text-muted sm:text-[15px]">
            Simple steps to better healthcare.
          </p>
        </div>

        <div className="grid gap-6 sm:grid-cols-3 sm:gap-0 sm:divide-x sm:divide-[#dfebf9]">
          {steps.map(({ Icon, title, description }, index) => (
            <div key={title} className="flex gap-3.5 sm:px-6 sm:first:pl-0 sm:last:pr-0">
              <span className="grid h-11 w-11 shrink-0 place-items-center rounded-full bg-white text-brand shadow-[0_8px_18px_-12px_rgba(13,32,91,0.5)]">
                <Icon className="h-[22px] w-[22px]" />
              </span>
              <div>
                <p className="text-[11.5px] font-semibold uppercase tracking-[0.08em] text-brand">
                  Step {index + 1}
                </p>
                <p className="mt-1 text-[14.5px] font-semibold text-ink">{title}</p>
                <p className="mt-1 text-[13px] leading-[1.6] text-muted">
                  {description}
                </p>
              </div>
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}
