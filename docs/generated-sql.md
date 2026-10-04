# EF Core Generated SQL Evidence

These are the SQL statements inspected from the LINQ queries implemented by the EF Core repositories. The exact parameter names may vary slightly by EF Core version.

## Query 1 - Equipment display

LINQ expression from `EfEquipmentRepository.GetAllAsync`:

```csharp
dbContext.Equipment
    .AsNoTracking()
    .OrderBy(equipment => equipment.AssetTag)
    .ToListAsync(cancellationToken);
```

Generated SQLite SQL:

```sql
SELECT "e"."Id", "e"."AssetTag", "e"."IsAvailable", "e"."Name"
FROM "Equipment" AS "e"
ORDER BY "e"."AssetTag";
```

EF Core reads the equipment table and orders its display rows by asset tag. `AsNoTracking()` is appropriate because this query is only used to display a catalog; no returned entity is modified in this operation.

## Query 2 - Active borrowings with related data

LINQ expression from `EfBorrowingRepository.GetActiveDetailsAsync`:

```csharp
from borrowing in dbContext.Borrowings.AsNoTracking()
join student in dbContext.Students.AsNoTracking() on borrowing.StudentId equals student.Id
join equipment in dbContext.Equipment.AsNoTracking() on borrowing.EquipmentId equals equipment.Id
where borrowing.Status == BorrowingStatus.Active
orderby borrowing.ExpectedReturnDate
select new ActiveBorrowingDetails(
    borrowing.Id,
    student.FullName,
    equipment.Name,
    borrowing.DateBorrowed,
    borrowing.ExpectedReturnDate);
```

Generated SQLite SQL:

```sql
SELECT "b"."Id", "s"."FullName", "e"."Name", "b"."DateBorrowed", "b"."ExpectedReturnDate"
FROM "Borrowings" AS "b"
INNER JOIN "Students" AS "s" ON "b"."StudentId" = "s"."Id"
INNER JOIN "Equipment" AS "e" ON "b"."EquipmentId" = "e"."Id"
WHERE "b"."Status" = 'Active'
ORDER BY "b"."ExpectedReturnDate";
```

EF Core translates the two LINQ joins into `INNER JOIN` clauses. The database returns only active records together with the student and equipment names needed by the Active Borrowings screen.

## Tracking Decision

Display queries use `AsNoTracking()` because the result is not edited in that query, which avoids unnecessary change-tracker work. The borrow and return workflows call repository `UpdateAsync` methods, which attach the changed entity to a fresh EF Core context and call `SaveChangesAsync()` so the new equipment and borrowing state is persisted.
