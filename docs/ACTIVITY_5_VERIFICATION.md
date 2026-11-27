# Activity 5 Verification Record

Date performed: 2026-11-27
Branch: `activity-5`
Database used for manual acceptance checks: `equipment_borrowing_lab5_test`
Database used for PostgreSQL integration tests: `equipment_borrowing_test`

## Build and automated tests

| Check | Result |
|---|---|
| `dotnet build` | Passed; all solution projects built successfully. |
| `dotnet test` | Passed; 24 tests succeeded, 0 failed. |
| PostgreSQL integration rollback test | Passed; the injected failure after the first insert left no borrowing rows and both equipment items available. |

## API and OpenAPI checks

| Check | Result |
|---|---|
| OpenAPI route list | Passed; expected `/api/v1` routes present and template `/WeatherForecast` route removed. |
| OpenAPI response documentation | Passed; borrowing search, get, return, and history response codes were checked. Checkout POST routes document 201 Created. |
| Student, equipment, and borrowing reads | Passed; seeded records returned. |
| Borrowing search | Passed; active/returned filters and Ada search returned matching records. |
| Invalid status, sort, and direction | Passed; each returned 400 with `{code,message}`. |
| Missing borrowing, student history, and equipment history | Passed; each returned 404 with `{code,message}`. |
| Malformed JSON and invalid return date | Passed; each returned 400 with `{code,message}`. |
| Ineligible student and unavailable equipment | Passed; each was rejected with 409. |
| Duplicate equipment IDs in multi-checkout | Passed; rejected with 400. |
| Duplicate student and equipment identifiers | Passed; each rejected with 409. |

## Checkout and rollback checks

| Check | Result |
|---|---|
| Multi-item checkout at the three-item limit | Passed; Ada reached three active items. |
| Attempted fourth active item | Passed; returned 409 and database state remained unchanged. |
| Multi-item checkout containing an unavailable item | Passed; returned 409, Dani had no borrowing rows, and the other item stayed available. |
| Seed reset and verification | Passed; database reported 4 students, 7 equipment items, 4 borrowings, and 3 active borrowings. |

## Desktop and recovery checks

| Check | Result |
|---|---|
| Student management | Passed; temporary student registration, update, and eligibility change appeared in the desktop app. |
| Equipment management | Passed; temporary equipment registration and update appeared in the desktop app. |
| API unavailable | Passed; desktop displayed a connection message when the API was stopped. |
| API recovery | Passed; after restarting the API, desktop refresh loaded the catalog again. |
| Database unavailable and recovery | Passed; API returned a safe 500 `{code,message}` response while the database name was invalid, then returned student data after restoring the connection. |

## Evidence files

- `evidence/05-openapi-routes.png`
- `evidence/05-student-limit-success.png`
- `evidence/05-student-limit-rollback.png`
- `evidence/05-multi-checkout-rollback.png`
- `evidence/05-student-management.png`
- `evidence/05-equipment-management.png`
- `evidence/05-seed-reset-verified.png`
- `evidence/08-desktop-api-unavailable.png`
- `evidence/08-desktop-api-recovered.png`

The Lab 5 database was restored to the controlled seed state after the manual acceptance checks. The PostgreSQL integration tests use the separate `equipment_borrowing_test` database.
