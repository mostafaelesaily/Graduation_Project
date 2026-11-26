# MedLink - Medical History (Health Records)

## What it is
Every patient has one **medical history timeline** in MedLink (the *Records* tab on mobile, *Medical History* on the website). It keeps the patient's health information in one place so any MedLink provider the patient visits sees the full picture.

## What a record contains
Each record is one visit or one document:
- **Record date** (the visit date)
- **Title** (for example "Cardiology follow-up")
- **Diagnosis** (written by the provider)
- **Notes** (optional)
- **Specialty** of the provider who wrote it
- Attached files when uploaded: lab results, imaging (X-ray, MRI ...), prescriptions

## Who adds records
- **Providers** add a record after a visit from *My Bookings -> patient -> Add Medical History*. Title, diagnosis and description are required, notes are optional.
- **Patients** can upload their own files (for example an old lab test or scan) from the Records screen.

## Who can see the history
- The **patient** always sees their full history.
- A **provider** can see a patient's history only if the provider has a booking with that patient. The history is loaded, summarized and shown to the provider, and **every access is logged**.
- The **admin** does not read patients' medical records.
- The patient can **share** the history from the Records screen.

## How the AI assistant uses it
- When the patient is logged in, the assistant receives a short summary of the patient's **own** most recent records (up to 10 from the last 3 years): date, specialty, title and diagnosis. Notes, doctor names and files are **not** sent.
- It uses them to choose a better specialty, to raise the urgency when a known condition makes symptoms more serious, and to suggest follow-up with the same specialty.
- It never diagnoses from the history, never says a condition got better or worse, never comments on medicines, and never says a lab result is normal or abnormal.

## Common questions
- *Can I delete a record a doctor added?* Records added by a provider are part of the medical history and are kept; contact support if something is wrong.
- *Which file types can I upload?* Images and PDF files (for example lab reports or scans).
- *Is my history safe?* It is protected by login, role-based access, HTTPS encryption and access logging.
