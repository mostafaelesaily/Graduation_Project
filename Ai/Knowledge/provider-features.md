# MedLink - Healthcare Provider Features

A provider is a **Doctor, Clinic, Hospital, Laboratory or Radiology Center**. A provider can use MedLink only after an admin approves its license (see `provider-registration.md`).

## 1. Account and profile
- Register as a provider (3 steps: create account, service & verification, admin review).
- Log in with email and password or Google; the provider portal opens on the dashboard.
- Edit profile: name, provider type, contact details, description.
- **Branch locations**: add one or more branches, each with branch name, address, postal code, service phone number and map position. The address is turned into map coordinates (Google Maps geocoding) so patients can find the branch in nearby search.

## 2. Services and prices
- Add the services the provider offers (for example "Cardiology consultation", "Complete blood count", "MRI knee") from the MedLink service list, each with its **price in EGP** and an active / inactive switch.
- Only active services appear to patients.

## 3. Schedule and availability
- Set the **weekly time slots** (working days and hours, days off).
- Patients can book only free slots; booked slots are hidden or crossed out.

## 4. My Bookings (booking requests)
- One table with all appointments, filtered by **Today, Next 7 days, Pending, Cancelled**.
- **Accept** or **cancel / decline** requests one by one or several at once.
- Cancelling shows a confirmation; the provider can add an optional reason and the patient is notified by email.
- Reschedule a booking when needed; the patient is notified.
- After the visit, mark the **visit outcome** (completed / no-show).

## 5. Patient medical records
- After a visit, **Add Medical History**: title, diagnosis and description are required; notes are optional. The record is added to the patient's timeline.
- View a patient's history only for patients who have a booking with this provider. Each access is logged.

## 6. AI pre-visit summary (doctor summary)
- When a patient books after chatting with the MedLink AI assistant, the booking details show a short **AI summary** for the provider: chief complaint, duration, symptoms, relevant history from the patient's records, medicines the patient said they took, red flags, urgency, the specialty the assistant suggested, and the patient's questions.
- It is a summary of what the patient said, **not a diagnosis** and not a treatment plan. The provider should confirm it with the patient.
- If the specialty the assistant suggested is different from the booked one, the summary says so.
- Only the provider of that booking can see it.
- Bookings made without a chat have no summary.

## 7. Ratings and reports
- See the reviews and ratings linked to the provider (from completed visits only).
- Follow activity: bookings and patient feedback in the dashboard.

## 8. Notifications
- New booking request (push), booking cancelled by the patient, verification result (push + email).

## What a provider cannot do
- Appear to patients before admin approval.
- See the medical history of a patient who has no booking with them.
- Delete or edit patient reviews (only the admin moderates reviews).
- Video consultations or pharmacy orders (not available yet).
