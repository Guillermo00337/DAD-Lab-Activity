# Activity 4 - Required Use-Case Catalog

**System:** Campus Equipment Borrowing System  
**Scope:** The fourteen mandatory Activity 4 cases. The primary actor for every case is **Laboratory Staff / Equipment Custodian**.  
**Stakeholders:** The custodian needs accurate, understandable workflows; students need correct records and preserved borrowing history.  
**Includes / Extends:** None required. Validation is an internal system behavior, not a separate actor-level use case.  
**Open Issues:** None specified in the supplied suite.

The complete original supplied use-case suite is preserved in `ACTIVITY_4_USE_CASE_SUITE_SOURCE.pdf`. This catalog summarizes the actor goal, trigger, preconditions, guarantee, flows, governing rules and acceptance outcome for the required cases. All cases use PostgreSQL.

## UC-01 - View Equipment Catalog

| Field | Specification |
|---|---|
| Scope / Level | Equipment catalog / User Goal |
| Trigger | Staff opens the equipment catalog. |
| Preconditions | Application is running. |
| Success guarantee | Current equipment identifiers, names, descriptions and availability are displayed; no records change. |
| Failure guarantee | Rejected/known failed operations preserve data; an unknown commit outcome follows BR-18. |
| Main success scenario | 1. Staff opens the catalog. 2. System retrieves equipment from PostgreSQL. 3. System displays each item and availability. |
| Alternative flows | 2a. No equipment: display an empty state. |
| Exception flows | 2e. Database unavailable: display an error and retry option. |
| Business rules | BR-05-07, BR-10, BR-18-19 |
| Special requirements | PostgreSQL persistence, understandable feedback and layer separation. |
| Acceptance | Catalog matches database records and availability; empty and database-error states differ. A01. |

## UC-02 - Find Available Equipment

| Field | Specification |
|---|---|
| Scope / Level | Equipment catalog / User Goal |
| Trigger | Staff requests available equipment. |
| Preconditions | Application is running. |
| Success guarantee | Only currently available items are shown; no records change. |
| Failure guarantee | Rejected/known failed operations preserve data; an unknown commit outcome follows BR-18. |
| Main success scenario | 1. Staff chooses the available-items view/filter. 2. System retrieves current equipment state. 3. System displays only available items. |
| Alternative flows | 3a. No items available: display an empty result. |
| Exception flows | 2e. Database failure: display an error and permit retry. |
| Business rules | BR-10, BR-18-19 |
| Special requirements | PostgreSQL-backed results, meaningful empty/error feedback. |
| Acceptance | Available items appear, unavailable items do not, and no-match/error states differ. A02. |

## UC-03 - Borrow Equipment

| Field | Specification |
|---|---|
| Scope / Level | Borrowing / User Goal |
| Trigger | Staff submits a single-item borrowing. |
| Preconditions | Application is running; staff can select a student and item. |
| Success guarantee | One active borrowing and corresponding unavailable state are persisted together. |
| Failure guarantee | Rejected/known failed operations preserve data; an unknown commit outcome follows BR-18. |
| Main success scenario | 1. Staff selects a student, item and expected return date. 2. System revalidates student eligibility, active limit, date and item availability. 3. System saves the active borrowing and availability change in one transaction. 4. System confirms and refreshes. |
| Alternative flows | 2a. Missing, ineligible, over-limit, unavailable or invalid-date request: reject with a reason and no change. |
| Exception flows | 2e. Stale/conflicting state: reject and refresh. 3e. Database failure: report failure or unknown outcome under BR-18. |
| Business rules | BR-08-11, BR-14-16, BR-18-19 |
| Special requirements | PostgreSQL persistence and transactionally coordinated writes. |
| Acceptance | Valid single issue persists; invalid requests leave data unchanged; outcome survives restart. A03. |

## UC-04 - View Active Borrowings

