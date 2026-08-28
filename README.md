# Campus Equipment Borrowing System

Laboratory Activity 1 for ITSD 81 - Desktop Application Development.

This repository contains the initial application structure for a campus equipment borrowing system. It does not include a database or graphical user interface yet. The goal is to show clear separation between domain rules, application use cases, and infrastructure details.

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
|
+-- README.md
+-- EquipmentBorrowing.sln
+-- src/
|   +-- EquipmentBorrowing.Domain/
|   +-- EquipmentBorrowing.Application/
|   +-- EquipmentBorrowing.Infrastructure/
|   +-- EquipmentBorrowing.Console/
+-- tests/
    +-- EquipmentBorrowing.Tests/
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
        v
Application ----> Domain
        ^
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
