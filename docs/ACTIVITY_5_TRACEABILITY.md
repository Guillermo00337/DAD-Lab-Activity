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

## Lab 4 use-case to Lab 5 implementation and evidence matrix

| Use case | Business rules | API | Test IDs | Implementation reference | Evidence |
|---|---|---|---|---|---|
| UC-01 Equipment catalog | BR-05-07, 10, 18-19 | API-01 | T01, T07 | `CatalogController.GetEquipment`; `EquipmentCatalogService.GetEquipmentAsync` / `GetAvailableEquipmentAsync`; `EfEquipmentRepository.GetAllAsync` / `GetAvailableAsync` | T01 PowerShell read results and `evidence/05-openapi-routes.png`; T07 `evidence/08-desktop-api-recovered.png` |
| UC-02 Equipment availability | BR-10, 18-19 | API-01 | T01, T07 | `CatalogController.GetEquipment`; `EquipmentCatalogService.GetAvailableEquipmentAsync`; availability changes through `EfBorrowingRepository.CheckoutAsync` / `ReturnAsync` | T01 PowerShell availability/read results; T07 `evidence/08-desktop-api-recovered.png` |
| UC-03 Borrow equipment | BR-08-11, 14-16, 18-19 | API-10, API-13 | T04, T07 | `BorrowingsController.Create` / `GetById`; `BorrowEquipmentService.BorrowAsync`; `EfBorrowingRepository.CheckoutAsync` / `GetByIdAsync` | T04 PowerShell create/read results; T07 desktop recovery screenshot |
| UC-04 Search borrowings | BR-14, 18-19 | API-14 | T01, T06, T07 | `BorrowingsController.Search`; `BorrowingQueryService.SearchAsync`; `EfBorrowingRepository.SearchAsync` | T01/T06 PowerShell search and filter outputs |
| UC-05 Return equipment | BR-10, 13-16, 18-19 | API-12, API-13 | T04, T07 | `T04 PowerShell return, readback, and repeat-return outputs; `evidence/05-returned-borrowing.png` (returned record); `evidence/05-camera-available-after-return.png` (equipment available after return)` |
| UC-06 Multi-item checkout | BR-08-12, 14-16, 18-19 | API-11 | T05, T07 | `BorrowingsController.CreateCheckout`; `BorrowEquipmentService.BorrowMultipleAsync`; `EfBorrowingRepository.CheckoutAsync` | `evidence/05-student-limit-success.png`, `evidence/05-student-limit-rollback.png`, `evidence/05-multi-checkout-rollback.png` |
| UC-07 Student history | BR-14, 18-19 | API-02, API-03, API-15 | T01, T06, T07 | `CatalogController.GetStudents` / `GetStudent`; `BorrowingsController.GetStudentHistory`; `BorrowingQueryService.GetStudentHistoryAsync`; `EfBorrowingRepository.GetStudentHistoryAsync` | T01/T06 PowerShell history and missing-ID outputs |
| UC-08 Equipment history | BR-14, 18-19 | API-01, API-07, API-16 | T01, T06, T07 | `CatalogController.GetEquipment` / `GetEquipmentById`; `BorrowingsController.GetEquipmentHistory`; `BorrowingQueryService.GetEquipmentHistoryAsync`; `EfBorrowingRepository.GetEquipmentHistoryAsync` | T01/T06 PowerShell history and missing-ID outputs |
| UC-09 Sort borrowing results | BR-17-19 | API-14 | T06, T07 | `BorrowingsController.Search`; `BorrowingQueryService.SearchAsync`; `EfBorrowingRepository.SearchAsync` | T06 PowerShell sort and invalid-sort outputs |
| UC-12 Register student | BR-01-02, 16, 18-19 | API-04 | T02, T07 | `StudentsController.Create`; `StudentManagementService`; `EfStudentRepository` | `evidence/05-student-management.png`; T02 PowerShell create/duplicate outputs |
| UC-13 Update student | BR-01-03, 14, 16, 18-19 | API-02, API-03, API-05 | T02, T07 | `StudentsController.Update`; `StudentManagementService.UpdateAsync`; `EfStudentRepository.UpdateDetailsAsync` | `evidence/05-student-management.png`; T02 PowerShell update output |
| UC-14 Manage borrowing eligibility | BR-04, 08, 13-14, 18-19 | API-02, API-03, API-06 | T02, T07 | `StudentsController.SetEligibility`; `StudentManagementService.SetEligibilityAsync`; `Student.SetBorrowingEligibility` | `evidence/05-student-management.png`; T02 PowerShell eligibility output |
| UC-15 Register equipment | BR-05-06, 10, 16, 18-19 | API-08 | T03, T07 | `EquipmentController.Create`; `EquipmentManagementService`; `EfEquipmentRepository` | `evidence/05-equipment-management.png`; T03 PowerShell create/duplicate outputs |
| UC-16 Update equipment | BR-05-07, 10, 14, 16, 18-19 | API-01, API-07, API-09 | T03, T07 | `EquipmentController.Update`; `EquipmentManagementService.UpdateAsync`; `EfEquipmentRepository.UpdateDetailsAsync` | `evidence/05-equipment-management.png`; T03 PowerShell update output |

The PowerShell request/response outputs referenced above are recorded in the T01-T08 execution section of `ACTIVITY_5_VERIFICATION.md`. Tests remain Partial wherever that record says the full standardized test was not completed or evidenced.