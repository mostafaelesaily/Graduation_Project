# MedLink Backend API Handoff

Base route prefix: `/api`

Authentication: JWT Bearer. SignalR hub: `/hubs/notifications`.

## Provider Booking APIs

### Get provider bookings
- **Method/Route:** `GET /api/Booking/provider/{providerProfileId}`
- **Authorization:** Authenticated; provider owner or Admin
- **Query parameters:**
  - `pageNum` (int, default 1)
  - `pageSize` (int, default 10)
  - `filter` (string, optional: `upcoming`, `today`, `past`, `cancelled`)
  - `startDate` (DateTime, optional)
  - `endDate` (DateTime, optional)
  - `search` (string, optional)
- **Response DTO:** `PaginatedResultDto<GetBookingDto>`
- **Example response:**
```json
{
  "data": [
    {
      "id": "guid",
      "userId": "patient-user-id",
      "providerProfileId": "guid",
      "bookingDate": "2026-11-22T10:00:00Z",
      "status": "Pending",
      "notes": "First visit",
      "cancellationReason": null,
      "patientName": "Patient Name",
      "createdAt": "2026-11-21T09:00:00Z"
    }
  ],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 1
}
```

### Get booking statistics
- **Method/Route:** `GET /api/Booking/statistics`
- **Authorization:** `Provider`
- **Response DTO:** `ProviderBookingStatisticsDto`
- **Example response:**
```json
{
  "todayCount": 2,
  "nextSevenDaysCount": 5,
  "pendingCount": 3,
  "cancelledCount": 1
}
```

### Get booking by ID
- **Method/Route:** `GET /api/Booking/{bookingId}`
- **Authorization:** Authenticated owner, provider owner, or Admin
- **Response DTO:** `GetBookingDto`

### Get my bookings
- **Method/Route:** `GET /api/Booking/my`
- **Authorization:** Authenticated
- **Query parameters:** `pageNum`, `pageSize`
- **Response DTO:** `PaginatedResultDto<GetBookingDto>`

### Create booking
- **Method/Route:** `POST /api/Booking`
- **Authorization:** `User`
- **Request DTO:** `CreateBookingDto`
- **Example request:**
```json
{
  "providerProfileId": "guid",
  "bookingDate": "2026-11-22T10:00:00Z"
}
```
- **Response DTO:** `GetBookingDto`
- **Status codes:** 200, 400, 404, 409

### Approve booking
- **Method/Route:** `PATCH /api/Booking/{bookingId}/approve`
- **Authorization:** `Provider`
- **Response DTO:** `GetBookingDto`

### Reject booking
- **Method/Route:** `PATCH /api/Booking/{bookingId}/reject`
- **Authorization:** `Provider`
- **Status code:** 204

### Cancel booking
- **Method/Route:** `PATCH /api/Booking/{bookingId}/cancel`
- **Authorization:** Authenticated owner, provider owner, or Admin
- **Request DTO:** `CancelBookingDto` (optional body)
- **Example request:**
```json
{
  "cancellationReason": "Patient requested cancellation"
}
```
- **Status code:** 204

### Bulk cancel bookings
- **Method/Route:** `PATCH /api/Booking/cancel/bulk`
- **Authorization:** `Provider`
- **Request DTO:** `BulkCancelBookingsDto`
- **Example request:**
```json
{
  "bookingIds": ["guid-1", "guid-2"],
  "cancellationReason": "Clinic closed"
}
```
- **Response DTO:** `BulkCancelBookingsResultDto`
- **Example response:**
```json
{
  "cancelledBookingIds": ["guid-1"],
  "rejectedBookingIds": ["guid-2"]
}
```

### Update booking notes
- **Method/Route:** `PUT /api/Booking/{bookingId}/notes`
- **Authorization:** Authenticated owner or Admin
- **Request DTO:** `UpdateBookingDto`
- **Status code:** 204

