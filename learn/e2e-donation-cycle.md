# E2E — Blood donation cycle (donor + admin)

Run 2026-11-11 against local API `http://localhost:5017` (SQL Server container `bdms-sqlserver`).
Script: [`e2e_donation_cycle.py`](e2e_donation_cycle.py) — `python3 learn/e2e_donation_cycle.py`.

Covers: donation lifecycle, **medical records**, **appointment**, **blood inventory**, both roles.
Result: **21/23 ok** — the two failures are expected donor 403s (see below).
Total request time **769 ms**; slowest call admin login (387 ms, PBKDF2 100k ×2 + JWT), all others ≤ 32 ms.

> SPEC.md stays the source of truth for the system. This file is the test log only.

## Flow exercised

```
admin login → create donation (Pending) → create medical record (Passed)
→ screening → approved → appointment (admin; donor 403) → confirmed → complete
→ donation completed + inventory row (2 units, Available)
donor: /me, medical record list, donation list, appointment list ✓
```

## Endpoints, payloads, latency

| # | Method | Endpoint | Payload | HTTP | ms | Note |
|---|--------|----------|---------|------|----|------|
| 1 | POST | `/api/auth/login` | `{"email":"admin@bdms.com","password":"Admin@123"}` | 200 | 386.9 | Login successful |
| 2 | POST | `/api/userauth/login` | `{"email":"somchai.j@email.com","password":"admin@123"}` | 200 | 160.3 | Login successful |
| 3 | GET | `/api/userauth/me` | — (Bearer donor) | 200 | 13.6 | |
| 4 | GET | `/api/auth/me` | — (Bearer admin) | 200 | 7.0 | |
| 5 | GET | `/api/hospital/list` | — | 200 | 6.6 | 4 hospitals |
| 6 | GET | `/api/donor/list` | — | 200 | 6.5 | profileId=1 |
| 7 | POST | `/api/donation/Create` | `{"donorId":1,"hospitalId":1,"createdBy":1,"bloodGroup":"O+","unitsDonated":2,"remarks":"e2e timed"}` | 200 | 24.4 | Pending, code `YANGONGENERALHOSPITAL_O+_03` |
| 8 | POST | `/api/medicalrecord/create` | `{"donationId":1014,"hospitalId":1,"hemoglobinLevel":13.5,"hivResult":"Negative",…,"screeningStatus":"Passed","screenedBy":1}` | 200 | 15.8 | id 1003 |
| 9 | GET | `/api/medicalrecord/list` | — (donor) | 200 | 15.6 | sees own record |
| 10 | PATCH | `/api/donation/UpdateStatus` | `{"id":1014,"status":"screening"}` | 200 | 17.8 | |
| 11 | PATCH | `/api/donation/UpdateStatus` | `{"id":1014,"status":"approved"}` | 200 | 8.4 | |
| 12 | POST | `/api/appointment/donation/1014` | `{"remarks":"e2e"}` **as donor** | **403** | 5.7 | expected — donor lacks `appointments.create` |
| 13 | POST | `/api/appointment/donation/1014` | `{"remarks":"e2e"}` as admin | 200 | 31.1 | Scheduled |
| 14 | PATCH | `/api/appointment/1006/status` | `{"status":"confirmed"}` | 200 | 9.2 | |
| 15 | POST | `/api/appointment/1006/complete` | — | 200 | 19.4 | donation→completed, inventory updated |
| 16 | GET | `/api/appointment/1006` | — | 200 | 5.1 | status `Completed` |
| 17 | GET | `/api/donation/List` | — (admin) | 200 | 5.9 | |
| 18 | GET | `/api/bloodinventory/hospital/1` | — | 200 | 6.3 | row: 2 units, `Available` |
| 19 | GET | `/api/donation/List` | — (donor) | 200 | 7.9 | |
| 20 | GET | `/api/appointment/list` | — (donor) | 200 | 10.9 | |
| 21 | GET | `/api/bloodinventory/available-stock` | — (donor) | **403** | 4.6 | expected — donor lacks `blood_inventories.view` |

Checks (no HTTP call): donation status = `completed` ✓ · inventory row for donation 1014 = 2 units `Available` ✓

## Gotchas found this run (already reflected in SPEC.md §2/§6)

1. Admin password is `Admin@123`, not `admin@123`.
2. Appointment create must be done by admin — donor has no `appointments.create`.
3. `donation/Create.donorId` is `Donors.id` (from `GET /api/donor/list`), not the login `Users.id`; wrong id → plain-text 400 EF save error.
4. Validation/error responses sometimes come back `text/plain`, not the JSON `Result` envelope.

## Not covered

Blood-request → inventory consumption, certificate generation, appointment reschedule (`PATCH {id}/time`),
second completion path (`PATCH donation/UpdateStatus → completed`), logout/refresh.
