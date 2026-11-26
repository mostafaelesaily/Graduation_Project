# MedLink - Patient (User) Features

The patient is the main user of MedLink. Everything below works on the website and the mobile app unless stated otherwise.

## 1. Account
- **Create account**: choose *Patient / User*, then full name, email, Egyptian phone number (+20) and password, or **Continue with Google**.
- **Verify email**: after sign-up a verification link is sent to the email. The user can resend the email or change the address. The account works only after verification.
- **Log in**: email and password, *Remember me*, *Forgot password* (reset by email), or Google sign-in. The session stays active with a refresh token, so the user does not have to log in again every time.
- **Profile**: update name, phone, password and health profile from the Profile tab.

## 2. Home page
- Greeting ("Good evening, <name>"), one search bar with an **AI Assistant** switch, and six service shortcuts (doctors, clinics, hospitals, labs, radiology, nearby providers).
- **Recommended providers** with rating, distance and fee in EGP, and the patient's **next appointment**.
- Mobile bottom navigation: **Home, Search, Appointments, Records, Profile**. The bell icon shows notifications.

## 3. Search
- **Normal search**: by specialty, provider type, city / area and name, with filters and comparison.
- **Nearby search**: uses the phone or browser location to show the nearest verified providers on Google Maps, sorted by distance. If location permission is denied, the patient types an address.
- **AI search**: the patient types or speaks the symptoms in Arabic or English (example: "my knee hurts after running"). The AI suggests a specialty and urgency, then shows verified MedLink providers nearby (plus real outside places from Google Maps, marked *External*). Results are ranked by specialty match, distance, rating and price. If nothing matches, alternatives are suggested.
- Hospital results show distance, rating, hours and address, with **Book Appointment** on each card.

## 4. AI Health Assistant (chat) - next phase
- Opened from the robot icon. The patient writes symptoms in plain words.
- The assistant suggests the right specialty, urgency and nearby verified providers to book.
- For serious symptoms (for example severe chest pain and difficulty breathing) it says so clearly and points to the nearest emergency room or ambulance 123.
- It answers only health questions and questions about using MedLink. It never diagnoses or names medicines.

- **Voice messages**: tap the mic, speak (Egyptian Arabic or English, up to 3 minutes), tap again to send. The assistant writes down what you said and replies in text, exactly like a typed message. The recording is not stored.

## 5. Provider profile
- Verified badge, provider type and specialty, services with fees in EGP, branch locations on the map, working hours, free time slots and reviews from completed visits.

## 6. Booking
- Pick the service, the day and a free time slot (booked slots are crossed out), review the fee, add optional notes, confirm.
- The booking is created as **Pending** and sent to the provider. The provider accepts it (**Confirmed**) or declines it. The patient is notified.
- Full steps: see `booking-workflow.md`.

## 7. My Appointments
- Tabs: **Upcoming, Past, Cancelled**, each booking with its status (Pending, Confirmed, Cancelled, Completed).
- View details, **cancel** an upcoming booking, or **reschedule** it to another free slot.
- **Rate the provider** after a completed visit.

## 8. Medical History (Records)
- One timeline with visit records, doctor notes, diagnoses, lab results and imaging.
- Providers add entries after a visit (title, diagnosis, notes, specialty, date).
- The patient can **upload files** (for example a lab test or scan) and **share** the history.
- A provider can see the patient's history only when the provider has a booking with that patient; every access is logged.

## 9. Ratings & Reviews
- Available only after a **completed** visit with that provider. Rating (1-5 stars) and an optional comment.
- Reviews are screened and moderated by the admin before they are shown.

## 10. Notifications
- Booking confirmation, booking accepted / changed / cancelled, appointment reminders.
- Channels: in-app / push (Firebase), SMS and email.

## What the patient cannot do (now)
- Video or online consultations, ordering medicines, paying with insurance, or paying online inside the app.
