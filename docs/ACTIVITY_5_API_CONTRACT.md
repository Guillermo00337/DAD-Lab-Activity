# Activity 5 API Contract

The machine-readable contract is [`openapi-v1.json`](openapi-v1.json), exported from the running API. Base URL for local development: `http://localhost:5080`.

## Endpoints

| Method | Path | Purpose |
|---|---|---|
| GET | `/api/v1/students` | List/search students; optional `search` query. |
| POST | `/api/v1/students` | Register a student. |
| GET | `/api/v1/students/{id}` | Get a student. |
| PUT | `/api/v1/students/{id}` | Update student details. |
| PATCH | `/api/v1/students/{id}/eligibility` | Change borrowing eligibility. |
| GET | `/api/v1/students/{id}/borrowings` | Get student borrowing history. |
| GET | `/api/v1/equipment` | List equipment; optional `available` query. |
| POST | `/api/v1/equipment` | Register equipment. |
| GET | `/api/v1/equipment/{id}` | Get equipment. |
| PUT | `/api/v1/equipment/{id}` | Update equipment details. |
| GET | `/api/v1/equipment/{id}/borrowings` | Get equipment borrowing history. |
| GET | `/api/v1/borrowings` | Search, filter, and sort borrowing records. |
| POST | `/api/v1/borrowings` | Create a single-item checkout. |
| GET | `/api/v1/borrowings/{id}` | Get one borrowing. |
| POST | `/api/v1/borrowings/{id}/return` | Return an active borrowing. |
| POST | `/api/v1/checkout-requests` | Atomically check out multiple items. |

## Request fields

- Student create/update: `studentNumber`, `fullName`, optional `contact`.
- Eligibility change: `isEligible`.
- Equipment create/update: `equipmentCode`, `name`, optional `description`.
- Single checkout: `studentId`, `equipmentId`, `expectedReturnDate`.
- Multi-item checkout: `studentId`, `equipmentIds` (at least two distinct IDs), `expectedReturnDate`.
- Return: an empty JSON object `{}`.

Dates use ISO format `YYYY-MM-DD`. A checkout’s expected return date must be later than today.

## Search parameters

`GET /api/v1/borrowings` accepts:

- `search`: partial match on student name, student number, or equipment name.
- `status`: `all`, `active`, or `returned`; default `all`.
- `sort`: `borrowedDate` or `expectedReturnDate`; default `borrowedDate`.
- `direction`: `asc` or `desc`; default `asc`.

## Responses and errors

- Reads and updates: `200 OK`.
- Successful creates and checkouts: `201 Created`.
- Invalid request or query values: `400 Bad Request`.
- Missing records: `404 Not Found`.
- Business conflicts and duplicate identifiers: `409 Conflict`.
- Unexpected server failures: `500 Internal Server Error`.

Errors use the JSON shape `{"code":"...","message":"..."}`. The OpenAPI file documents request and response schemas for each route.
