# Activity 5 Traceability and Verification

Date verified: 2026-11-27
Branch: `activity-5`

## API and data traceability

| Feature | API path(s) | Verification observed |
|---|---|---|
| Student catalog and management | `/api/v1/students`, `/api/v1/students/{id}`, `/api/v1/students/{id}/eligibility` | Listed students; registered a temporary student, updated its details and eligibility; duplicate student number returned 409. |
| Equipment catalog and management | `/api/v1/equipment`, `/api/v1/equipment/{id}` | Listed equipment; registered and updated a temporary item; duplicate equipment code returned 409. |
| Single checkout and return | `/api/v1/borrowings`, `/api/v1/borrowings/{id}`, `/api/v1/borrowings/{id}/return` | Created and read a borrowing, returned it, verified persistence, and confirmed a repeated return is rejected. |
| Multi-item checkout | `/api/v1/checkout-requests` | Successful checkout at the three-item student limit returned 201. Over-limit and unavailable-item requests returned 409 and left no partial database changes. |
| Borrowing search and sort | `GET /api/v1/borrowings` | Active and returned filters and student-name search returned matching rows. Invalid status, sort, or direction returned 400. |
| Student and equipment histories | `/api/v1/students/{id}/borrowings`, `/api/v1/equipment/{id}/borrowings` | History routes appeared in OpenAPI; missing student/equipment IDs returned 404. |
| OpenAPI | `/openapi/v1.json` | Expected API routes and response codes were present; the template WeatherForecast route was removed. |
| Desktop/API boundary | Avalonia desktop app | Desktop loaded data from the API, showed management records, displayed an API-unavailable message, and recovered after API restart. |
| Database error handling | API requests with unavailable database | API returned a safe 500 `{code,message}` response; after restoring the database connection, API reads succeeded. |

## Build and automated test results

- `dotnet build`: succeeded for all solution projects.
- `dotnet test`: 24 succeeded, 0 failed.
- PostgreSQL integration tests include injected failure after the first checkout insert; the test verified no partial borrowing rows and that both equipment items remained available.

## Controlled seed and state

The guarded script `activity5-seed-reset.sql` restores the Lab 5 database. After manual acceptance testing, verification showed:

- Students: 4
- Equipment: 7
- Borrowings: 4
- Active borrowings: 3

The integration tests use the separate `equipment_borrowing_test` database.

## Evidence

- `evidence/05-openapi-routes.png`
- `evidence/05-student-limit-success.png`
- `evidence/05-student-limit-rollback.png`
- `evidence/05-multi-checkout-rollback.png`
- `evidence/05-student-management.png`
- `evidence/05-equipment-management.png`
- `evidence/05-seed-reset-verified.png`
- `evidence/08-desktop-api-unavailable.png`
- `evidence/08-desktop-api-recovered.png`

The desktop equipment screenshot is clipped at the right edge; replace it with a full-width capture if possible.
