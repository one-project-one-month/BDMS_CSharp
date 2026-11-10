# BDMS — System Specification

Blood Donation Management System. ASP.NET Core 8 Web API with MediatR (CQRS) and EF Core against SQL Server 2022.

## 1. Solution layout

| Project | Contents |
|---|---|
| `BDMS.Api` | Controllers, `Program.cs`, Swagger, CORS, appsettings |
| `BDMS.Domain` | Feature modules (commands/queries/handlers/services), `FeatureManager.cs` (all DI + auth policy registration) |
| `BDMS.Database` | `AppDbContext`, entity models (`AppDbContextModels`), `InMemoryDbSeeder` |
| `BDMS.Shared` | `Result<T>` envelope, `DevCode` (password hashing), enums + `ToDatabaseValue()` extensions, JWT settings, `BaseController` |
| `Testing` | xUnit + Moq + EF InMemory test suite |

Build/run/test (see `CLAUDE.md` for agent conventions):

```bash
docker compose up -d          # SQL Server 2022 on localhost,1433 (sa / BDMS@2026, db BDMS)
dotnet run --project BDMS.Api # http://localhost:5017 — Swagger UI at /swagger
dotnet test                   # xUnit suite
```

Configuration: `appsettings.Development.json` (localhost SQL Server) overrides `appsettings.json` (stale remote somee.com connection string). Setting `"DatabaseProvider": "InMemory"` switches to EF InMemory and runs `InMemoryDbSeeder`.

## 2. Authentication & authorization

- **JWT issued as HttpOnly cookies**: `BDMS_Admin_Token` (admin/staff login via `AuthController`) and `BDMS_Client_Token` (user/donor login via `UserAuthController`). Cookies are `Secure; SameSite=None`. `OnMessageReceived` reads either cookie, but a standard `Authorization: Bearer <token>` header also works.
- Token config lives in the `Jwt` section (key, issuer, audience, 60-minute expiry).
- Passwords: PBKDF2 in `BDMS.Shared/DevCode.cs` — HMAC-SHA256, 100 000 iterations, 16-byte salt, 32-byte key, base64(salt‖key), static salt `BDMS_STATIC_SALT` (deterministic in dev).

### Roles and policies

Roles: `admin`, `staff`, `donor`, `user`. Policies are registered in `BDMS.Domain/FeatureManager.cs` (not `Program.cs`):

| Policy | Roles |
|---|---|
| `AdminOnly` | admin |
| `StaffOnly` | staff |
| `DonarOnly` *(sic)* | donor |
| `ClientOnly` | user |
| `AdminDonar` | admin, donor |
| `AdminClient` | admin, user |
| `AdminClientDonar` | admin, user, donor |

Note: no endpoint currently uses `StaffOnly`; `BloodInventoryController` and `DashboardController` carry **no `[Authorize]`** and are anonymous.

### Seeded dev accounts (`InMemoryDbSeeder` / `BDMS_seed.sql`)

| Email | Password | Role |
|---|---|---|
| `admin@bdms.com` | `admin@123` | admin |
| `staff@bdms.com` | `staff@123` | staff |
| `somchai.j@email.com` | `admin@123` | donor |
| `niran.s@bgh.co.th` | `admin@123` | staff |

> Known bug: `BDMS_seed.sql` stores bcrypt-format hashes for users 3–4, which `VerifyPassword` cannot parse (it expects base64 PBKDF2). Donor/staff logins from a fresh seed fail with a Base-64 exception until hashes are regenerated.

## 3. API conventions

- Route pattern `api/[controller]/...`; see Swagger at `/swagger` for live schema.
- Responses are a `Result<T>` JSON envelope: `{ isSuccess, isError, data, message }` (via `BaseController.Execute`). Some validation-error paths return the message as plain text — clients must handle both.
- Enums serialize as strings (`JsonStringEnumConverter`).
- CORS allows `http://localhost:5173` with credentials.

### Endpoint map

| Controller | Endpoints (policy) |
|---|---|
| **Auth** | `POST login`, `POST logout`, `GET me` — admin/staff login, sets admin cookie |
| **UserAuth** | `POST login`, `POST register`, `POST logout`, `GET me` `[Authorize]`, `POST refresh-token` `[Authorize]` — sets client cookie |
| **Donation** | `GET List` (AdminClientDonar), `GET {HospitalId}/{DonationDate}` (AdminDonar), `POST Create` (AdminDonar), `GET Edit` (AdminDonar), `PUT Update` (AdminOnly), `PATCH UpdateStatus` (AdminDonar), `DELETE Delete` (AdminOnly) |
| **Appointment** | `GET list`, `GET {id}` (AdminClientDonar), `POST donation/{donationId}` (AdminDonar), `PATCH {id}/status` (AdminDonar), `PATCH {id}/time` (AdminClientDonar), `POST {id}/complete` (AdminOnly), `DELETE {id}` (AdminOnly) |
| **BloodRequest** | `GET list`, `GET {id}` (AdminClientDonar), `POST create` (AdminClient), `PUT update`, `PATCH {id}/status`, `DELETE {id}` (AdminOnly) |
| **BloodInventory** | `GET list`, `GET {id}`, `GET hospital/{hospitalId}`, `GET available-stock`, `POST add/{donationId}`, `PATCH use`, `POST stock-take`, `DELETE {id}` — **anonymous** |
| **Donor** | `GET list` (AdminClientDonar), `POST create` (AdminClient), `GET {id}`/`PUT update`/`PATCH {id}/status`/`DELETE delete` (AdminOnly) |
| **Dashboard** | `GET {userId}` — anonymous |
| **Certificate** | `POST Generate` (AdminOnly), `GET list`/`GET {id}`/`GET donor/{donorId}` (AdminClientDonar) |
| **Announcement** | `GET List`, `GET {id}` (AdminClientDonar), `POST`/`PUT {id}`/`DELETE {id}` (AdminOnly) |
| **Hospital** | `GET list`, `GET get` (AdminClient), `PUT update`, `DELETE delete` (AdminOnly) |
| **MedicalRecord / Permission / Role / RolePermission / User** | AdminOnly (class-level on MedicalRecord, Permission, Role, User) |

