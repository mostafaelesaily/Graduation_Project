# MedLink AI Knowledge Context (for gemini-3.8-flash)

Knowledge only: the prompts and the MedLink knowledge files that the backend loads and sends to the model. No code.

```
MedLink_AI_Knowledge/
└── AI/
    ├── Prompts/
    │   ├── system-prompt.txt            the chat assistant: role, rules, language, output JSON
    │   └── previsit-summary-prompt.txt  the doctor summary written after a booking
    └── Knowledge/
        ├── application-overview.md      what MedLink is, roles, provider types, specialties
        ├── user-features.md             what a patient can do
        ├── provider-features.md         what a provider can do (incl. doctor summary)
        ├── admin-features.md            what the admin can do
        ├── booking-workflow.md          booking steps, statuses, cancel, payment at the visit
        ├── provider-registration.md     the 3 registration steps
        ├── medical-history.md           records, who sees them, how the AI uses them
        ├── ai-features.md               chat, safety, recommendations, voice, summary, privacy
        ├── specialty-guide.md           symptoms -> specialty and urgency
        ├── account-and-security.md      sign up, login, security
        ├── notifications.md             which messages are sent and how
        ├── faq.md                       common questions
        └── navigation-map.json          screen keys the assistant can point to
```

## 1. Chat assistant
**System instruction** = `system-prompt.txt` followed by every file in `Knowledge/`, each one wrapped like this:

```
<<<KNOWLEDGE FILE: booking-workflow.md>>>
...file content...
<<<END OF booking-workflow.md>>>
```

**Each request** sends the earlier messages of the same conversation as previous turns, then one user turn with this context (only the blocks you have):

```
USER_ROLE: patient
AREA: Nasr City, Cairo
PATIENT_MEDICAL_HISTORY:
* 2026-06-10 | Cardiology | Hypertension follow-up | diagnosis: Hypertension
VERIFIED_PROVIDERS: [{"provider_id":12,"name":"Dr. Karim Adel","specialty":"Orthopedics","provider_type":"Clinic","price_egp":350,"distance_km":1.8,"rating":4.6,"review_count":31}]
USER MESSAGE:
عندي وجع في ركبتي بعد الجري من أسبوع
```

- **PATIENT_MEDICAL_HISTORY**: only the logged-in patient's own records: date, specialty, title and diagnosis. No notes, no doctor names. Write `none recorded` if there are none.
- **VERIFIED_PROVIDERS**: verified, active providers near the patient, with their distance already worked out. The prompt tells the model to pick the specialty, then sort by location, rating and price, and to use only these providers.
- **Voice message**: send the recording (up to 3 minutes) as an audio part in the user turn instead of the text. The model writes what it heard in `transcript` and replies in text.
- Do not send the name, phone, email, national ID or exact coordinates.

**Recommended settings**: response type JSON, temperature 0.2, thinking level low.

**The model returns** one JSON object (format in Section 11 of the prompt): `scope`, `language`, `reply`, `specialty`, `provider_type`, `urgency`, `recommended_provider_ids`, `suggested_action`, `transcript`, `disclaimer`.
The app shows `reply` and `disclaimer`, the cards for `recommended_provider_ids`, and one button for `suggested_action`.

## 2. Doctor summary (after a booking)
After a booking made from a chat, call the model again with `previsit-summary-prompt.txt` as the system instruction. The input is the conversation, the patient's medical history, the booked specialty and service, and the output language (`en` or `ar`). It returns JSON: chief complaint, duration, symptoms, relevant history, medicines mentioned, red flags, urgency, suggested specialty and patient questions. It contains no diagnosis. Show it only to the provider of that booking.

## 3. Product decisions written into these files
- Answers only health questions and MedLink questions. Everything else gets a polite refusal (for example football scores).
- No diagnosis, no medicine names or doses. Emergencies start with "call 123 / nearest emergency room".
- The patient can write in Egyptian street Arabic, Franco or English. Arabic replies are in simple standard Arabic, English replies in English.
- The patient only sees the reply: at most one question and one button.
- Conversation memory lasts 2 hours. Voice messages are up to 3 minutes.
- Usage limit: 10 messages per minute and 100 per day per patient.
- No online payment and no insurance. The patient pays the doctor directly at the visit.
