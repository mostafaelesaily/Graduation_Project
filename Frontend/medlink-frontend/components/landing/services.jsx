import {
  ClinicIcon,
  DoctorIcon,
  HospitalIcon,
  LabIcon,
  NearbyIcon,
  RadiologyIcon,
} from "./icons";

const services = [
  { title: "Doctors", Icon: DoctorIcon },
  { title: "Hospitals", Icon: HospitalIcon },
  { title: "Clinics", Icon: ClinicIcon },
  { title: "Laboratories", Icon: LabIcon },
  { title: "Radiology", Icon: RadiologyIcon },
  { title: "Nearby", Icon: NearbyIcon },
];

export default function Services() {
  return (
    <section id="services" className="scroll-mt-24 bg-white">
      <div className="mx-auto max-w-[1186px] px-5 py-10 sm:px-8 sm:py-12 lg:px-[50px] lg:py-8">
        <h2 className="text-[24px] font-bold tracking-[-0.02em] text-ink sm:text-[26px]">
          Our Services
        </h2>
        <p className="mt-2 text-[14.5px] text-muted sm:text-[15px]">
          Access tons of healthcare services, all in one place.
        </p>

        <div className="mt-7 grid grid-cols-2 gap-3.5 sm:grid-cols-3 lg:mt-5 lg:grid-cols-6 lg:gap-x-[27px]">
          {services.map(({ title, Icon }) => (
            <a
              key={title}
              href="#services"
              className="group flex flex-col items-center justify-center rounded-2xl border border-[#e8f1fc] bg-[#f5f9fe] px-3 py-7 text-center transition-all hover:-translate-y-1 hover:border-brand-line hover:bg-white hover:shadow-[0_16px_32px_-22px_rgba(13,32,91,0.45)] lg:py-9"
            >
              <Icon className="h-8 w-8 text-brand transition-transform group-hover:scale-105" />
              <span className="mt-3.5 text-[13.5px] font-semibold text-ink sm:text-[14px]">
                {title}
              </span>
            </a>
          ))}
        </div>
      </div>
    </section>
  );
}
