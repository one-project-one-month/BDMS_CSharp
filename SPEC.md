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

### Roles and permissions

Roles: `admin`, `staff`, `donor`, `user`. Authorization is **dynamic RBAC** via `[Permission("resource.action")]` (`BDMS.Api/Filters/PermissionAttribute.cs`), not static role policies. `FeatureManager` only calls `AddAuthorization()` (JWT/cookie auth still registered there).

`PermissionFilter` allows the request when:

1. The caller is authenticated (otherwise 401).
2. The caller has role `admin` (bypass), **or**
3. The JWT has a matching `Permission` claim, **or**
4. A live `Role_Permissions` row matches (covers mid-session grants and tokens without claims).

Otherwise 403. Seeded permission names live in `InMemoryDbSeeder` / `BDMS.sql` (e.g. `donations.view`, `appointments.complete`, `permissions.manage`). Admin is seeded with every permission; staff/donor/user get subsets.

Login, logout, register, and `/me` / refresh stay cookie/JWT auth only (`[Authorize]` where a current user is required). `DashboardController` has `[Authorize]` only — there is no `dashboard.*` permission.

### Seeded dev accounts (`InMemoryDbSeeder` / `BDMS_seed.sql`)

| Email | Password | Role |
|---|---|---|
| `admin@bdms.com` | `Admin@123` | admin |
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

| Controller | Endpoints (permission) |
|---|---|
| **Auth** | `POST login`, `POST logout`, `GET me` — admin/staff login, sets admin cookie |
| **UserAuth** | `POST login`, `POST register`, `POST logout`, `GET me` `[Authorize]`, `POST refresh-token` `[Authorize]` — sets client cookie |
| **Donation** | `GET List` / `GET {HospitalId}/{DonationDate}` / `GET Edit` (`donations.view`), `POST Create` (`donations.create`), `PUT Update` (`donations.update`), `PATCH UpdateStatus` (`donations.approve`), `DELETE Delete` (`donations.delete`) |
| **Appointment** | `GET list` / `GET {id}` (`appointments.view`), `POST donation/{donationId}` (`appointments.create`), `PATCH {id}/status` / `PATCH {id}/time` (`appointments.update`), `POST {id}/complete` (`appointments.complete`), `DELETE {id}` (`appointments.delete`) |
| **BloodRequest** | `GET list` / `GET {id}` (`blood_requests.view`), `POST create` (`blood_requests.create`), `PUT update` (`blood_requests.update`), `PATCH {id}/status` (`blood_requests.approve`), `DELETE {id}` (`blood_requests.delete`) |
| **BloodInventory** | `GET list` / `{id}` / `hospital/{hospitalId}` / `available-stock` (`blood_inventories.view`), `POST add/{donationId}` (`blood_inventories.create`), `PATCH use` / `POST stock-take` (`blood_inventories.update`), `DELETE {id}` (`blood_inventories.delete`) |
| **Donor** | `GET list` / `GET {id}` (`donors.view`), `POST create` (`donors.create`), `PUT update` / `PATCH {id}/status` (`donors.update`), `DELETE delete` (`donors.delete`) |
| **Dashboard** | `GET {userId}` — `[Authorize]` |
| **Certificate** | `POST Generate` (`certificates.create`), `GET list` / `{id}` / `donor/{donorId}` (`certificates.view`) |
| **Announcement** | `GET List` / `GET {id}` (`announcements.view`), `POST` (`announcements.create`), `PUT {id}` (`announcements.update`), `DELETE {id}` (`announcements.delete`) |
| **Hospital** | `GET list` / `GET get` (`hospitals.view`), `PUT update` (`hospitals.update`), `DELETE delete` (`hospitals.delete`) |
| **MedicalRecord** | `GET list` / `{id}` (`medical_records.view`), `POST create` (`medical_records.create`), `PUT update` (`medical_records.update`), `DELETE {id}` (`medical_records.delete`) |
| **Permission / RolePermission** | `GET list` (`permissions.view`), create/edit/delete (`permissions.manage`) |
| **Role** | `GET list` (`roles.view`), `POST create` (`roles.create`), `PUT edit` (`roles.update`), `DELETE delete` (`roles.delete`) |
| **User** | `GET list` / `{id}` (`users.view`), `POST create` (`users.create`), `PUT update` / `PATCH {id}/status` (`users.update`), `DELETE` (`users.delete`) |

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

- **Create** (`POST /api/appointment/donation/{donationId}`, `appointments.create`). Hard preconditions: donation exists, donor active, **donation status = `approved`**, `DonationDate` set, no other open appointment for the donor. New appointment: `AppointmentDate = donation.DonationDate`, time `09:00`, status `scheduled`, `BloodRequestId = null`.
- **Transitions** (`PATCH {id}/status`): `scheduled → confirmed | cancelled`, `confirmed → cancelled` **only**. `confirmed → completed` is rejected — completion goes through `/complete`.
- **Reschedule** (`PATCH {id}/time`, `appointments.update`).
- **Complete** (`POST {id}/complete`, `appointments.complete`, appointment must be `confirmed`):
  - *Donation appointment*: marks donation `completed`; creates inventory row if none exists (units = `UnitsDonated`, `CollectedAt = DonationDate`, `ExpiredAt = collected + 42 days`, status `Available`). If a row already exists (e.g. the donation was completed via `UpdateStatus` first), it is left untouched and the message reports "already existed" — **no error, no duplicate**.
  - *Blood-request appointment*: validates available stock at the appointment's hospital (rich error message including other-hospital availability), consumes units FIFO by expiry, marks request `Fulfilled`.

### Blood request lifecycle

`pending → approved → fulfilled / rejected` (`blood_requests.approve` / `blood_requests.update` / `blood_requests.delete` for mutations). Requests are created with `blood_requests.create`; fulfilment happens through a blood-request appointment consuming inventory.

### Inventory

Rows originate from completed donations (42-day shelf life) or `POST add/{donationId}`; consumed via request fulfilment (`PATCH use`) or stock-taking. Statuses: `available` / `used` (+ stock-take handling).

## 6. Reference: E2E happy path (verified 2026-11-09)

```
POST /api/auth/login                 admin
POST /api/donation/Create            {donorId, hospitalId, bloodGroup, unitsDonated, createdBy} → status Pending
PATCH /api/donation/UpdateStatus     → screening → approved
POST /api/appointment/donation/{id}  admin → scheduled  (donor lacks appointments.create → 403)
PATCH /api/appointment/{id}/status   admin → confirmed
POST /api/appointment/{id}/complete  admin → appointment+donation completed, inventory row created
```

> `donorId` in `donation/Create` is **`Donors.id`** (profile id from `GET /api/donor/list`), not the login `Users.id` — `FK_Donations_Donor → Donors.id`. Passing the user id fails with a plain-text 400 (EF save error).