## 4. Domain model (core tables)

```
Users ──< Donors >── Donations ──< Appointments            (Appointment.DonationId)
  │                    │                 │
  │                    └──< Blood_Inventories >── Blood_Requests (Appointment.BloodRequestId)
  └── Roles (admin | staff | donor | user)
Hospitals (all links)
```

DB columns are snake_case (`donation_id`, `deleted_at`); table names are mostly plural with underscores (`Blood_Inventories`, `Blood_Requests`, `Medical_Records`). Soft delete = `deleted_at` timestamp, but several service queries use `DeletedAt` navigation-free filtering — always filter `DeletedAt == null`.

Status enums (`BDMS.Shared/Enums`), stored lowercase via `ToDatabaseValue()`:

- **Donation**: `pending, screening, approved, rejected, cancelled, completed`
- **Appointment**: `Scheduled, Confirmed, Cancelled, Completed` (stored lowercased by handlers)
- **BloodInventory**: Available / Used … (stored lowercase)
- **BloodRequest**: Pending / Approved / Fulfilled / Rejected

## 5. Business flows & invariants

### Donation lifecycle (the donation cycle)

```
pending → screening → approved → completed
                     ↘ rejected / cancelled
```

1. **Create** (`POST /api/donation/Create`, admin or donor): generates `DonationCode` as `{HOSPITALCODE}_{BLOODTYPECODE}_{NN}` — a per-hospital, per-blood-group sequence guarded by an in-process `SemaphoreSlim` per hospital. Status starts `Pending`; `DonationDate` = today (server UTC).
2. **Screening/Approval**: `PATCH /api/donation/UpdateStatus` with `{ id, status }`. *There is no server-side transition validation* — any status may be set by admin/donor.
3. **Completion** (two equivalent paths — exactly one should run):
   - `PATCH UpdateStatus → completed`, which calls `BloodInventoryService.AddtoInventory` when `previousStatus != completed` (guard order: status is read **before** mutation).
   - `POST /api/appointment/{id}/complete` (below), which creates the inventory row inline.
   - Both paths are now idempotent w.r.t. inventory: a second run skips creating a duplicate row rather than failing.

### Appointment lifecycle

- **Create** (`POST /api/appointment/donation/{donationId}`, admin/donor). Hard preconditions: donation exists, donor active, **donation status = `approved`**, `DonationDate` set, no other open appointment for the donor. New appointment: `AppointmentDate = donation.DonationDate`, time `09:00`, status `scheduled`, `BloodRequestId = null`.
- **Transitions** (`PATCH {id}/status`): `scheduled → confirmed | cancelled`, `confirmed → cancelled` **only**. `confirmed → completed` is rejected — completion goes through `/complete`.
- **Reschedule** (`PATCH {id}/time`, AdminClientDonar).
- **Complete** (`POST {id}/complete`, AdminOnly, appointment must be `confirmed`):
  - *Donation appointment*: marks donation `completed`; creates inventory row if none exists (units = `UnitsDonated`, `CollectedAt = DonationDate`, `ExpiredAt = collected + 42 days`, status `Available`). If a row already exists (e.g. the donation was completed via `UpdateStatus` first), it is left untouched and the message reports "already existed" — **no error, no duplicate**.
  - *Blood-request appointment*: validates available stock at the appointment's hospital (rich error message including other-hospital availability), consumes units FIFO by expiry, marks request `Fulfilled`.

### Blood request lifecycle

`pending → approved → fulfilled / rejected` (admin only for mutations). Requests are created by admin/user (`AdminClient`); fulfilment happens through a blood-request appointment consuming inventory.

### Inventory

Rows originate from completed donations (42-day shelf life) or `POST add/{donationId}`; consumed via request fulfilment (`PATCH use`) or stock-taking. Statuses: `available` / `used` (+ stock-take handling).

## 6. Reference: E2E happy path (verified 2026-11-09)

```
POST /api/auth/login                 admin
POST /api/donation/Create            {donorId, hospitalId, bloodGroup, unitsDonated, createdBy} → status Pending
PATCH /api/donation/UpdateStatus     → screening → approved
POST /api/appointment/donation/{id}  donor → scheduled
PATCH /api/appointment/{id}/status   admin → confirmed
POST /api/appointment/{id}/complete  admin → appointment+donation completed, inventory row created
```
