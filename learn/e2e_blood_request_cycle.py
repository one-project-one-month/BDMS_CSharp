#!/usr/bin/env python3
"""BDMS E2E timed — blood request cycle (create → approve → appointment → fulfil → inventory)."""
import json, time, urllib.request, urllib.error, sys

BASE = "http://localhost:5017"
log = []

def call(method, path, body=None, token=None):
    payload = json.dumps(body) if body is not None else None
    req = urllib.request.Request(BASE + path, method=method)
    req.add_header("Content-Type", "application/json")
    if token:
        req.add_header("Authorization", "Bearer " + token)
    data = payload.encode() if payload else None
    t0 = time.perf_counter()
    try:
        with urllib.request.urlopen(req, data) as r:
            raw = r.read().decode(); code = r.status
    except urllib.error.HTTPError as e:
        raw = e.read().decode(); code = e.code
    ms = (time.perf_counter() - t0) * 1000
    try:
        parsed = json.loads(raw)
    except Exception:
        parsed = {"_raw": raw[:200]}
    ok = 200 <= code < 300 and parsed.get("isSuccess") is not False
    note = (parsed.get("message") or (parsed.get("_raw") or ""))[:80]
    log.append((method, path, payload, code, ok, round(ms, 1), note))
    return code, parsed

# 1. logins
c, r = call("POST", "/api/auth/login", {"email": "admin@bdms.com", "password": "Admin@123"})
admin = (r.get("data") or {}).get("token")
c, r = call("POST", "/api/userauth/login", {"email": "somchai.j@email.com", "password": "admin@123"})
donor = (r.get("data") or {}).get("token")
donor_uid = ((r.get("data") or {}).get("userInfo") or {}).get("userId")
if not admin or not donor:
    print("ABORT: login failed"); sys.exit(1)

# 2. lookups + stock baseline
c, r = call("GET", "/api/hospital/list", token=admin)
hid = (r.get("data") or [{}])[0].get("id")
c, r = call("GET", f"/api/bloodinventory/hospital/{hid}", token=admin)
before = sum((x.get("units") or 0) for x in (r.get("data") or [])
             if str(x.get("status", "")).lower() == "available"
             and str(x.get("bloodGroup", "")).upper() == "O+")
log.append(("CHECK", f"baseline O+ available units @hospital {hid}", None, 200, before > 0, 0, f"units={before}"))

# 3. create blood request — donor first (expect 403), then admin
body = {"userId": donor_uid, "hospitalId": hid, "patientName": "E2E Patient",
        "bloodGroup": "O+", "unitsRequired": 1, "contactPhone": "0800000000",
        "urgency": "high", "requiredDate": "2026-11-20", "reason": "e2e cycle"}
c, r = call("POST", "/api/bloodrequest/create", body, token=donor)
if not r.get("isSuccess"):
    c, r = call("POST", "/api/bloodrequest/create", body, token=admin)
req_id = (r.get("data") or {}).get("id")
if not req_id:
    print("ABORT: blood request create failed"); sys.exit(1)

# 4. donor view check (no blood_requests.view expected)
call("GET", "/api/bloodrequest/list", token=donor)

# 5. approve (admin) → auto-creates scheduled appointment
c, r = call("PATCH", f"/api/bloodrequest/{req_id}/status", {"status": "approved"}, token=admin)
if not r.get("isSuccess"):
    print("ABORT: approve failed"); sys.exit(1)

c, r = call("GET", "/api/appointment/list", token=admin)
appt = next((a for a in (r.get("data") or [])
             if (a.get("bloodRequestId") or a.get("BloodRequestId")) == req_id), {})
appt_id = appt.get("id") or appt.get("Id")
log.append(("CHECK", "auto-created appointment for request", None, 200, bool(appt_id), 0,
            f"apptId={appt_id} status={appt.get('status')}"))
if not appt_id:
    print("ABORT: no auto appointment"); sys.exit(1)

# 6. confirm + complete (admin)
call("PATCH", f"/api/appointment/{appt_id}/status", {"status": "confirmed"}, token=admin)
c, r = call("POST", f"/api/appointment/{appt_id}/complete", token=admin)
if not r.get("isSuccess"):
    print("ABORT: complete failed"); sys.exit(1)

# 7. verify
c, r = call("GET", f"/api/bloodrequest/{req_id}", token=admin)
st = str((r.get("data") or {}).get("status", "")).lower()
log.append(("CHECK", "blood request status == fulfilled", None, 200, st == "fulfilled", 0, st))

c, r = call("GET", f"/api/bloodinventory/hospital/{hid}", token=admin)
rows = r.get("data") or []
after = sum((x.get("units") or 0) for x in rows
            if str(x.get("status", "")).lower() == "available"
            and str(x.get("bloodGroup", "")).upper() == "O+")
used = any((x.get("requestId") or x.get("RequestId")) == req_id for x in rows)
# partial consumption only decrements units; requestId/used set only when a row is fully drained
log.append(("CHECK", "inventory consumed (available O+ decreased)", None, 200, after < before, 0,
            f"before={before} after={after} full-row tag requestId={used}"))

call("GET", "/api/bloodrequest/list", token=admin)
call("GET", "/api/appointment/list", token=donor)

# report
print("| # | Method | Endpoint | Payload | HTTP | OK | ms | Note |")
print("|---|--------|----------|---------|------|----|----|------|")
for i, (m, p, pl, code, ok, ms, note) in enumerate(log, 1):
    pl = (pl[:70] + "…") if pl and len(pl) > 70 else (pl or "—")
    print(f"| {i} | {m} | {p} | {pl.replace('|','\\\\|')} | {code} | {'✓' if ok else '✗'} | {ms} | {(note or '').replace('|','\\\\|')} |")

fails = [x for x in log if not x[4]]
total = sum(x[5] for x in log if x[0] != "CHECK")
print(f"\n{len(log)-len(fails)}/{len(log)} ok | total {round(total,1)} ms | "
      f"max {max(x[5] for x in log if x[0] != 'CHECK')} ms")
sys.exit(1 if fails else 0)
