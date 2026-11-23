# Activity 4 preparation and verification status

This source package is a prepared Activity 4 implementation based on the uploaded Activity 3 project and the two supplied Activity 4 specification PDFs.

## What has been prepared

- The operational EF Core provider is changed from SQLite to PostgreSQL/Npgsql.
- A PostgreSQL initial schema migration and model snapshot are included.
- Database constraints include foreign keys, required-field/date checks, normalized case-insensitive identifier indexes and a partial unique index for one active borrowing per equipment item.
- Repository and application-service workflows cover the fourteen required use cases UC-01-09 and UC-12-16.
- Multi-item checkout and return are implemented within database transactions.
- The Avalonia UI has borrowing, search, student, equipment and history sections.
- Setup, use-case, traceability and verification documents are included.

## What has not been verified in this preparation environment

The .NET SDK and a PostgreSQL server were not available in the preparation environment. Therefore this archive has **not** been locally compiled, run against PostgreSQL, or exercised through the acceptance scenarios. The migration and model snapshot were authored for this implementation and must be validated with the local EF Core tools against your installed PostgreSQL version. Do not treat the presence of source files or a successful build alone as proof of database behavior.

## Required local verification

1. Install .NET SDK 10 and PostgreSQL.
2. Follow `POSTGRESQL_SETUP.md` to create a clean local database and set `EQUIPMENT_BORROWING_CONNECTION_STRING` in PowerShell.
3. Run `dotnet tool restore`, `dotnet restore`, and `dotnet build`.
4. Apply the migration with the documented `dotnet-ef database update` command.
5. Run the Avalonia application and complete `ACTIVITY_4_VERIFICATION_CHECKLIST.md`.
6. Populate the observed-result column in `ACTIVITY_4_TRACEABILITY.md` only with results you actually observed. Capture actual generated SQL from the log as described in `ACTIVITY_4_SQL_EVIDENCE.md`.

Use a disposable database for the controlled rollback scenario. Never enable the test failure flag against shared or production data.

## Git history

The uploaded Activity 3 ZIP did not contain a `.git` directory. This archive therefore cannot preserve the earlier commit history. Apply the prepared source changes to your original local Git working copy, retaining its existing `.git` folder, and follow `GIT_HISTORY_GUIDE.md` for a meaningful history export.
