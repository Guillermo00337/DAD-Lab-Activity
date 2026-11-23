# Activity 4 — Local Verification Checklist

Use this after configuring PostgreSQL. Record the date, database name and observed outcome for each row. Keep screenshots or SQL readback beside the notes. Do not claim a test passed until it has been performed locally.

## Environment and schema

- [ ] .NET SDK 10.x installed (`dotnet --info`).
- [ ] PostgreSQL service running.
- [ ] `equipment_borrowing` database created.
- [ ] `EQUIPMENT_BORROWING_CONNECTION_STRING` set in the current PowerShell session.
- [ ] `dotnet tool restore` completed.
- [ ] `dotnet restore` completed.
- [ ] `dotnet build` completed with no errors.
- [ ] EF migration applied to a clean database using the documented command.
- [ ] pgAdmin shows `Students`, `Equipment`, `Borrowings`, and `__EFMigrationsHistory`.
- [ ] Primary keys, foreign keys, check constraints and unique indexes inspected.

## Application workflows

- [ ] UC-01 catalog shows identifier, name, description and availability.
- [ ] UC-02 available list excludes unavailable items.
- [ ] UC-03 valid single borrow; invalid cases rejected without data change.
- [ ] UC-04 active borrowing list shows related student/equipment.
- [ ] UC-05 return updates borrowing and equipment together; repeat return rejected.
- [ ] UC-06 two distinct items issue in one transaction; fewer-than-two/duplicates/unavailable/over-limit/invalid-date requests rejected.
- [ ] UC-06 controlled failure after a write attempt leaves no partial rows or availability changes (fresh readback).
- [ ] UC-07 student history shows that student's active and returned records only.
- [ ] UC-08 equipment history shows that item's active and returned records only.
- [ ] UC-09 name/number/equipment partial search, status filter, both sort fields/directions, combined criteria and clear work.
- [ ] UC-12 student registration defaults eligible and survives restart.
- [ ] UC-13 student edits preserve student ID and borrowing history.
- [ ] UC-14 restriction blocks new borrow but permits return of existing active borrowings.
- [ ] UC-15 equipment registration defaults available and survives restart.
- [ ] UC-16 descriptive edits preserve equipment ID, availability and history.

## Persistence, feedback and documentation

- [ ] Student, equipment and borrowing changes survive closing/restarting the app.
- [ ] Empty results are distinguishable from missing records and connection failures.
- [ ] Loading state remains visible during async history/search retrieval.
- [ ] A safe connection failure is reported without exposing credentials; recovery works after reconnect.
- [ ] Commit-time unknown outcome is not automatically retried or reported as a definite rollback.
- [ ] Two actual EF-generated SELECT statements captured from `%LOCALAPPDATA%\EquipmentBorrowing\efcore-postgresql-sql.log`.
- [ ] `ACTIVITY_4_TRACEABILITY.md` populated with observed results and evidence references.
- [ ] Git commits made in the original repository, preserving prior history.

## Evidence to retain

1. Successful `dotnet build` output.
2. Migration command output.
3. pgAdmin screenshots of Students, Equipment, Borrowings and migration history.
4. Successful checkout and its fresh PostgreSQL readback.
5. Restart readback of a persisted borrowing.
6. Return and restart readback.
7. Multi-item checkout and controlled rollback before/after readback.
8. Student and equipment registration/update/history evidence.
9. Search/filter/sort demonstration, including empty result and retry/error state.
10. Two actual generated SQL queries plus short explanations.
