# MedLink - Admin Features

The admin is the MedLink platform administrator. The admin panel is on the website only.

## 1. Provider requests (verification)
- Every new provider is checked before patients can find them.
- The **Provider Requests** page lists pending applications with the provider's details, service information, tax number and uploaded medical license.
- **Accept**: the admin confirms; the provider is notified by email and can start receiving patients. The profile goes live with a Verified badge.
- **Reject**: the admin confirms and can add a reason (for example "license image is not clear"); the provider is notified by email with the reason.
- Target review time: up to 48 hours.

## 2. Manage users
- View, search, activate or deactivate patient and provider accounts.

## 3. Moderate reviews
- Reviews are screened before they appear. The admin can approve or remove reviews that are offensive, fake or not related to the visit.

## 4. Reports
- The admin chooses report criteria (period, provider, specialty, area).
- The system validates the criteria, aggregates provider, booking and review data and formats the report (for example bookings per specialty, cancellations, top-rated providers). Reports are built from the data warehouse so they never slow down booking.

## 5. Monitor platform and security
- Watch system activity and security events (logins, record access logs).

## Notes for the assistant
- Patients and providers cannot contact an admin through the AI assistant. If a provider asks about a pending or rejected request, explain the process and that the result arrives by email.
- Never share another user's data or an admin decision that is not in the conversation context.