| Field | Specification |
|---|---|
| Scope / Level | Borrowing / User Goal |
| Trigger | Staff opens active borrowings. |
| Preconditions | Application is running. |
| Success guarantee | Only active records with related students and equipment are displayed; no data changes. |
| Failure guarantee | Rejected/known failed operations preserve data; an unknown commit outcome follows BR-18. |
| Main success scenario | 1. Staff opens active borrowings. 2. System retrieves active records and related student/equipment. 3. System displays identities and relevant dates. |
| Alternative flows | 3a. No active records: display an empty state. |
| Exception flows | 2e. Database failure: display an error and allow retry. |
| Business rules | BR-14, BR-18-19 |
| Special requirements | Asynchronous PostgreSQL-backed retrieval. |
| Acceptance | Active records and related identities are correct; returned records are excluded; empty/error states differ. A04. |

## UC-05 - Return Equipment

| Field | Specification |
|---|---|
| Scope / Level | Borrowing / User Goal |
| Trigger | Staff submits return of a selected active borrowing. |
| Preconditions | An active borrowing can be selected. |
| Success guarantee | The same borrowing is returned with a return date; equipment becomes available and history is retained. |
| Failure guarantee | Rejected/known failed operations preserve data; an unknown commit outcome follows BR-18. |
| Main success scenario | 1. Staff selects an active borrowing and confirms return. 2. System verifies current status. 3. System records return and equipment availability together. 4. System confirms and refreshes. |
| Alternative flows | 2a. Already returned: reject repeat return without changes. |
| Exception flows | 2e. Missing/stale record: explain and refresh. 3e. Database failure: report failure or unknown outcome. |
| Business rules | BR-10, BR-13-16, BR-18-19 |
| Special requirements | Transactional return and preserved history. |
| Acceptance | Return persists across restart, equipment is available, history remains, repeat return is rejected. A05. |

## UC-06 - Check Out Multiple Equipment Items

| Field | Specification |
|---|---|
| Scope / Level | Borrowing / User Goal |
| Trigger | Staff submits a multi-item checkout. |
| Preconditions | Application is running; student and equipment can be selected. |
| Success guarantee | One active borrowing per selected item and all availability changes commit together. |
| Failure guarantee | Known failure/rejection leaves no changes; lost commit acknowledgement is reported as unknown until verified. |
| Main success scenario | 1. Staff selects one student, at least two items and one expected return date. 2. System revalidates references, eligibility, active count, availability and date. 3. System commits all borrowing rows and availability changes as one transaction. 4. System confirms and refreshes. |
| Alternative flows | 2a. Duplicate/fewer than two items, invalid date, ineligible student, limit violation or unavailable item: reject with a reason. |
| Exception flows | 2e. Stale/conflicting state: reject and refresh. 3e. Known write failure: rollback and report failure. 3f. Unknown commit outcome: follow BR-18. |
| Business rules | BR-08-12, BR-14-16, BR-18-19 |
| Special requirements | Atomic PostgreSQL transaction; no partial checkout. |
| Acceptance | Two valid items create two active records; invalid requests create none; controlled transaction failure leaves no partial changes. A06. |

## UC-07 - View Student Borrowing History

| Field | Specification |
|---|---|
| Scope / Level | Student history / User Goal |
| Trigger | Staff selects a student and opens history. |
| Preconditions | Application is running; student selection is available. |
| Success guarantee | Only the selected student's active and returned records and related equipment are shown; no data changes. |
| Failure guarantee | Rejected/known failed operations preserve data; an unknown commit outcome follows BR-18. |
| Main success scenario | 1. Staff selects a student. 2. System asynchronously retrieves related borrowings and equipment. 3. System displays identity, item, checkout, expected return, status and actual return date when present. |
| Alternative flows | 3a. No history: display an empty state. 1a. Another student selected: replace results with that student's records. |
| Exception flows | 2e. Missing/stale student or database failure: explain and allow reselection/retry. |
| Business rules | BR-14, BR-18-19 |
| Special requirements | Asynchronous loading, responsive window and loading/error states. |
| Acceptance | Histories for two students do not mix; active, returned and empty results are accurate. A07. |

