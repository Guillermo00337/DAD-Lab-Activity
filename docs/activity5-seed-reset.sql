BEGIN;
SET LOCAL TIME ZONE 'Asia/Manila';

DO $guard$
BEGIN
    IF current_database() <> 'equipment_borrowing_lab5_test' THEN
        RAISE EXCEPTION 'Safety stop: this seed script is only for equipment_borrowing_lab5_test.';
    END IF;
END
$guard$;

DELETE FROM "Borrowings";
DELETE FROM "Equipment";
DELETE FROM "Students";

INSERT INTO "Students"
    ("StudentNumber", "FullName", "ContactInformation", "IsAllowedToBorrow", "MaxActiveBorrowings")
VALUES
    ('ST-100', 'Ada Cruz',  NULL, TRUE,  3),
    ('ST-200', 'Ben Diaz',  NULL, FALSE, 3),
    ('ST-300', 'Cara Lim',  NULL, TRUE,  3),
    ('ST-400', 'Dani Ong',  NULL, TRUE,  3);

INSERT INTO "Equipment"
    ("AssetTag", "Name", "Description", "IsAvailable")
VALUES
    ('EQ-001', 'Camera',     'Digital camera',           FALSE),
    ('EQ-002', 'Laptop',     'Laboratory laptop',        TRUE),
    ('EQ-003', 'Projector',  'Classroom projector',      TRUE),
    ('EQ-004', 'Tablet',     'Drawing tablet',            FALSE),
    ('EQ-005', 'Microphone', 'Wireless microphone',       FALSE),
    ('EQ-006', 'Tripod',     'Camera tripod',             TRUE),
    ('EQ-007', 'Speaker',    'Portable speaker',          TRUE);

INSERT INTO "Borrowings"
    ("StudentId", "EquipmentId", "DateBorrowed", "ExpectedReturnDate", "DateReturned", "Status")
SELECT s."Id", e."Id", CURRENT_DATE - 9, CURRENT_DATE - 1, NULL, 0
FROM "Students" s
JOIN "Equipment" e ON e."AssetTag" = 'EQ-001'
WHERE s."StudentNumber" = 'ST-100';

INSERT INTO "Borrowings"
    ("StudentId", "EquipmentId", "DateBorrowed", "ExpectedReturnDate", "DateReturned", "Status")
SELECT s."Id", e."Id", CURRENT_DATE - 8, CURRENT_DATE + 2, CURRENT_DATE - 2, 1
FROM "Students" s
JOIN "Equipment" e ON e."AssetTag" = 'EQ-003'
WHERE s."StudentNumber" = 'ST-100';

INSERT INTO "Borrowings"
    ("StudentId", "EquipmentId", "DateBorrowed", "ExpectedReturnDate", "DateReturned", "Status")
SELECT s."Id", e."Id", CURRENT_DATE - 7, CURRENT_DATE + 2, NULL, 0
FROM "Students" s
JOIN "Equipment" e ON e."AssetTag" IN ('EQ-004', 'EQ-005')
WHERE s."StudentNumber" = 'ST-300';

COMMIT;