### Check availability
- **Method/Route:** `GET /api/Booking/availability`
- **Query parameters:** `providerProfileId`, `bookingDate`
- **Response:** `boolean`

---

## Provider Availability APIs

### Get availability
- **Method/Route:** `GET /api/Provider/availability`
- **Authorization:** `Provider`
- **Response DTO:** `IReadOnlyList<ProviderAvailabilityDto>`
- **Example response:**
```json
[
  {
    "dayOfWeek": 1,
    "startTime": "09:00:00",
    "endTime": "12:00:00"
  }
]
```

### Set availability
- **Method/Route:** `PUT /api/Provider/availability`
- **Authorization:** `Provider`
- **Request DTO:** `SetProviderAvailabilityDto`
- **Example request:**
```json
{
  "intervals": [
    {
      "dayOfWeek": 1,
      "startTime": "09:00:00",
      "endTime": "12:00:00"
    },
    {
      "dayOfWeek": 1,
      "startTime": "13:00:00",
      "endTime": "17:00:00"
    }
  ]
}
```
- **Status code:** 204

Availability behavior:
- Empty intervals for a configured day represents Day Off.
- Never-configured providers retain existing booking behavior.
- Configured providers are restricted to configured working intervals.
- Overlapping or invalid intervals are rejected.

---

## Provider Profile, Location, Document, and Service APIs

### Get profile
- **Method/Route:** `GET /api/Provider/profile`
- **Authorization:** `Provider`
- **Response DTO:** `GetProviderProfileDto`

### Update profile
- **Method/Route:** `PUT /api/Provider/profile`
- **Authorization:** `Provider`
- **Request DTO:** `UpdateProviderProfileDto`

### Locations
- `GET /api/Provider/locations`
- `POST /api/Provider/locations`
- `PUT /api/Provider/locations/{locationId}`
- `DELETE /api/Provider/locations/{locationId}`

### Documents
- `GET /api/Provider/documents`
- `POST /api/Provider/documents` (`multipart/form-data`, field: `file`)
- `DELETE /api/Provider/documents/{documentId}`

### Services
- `GET /api/Provider/services`
- `POST /api/Provider/services`
- `PUT /api/Provider/services/{serviceId}`
- `DELETE /api/Provider/services/{serviceId}`
- `GET /api/Provider/services/public/{providerProfileId}`

---

## Medical Record APIs

### Create medical record
- **Method/Route:** `POST /api/Provider/medical-records`
- **Authorization:** `Provider`
- **Request DTO:** `CreateMedicalRecordDto`
- **Example request:**
```json
{
  "patientId": "user-id",
  "title": "Follow-up visit",
  "recordDate": "2026-11-22T10:00:00Z",
  "description": "Patient reported improvement.",
  "diagnosis": "Stable condition",
  "notes": "Continue current medication"
}
```

### Get my medical records
- **Method/Route:** `GET /api/Provider/medical-records/my`
- **Authorization:** `User`

### Get medical record by ID
- **Method/Route:** `GET /api/Provider/medical-records/{recordId}`
- **Authorization:** Authenticated owner or creating provider

### Get records created by provider
- **Method/Route:** `GET /api/Provider/medical-records/created`
- **Authorization:** `Provider`

---

## Review APIs

### Create review
- **Method/Route:** `POST /api/Review`
- **Authorization:** `User`
- **Request DTO:** `CreateReviewDto`
- **Example request:**
```json
{
  "providerProfileId": "guid",
  "rating": 5,
  "comment": "Excellent service"
}
```

### Get reviews by provider
- **Method/Route:** `GET /api/Review/provider/{providerProfileId}`

### Get my reviews
- **Method/Route:** `GET /api/Review/my`
- **Authorization:** `User`

---

## Admin Provider Request APIs

### Get provider requests
- **Method/Route:** `GET /api/admin/provider-requests`
- **Authorization:** `Admin`
- **Query parameters:**
  - `pageNum` (int, default 1)
  - `pageSize` (int, default 10)
  - `search` (string, optional)
  - `status` (enum, optional: `Approved`, `Pending`, `Rejected`)