## UC-08 - View Equipment Borrowing History

| Field | Specification |
|---|---|
| Scope / Level | Equipment history / User Goal |
| Trigger | Staff selects equipment and opens history. |
| Preconditions | Application is running; equipment selection is available. |
| Success guarantee | Only the selected item's active and returned records and related students are shown; no data changes. |
| Failure guarantee | Rejected/known failed operations preserve data; an unknown commit outcome follows BR-18. |
| Main success scenario | 1. Staff selects an item. 2. System asynchronously retrieves its borrowings and students. 3. System displays equipment identity, student, checkout, expected return, status and actual return date. |
| Alternative flows | 3a. No history: display an empty state. 1a. Another item selected: replace details. |
| Exception flows | 2e. Missing/stale item or database failure: explain and allow reselection/retry. |
| Business rules | BR-14, BR-18-19 |
| Special requirements | Asynchronous loading, responsive window and loading/error states. |
| Acceptance | Histories for two items do not mix; active, returned and empty results are accurate. A08. |

## UC-09 - Search, Filter, and Sort Borrowings

| Field | Specification |
|---|---|
| Scope / Level | Borrowing queries / User Goal |
| Trigger | Staff enters criteria or opens the borrowing list. |
| Preconditions | Application is running. |
| Success guarantee | Only matching records are shown in requested order; no data changes. |
| Failure guarantee | Rejected/known failed operations preserve data; an unknown commit outcome follows BR-18. |
| Main success scenario | 1. Staff opens borrowings and chooses search text, status filter and/or sort. 2. System asynchronously loads matching related records. 3. System displays student, equipment, dates and status in requested order. 4. Staff may clear criteria. |
| Alternative flows | 1a. Criteria may be used separately or together. 3a. No matches: show an empty result. 4a. Clear restores the full list. |
| Exception flows | 2e. Database failure: show an error and offer retry. |
| Business rules | BR-17-19 |
| Special requirements | Case-insensitive partial search, combined filters, asynchronous retrieval and loading/error states. |
| Acceptance | Search each specified field, each status, both sort fields/directions, combined criteria and reset against known data. A09. |

## UC-12 - Register Student

| Field | Specification |
|---|---|
| Scope / Level | Student management / User Goal |
| Trigger | Staff submits a new student. |
| Preconditions | Application is running; registration form is accessible. |
| Success guarantee | One uniquely identified student with supplied details and default eligibility is persisted. |
| Failure guarantee | Rejected/known failed operations preserve data; an unknown commit outcome follows BR-18. |
| Main success scenario | 1. Staff enters institutional number, full name and optional contact. 2. System validates required fields and uniqueness. 3. System saves the student. 4. System confirms and displays the saved record. |
| Alternative flows | 2a. Missing/blank required data or duplicate normalized number: reject and identify the problem. 1a. Contact omitted: store as absent. |
| Exception flows | 3e. Concurrent duplicate or database failure: report failure/unknown outcome; no duplicate is created. |
| Business rules | BR-01-02, BR-16, BR-18-19 |
| Special requirements | PostgreSQL persistence and case-insensitive identifier uniqueness. |
| Acceptance | Valid student can be retrieved after restart; blank required fields and duplicate normalized number are rejected. A12. |

## UC-13 - Update Student Information

| Field | Specification |
|---|---|
| Scope / Level | Student management / User Goal |
| Trigger | Staff submits changes to a selected student. |
| Preconditions | A student exists and can be located/selected. |
| Success guarantee | Allowed changes persist for the same student; borrowing relationships/history remain intact. |
| Failure guarantee | Rejected/known failed operations preserve data; an unknown commit outcome follows BR-18. |
| Main success scenario | 1. Staff finds and selects a student. 2. Staff edits permitted fields. 3. System validates and saves changes. 4. System confirms updated details. |
| Alternative flows | 2a. Optional contact left blank: clear it. 3a. Blank required field or duplicate number: reject without changes. |
| Exception flows | 3e. Missing/stale student, conflicting update or database failure: explain and refresh/retry. |
| Business rules | BR-01-03, BR-14, BR-16, BR-18-19 |
| Special requirements | Keep stable internal identity and foreign-key history. |
| Acceptance | Update and restart preserves changed details and linked borrowings; invalid/duplicate edits are rejected. A13. |

