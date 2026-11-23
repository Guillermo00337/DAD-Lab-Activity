-- Run in pgAdmin Query Tool or psql after the Activity 4 migration.

-- 1. Tables and migrations
SELECT table_name
FROM information_schema.tables
WHERE table_schema = 'public'
ORDER BY table_name;

SELECT "MigrationId", "ProductVersion"
FROM "__EFMigrationsHistory"
ORDER BY "MigrationId";

-- 2. Students and equipment
SELECT "Id", "StudentNumber", "FullName", "ContactInformation", "IsAllowedToBorrow", "MaxActiveBorrowings"
FROM "Students" ORDER BY "Id";

SELECT "Id", "AssetTag", "Name", "Description", "IsAvailable"
FROM "Equipment" ORDER BY "Id";

-- 3. Complete borrowing history with linked names
SELECT b."Id", s."StudentNumber", s."FullName", e."AssetTag", e."Name" AS "EquipmentName",
       b."DateBorrowed", b."ExpectedReturnDate", b."DateReturned", b."Status"
FROM "Borrowings" b
JOIN "Students" s ON s."Id" = b."StudentId"
JOIN "Equipment" e ON e."Id" = b."EquipmentId"
ORDER BY b."DateBorrowed" DESC, b."Id";

-- 4. Detect any item with more than one active borrowing (should return no rows)
SELECT "EquipmentId", COUNT(*) AS active_count
FROM "Borrowings"
WHERE "Status" = 0
GROUP BY "EquipmentId"
HAVING COUNT(*) > 1;

-- 5. Detect inconsistent equipment availability vs active borrowings (should return no rows)
SELECT e."Id", e."AssetTag", e."IsAvailable", COUNT(b."Id") AS active_count
FROM "Equipment" e
LEFT JOIN "Borrowings" b ON b."EquipmentId" = e."Id" AND b."Status" = 0
GROUP BY e."Id", e."AssetTag", e."IsAvailable"
HAVING (e."IsAvailable" = FALSE AND COUNT(b."Id") <> 1)
    OR (e."IsAvailable" = TRUE AND COUNT(b."Id") <> 0);

-- 6. Check the active-loan limit (should return no rows)
SELECT "StudentId", COUNT(*) AS active_count
FROM "Borrowings"
WHERE "Status" = 0
GROUP BY "StudentId"
HAVING COUNT(*) > 3;

-- 7. Inspect active-item unique index and normalized identifier indexes
SELECT indexname, indexdef
FROM pg_indexes
WHERE schemaname = 'public'
  AND indexname IN (
      'UX_Students_StudentNumber_Normalized',
      'UX_Equipment_AssetTag_Normalized',
      'UX_Borrowings_OneActivePerEquipment'
  )
ORDER BY indexname;

-- 8. Inspect foreign keys and check constraints
SELECT conrelid::regclass AS table_name, conname, pg_get_constraintdef(oid) AS definition
FROM pg_constraint
WHERE contype IN ('f', 'c')
  AND connamespace = 'public'::regnamespace
ORDER BY table_name::text, conname;
