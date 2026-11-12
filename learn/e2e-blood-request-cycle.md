# E2E — Blood request cycle (donor + admin)

Run 2026-11-11 against local API `http://localhost:5017`.
Script: [`e2e_blood_request_cycle.py`](e2e_blood_request_cycle.py) — `python3 learn/e2e_blood_request_cycle.py`.

Result: **17/19 ok** — the two failures are expected donor 403s.
Total request time **416 ms**; slowest admin login (176 ms), all others ≤ 36 ms.

> SPEC.md stays the source of truth. This file is the test log only.

## Flow exercised

```
admin login → create blood request (donor 403 → admin fallback, pending)
→ approve (admin) → auto-created scheduled appointment 1008
→ confirmed (admin) → complete (admin)
→ request fulfilled + inventory consumed (O+ 3 → 2 units, row fully drained → status used + requestId tagged)
donor: blood request list 403 (no blood_requests.view) ✓ expected; appointment list ✓
```

## How the flow actually works (verified)

- **Approve requires `RequiredDate`** — set it at create or approval is rejected.
- **Approve auto-creates the appointment** (`EnsureAppointmentStartedForApprovedRequest`): status `scheduled`, date = `RequiredDate`, remarks "Auto-created when blood request was approved". No manual `POST /api/appointment/...` step — unlike the donation flow.
- **`POST /api/appointment/{id}/complete`** on a blood-request appointment: checks available units **summed per hospital** (FIFO by expiry), consumes rows, marks request `fulfilled`.
  - Partial row: only `units` decremented, status stays `Available`, `requestId` stays null.
  - Row fully drained: `units=0`, `status=used`, `requestId` set.
- **Alternate path (not exercised):** `PATCH /api/bloodrequest/{id}/status {"status":"fulfilled","donorId":N}` consumes stock directly — donor blood group must match request.

## Endpoints, payloads, latency

| # | Method | Endpoint | Payload | HTTP | ms | Note |
|---|--------|----------|---------|------|----|------|
| 1 | POST | `/api/auth/login` | `{"email":"admin@bdms.com","password":"Admin@123"}` | 200 | 175.6 | |
| 2 | POST | `/api/userauth/login` | `{"email":"somchai.j@email.com","password":"admin@123"}` | 200 | 121.0 | |
| 3 | GET | `/api/hospital/list` | — | 200 | 6.5 | hospitalId=1 |
| 4 | GET | `/api/bloodinventory/hospital/1` | — | 200 | 6.0 | baseline O+ = 3 units |
| 5 | POST | `/api/bloodrequest/create` | `{"userId":3,"hospitalId":1,"patientName":"E2E Patient","bloodGroup":"O+","unitsRequired":1,"contactPhone":"0800000000","urgency":"high","requiredDate":"2026-11-20","reason":"e2e cycle"}` **as donor** | **403** | 5.4 | expected — no `blood_requests.create` |
| 6 | POST | `/api/bloodrequest/create` | same as #5, as admin | 200 | 21.6 | request 1004 created |
| 7 | GET | `/api/bloodrequest/list` | — (donor) | **403** | 3.7 | expected — no `blood_requests.view` |
| 8 | PATCH | `/api/bloodrequest/1004/status` | `{"status":"approved"}` | 200 | 15.5 | auto-created appointment 1008 `scheduled` |
| 9 | GET | `/api/appointment/list` | — | 200 | 3.6 | finds appt with `bloodRequestId=1004` |
| 10 | PATCH | `/api/appointment/1008/status` | `{"status":"confirmed"}` | 200 | 6.0 | |
| 11 | POST | `/api/appointment/1008/complete` | — | 200 | 35.6 | "Appointment completed and blood request fulfilled" |
| 12 | GET | `/api/bloodrequest/1004` | — | 200 | 3.7 | status `fulfilled` |
| 13 | GET | `/api/bloodinventory/hospital/1` | — | 200 | 2.9 | O+ 3→2, drained row `used` + `requestId=1004` |
| 14 | GET | `/api/bloodrequest/list` | — (admin) | 200 | 3.6 | |
| 15 | GET | `/api/appointment/list` | — (donor) | 200 | 5.3 | sees the blood-request appointment |

## Gotchas

1. Donor has **no** `blood_requests.*` permissions — create/list are admin-only in practice (donor 403 both).
2. Approval hard-fails without `RequiredDate`.
3. Stock check on complete is **units summed**, not row count; a hospital can pass with one multi-unit row.
4. `requestId` on an inventory row only appears when that row is **fully** consumed — partial draws just lower `units`.

## Not covered

Reject path, direct `fulfilled` + `donorId` path, insufficient-stock error message, other-hospital stock hint, request update/delete.
