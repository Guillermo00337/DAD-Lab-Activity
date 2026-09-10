# Campus Equipment Borrowing System

Laboratory Activity 1 for ITSD 81 - Desktop Application Development.

This repository contains the application structure for a campus equipment borrowing system. Laboratory Activity 1 established the domain, application, infrastructure, and test layers. Laboratory Activity 2 extends the same solution with an Avalonia desktop interface using MVVM.

## Part A - System Analysis

### Actors

| Actor | Expectation |
|---|---|
| Authorized Student | Requests to borrow available laboratory equipment and expects the system to approve or reject the request based on borrowing rules. |
| Laboratory Staff | Maintains equipment availability and expects borrowing records to show who borrowed equipment and when it should be returned. |

### Use Cases

| Item | Description |
|---|---|
| Use Case | Borrow Equipment |
| Primary Actor | Authorized Student |
| Preconditions | Student and equipment records are already available in the system. |
| Main Action | Student requests to borrow a specific piece of equipment. |
| Expected Result | The system creates an active borrowing record and marks the equipment unavailable. |
| Possible Failure | Student does not exist, is not allowed to borrow, equipment does not exist, equipment is unavailable, or student reached the active borrowing limit. |

| Item | Description |
|---|---|
| Use Case | Return Equipment |
| Primary Actor | Laboratory Staff |
| Preconditions | There is an active borrowing record for the equipment. |
| Main Action | Staff records the returned equipment. |
| Expected Result | Borrowing is marked returned and equipment becomes available again. |
| Possible Failure | Borrowing record does not exist or has already been returned. |

| Item | Description |
|---|---|
| Use Case | Find Available Equipment |
| Primary Actor | Authorized Student |
| Preconditions | Equipment records exist in the system. |
| Main Action | Student or staff checks which equipment can be borrowed. |
| Expected Result | The system lists equipment currently marked as available. |
| Possible Failure | No equipment is available or the equipment repository cannot provide data. |

### Domain Concepts

| Concept | Information It Contains | Rules or State | Not Its Responsibility |
|---|---|---|---|
| Student | Id, student number, full name, borrowing permission, maximum active borrowings | Knows whether the student is allowed to start another borrowing based on active count | Loading itself from a database or displaying UI messages |
| Equipment | Id, asset tag, name, availability | Can be marked borrowed or available | Deciding which student may borrow it |
| Borrowing | Id, student id, equipment id, borrowed date, expected return date, returned date, status | Starts as active and can be marked returned | Querying repositories or changing equipment storage directly |

## 1. Solution Structure

```text
EquipmentBorrowing/
│
├── README.md
├── EquipmentBorrowing.sln
│
├── src/
│   ├── EquipmentBorrowing.Domain/
│   │   ├── Student.cs
│   │   ├── Equipment.cs
│   │   ├── Borrowing.cs
│   │   └── BorrowingStatus.cs
│   │
│   ├── EquipmentBorrowing.Application/
│   │   ├── Interfaces/
│   │   └── Services/
│   │
│   └── EquipmentBorrowing.Infrastructure/
│       └── Repositories/
│
└── tests/
    └── EquipmentBorrowing.Tests/
```

### Domain

Contains the main problem concepts and rules:

- `Student`
- `Equipment`
- `Borrowing`
- `BorrowingStatus`

### Application

Contains use case coordination and repository abstractions:

- `BorrowEquipmentService`
- `BorrowEquipmentRequest`
- `BorrowEquipmentResult`
- `IStudentRepository`
- `IEquipmentRepository`
- `IBorrowingRepository`

### Infrastructure

Contains technical implementations of application interfaces:

- `InMemoryStudentRepository`
- `InMemoryEquipmentRepository`
- `InMemoryBorrowingRepository`

These can later be replaced by database-backed repositories without changing the application service.

### Tests

Contains the initial test project structure. More tests can be added later for domain behavior and application services.

## 2. Dependency Direction

```text
Console Demo / Future UI
        |
        ↓
Application ----> Domain
        ↑
        |
Infrastructure
```

Project references:

- `EquipmentBorrowing.Domain` depends on no other project.
- `EquipmentBorrowing.Application` depends on `Domain`.
- `EquipmentBorrowing.Infrastructure` depends on `Application` and `Domain`.
- `EquipmentBorrowing.Console` depends on `Application`, `Domain`, and `Infrastructure`.
- `EquipmentBorrowing.Tests` depends on `Application` and `Domain`.

The application layer depends on repository interfaces, not repository implementations. Infrastructure implements those interfaces.

## 3. Use Case Mapping

```text
Actor:
Authorized Student

Use Case:
Borrow Equipment

Application Service:
BorrowEquipmentService

Domain Objects Used:
Student, Equipment, Borrowing, BorrowingStatus

Repository Interfaces Used:
IStudentRepository, IEquipmentRepository, IBorrowingRepository

Infrastructure Implementations Used:
InMemoryStudentRepository, InMemoryEquipmentRepository, InMemoryBorrowingRepository
```

## 4. Reflection

1. The application service should depend on repository interfaces because the use case should not care whether data comes from memory, a file, SQLite, or another database. This keeps business logic separate from storage details.

