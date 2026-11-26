# MedLink - Application Overview

## What MedLink is
MedLink is an AI-powered healthcare discovery and appointment platform for Egypt, available as a **website** (React) and a **mobile app** for Android and iOS (Flutter). Patients find, compare and book verified healthcare providers and keep their medical history in one place, guided by AI.

- Graduation project of Team 58, El Shorouk Academy, academic year 2026/2027, supervised by Dr. Hany Maher.
- Language: English now; Arabic interface support is planned (the AI assistant already understands Egyptian street Arabic, Franco-Arabic, standard Arabic and English, and replies in simple standard Arabic or English).
- Currency: Egyptian Pound (EGP). Phone numbers: Egyptian (+20). Ambulance: 123.

## The problem MedLink solves
1. Provider information is scattered across websites, pages and phone calls.
2. Patients do not know which specialist fits their symptoms.
3. Booking is slow and manual (phone calls, no live view of free slots).
4. Medical history is lost on paper, so doctors miss the full picture.
5. Each app covers one service; labs and radiology are left out.
6. Search uses basic filters only, with no AI recommendations.

## What MedLink offers
| Feature | Short description |
|---|---|
| AI-Based Search | Type or speak a need in plain words ("my knee hurts after running near Nasr City"); the AI turns it into a specialty, provider type, urgency and area, then shows matching verified providers. |
| Symptom Analysis | Suggests the suitable specialty and urgency level from the symptoms. |
| AI Health Assistant (chat) | Answers health and MedLink-usage questions 24/7 within safe limits (planned for the next phase). |
| Location Search | Nearest providers on Google Maps, sorted by distance. |
| Smart Recommendations | Ranked by specialty match, distance, rating and price. |
| Appointment Booking | Live time slots, booking request, provider confirmation, reminders. |
| Medical History | Visits, diagnoses, notes, lab results and imaging in one timeline. |
| Online Registration | Patients and verified providers sign up securely (email + verification, or Google). |
| Provider Dashboard | Providers manage services, prices, branches, schedule and booking requests. |
| Ratings & Reviews | Patients rate a provider after a completed visit. |

## Who uses MedLink (roles)
| Role | What they do |
|---|---|
| **Patient** | Searches, uses AI search, books, cancels or reschedules, keeps a medical history, reviews providers. Includes family members booking for someone. |
| **Healthcare Provider** | A Doctor, Clinic, Hospital, Laboratory or Radiology Center. Registers with a medical license, publishes services and prices, manages booking requests, adds medical records after a visit. |
| **Admin** | Verifies provider licenses (accept / reject), manages users, moderates reviews, generates reports and monitors the platform. |

External systems: **Google Maps** (map, geocoding, distance) and a **messaging gateway** (email, SMS, push notifications through Firebase).

## Provider types
Doctor, Clinic, Hospital, Laboratory, Radiology Center. Only providers approved by an admin appear to patients (they show a **Verified** badge).

## Specialty categories (Service.Category)
General Practice, Internal Medicine, Cardiology, Dermatology, Orthopedics, Pediatrics, Obstetrics and Gynecology, ENT (Ear, Nose and Throat), Ophthalmology, Dentistry, Neurology, Psychiatry, Urology, Gastroenterology, Pulmonology (Chest), Endocrinology and Diabetes, Nephrology, General Surgery, Physiotherapy, Laboratory Tests, Radiology and Imaging.

## Main system processes
1. Manage Accounts (patient accounts and provider accounts)
2. Search Providers (validate search, find verified providers, compute distance, rank)
3. Manage Bookings (check slots, create booking, provider decision, cancel, reschedule, visit outcome)
4. Manage Reviews (completed visits only, screened and moderated)
5. Manage Medical History (record visits, patient views own history, provider sees it only when they have a booking with the patient, every access is logged)
6. Provide Health Guidance (AI: symptoms -> specialty and urgency)
7. Send Notifications (email, SMS, push to patient or provider)
8. Generate Reports (admin reports on providers, bookings, reviews)

## Main data kept
Patients, Providers (with branch locations and license documents), Services and Provider Services (price per provider), Bookings, Reviews, Medical Records, Notifications, Refresh Tokens.

## How the AI works (short)
- The apps never call the AI model directly; every request goes through the MedLink ASP.NET Core backend, which checks the login (JWT).
- The model only reads the request and returns structured JSON (specialty, type, urgency, area). **Providers come from the MedLink database and Google Maps, so the AI never invents providers.** Places that come from Google Maps and are not registered on MedLink are marked **External**.
- Privacy: only the search text and area are sent to the model; no name, phone, email or national ID. Searches are not stored.
- The AI gives guidance only. It does not diagnose and never names medicines or doses. Urgent cases show "Call 123 or go to the nearest emergency room".

## Security
Passwords are hashed, login uses JWT access tokens and refresh tokens, role-based access (patient / provider / admin), HTTPS everywhere, medical history shared only with the patient's providers.

## Not available yet (planned enhancements)
- Telemedicine (video / online consultation)
- Pharmacy integration (sending prescriptions / ordering medicines)
- Full Arabic interface
- Medical insurance is **not** part of MedLink.
