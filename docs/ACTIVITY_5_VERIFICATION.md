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

## Consolidated T01-T08 Execution Record

Environment: Windows, PostgreSQL, API at `http://localhost:5080`, Lab 5 database `equipment_borrowing_lab5_test`. Checks were performed on 2026-11-27 using the server's local date. IDs below are the IDs returned by the API at the time of each check; they can change after reseeding.

| Test | Result | Observed execution, failures, fixes, and retests | Evidence |
|---|---|---|---|
| T01 - Independent API reads and OpenAPI | Partial | Read students, equipment, borrowings, and borrowing ID 5 independently of the desktop app. Checked active/returned filters, search, sort parameters, missing borrowing/student/equipment responses (404), and invalid query values (400). Retrieved OpenAPI and confirmed the documented route list. The complete T01 checklist, including wrong method/route and empty-history cases, was not evidenced here. | `05-openapi-routes.png`; PowerShell HTTP outputs shared in the work record |
| T02 - Student management | Partial | Created temporary student `LAB5-TEMP-01`, updated its details, changed eligibility to false, and confirmed the student remained visible. A duplicate student number returned 409. The list endpoint briefly returned a safe 500, then a fresh request succeeded. The full sequence involving restriction, preserving an existing loan, and re-enabling eligibility was not evidenced. | `05-student-management.png`; PowerShell create/update/eligibility, duplicate, and recovery outputs |
| T03 - Equipment management | Partial | Created temporary equipment `LAB5-TEMP-EQ-01`, updated its descriptive fields, and confirmed duplicate equipment code returned 409. The UI showed the catalog and management form. A direct attempt to alter protected availability through an update was not evidenced. | `05-equipment-management.png`; PowerShell create/update/duplicate outputs |
| T04 - Single borrowing and return | Partial | Created a single borrowing and read it back. Returned borrowing 5; a second return attempt returned 409. A past expected return date returned 400, an ineligible student returned 409, and unavailable equipment returned 409. The full missing-reference and PostgreSQL readback sequence was not captured in the evidence listed here. | `05-returned-borrowing.png`; PowerShell request/response outputs |
| T05 - Multi-item checkout and atomicity | Partial | A two-item checkout succeeded with 201. An over-limit checkout returned 409; a database query confirmed no partial borrowing rows were added. Duplicate item IDs returned 400. A checkout containing unavailable equipment returned 409; a fresh database query confirmed the other item remained available and no borrowing was added. | `05-student-limit-success.png`, `05-student-limit-rollback.png`, `05-multi-checkout-rollback.png`; database query screenshots and PowerShell outputs |
| T06 - Search, filters, sorting, and histories | Partial | Queried all, active, and returned borrowings; searched for Ada; and used borrowed-date and expected-return-date sorting. Invalid status, sort, and direction each returned 400. Missing student and equipment history IDs returned 404. Empty-history behavior was not evidenced. | PowerShell query outputs shared in the work record |
| T07 - Avalonia HTTP integration | Partial | The desktop displayed an API-unavailable message when the API could not be reached. After the API was restored and Refresh was used, catalog data loaded again. API routes were also exercised independently of the desktop. A complete responsiveness/code walkthrough and fresh database comparison were not included in the screenshots. | `08-desktop-api-unavailable.png`, `08-desktop-api-recovered.png` |
| T08 - Unreachable API, server/database error, and recovery | Partial | Distinguished no HTTP response when the API was unavailable from a received safe 500 response when the API was running with an unavailable database. Restoring the database connection allowed requests to succeed again. The Lab 5 seed reset was verified at 4 students, 7 equipment records, 4 borrowings, and 3 active borrowings. | `08-desktop-api-unavailable.png`, `08-desktop-api-recovered.png`, `05-seed-reset-verified.png`; PowerShell curl output showing 500 |