2. If SQLite were added later, the `Domain` project, repository interfaces, and `BorrowEquipmentService` could remain mostly unchanged. The main change would be adding SQLite repository implementations in `Infrastructure`.

3. Avalonia Views would eventually belong in a separate UI project, such as `EquipmentBorrowing.Desktop`, not in `Domain`, `Application`, or `Infrastructure`.

4. An Avalonia button should not directly execute database queries. It should call an application service, and the application service should coordinate the use case through interfaces.

5. `BorrowEquipmentService.BorrowAsync` represents the actual business operation requested by the actor. It validates the student, equipment, availability, active borrowing limit, and then creates the borrowing record.

## Running the Demonstration

From the repository root:

```text
dotnet run --project src/EquipmentBorrowing.Console/EquipmentBorrowing.Console.csproj --no-restore
```

The console program demonstrates one successful borrowing request and one failed request where a student is not allowed to borrow equipment.

## Laboratory Activity 2 - Avalonia UI and MVVM

### Desktop Project

`EquipmentBorrowing.Desktop` is the presentation layer of the system. It is responsible for:

- displaying equipment and active borrowing information;
- collecting user input;
- maintaining presentation state in ViewModels;
- invoking application services through commands; and
- showing success, validation, and failure messages to the user.

The Desktop project references `EquipmentBorrowing.Application`, `EquipmentBorrowing.Domain`, and `EquipmentBorrowing.Infrastructure` so it can compose the working application. The existing Domain and Application projects do not reference Avalonia.

### Updated Architecture

```text
Avalonia View
      |
      | Binding / Command
      v
ViewModel
      |
      | Application Operation
      v
Application Service
      |
      +----------> Domain
      |
      v
Repository Interface
      ^
      |
Infrastructure Implementation
```

The View contains XAML layout and bindings. The ViewModel contains selected values, observable collections, commands, and user-facing messages. Application services contain the use case workflow and business validation. Infrastructure repositories provide in-memory storage behind repository interfaces.

### Borrow Equipment Flow

1. The user opens the Equipment section.
2. `EquipmentView` displays students and equipment using XAML data binding.
3. The user selects a student, selects equipment, chooses an expected return date, and presses `Borrow Equipment`.
4. `EquipmentViewModel` performs presentation validation, such as checking that a student and equipment item were selected.
5. `EquipmentViewModel` calls `BorrowEquipmentService.BorrowAsync`.
6. `BorrowEquipmentService` checks the business rules: student exists, student is allowed, equipment exists, equipment is available, and the student has not reached the active borrowing limit.
7. If the request is valid, the service creates a borrowing record and marks the equipment unavailable.
8. The ViewModel refreshes the equipment list and displays the result message.

### Return Equipment Flow

1. The user opens the Active Borrowings section.
2. `BorrowingsView` displays active borrowing records using XAML data binding.
3. The user selects an active borrowing and presses `Return Equipment`.
4. `BorrowingsViewModel` checks that a borrowing was selected.
5. `BorrowingsViewModel` calls `ReturnEquipmentService.ReturnAsync`.
6. `ReturnEquipmentService` locates the borrowing, verifies that it has not already been returned, marks it returned, and marks the equipment available again.
7. The ViewModel refreshes the active borrowing list and displays the result message.

### Activity 2 Components

```text
src/
├── EquipmentBorrowing.Application/
│   ├── Interfaces/
│   └── Services/
│       ├── BorrowEquipmentService.cs
│       ├── ReturnEquipmentService.cs
│       ├── EquipmentCatalogService.cs
│       ├── StudentCatalogService.cs
│       └── ActiveBorrowingsService.cs
│
├── EquipmentBorrowing.Infrastructure/
│   └── Repositories/
│
└── EquipmentBorrowing.Desktop/
    ├── Views/
    │   ├── EquipmentView.axaml
    │   └── BorrowingsView.axaml
    ├── ViewModels/
    │   ├── MainWindowViewModel.cs
    │   ├── EquipmentViewModel.cs
    │   └── BorrowingsViewModel.cs
    ├── App.axaml
    ├── App.axaml.cs
    └── MainWindow.axaml
```

### Architectural Reflection

1. The View should not call a repository directly because the View's job is presentation, not data access or business workflow coordination.

2. Business rules should not be implemented in the ViewModel because those rules belong to the existing Domain and Application layers. Keeping them there allows the same rules to be reused by a console app, desktop app, test project, or future web interface.

3. The ViewModel is responsible for presentation state, selected values, observable collections, commands, simple input validation, and user-facing feedback.

4. The Application layer can work without knowing Avalonia is being used because it exposes ordinary C# services and interfaces. Avalonia only appears in the Desktop project.

5. Registering dependencies in one composition point keeps object creation organized. ViewModels receive services through constructors instead of creating repositories or services themselves.

6. If the in-memory repositories were replaced by SQLite later, the Views, ViewModels, Domain models, repository interfaces, and application services should remain largely unchanged. The main change would be new Infrastructure repository implementations.

### Running the Desktop Application

From the repository root:

```text
dotnet restore
dotnet build
dotnet run --project src/EquipmentBorrowing.Desktop/EquipmentBorrowing.Desktop.csproj
```

Use the Equipment section to perform a borrowing transaction. Use the Active Borrowings section to return borrowed equipment.