## UC-14 - Change Student Borrowing Eligibility

| Field | Specification |
|---|---|
| Scope / Level | Student management / User Goal |
| Trigger | Staff requests a change to a student's eligibility. |
| Preconditions | A student exists and can be selected. |
| Success guarantee | Requested eligibility is persisted; existing active borrowings/history are unchanged. |
| Failure guarantee | Rejected/known failed operations preserve data; an unknown commit outcome follows BR-18. |
| Main success scenario | 1. Staff finds/selects a student. 2. Staff chooses permit or restrict and submits. 3. System saves and confirms eligibility. |
| Alternative flows | 2a. Requested state already set: explain and make no unintended change. 3a. Restricted student with active loans may still return them. |
| Exception flows | 3e. Missing/stale student or database failure: report failure/unknown outcome. |
| Business rules | BR-04, BR-08, BR-13-14, BR-18-19 |
| Special requirements | Eligibility changes must not cancel active borrowings or block returns. |
| Acceptance | Restriction blocks new issue but does not affect active loans/returns; permitting restores new issue when otherwise valid. A14. |

## UC-15 - Register Equipment

| Field | Specification |
|---|---|
| Scope / Level | Equipment management / User Goal |
| Trigger | Staff submits a new physical item. |
| Preconditions | Application is running; registration form is accessible. |
| Success guarantee | One uniquely identified item is persisted as available with no active borrowing. |
| Failure guarantee | Rejected/known failed operations preserve data; an unknown commit outcome follows BR-18. |
| Main success scenario | 1. Staff enters identifier, name and optional description. 2. System validates required fields and uniqueness. 3. System saves item with initial availability. 4. System confirms and displays it. |
| Alternative flows | 2a. Required field blank or normalized identifier duplicate: reject and identify issue. 1a. Optional description omitted: store as absent. |
| Exception flows | 3e. Concurrent duplicate or database failure: report failure/unknown outcome; no duplicate is created. |
| Business rules | BR-05-06, BR-10, BR-16, BR-18-19 |
| Special requirements | PostgreSQL persistence and normalized case-insensitive identifier uniqueness. |
| Acceptance | Valid item appears available after restart; blank required fields and duplicates are rejected. A15. |

## UC-16 - Update Equipment Information

| Field | Specification |
|---|---|
| Scope / Level | Equipment management / User Goal |
| Trigger | Staff submits changes to selected equipment. |
| Preconditions | An item exists and can be located/selected. |
| Success guarantee | Allowed descriptive changes persist for the same item; availability and borrowing history remain intact. |
| Failure guarantee | Rejected/known failed operations preserve data; an unknown commit outcome follows BR-18. |
| Main success scenario | 1. Staff finds/selects an item. 2. Staff edits permitted identifier, name or description. 3. System validates and saves. 4. System confirms updated details. |
| Alternative flows | 2a. Optional description cleared: store as absent. 3a. Blank required field or duplicate identifier: reject. 2b. Availability/borrowing state cannot be edited as descriptive fields. |
| Exception flows | 3e. Missing/stale item, conflicting update or database failure: explain and refresh/retry. |
| Business rules | BR-05-07, BR-10, BR-14, BR-16, BR-18-19 |
| Special requirements | Preserve stable identity, availability and history. |
| Acceptance | Edit and restart preserves new details and linked history/availability; duplicate and blank required edits are rejected. A16. |

## Deferred and future cases

- **Deferred to Laboratory Activity 5:** UC-10, UC-11, UC-17-UC-27.
- **Future scope:** UC-28-UC-33.

No implementation or extra documentation is required for these cases in Activity 4 beyond their classification in the supplied catalog.
