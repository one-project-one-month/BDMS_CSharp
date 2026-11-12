# E2E — auth lifecycle (client + admin)

Run 2026-11-11 against local API `http://localhost:5017`.
Script: [`e2e_auth_cycle.py`](e2e_auth_cycle.py) — `python3 learn/e2e_auth_cycle.py`.

Result: **25/25 checks passed** (23 requests, 1.39 s total).

> SPEC.md stays the source of truth. This file is the test log only.

## What was covered

Client (`/api/userauth`): register → login → me → refresh-token → logout, plus rejection paths and cookie behaviour.
Admin (`/api/auth`): login → me → logout, role gate, cookie swap with client login.

## Checks

| Check | Result | Detail |
|-------|--------|--------|
| register: new user 200 + token | PASS | "Registration successful." + `BDMS_Client_Token` cookie set |
| register: duplicate email rejected | PASS | 400 "Email already exists." |
| register: password mismatch rejected | PASS | 400 "Passwords do not match." |
| login: correct credentials | PASS | 200 + token (~130 ms) |
| login: wrong password | PASS | 401 "Invalid password" |
| login: unknown email | PASS | 401 "Invalid email or password" |
| me: valid Bearer token | PASS | 200, returns registered email |
| me: no token / garbage token | PASS | 401 / 401 |
| me: `BDMS_Client_Token` cookie | PASS | 200 (hand-sent Cookie header) |
| refresh-token: 200 + rotated token | PASS | new token works on `me` |
| refresh-token: no token | PASS | 401 |
| client logout | PASS | 200 + `BDMS_Client_Token=;` (epoch expiry) |
| me after logout with old Bearer | PASS* | 200 — stateless JWT, no server-side denylist (*by design, see notes) |
| admin login | PASS | 200 + token + `BDMS_Admin_Token` cookie |
| admin login wrong password | PASS | 401 |
| admin me | PASS | 200 |
| admin me with donor token | PASS | 401 "Access denied. Admin or staff role required." |
| admin logout | PASS | 200 + clears admin cookie |
| admin me after logout (Bearer) | PASS* | 200 — stateless, same as client |
| client login clears admin cookie | PASS | `BDMS_Admin_Token=;` in Set-Cookie |
| admin login clears client cookie | PASS | `BDMS_Client_Token=;` in Set-Cookie |

## Notes / observations (not failures)

- **Logout is cookie-only.** No token denylist/revocation: the JWT keeps working until expiry if replayed.
  Fine for this app's scope; relevant if you ever need forced logout.
- **Refresh rotates the token** (new JWT issued). Same-second refresh can return a byte-identical token
  (same claims + `iat`) — observed once; not a bug.
- **Cookie flags:** `HttpOnly; Secure; SameSite=None`. urllib will not round-trip `Secure` cookies over
  `http://`, so the script hand-sends the `Cookie` header — a real browser on `localhost` treats it as a
  secure context and works normally.
- **Register latency** ~1.9 s first call (cold PBKDF2 + model warmup), ~130 ms after.
- Login responses distinguish "Invalid password" vs "Invalid email or password" (email enumeration
  nuance — left as-is, SPEC doesn't require uniformity).

## Not covered here

- Admin `refresh-token` (endpoint exists only on userauth).
- Password reset / change-password (no endpoint in SPEC).
- Token expiry / clock skew.
