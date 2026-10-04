-- Campus Equipment Borrowing System: Activity 3 SQL evidence.

-- 1. Basic retrieval: all equipment records.
SELECT "Id", "AssetTag", "Name", "IsAvailable"
FROM "Equipment"
ORDER BY "AssetTag";

-- 2. Filtering: only equipment currently available to borrow.
SELECT "Id", "AssetTag", "Name"
FROM "Equipment"
WHERE "IsAvailable" = 1
ORDER BY "AssetTag";

-- 3. Join: active borrowings together with their student and equipment.
SELECT
    s."FullName" AS "Student",
    e."Name" AS "Equipment",
    b."DateBorrowed" AS "Borrowed",
    b."ExpectedReturnDate" AS "Due"
FROM "Borrowings" AS b
INNER JOIN "Students" AS s ON s."Id" = b."StudentId"
INNER JOIN "Equipment" AS e ON e."Id" = b."EquipmentId"
WHERE b."Status" = 'Active'
ORDER BY b."ExpectedReturnDate";

-- 4. Aggregate: active borrowing count per student.
SELECT
    s."StudentNumber",
    s."FullName",
    COUNT(b."Id") AS "ActiveBorrowingCount"
FROM "Students" AS s
LEFT JOIN "Borrowings" AS b
    ON b."StudentId" = s."Id"
    AND b."Status" = 'Active'
GROUP BY s."Id", s."StudentNumber", s."FullName"
ORDER BY "ActiveBorrowingCount" DESC, s."FullName";

-- 5. Update example: mark a returned item available again.
UPDATE "Equipment"
SET "IsAvailable" = 1
WHERE "Id" = 1;
