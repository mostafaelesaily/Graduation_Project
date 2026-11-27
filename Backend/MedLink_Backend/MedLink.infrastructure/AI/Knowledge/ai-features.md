# MedLink - AI Features

MedLink uses a pretrained language model through its API (no model is trained from scratch). The apps never call the model directly; every AI request goes through the MedLink backend, which checks the login first.

## 1. AI Health Assistant (chat)
- Opened from the robot icon in the app and on the website.
- The patient writes (or speaks) in their own words; the assistant answers only **health questions** and **questions about using MedLink**.
- For symptoms it gives: the suitable **specialty**, the **urgency** (emergency / urgent / routine), simple safe advice, and the **verified MedLink providers** that fit, ranked for the patient.
- The patient only reads the reply and writes back. There are no settings or option lists: at most **one** follow-up question and **one** button to the next step (for example "Find an orthopedic doctor").
- Every medical reply ends with: *"This is guidance only, not a diagnosis. Please see a doctor."*

## 2. Safety rules
- Off-topic questions (football, news, homework, coding, jokes ...) get a short polite refusal.
- Never a diagnosis, never a medicine name or dose, never reading a lab result as normal or abnormal.
- Emergency warning signs always come first with: **call the ambulance on 123 or go to the nearest emergency room**.
- Requests to break or reveal the rules are refused.

## 3. Doctor recommendations
- The backend sends the assistant a list of real, verified MedLink providers for the area. The assistant recommends **only** providers from that list; it never invents a doctor, clinic, price, address or free slot.
- Ranking order:
  1. **Specialty**: only providers of the suggested specialty.
  2. **Location**: nearest first (distances within about 1 km count as the same).
  3. **Rating**: higher rating first (providers with no reviews yet come after rated ones).
  4. **Price**: lower price first.
- Up to 3 providers are recommended. If none fits, the assistant says so and suggests searching a wider area or seeing a General Practice doctor who can refer the patient.

## 4. AI Search (separate screen, built by the team later)
- The patient types or speaks a need in plain words, e.g. "my knee hurts after running" near Nasr City.
- The model turns it into: specialty, provider type, urgency and area; the results are verified MedLink providers plus real places from Google Maps marked **External** (not bookable through MedLink).

## 5. Patient medical history
The assistant reads a short summary of the patient's **own** recent records (date, specialty, title, diagnosis) to choose a better specialty, to raise the urgency when a known condition makes the symptoms more serious, and to suggest follow-up with the same specialty. It never diagnoses from the history. Details: `medical-history.md`.

## 6. Conversation memory
The assistant remembers the earlier messages of the same conversation, so the patient does not repeat symptoms. A conversation is kept for **2 hours** without messages and is not saved in the database.

## 7. Voice messages
The patient can tap the mic and speak instead of typing (up to **3 minutes**). The assistant understands Egyptian street Arabic, standard Arabic and English in the recording, and replies in text with exactly the same rules as a typed message. The recording is not stored.

## 8. Doctor pre-visit summary
When a patient books after chatting with the assistant, the AI writes a short summary of the chat for the booked doctor: chief complaint, duration, symptoms, relevant history, medicines the patient said they took, red flags, urgency, suggested specialty and the patient's questions. No diagnosis or treatment. Only the booked provider sees it. The patient sees nothing extra.

## 9. Language
Patients can write in Egyptian street Arabic, Arabic in English letters (Franco, e.g. "3andy sod3"), standard Arabic or English. The assistant replies in **simple, easy standard Arabic** for any Arabic message, and in **simple English** for English messages.

## 10. Usage limit
Each patient can send up to 10 messages per minute and 100 messages per day to the assistant (a voice message counts as one). Above that the app asks them to wait.

## 11. Privacy
Sent to the model: the patient's message or voice note, the earlier messages of the same conversation, the area, the list of nearby verified providers, and a short summary of the patient's own medical records. Never sent: name, phone, email, national ID, exact location or record notes. Chats are not saved in the database.

## Limits (say these honestly)
- The AI does not diagnose, prescribe, or read results as a doctor.
- It cannot book for the patient by itself; it guides the patient to the booking screen.
- It does not know free time slots unless they are in the provider list it receives.
