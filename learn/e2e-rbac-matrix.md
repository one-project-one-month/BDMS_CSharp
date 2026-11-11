# E2E — RBAC matrix (4 roles × permission families)

Run 2026-11-11 against local API `http://localhost:5017`.
Script: [`e2e_rbac_matrix.py`](e2e_rbac_matrix.py) — `python3 learn/e2e_rbac_matrix.py`.

Result: **142/142 checks passed** (146 requests, 1.39 s total), after fixes below.

> SPEC.md stays the source of truth. This file is the test log only.

## What was covered

- Unauthenticated baseline: permission-guarded endpoint and `[Authorize]`-only dashboard → 401.
- Dashboard: any authenticated role → 200.
- 33 permission probes × 4 roles (admin/staff/donor/user), expectations from
  `InMemoryDbSeeder` Role_Permissions. Denied → 403; allowed reads → 2xx;
  allowed writes → valid-shaped body with FK `999999` so they fail business
  validation (400) instead of mutating. `DENY_ONLY` family (announcements.create/delete,
  users.create, roles.create) asserts denials only.
- Fresh `user` role comes from a real `register` call in the script.

## Result by permission family

All 33 families fully green (4/4 cells, or 3/3 for DENY_ONLY):

`donations.view/create/approve/delete` · `appointments.view/create/update/complete/delete` ·
`blood_requests.view/create/approve/delete` · `blood_inventories.view/create/update/delete` ·
`donors.view/create/delete` · `medical_records.view/create/delete` ·
`announcements.view/create/delete` · `certificates.view/create` ·
`hospitals.view/delete` · `users.view/create` · `roles.view/create` · `permissions.view`

## Defects found and fixed in this chunk

| # | Defect | Fix |
|---|--------|-----|
| 1 | Live DB missing `appointments.complete` in the `Permissions` catalog entirely (BDMS.sql seeds it) — staff could never complete appointments, silently | `INSERT` permission + admin/staff grants |
| 2 | Live DB Role_Permissions drifted from seeder: missing donor `appointments.create/update`, user `appointments.update`, user `hospitals.view` (6 grant rows) | `INSERT` grants matching BDMS.sql/seeder |
| 3 | `GET /api/announcement/List` → 400 for every role: entity maps `category`, live table (and BDMS.sql) had no such column | `ALTER TABLE Announcements ADD category VARCHAR(20) NOT NULL DEFAULT 'News'`; same column added to `BDMS.sql` CREATE |
| 4 | Dashboard → 400 for every role: called `sp_GetDashboardData`, which exists in no DB and no repo file | `DashboardService` now always uses the existing EF path; Dapper/SP branch deleted |
| 5 | `PermissionFilter` skipped its claim-check step (comments jumped 1→3) though `TokenService` still issues `Permission` claims — `PermissionFilterTests.UserWithMatchingClaim` was born red in f3448db | Re-added claim match as step 2 (claims → DB fallback); tests 91/91 |
| 6 | Staff password drift: live DB is `Staff@123`, seeder says `staff@123` | Script uses live password; noted as drift |

## Notes / observations (not failures)

- **Model-binding 400 runs before `PermissionFilter`** — write probes must send valid-shaped
  bodies or denied roles see 400 instead of 403 (`donors.create` initially hit this:
  missing `gender`/`dateOfBirth`).
- **Admin bypasses everything** (role check), so admin cells only prove non-401/403.
- **`appointments.complete` absence was invisible to a naive grants-only diff** —
  always diff the `Permissions` catalog itself, not just `Role_Permissions`.
- Live DB seed (BDMS.sql) and `InMemoryDbSeeder` agree on intent; only the live rows had drifted.
- Staff login `staff@bdms.com` / `Staff@123`; donor `somchai.j@email.com` / `admin@123`;
  admin via `/api/auth/login` / `Admin@123`.

## Not covered here

- Write-side mutation checks (allowed writes all stop at 400 by design).
- `*.update` / `*.delete` families not in the probe list (users, roles, permissions.manage,
  medical_records.update, blood_requests.update, donations.update, announcements.update,
  certificates.update, donors.update) — chunk 3 CRUD territory.
- Permission-claim freshness: claims are checked before live DB, so a revocation in DB
  is not enforced until the token re-issues (matches the filter's documented intent).
