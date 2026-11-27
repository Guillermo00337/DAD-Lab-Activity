# Activity 5 Local Setup

Requirements: .NET 10 SDK and PostgreSQL. Run commands from the repository root.

## Lab 5 database

Create `equipment_borrowing_lab5_test` in PostgreSQL. In PowerShell, set the API connection string using your local credentials; keep real credentials out of the repository:

`$env:EQUIPMENT_BORROWING_CONNECTION_STRING = "Host=localhost;Port=5432;Database=equipment_borrowing_lab5_test;Username=YOUR_USERNAME;Password=YOUR_PASSWORD"`

Apply migrations with `dotnet ef database update --project .\src\EquipmentBorrowing.Infrastructure\EquipmentBorrowing.Infrastructure.csproj --startup-project .\src\EquipmentBorrowing.Api\EquipmentBorrowing.Api.csproj --context EquipmentBorrowingDbContext`.

To restore seed data, open `docs\activity5-seed-reset.sql` in pgAdmin Query Tool connected to `equipment_borrowing_lab5_test` and press F5. The script checks the database name before changing data.

## Run the API

Set `$env:ASPNETCORE_ENVIRONMENT = "Development"` and `$env:ASPNETCORE_URLS = "http://localhost:5080"`, then run `dotnet run --project .\src\EquipmentBorrowing.Api\EquipmentBorrowing.Api.csproj --no-launch-profile`.

OpenAPI: `http://localhost:5080/openapi/v1.json`.

## Run the desktop app

In a second PowerShell window, run `dotnet run --project .\src\EquipmentBorrowing.Desktop\EquipmentBorrowing.Desktop.csproj`. The desktop app connects to the API at `http://localhost:5080`.

## Build and test

The PostgreSQL integration tests use the separate database `equipment_borrowing_test`. Set `$env:EQUIPMENT_BORROWING_TEST_CONNECTION_STRING` to a connection string for that database, using your local credentials, then run `dotnet build` and `dotnet test`. The test fixture checks the database name and applies migrations; it does not delete or recreate the database.

PowerShell environment variables are lost when the window closes, so set them again in a new session. Never commit real database credentials.