- **Response DTO:** `PaginatedResultDto<GetProviderRequestDto>`
- **Example response item:**
```json
{
  "providerId": "guid",
  "fullName": "Provider Name",
  "serviceName": "Al-Noor Lab",
  "email": "provider@example.com",
  "phoneNumber": "01012345678",
  "licenseImageUrl": "ProviderLicenses/license.pdf",
  "createdAt": "2026-11-21T09:00:00Z"
}
```

### Get provider documents
- **Method/Route:** `GET /api/admin/providers/{providerId}/documents`
- **Authorization:** `Admin`
- **Response DTO:** `IReadOnlyList<GetProviderDocumentDto>`

### Accept provider
- **Method/Route:** `PATCH /api/admin/providers/{providerId}/accept`
- **Authorization:** `Admin`
- **Status code:** 204

### Reject provider
- **Method/Route:** `PATCH /api/admin/providers/{providerId}/reject`
- **Authorization:** `Admin`
- **Query parameter:** `reason` (string, optional)
- **Status code:** 204

---

## Requirements checklist

| Requirement | Status |
|---|---|
| Provider booking statistics | Implemented |
| Provider booking filters | Implemented |
| Provider booking search | Implemented |
| Provider booking pagination | Implemented |
| Patient name in booking response | Implemented |
| Accept booking | Implemented |
| Cancel booking | Implemented |
| Bulk cancellation | Implemented |
| Cancellation reason | Implemented |
| Cancellation email | Implemented |
| Weekly availability management | Implemented |
| Availability validation | Implemented |
| Availability booking enforcement | Implemented for configured providers |
| MedicalRecord required fields | Implemented |
| MedicalRecord access control | Implemented |
| Admin search/filter/pagination | Implemented |
| Admin approval/rejection | Implemented |
| Admin rejection reason | Implemented |
| Admin email notifications | Implemented |
| Review create/retrieve | Implemented |

---

## Swagger verification
The application was previously started in Development mode and `/swagger/v1/swagger.json` was inspected. The generated document exposed the implemented controller operations, including Review endpoints and the UploadDocument multipart endpoint.

## Migration preparation

### Existing database
Apply only after approval:
1. `20261120141549_AddBookingExpiredStatusToSlotIndex`
2. `20261120195255_AddMedicalRecordProviderServiceReview`
3. `20261120201058_AddStoredFileProviderProfileRelationshipIfMissing`
4. `20261120213747_CompleteRequirementsSchema`
5. `20261120215629_AddProviderAvailabilityConfiguredFlag`

### Fresh database
Apply all migrations in chronological order, including the recovered StoredFile migration only if its file is restored from Git history. Do not restore the recovered migration as-is because it incorrectly recreates the `Bookings` table.

### Preconditions and risks
- Do not apply migrations without reviewing data-length risks.
- `MedicalRecords.Description` is changed to required `nvarchar(500)`; existing values longer than 500 characters will cause migration failure.
- Existing NULL `Description` or `Diagnosis` values are converted to empty strings.
- `HasAvailabilityConfigured` defaults to `false`, preserving legacy booking behavior for existing providers.
- No migration was applied during this handoff preparation.

## Final report
- Handoff document path: `Backend/MedLink_Backend/API_HANDOFF.md`
- Endpoints covered: Provider Booking, Provider Availability, Provider Profile/Locations/Documents/Services, Medical Records, Reviews, Admin Provider Requests.
- Genuine missing backend requirement: None identified for the documented Provider/Admin dashboard scope, except frontend/API contract compatibility remains unverified.
- Build: Succeeded, 0 errors.
- Tests: 124 passed, 0 failed, 0 skipped.
- Migration status: Generated and reviewed, not applied.
- No production code, appsettings, migrations, database, or Git state was modified during handoff preparation.