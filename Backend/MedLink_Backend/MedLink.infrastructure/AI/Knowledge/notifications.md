# MedLink - Notifications

MedLink sends messages through **push notifications (Firebase Cloud Messaging)**, **SMS** and **email**. In the app, the **bell icon** shows all notifications; unread ones are highlighted.

| Event | Who receives it | Channel |
|---|---|---|
| Verification code / email verification | Patient or provider | Email / SMS |
| Booking request sent (Pending) | Patient | Push |
| New booking request | Provider | Push |
| Booking accepted (Confirmed) | Patient | Push + SMS |
| Booking declined or cancelled by the provider (with optional reason) | Patient | Push + email |
| Booking cancelled by the patient | Provider | Push |
| Booking rescheduled | The other side | Push |
| Appointment reminder before the visit | Patient | Push + SMS |
| Provider verification result (accepted / rejected with reason) | Provider | Push + email |

The backend writes each message in the user's language, sends it, and records whether it was delivered.

## Common questions
- *I don't get notifications.* Allow notifications for MedLink in the phone settings, check that you are logged in, and check spam for emails.
- *Can I turn off SMS reminders?* Notification preferences are in the Profile tab.
