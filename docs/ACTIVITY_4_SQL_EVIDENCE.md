# EF Core SQL evidence - Activity 4

The desktop application's EF Core command logging is configured to write SQL command output to:

```text
%LOCALAPPDATA%\EquipmentBorrowing\efcore-postgresql-sql.log
```

The application does not enable sensitive-data logging, so the log is not intended to expose credentials or parameter values. This file is generated on the student's machine after running the application; it is not included in the source archive.

## Capture two actual SELECT statements

1. Set `EQUIPMENT_BORROWING_CONNECTION_STRING` as described in `POSTGRESQL_SETUP.md`.
2. Start the desktop application and open **Borrowings / Search**. The async borrowing query should appear in the log.
3. Open **History**, select a student and load the student's history. The related student-history query should appear.
4. Open the log in Notepad or VS Code. The two statements below were copied from the current log. Do not include connection strings or personal data in evidence.
5. Capture the log showing these queries and save it as `docs/evidence/08-efcore-sql-log.png`.

## Query 1 - Active borrowing search (captured from the EF Core log)

```sql
SELECT b."Id", s."Id", s."StudentNumber", s."FullName", e."Id", e."AssetTag", e."Name", b."DateBorrowed", b."ExpectedReturnDate", b."DateReturned", b."Status"
FROM "Borrowings" AS b
INNER JOIN "Students" AS s ON b."StudentId" = s."Id"
INNER JOIN "Equipment" AS e ON b."EquipmentId" = e."Id"
WHERE b."Status" = 0
ORDER BY b."DateBorrowed" DESC, b."Id"
```

Explanation: This captured run used the Active status filter and an empty text search. The query joins borrowing rows to student and equipment data, returns the checkout/due/return dates and status, and orders newest checkout first with borrowing ID as a stable tie-breaker.

## Query 2 - Student history (captured from the EF Core log)

```sql
SELECT b."Id", s."Id", s."StudentNumber", s."FullName", e."Id", e."AssetTag", e."Name", b."DateBorrowed", b."ExpectedReturnDate", b."DateReturned", b."Status"
FROM "Borrowings" AS b
INNER JOIN "Students" AS s ON b."StudentId" = s."Id"
INNER JOIN "Equipment" AS e ON b."EquipmentId" = e."Id"
WHERE s."Id" = @studentId
ORDER BY b."DateBorrowed" DESC, b."Id"
```

Explanation: This query was logged when student history was loaded. It uses the `@studentId` parameter (the log masks parameter values), returns that student's active and returned borrowing records with equipment details, and orders by checkout date then borrowing ID.

These statements were copied from `%LOCALAPPDATA%\EquipmentBorrowing\efcore-postgresql-sql.log`. The log contains no literal student ID for the parameterized history query.

## Actual PostgreSQL Verification Results

The following observations are supported by the pgAdmin and application screenshots currently in `docs/evidence`. They describe the observed test state; they are not a claim that every Activity 4 case has passed.

| Verification | Observed result | Evidence |
|---|---|---|
| Migration and schema | pgAdmin showed the PostgreSQL migration in `__EFMigrationsHistory` and the `Students`, `Equipment`, `Borrowings`, and `__EFMigrationsHistory` tables in `public`. | `17-schema-migration.png` |
| Single checkout | Borrowing `Id=1` was for Ana Reyes and the Digital Camera. The database readback showed checkout `2026-11-23`, expected return `2026-12-30`, `DateReturned = NULL`, and active status (`0`). | `03-single-borrow-readback.png`; app view `03-history-dates-aligned.png` |
| Restart readback | After restarting the application, the active list still showed the Ana Reyes / Digital Camera and Lia Cruz / Drawing Tablet records with the same checkout and due dates. | `07-restart-readback.png` |
| Return | The app's Returned filter showed Ana Reyes / Digital Camera with checkout and return dates `23/11/2026` and status `Returned`; the Active filter subsequently showed Lia Cruz / Drawing Tablet only. The catalog later reported 3 of 4 equipment items available. | `05-returned-borrowing.png`, `05-camera-available-after-return.png` |
| Same-day date rejection | The app rejected Ana Reyes's attempted MIC-001 checkout with expected return date `2026-11-24`. A pgAdmin query showed MIC-001 `IsAvailable = true` and `SameDayBorrowingCount = 0`. | `03-reject-same-day-date.png`, `03-reject-same-day-readback.png` |
| Additional active record | Borrowing `Id=5` was read back with `StudentId=3`, `EquipmentId=3`, checkout `2026-11-23`, expected return `2026-12-30`, `DateReturned = NULL`, and active status (`0`). The app identified it as Lia Cruz / Drawing Tablet. | App readback `03-active-borrowings.png`; pgAdmin readback was shown during the session |
| Multi-item checkout | The app reported “2 items checked out successfully”; the Active list showed Ana Reyes borrowing `Id=6` (Laptop) and `Id=7` (Digital Camera), checkout `2026-11-23`, expected return `2026-11-30`. Fresh pgAdmin readback confirmed both rows active (`Status=0`), `DateReturned = NULL`, and both equipment items unavailable (`IsAvailable = false`). | `06-multi-item-success.png`, `06-multi-item-active-list.png`, `06-multi-item-postgres-readback.png` |
| Controlled rollback | A deliberate failure was shown and subsequent pgAdmin reads were captured. A before-test baseline was not captured, so these screenshots alone do not establish that the database state was unchanged by the attempt. | `06-rollback-app.png`, `06-rollback-borrowings-readback.png`, `06-rollback-equipment-readback.png`, `06-rollback-equipment-before-after.png` |
| Constraints and indexes | The filtered constraint query returned 15 primary-key, foreign-key, unique, and check-constraint rows. The index query returned 10 indexes, including the one-active-borrowing-per-equipment and normalized-asset-tag unique indexes. | `17-schema-migration.png`, `17-schema-indexes.png`; capture the constraint result as `17-schema-constraints.png` |

The two generated-SQL statements above are copied from the local EF Core log. The available evidence folder also contains `08-efcore-active-query.png` and `08-efcore-student-history-query.png`. Direct invalid-write checks, a before-test rollback baseline, and other cases in the traceability matrix remain pending.

