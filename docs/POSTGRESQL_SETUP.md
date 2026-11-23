# PostgreSQL local setup — Activity 4

This solution uses PostgreSQL through EF Core's Npgsql provider. It no longer uses SQLite as its operational database. A clean PostgreSQL database is expected; transferring old SQLite rows is not part of the Activity 4 brief.

## Prerequisites

- .NET SDK 10.x
- PostgreSQL 17+ (a local installation is sufficient) and pgAdmin or `psql`
- The `dotnet-ef` local tool from `.config/dotnet-tools.json`

## Create a local database

In pgAdmin, connect to your local PostgreSQL server, right-click **Databases**, choose **Create → Database**, and name it `equipment_borrowing`. Alternatively, in `psql` run:

```sql
CREATE DATABASE equipment_borrowing;
```

The PostgreSQL server must be running before the application starts.

## Set the connection string without committing credentials

Open PowerShell in the repository root and set the environment variable for the current terminal session. Substitute your local username/password:

```powershell
$env:EQUIPMENT_BORROWING_CONNECTION_STRING = "Host=localhost;Port=5432;Database=equipment_borrowing;Username=postgres;Password=YOUR_LOCAL_PASSWORD"
```

The project deliberately does not contain a real password. Do not commit a password, `.env` file, user secrets export, or a full connection string containing credentials.

## Restore, build and apply migrations

```powershell
dotnet tool restore
dotnet restore
dotnet build
dotnet tool run dotnet-ef database update --project src\EquipmentBorrowing.Infrastructure\EquipmentBorrowing.Infrastructure.csproj --startup-project src\EquipmentBorrowing.Desktop\EquipmentBorrowing.Desktop.csproj
```

The `EquipmentBorrowingDbContextFactory` reads `EQUIPMENT_BORROWING_CONNECTION_STRING` for design-time EF commands. Startup also calls `Database.MigrateAsync()` so pending migrations are applied when the desktop app starts. Do not manually create the tables.

## Run the application

```powershell
dotnet run --project src\EquipmentBorrowing.Desktop\EquipmentBorrowing.Desktop.csproj
```

The first startup creates the three tables and seeds a small set of students/equipment only when the corresponding table is empty. It does not delete or overwrite existing rows on subsequent starts.

## Verify migration and schema

Use pgAdmin's Query Tool or `psql`:

```sql
SELECT * FROM "__EFMigrationsHistory" ORDER BY "MigrationId";
SELECT * FROM "Students" ORDER BY "Id";
SELECT * FROM "Equipment" ORDER BY "Id";
SELECT * FROM "Borrowings" ORDER BY "Id";
```

See `postgresql-verification-queries.sql` for index, constraint, FK and active-loan checks. Capture a screenshot of all three tables and migration history for the lab submission.

## Context and dependency-injection lifetime

The desktop composition root registers `AddDbContextFactory<EquipmentBorrowingDbContext>`. Repository classes are stateless singleton services that request a fresh context from the factory for each operation and dispose it after the asynchronous query/write. A DbContext is not thread-safe and is not retained by a ViewModel or shared across UI commands. This supports concurrent async loading of lists and histories.

## Connection and migration troubleshooting

- **Connection refused:** check PostgreSQL service status, host and port.
- **Database does not exist:** create `equipment_borrowing` before running the migration.
- **Password authentication failed:** re-enter your local PostgreSQL password in the current PowerShell session.
- **Migration not found:** check that the Infrastructure project is selected as `--project`, Desktop as `--startup-project`, and the current code contains `202611200001_InitialPostgresSchema`.
- **Startup connection error:** the desktop app presents a startup error window. Correct the connection variable, then restart.

The variable is session-scoped; set it again in a new PowerShell terminal. This setup does not require Docker, a cloud database or a paid service.

## Controlled rollback demonstration (disposable database only)

For Appendix C scenario A06, you can deliberately fail a multi-item transaction after the first borrowing and availability change have been sent to PostgreSQL but before the transaction commits. Use a disposable test database only:

```powershell
$env:EQUIPMENT_BORROWING_TEST_FAIL_AFTER_FIRST_INSERT = "true"
dotnet run --project src\EquipmentBorrowing.Desktop\EquipmentBorrowing.Desktop.csproj
```

Select an eligible student and at least two available items, then submit the multi-item checkout. The repository writes the first row inside the still-open serializable transaction, triggers the test seam, and explicitly rolls the transaction back. The UI reports that this is a test-only failure. Query `Borrowings` and `Equipment` using a fresh pgAdmin connection and confirm there are no new active rows and all selected items remain available. Afterwards disable the seam in the same PowerShell session:

```powershell
Remove-Item Env:EQUIPMENT_BORROWING_TEST_FAIL_AFTER_FIRST_INSERT
```

Never enable this flag against shared or production data. It is deliberately limited to multi-item checkout and is disabled by default.
