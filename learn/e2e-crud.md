# E2E chunk 3 — remaining CRUD

Run 2026-11-11 against local API `http://localhost:5017`.
Script: [`e2e_crud_chunk3.py`](e2e_crud_chunk3.py) — `python3 learn/e2e_crud_chunk3.py`.

Result: **73/73 checks passed** (82 requests, 0.84 s) after fixing 3 backend bugs + 1 schema class.
Covers: donation PUT/DELETE, bloodrequest PUT/DELETE, donor CRUD, medicalrecord PUT/DELETE,
hospital update/delete, user CRUD, role CRUD, permission CRUD, certificate Generate,
announcement CRUD, plus the 16 RBAC deny probes for `*.update` / `permissions.manage`
families the matrix never covered.

`dotnet test` stays **91/91**.

> SPEC.md stays the source of truth. This file is the test log only.

## Checks (final run)

| # | Check | Result | Detail |
|---|-------|--------|--------|
| A1–A5 | donation create → PUT remarks → verify → DELETE → gone | PASS | id 1032 |
| B1–B3 | request create → PUT (patient/units/reason) → persisted, `bloodRequestCode` immutable | PASS | code `YANGONGENERALHOSPITAL_O+_19…` |
| B4 | PUT invalid bloodGroup rejected | PASS | 400 |
| B5–B6 | DELETE request → gone | PASS | 400 after delete |
| C1–C5 | user create → donor create → PUT address → verify | PASS | donor 1010 |
| C6 | certificate Generate for donor with no donations rejected | PASS | 400 |
| C7–C8 | deactivate donor → DELETE → not found | PASS | deactivate already stamps `deleted_at` |
| C8b–C8c | reactivate → DELETE active donor | PASS | 200 |
| C8d | re-create donor for same user/nic after delete | PASS | filtered unique index works |
| C9–C11 | donor/user gone after delete | PASS | 400 |
| D1–D6 | medical record create → PUT hgb/notes → verify → DELETE → gone | PASS | id 1009 |
| E1–E3 | hospital PUT phone → verify → restore original | PASS | +95-1-256112 restored |
| E4–E6 | scratch hospital insert → API DELETE → gone | PASS | re-insert of same email works (filtered index) |
| E7 | DELETE nonexistent hospital → not found | PASS | 400 |
| F1–F6 | role create → duplicate 400 → rename → delete → gone | PASS | |
| G1–G6 | permission create → duplicate 400 → rename → delete → gone | PASS | |
| H1–H2 | certificate Generate for donor with donations → readable | PASS | id 1007 |
| I1–I5 | announcement create → PUT → verify → DELETE → gone | PASS | category News→Awareness |
| J ×16 | deny probes: donations/blood_requests/donors/medical_records/announcements/users/roles/permissions/hospitals `*.update`+`permissions.manage` against non-privileged roles | PASS | all 403 |

## Findings — all fixed

### 1. Role/Permission/RolePermission create always failed — DTO added to DbContext

- `RoleService.CreateRole`, `PermissionService.CreatePermission`,
  `RolePermissionService.CreateRolePermission`/`UpdateRolePermission` all did
  `_db.AddAsync(<ReqRespModel DTO>)`. Those types aren't in the EF model, so every call
  threw and returned `SystemError` (born red; the catch masked it as
  "Error creating role/permission").
- **Fixed:** construct and add the mapped entities (`Role`, `Permission`, `RolePermission`
  with `CreatedAt`/`UpdatedAt`) and map back to the DTO for the response.

### 2. `DeleteRoleHandler` called `CreateRole`

- Copy-paste bug: `DeleteRoleCommand` handler invoked `_roleService.CreateRole(...)`,
  so DELETE /api/role/delete tried to *create* a nameless role (400) and never deleted.
- **Fixed:** calls `_roleService.DeleteRoleById(command.Id)`.

### 3. Unique indexes ignored soft delete — delete + re-create impossible

- `UQ_Hospitals_Email` / `UQ_Hospitals_Phone` / `UQ_Donors_NicNo` / `UQ_Donors_UserId`
  were unfiltered UNIQUE constraints: a soft-deleted row held the email/phone/nic/user
  forever, so re-creating (scratch hospital insert, donor re-add for the same user) failed.
  Same class as chunk 2's `UQ_BloodInventories_Donation` / `UQ_MedicalRecords_Donation`.
- **Fixed:** `.HasFilter("[deleted_at] IS NULL")` in `AppDbContext.cs` (4 indexes);
  live DB constraints dropped and recreated as filtered indexes;
  `BDMS.sql` updated for these 4 **and** chunk 2's 2 (inline `CONSTRAINT … UNIQUE`
  replaced with filtered `CREATE UNIQUE INDEX`).

### 4. Filtered index × trigger `QUOTED_IDENTIFIER OFF` (error 1934) — chunk-2 regression A again

- `trg_Hospitals_UpdatedAt` / `trg_Donors_UpdatedAt` were baked with
  `QUOTED_IDENTIFIER OFF`. Once the filtered indexes existed, **every UPDATE** on
  Hospitals/Donors failed with error 1934 (surfaced as
  "An error occurred while saving the entity changes").
- **Fixed:** both triggers recreated with `SET QUOTED_IDENTIFIER ON` in the creating
  batch (verified `uses_quoted_identifier = 1`);
  `BDMS.sql` now runs `SET QUOTED_IDENTIFIER ON;` before the trigger section so fresh
  installs bake it in too.

## Notes / edges documented (not bugs)

- `PATCH /api/donor/{id}/status` with `isActive:false` also stamps `deleted_at`
  (deactivate ≡ soft delete; reactivate clears it). A subsequent DELETE reports
  "Donor not found" because DELETE only matches active donors — script asserts this.
- "Gone after delete" GETs return **400 with a text/plain body** (no JSON `isSuccess`)
  — parse defensively, as always.
- Certificates have no delete endpoint: each run leaves one generated certificate row.
- RBAC allow-side for the new `*.update` probes was not re-asserted (matrix already
  covers allowed roles for the family; deny cells are the new coverage).

## Leftover test data (harmless)

Probe users from debugging (`probeuser1/2`, `del_donor_probe`), soft-deleted scratch
hospitals/donors/roles/permissions from failed intermediate runs, and one certificate
per run. Clean-up on demand only.
