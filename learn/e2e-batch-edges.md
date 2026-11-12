# E2E batch — edge cases (#4, #1–3, #6–7)

Run 2026-11-11 against local API `http://localhost:5017`.
Script: [`e2e_batch_edges.py`](e2e_batch_edges.py) — `python3 learn/e2e_batch_edges.py`.

Result: **19/19 checks passed** (55 requests, 1.85 s total) after fixing 3 findings + 2 regressions they exposed.
Covers: donation completion idempotency (#4), blood-request reject/direct-fulfil/insufficient-stock (#1–3),
appointment transitions (#6), inventory admin ops (#7).

> SPEC.md stays the source of truth. This file is the test log only.

## Checks (final run)

| # | Check | Result | Detail |
|---|-------|--------|--------|
| 4a | `UpdateStatus → completed` creates 1 inventory row | PASS | donation 1020 |
| 4a | second `→ completed` is idempotent | PASS | still 1 row |
| 6b | reschedule via `PATCH {id}/time` (while Scheduled) | PASS | date=2026-11-25 time=14:30 |
| 6a | `confirmed → completed` via PATCH rejected | PASS | 400 "Invalid status transition: Confirmed -> Completed" |
| 6d | reschedule rejected once Confirmed | PASS | 400 "…only be updated when status is 'Scheduled'" |
| 4b | appointment `/complete` creates 1 inventory row | PASS | donation 1021 |
| 4b | `UpdateStatus → completed` after `/complete` stays 1 row (cross-path) | PASS | http 200, rows=1 |
| 6c | `DELETE` appointment then GET fails | PASS | 400 "Appointment not found" |
| 1 | reject path (`→ rejected`) | PASS | request 1020 |
| 1 | rejected request has no open appointment | PASS | open=0 |
| 2 | direct `fulfilled` + `donorId` | PASS | request 1021 fulfilled |
| 2 | exactly 1 unit consumed (partial row allowed) | PASS | available 4→3, tagged_rows=1 |
| 2 | donor blood-group mismatch rejected | PASS | 400 "Donor blood group does not match…" |
| 3 | insufficient stock fails `/complete` | PASS | 400 "Insufficient stock… Available: 3, Required: 99…" |
| 7 | admin `available-stock` | PASS | 200 |
| 7 | `stock-take?hospitalId=1` | PASS | "0 unit(s) marked as expired" |
| 7 | `DELETE {id}` inventory row | PASS | soft-deleted, gone from list |
| 7 | `POST add/{donationId}` re-create after delete | PASS | 200, 1 live row |
| 7 | `PATCH use` tags row | PASS | 200 "Blood unit marked as used." |

## Findings — all fixed

### 1. `PATCH /api/bloodinventory/use` always 500 — handler never registered

- `UseFromInventoryHandler` had a `Handle` method but did not implement
  `IRequestHandler<UseFromInventoryCommand, Result<BloodInventoryResModel>>`.
- **Fixed:** interface + `using MediatR;` added.

### 2. Delete + re-add impossible — unique index ignores soft delete

- `UQ_BloodInventories_Donation` / `UQ_MedicalRecords_Donation` were unfiltered unique constraints;
  soft-deleted rows still held `donation_id`, blocking re-INSERT.
- **Fixed:** `.HasFilter("[deleted_at] IS NULL")` in `AppDbContext.cs` (both indexes);
  live DB constraints dropped and recreated as filtered indexes.

### 3. Direct fulfil wasted whole rows

- Direct `fulfilled` path did `.Take(n rows)` → marked full rows used (a 2-unit row eaten for a 1-unit request).
- **Fixed:** shared `IBloodInventoryService.ConsumeForRequest` (FIFO by expiry, decrement partial rows,
  tag `request_id` only when drained) called from both `BloodRequestService.UpdateStatus`
  and `AppointmentService.CompleteAppointment`.

## Regressions the fixes exposed (also fixed)

### A. Filtered index × trigger `QUOTED_IDENTIFIER` conflict (error 1934)

- `trg_BloodInventories_UpdatedAt` / `trg_MedicalRecords_UpdatedAt` self-UPDATE the same table and were
  created under `QUOTED_IDENTIFIER OFF` (baked in). Filtered indexes require ON → every UPDATE on those
  tables failed with error 1934.
- **Fixed:** both triggers recreated with `SET QUOTED_IDENTIFIER ON` in the creating batch
  (`OBJECTPROPERTY(..., 'IsQuotedIdentOn') = 1` verified). `trg_BloodRequests_UpdatedAt` left as-is —
  no filtered index on that table.

### B. `AddDbContext` registered `AppDbContext` as **Transient** — cross-service writes lost

- `FeatureManager.cs` passed `ServiceLifetime.Transient` for the context. Each injected service got its
  **own** `AppDbContext`. `BloodRequestService` saved the request row while `BloodInventoryService`'s
  mutations (from `ConsumeForRequest`) lived on a different context that was never saved →
  direct fulfil returned 200 `fulfilled` with **stock untouched**, no error.
- **Fixed:** removed the lifetime overrides (default **Scoped**) so all services in a request share one context.
- Symptom recap: fulfil success + `avail_units` unchanged + inventory `UPDATE` never appeared in the API log.

## Verified-by-design behaviour (not bugs)

- Reschedule allowed only while `Scheduled` (400 after confirm).
- `confirmed → completed` via PATCH always rejected — completion is `/complete` only.
- Insufficient-stock message includes the other-hospital / not-available hints.
- Stock-take only expires rows past `expired_at`; 0 expired is correct.

## Unit tests

`dotnet test` → **90/91**. Sole failure: `PermissionFilterTests.OnActionExecutionAsync_UserWithMatchingClaim_CallsNext`
— pre-existing on clean HEAD (verified via stash earlier), unrelated to these changes.

## Not covered (still open from the master list)

Logout/register/refresh, donation CRUD beyond status, donor/announcement/certificate/
medical-record-update/hospital-update/user-role-permission CRUD, dashboard,
blood-request update, other-hospital stock hint happy path.
