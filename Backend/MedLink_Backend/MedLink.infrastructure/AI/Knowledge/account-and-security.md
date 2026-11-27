# MedLink - Accounts, Login and Security

## Account types
- **Patient / User**: books care and keeps a medical history.
- **Provider**: Doctor, Clinic, Hospital, Laboratory or Radiology Center (needs admin approval).
- **Admin**: MedLink staff (accounts are created by MedLink, not by sign-up).

## Sign up
- Patient: full name, email, Egyptian phone (+20), password, or Continue with Google.
- Provider: same plus the 14-digit national ID, then verification documents (see provider registration).
- An email can be used for only one account; if it is already registered an error is shown.

## Email verification
- A verification link is sent after sign-up; the account cannot log in until it is verified.
- Didn't arrive? Check spam, press *Resend email*, or change the address.

## Log in
- Email (or phone) and password, *Remember me*, or Google sign-in.
- Wrong details or an unverified account show an error.
- *Forgot password* sends a reset link by email.

## Sessions
- After login the backend issues an **access token (JWT)** and a **refresh token**. The app refreshes the session automatically, so the user does not log in again each time.
- *Log out* ends the session on that device.

## Security and privacy
- Passwords are stored hashed, never in plain text.
- All traffic uses HTTPS.
- Role-based access: patients, providers and admins only see what their role allows.
- Providers see a patient's medical history only if they have a booking with the patient; access is logged.
- MedLink staff and the AI assistant will **never** ask for your password, OTP code or bank card. Do not share them with anyone.

## Account problems the assistant can help with
- Can't log in -> check email/password, verify the email, use Forgot password.
- Didn't get the email -> spam folder, resend, check the address.
- Want to change phone or name -> Profile tab.
- Account disabled -> contact Help & Support.
