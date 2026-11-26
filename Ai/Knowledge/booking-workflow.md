# MedLink - Booking Workflow

## Booking statuses
| Status | Meaning |
|---|---|
| **Pending** | The patient sent the request; waiting for the provider to accept. |
| **Confirmed** | The provider accepted. The patient gets a confirmation and reminders. |
| **Cancelled** | Cancelled by the patient or the provider (or declined by the provider). |
| **Completed** | The visit happened. The patient can now review the provider, and the provider can add a medical record. |

## Steps to book (patient)
1. **Find a provider**: normal search, nearby search on the map, or AI search from symptoms.
2. **Open the provider profile**: services with fees, locations, working hours, reviews and free slots.
3. **Choose the service** (for example "Consultation - 400 EGP").
4. **Choose the day and a free time slot**. Booked slots are crossed out.
5. **Review** the fee, add optional **notes** for the provider (for example the main symptoms).
6. **Confirm**. The system checks again that the slot is still free.
   - If someone took the slot a moment ago, other free slots are suggested.
7. The booking is saved as **Pending** and forwarded to the provider; a notification is queued.
8. The provider **accepts** -> status **Confirmed**; the patient gets a push notification and SMS. Or the provider **declines** -> **Cancelled**, the patient is notified and can book another slot.
9. **Reminder** before the appointment (push + SMS).
10. After the visit the provider records the **visit outcome** -> **Completed**.
11. The patient can **rate and review** the provider; the provider can add a **medical record**.

## AI summary for the doctor
If the patient chatted with the MedLink assistant before booking (in the same conversation), MedLink attaches a short AI summary of that chat to the booking for the provider: complaint, duration, symptoms, relevant history and urgency. No diagnosis. The patient does not need to do anything; the booking works the same with or without it.

## Cancel
- Patient: My Appointments -> choose an upcoming booking -> **Cancel** -> confirm. The provider is notified.
- Provider: My Bookings -> Cancel -> confirmation with an optional reason -> the patient is notified by email.
- Past bookings and bookings that are already cancelled cannot be cancelled.

## Reschedule
- Patient or provider chooses a new free slot for an upcoming booking; the other side is notified.

## Fees and payment
- The fee shown is the provider's price for that service, in EGP.
- MedLink does not support medical insurance and has no online payment. The patient pays the fee directly to the provider at the visit.

## Rules the system enforces
- Only verified providers can be booked.
- A slot cannot be booked twice.
- Reviews are allowed only for completed visits.
- The provider controls availability (weekly time slots).
