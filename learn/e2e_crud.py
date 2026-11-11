#!/usr/bin/env python3
"""BDMS E2E chunk 3 — remaining CRUD: donation/bloodrequest/donor/medicalrecord/
hospital/user/role/permission/announcement update+delete, certificate Generate,
plus RBAC deny probes for the *.update / permissions.manage families the matrix
never probed. Text/plain errors parsed defensively; bodies are valid-shaped
because model-binding 400 runs BEFORE PermissionFilter."""
import json, time, urllib.request, urllib.error, sys, uuid, subprocess

BASE = "http://localhost:5017"
log, checks = [], []

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
        parsed = {"_raw": raw[:250]}
    ok = 200 <= code < 300 and parsed.get("isSuccess") is not False
    note = (parsed.get("message") or (parsed.get("_raw") or ""))[:90]
    log.append((method, path, payload, code, ok, round(ms, 1), note))
    return code, parsed

def check(name, ok, detail=""):
    checks.append((name, ok, detail))
    print(("PASS" if ok else "FAIL"), "|", name, "|", detail)

def sql(q):
    # filtered indexes on Hospitals require QUOTED_IDENTIFIER ON (sqlcmd defaults OFF)
    r = subprocess.run(
        ["docker", "exec", "bdms-sqlserver", "/opt/mssql-tools18/bin/sqlcmd",
         "-S", "localhost", "-U", "sa", "-P", "BDMS@2026", "-C", "-d", "BDMS",
         "-Q", "SET QUOTED_IDENTIFIER ON; " + q, "-W", "-h", "-1"],
        capture_output=True, text=True, timeout=30)
    return [l.strip() for l in r.stdout.splitlines() if l.strip()]

def data_of(r):
    return r.get("data")

def gone(code, r):
    """after a delete, GETs return 400 (often text/plain, no isSuccess key)"""
    return code >= 400 or r.get("isSuccess") is False

# ---- logins
c, r = call("POST", "/api/auth/login", {"email": "admin@bdms.com", "password": "Admin@123"})
admin = (data_of(r) or {}).get("token")
c, r = call("POST", "/api/userauth/login", {"email": "staff@bdms.com", "password": "Staff@123"})
staff = (data_of(r) or {}).get("token")  # live-DB password; seeder staff@123 is drift
c, r = call("POST", "/api/userauth/login", {"email": "somchai.j@email.com", "password": "admin@123"})
donor = (data_of(r) or {}).get("token")
if not admin or not staff or not donor:
    print("ABORT: login failed", {"admin": bool(admin), "staff": bool(staff), "donor": bool(donor)})
    sys.exit(2)

stamp = uuid.uuid4().hex[:8]
c, r = call("POST", "/api/userauth/register", {
    "userName": f"crud_{stamp}", "email": f"e2e.crud.{stamp}@test.local",
    "password": "Passw0rd!123", "confirmPassword": "Passw0rd!123"})
user = (data_of(r) or {}).get("token")
if not user:
    print("ABORT: register failed"); sys.exit(2)
TOKENS = {"admin": admin, "staff": staff, "donor": donor, "user": user}

# ---- lookups
c, r = call("GET", "/api/hospital/list", token=admin)
hospitals = data_of(r) or []
hid = hospitals[0].get("id")
orig_hospital = next(h for h in hospitals if h.get("id") == hid)

c, r = call("GET", "/api/donor/list", token=admin)
donors = data_of(r) or []
donor_pid = next(d.get("id") for d in donors if (d.get("userId") or d.get("UserId")) == 3)

c, r = call("GET", "/api/role/list", token=admin)
roles = data_of(r) or []
donor_role_id = next(x.get("id") for x in roles if str(x.get("name", "")).lower() == "donor")

def mk_donation(remark):
    c, r = call("POST", "/api/donation/Create", {
        "donorId": donor_pid, "hospitalId": hid, "createdBy": 1,
        "bloodGroup": "O+", "unitsDonated": 1, "remarks": remark}, token=admin)
    return (data_of(r) or {}).get("id")

def mk_request(tag):
    c, r = call("POST", "/api/bloodrequest/create", {
        "userId": 3, "hospitalId": hid, "patientName": f"E2E {tag}",
        "bloodGroup": "O+", "unitsRequired": 1, "urgency": "high",
        "requiredDate": "2026-11-20", "reason": tag}, token=admin)
    return (data_of(r) or {}).get("id")

# ================= A. Donation PUT / DELETE
did = mk_donation(f"chunk3-{stamp}")
check("A1 donation created", bool(did), f"id={did}")

c, r = call("GET", f"/api/donation/Edit?DonationId={did}", token=admin)
before = data_of(r) or {}
c, r = call("PUT", "/api/donation/Update", {
    "id": did, "donorId": before.get("donorId"), "hospitalId": before.get("hospitalId"),
    "bloodGroup": before.get("bloodGroup", "O+"), "unitsDonated": before.get("unitsDonated", 1),
    "donationDate": before.get("donationDate"), "status": before.get("status", "pending"),
    "remarks": f"chunk3-updated-{stamp}"}, token=admin)
check("A2 PUT donation update", c == 200, f"http={c}")

c, r = call("GET", f"/api/donation/Edit?DonationId={did}", token=admin)
got = (data_of(r) or {}).get("remarks", "")
check("A3 update persisted", got == f"chunk3-updated-{stamp}", f"remarks={got}")

c, r = call("DELETE", f"/api/donation/Delete?DonationId={did}", token=admin)
check("A4 DELETE donation", c == 200, f"http={c}")
c, r = call("GET", f"/api/donation/Edit?DonationId={did}", token=admin)
check("A5 donation gone after delete", gone(c, r), f"http={c}")

# ================= B. Blood request PUT / DELETE (+ invalid bg edge)
rid = mk_request(f"chunk3-{stamp}")
check("B1 request created", bool(rid), f"id={rid}")

c, r = call("PUT", "/api/bloodrequest/update", {
    "id": rid, "userId": 3, "hospitalId": hid, "patientName": f"E2E chunk3 v2 {stamp}",
    "bloodGroup": "O+", "unitsRequired": 2, "urgency": "medium",
    "requiredDate": "2026-11-21", "reason": f"chunk3-updated-{stamp}"}, token=admin)
check("B2 PUT blood request update", c == 200, f"http={c} msg={(r.get('message') or '')[:50]}")

c, r = call("GET", f"/api/bloodrequest/{rid}", token=admin)
d = data_of(r) or {}
check("B3 update persisted + code immutable",
      d.get("reason") == f"chunk3-updated-{stamp}" and d.get("unitsRequired") == 2
      and bool(d.get("bloodRequestCode")),
      f"reason={d.get('reason')} units={d.get('unitsRequired')} code={d.get('bloodRequestCode')}")

c, r = call("PUT", "/api/bloodrequest/update", {
    "id": rid, "userId": 3, "hospitalId": hid, "patientName": "x",
    "bloodGroup": "INVALID", "unitsRequired": 1, "urgency": "low"}, token=admin)
check("B4 PUT invalid bloodGroup rejected", c == 400 or r.get("isSuccess") is False,
      f"http={c} msg={(r.get('message') or '')[:60]}")

c, r = call("DELETE", f"/api/bloodrequest/{rid}", token=admin)
check("B5 DELETE blood request", c == 200, f"http={c}")
c, r = call("GET", f"/api/bloodrequest/{rid}", token=admin)
check("B6 request gone after delete", gone(c, r), f"http={c}")

# ================= C. User + Donor CRUD, certificate-with-no-donation edge
c, r = call("POST", "/api/user/create", {
    "username": f"crud_{stamp}", "email": f"e2e.crud.donor.{stamp}@test.local",
    "password": "Passw0rd!123", "userRoleId": donor_role_id, "userHospitalId": hid}, token=admin)
new_uid = (data_of(r) or {}).get("userId") or (data_of(r) or {}).get("id")
check("C1 user created", c == 200 and bool(new_uid), f"http={c} userId={new_uid}")

nic = f"E2E-{stamp}"
c, r = call("POST", "/api/donor/create", {
    "userId": new_uid, "nicNo": nic, "dateOfBirth": "1995-04-04", "gender": "male",
    "bloodGroup": "O+", "isActive": True, "remarks": "chunk3"}, token=admin)
check("C2 donor created for new user", c == 200, f"http={c} msg={(r.get('message') or '')[:60]}")

c, r = call("GET", "/api/donor/list", token=admin)
new_donor = next((x for x in (data_of(r) or []) if x.get("nicNo") == nic), {})
new_did = new_donor.get("id")
check("C3 donor found in list", bool(new_did), f"donorId={new_did}")

c, r = call("PUT", "/api/donor/update", {
    "id": new_did, "userId": new_uid, "nicNo": nic, "dateOfBirth": "1995-04-04",
    "gender": "male", "bloodGroup": "O+", "isActive": True,
    "address": f"12 Test St {stamp}", "remarks": "chunk3-updated"}, token=admin)
check("C4 PUT donor update", c == 200, f"http={c} msg={(r.get('message') or '')[:60]}")

c, r = call("GET", f"/api/donor/{new_did}?donorId={new_did}", token=admin)
d = data_of(r) or {}
check("C5 donor update persisted", d.get("address") == f"12 Test St {stamp}",
      f"address={d.get('address')}")

c, r = call("POST", "/api/certificate/Generate",
            {"donorId": new_did, "certificateTitle": "should fail"}, token=admin)
check("C6 certificate rejected — donor has no donations",
      c == 400 or r.get("isSuccess") is False, f"http={c} msg={(r.get('message') or '')[:70]}")

c, r = call("PATCH", f"/api/donor/{new_did}/status", {"isActive": False}, token=admin)
check("C7 donor deactivate", c == 200, f"http={c}")

# deactivate also stamps deleted_at, so DELETE (active-only) reports not-found
c, r = call("DELETE", f"/api/donor/delete?donorId={new_did}", token=admin)
check("C8 DELETE after deactivate → not found (already soft-deleted)", gone(c, r), f"http={c}")

c, r = call("PATCH", f"/api/donor/{new_did}/status", {"isActive": True}, token=admin)
check("C8b reactivate clears soft delete", c == 200, f"http={c}")

c, r = call("DELETE", f"/api/donor/delete?donorId={new_did}", token=admin)
check("C8c DELETE active donor", c == 200, f"http={c} msg={(r.get('message') or '')[:50]}")

# filtered UQ_Donors_* must allow re-creating donor for same user/nic after delete
c, r = call("POST", "/api/donor/create", {
    "userId": new_uid, "nicNo": nic, "dateOfBirth": "1995-04-04", "gender": "male",
    "bloodGroup": "O+", "isActive": True, "remarks": "chunk3-recreate"}, token=admin)
check("C8d re-create donor after delete (filtered unique index)", c == 200,
      f"http={c} msg={(r.get('message') or '')[:60]}")
c, r = call("GET", "/api/donor/list", token=admin)
redonor = next((x for x in (data_of(r) or []) if x.get("nicNo") == nic), {})
if redonor.get("id"):
    call("DELETE", f"/api/donor/delete?donorId={redonor.get('id')}", token=admin)

c, r = call("GET", f"/api/donor/{new_did}?donorId={new_did}", token=admin)
check("C9 donor gone after delete", gone(c, r), f"http={c}")

c, r = call("DELETE", f"/api/user/delete/{new_uid}", token=admin)
check("C10 DELETE user", c == 200, f"http={c} msg={(r.get('message') or '')[:50]}")
c, r = call("GET", f"/api/user/{new_uid}", token=admin)
check("C11 user gone after delete", gone(c, r), f"http={c}")

# ================= D. Medical record CRUD (own donation)
mdid = mk_donation(f"chunk3-mr-{stamp}")
check("D1 donation for record created", bool(mdid), f"id={mdid}")

c, r = call("POST", "/api/medicalrecord/create", {
    "donationId": mdid, "hospitalId": hid, "hemoglobinLevel": 13.5,
    "hivResult": "Negative", "hepatitisBResult": "Negative", "hepatitisCResult": "Negative",
    "malariaResult": "Negative", "syphilisResult": "Negative",
    "screeningStatus": "Passed", "screeningNotes": "chunk3"}, token=admin)
mrid = (data_of(r) or {}).get("id")
check("D2 medical record created", c == 200 and bool(mrid), f"http={c} id={mrid}")

c, r = call("PUT", "/api/medicalrecord/update", {
    "id": mrid, "donationId": mdid, "hospitalId": hid, "hemoglobinLevel": 14.2,
    "hivResult": "Negative", "hepatitisBResult": "Negative", "hepatitisCResult": "Negative",
    "malariaResult": "Negative", "syphilisResult": "Negative",
    "screeningStatus": "Passed", "screeningNotes": "chunk3-updated"}, token=admin)
check("D3 PUT medical record update", c == 200, f"http={c} msg={(r.get('message') or '')[:50]}")

c, r = call("GET", f"/api/medicalrecord/{mrid}", token=admin)
d = data_of(r) or {}
check("D4 update persisted", d.get("hemoglobinLevel") in (14.2, "14.2") and
      d.get("screeningNotes") == "chunk3-updated",
      f"hgb={d.get('hemoglobinLevel')} notes={d.get('screeningNotes')}")

c, r = call("DELETE", f"/api/medicalrecord/{mrid}", token=admin)
check("D5 DELETE medical record", c == 200, f"http={c}")
c, r = call("GET", f"/api/medicalrecord/{mrid}", token=admin)
check("D6 record gone after delete", gone(c, r), f"http={c}")

# ================= E. Hospital update / delete (scratch row via SQL)
new_phone = "+959 555 0001"
c, r = call("PUT", "/api/hospital/update", {
    "id": hid, "name": orig_hospital.get("name"), "address": orig_hospital.get("address"),
    "phone": new_phone, "email": orig_hospital.get("email"),
    "isVerified": orig_hospital.get("isVerified", False)}, token=admin)
check("E1 PUT hospital update", c == 200, f"http={c} msg={(r.get('message') or '')[:50]}")

c, r = call("GET", f"/api/hospital/get?id={hid}", token=admin)
d = data_of(r) or {}
check("E2 hospital update persisted", d.get("phone") == new_phone, f"phone={d.get('phone')}")

# restore original phone
c, r = call("PUT", "/api/hospital/update", {
    "id": hid, "name": orig_hospital.get("name"), "address": orig_hospital.get("address"),
    "phone": orig_hospital.get("phone"), "email": orig_hospital.get("email"),
    "isVerified": orig_hospital.get("isVerified", False)}, token=admin)
check("E3 hospital phone restored", c == 200 and
      (data_of(r) or {}).get("phone") == orig_hospital.get("phone"),
      f"phone={(data_of(r) or {}).get('phone')}")

rows = sql("INSERT INTO Hospitals (name,address,phone,email,is_active,is_verified,created_at,updated_at) "
           "OUTPUT INSERTED.id VALUES ('E2E Scratch Clinic','n/a','000','e2e.scratch@x.local',1,0,SYSUTCDATETIME(),SYSUTCDATETIME());")
scratch_id = next((int(x) for x in rows if x.isdigit()), None)
check("E4 scratch hospital inserted", bool(scratch_id), f"id={scratch_id}")

c, r = call("DELETE", f"/api/hospital/delete?id={scratch_id}", token=admin)
check("E5 DELETE scratch hospital", c == 200, f"http={c} msg={(r.get('message') or '')[:50]}")
c, r = call("GET", f"/api/hospital/get?id={scratch_id}", token=admin)
check("E6 scratch hospital gone", gone(c, r), f"http={c}")

c, r = call("DELETE", "/api/hospital/delete?id=999999", token=admin)
check("E7 DELETE nonexistent hospital → not found",
      c in (400, 404) or r.get("isSuccess") is False, f"http={c} msg={(r.get('message') or '')[:50]}")

# ================= F. Role CRUD + duplicate edge
role_name = f"e2e_role_{stamp}"
c, r = call("POST", "/api/role/create", {"name": role_name}, token=admin)
role_id = (data_of(r) or {}).get("id")
check("F1 role created", c == 200 and bool(role_id), f"http={c} id={role_id}")

c, r = call("POST", "/api/role/create", {"name": role_name}, token=admin)
check("F2 duplicate role rejected", c == 400 or r.get("isSuccess") is False,
      f"http={c} msg={(r.get('message') or '')[:50]}")

c, r = call("PUT", "/api/role/edit", {"id": role_id, "name": role_name + "_v2"}, token=admin)
check("F3 PUT role edit", c == 200, f"http={c}")
c, r = call("GET", "/api/role/list", token=admin)
names = [x.get("name") for x in (data_of(r) or [])]
check("F4 role rename persisted", role_name + "_v2" in names, f"names_has_v2={role_name + '_v2' in names}")

c, r = call("DELETE", "/api/role/delete", {"id": role_id}, token=admin)
check("F5 DELETE role", c == 200, f"http={c} msg={(r.get('message') or '')[:50]}")
c, r = call("GET", "/api/role/list", token=admin)
names = [x.get("name") for x in (data_of(r) or [])]
check("F6 role gone from list", role_name + "_v2" not in names and role_name not in names, "")

# ================= G. Permission CRUD + duplicate edge
perm_name = f"e2e.tmp.{stamp}"
c, r = call("POST", "/api/permission/create", {"name": perm_name}, token=admin)
perm_id = (data_of(r) or {}).get("id")
check("G1 permission created", c == 200 and bool(perm_id), f"http={c} id={perm_id}")

c, r = call("POST", "/api/permission/create", {"name": perm_name}, token=admin)
check("G2 duplicate permission rejected", c == 400 or r.get("isSuccess") is False,
      f"http={c} msg={(r.get('message') or '')[:50]}")

c, r = call("PUT", "/api/permission/edit", {"id": perm_id, "name": perm_name + ".v2"}, token=admin)
check("G3 PUT permission edit", c == 200, f"http={c}")
c, r = call("GET", "/api/permission/list", token=admin)
names = [x.get("name") for x in (data_of(r) or [])]
check("G4 permission rename persisted", perm_name + ".v2" in names, "")

c, r = call("DELETE", "/api/permission/delete", {"id": perm_id}, token=admin)
check("G5 DELETE permission", c == 200, f"http={c} msg={(r.get('message') or '')[:50]}")
c, r = call("GET", "/api/permission/list", token=admin)
names = [x.get("name") for x in (data_of(r) or [])]
check("G6 permission gone from list", perm_name not in names and perm_name + ".v2" not in names, "")

# ================= H. Certificate Generate (donor with donations)
c, r = call("POST", "/api/certificate/Generate", {
    "donorId": donor_pid, "certificateTitle": f"E2E Chunk3 {stamp}",
    "certificateDescription": "generated by e2e_crud_chunk3"}, token=admin)
cert = data_of(r) or {}
cert_id = cert.get("id")
check("H1 certificate generated", c == 200 and bool(cert_id), f"http={c} id={cert_id}")

c, r = call("GET", f"/api/certificate/{cert_id}", token=admin)
d = data_of(r) or {}
check("H2 certificate readable", c == 200 and bool(d), f"title={d.get('certificateTitle')}")

# ================= I. Announcement CRUD
c, r = call("POST", "/api/announcement", {
    "title": f"E2E chunk3 {stamp}", "content": "created by e2e_crud_chunk3",
    "category": "News", "isActive": True}, token=admin)
ann = data_of(r) or {}
ann_id = ann.get("id")
check("I1 announcement created", c == 200 and bool(ann_id), f"http={c} id={ann_id}")

c, r = call("PUT", f"/api/announcement/{ann_id}", {
    "id": ann_id, "title": f"E2E chunk3 updated {stamp}", "content": "updated",
    "category": "Awareness", "isActive": True}, token=admin)
check("I2 PUT announcement update", c == 200, f"http={c} msg={(r.get('message') or '')[:50]}")

c, r = call("GET", f"/api/announcement/{ann_id}", token=admin)
d = data_of(r) or {}
check("I3 update persisted", d.get("title") == f"E2E chunk3 updated {stamp}" and
      d.get("category") in ("Awareness", 3), f"title={d.get('title')} cat={d.get('category')}")

c, r = call("DELETE", f"/api/announcement/{ann_id}", token=admin)
check("I4 DELETE announcement", c == 200, f"http={c}")
c, r = call("GET", f"/api/announcement/{ann_id}", token=admin)
check("I5 announcement gone after delete", gone(c, r), f"http={c}")

# ================= J. Deny probes — update families missing from RBAC matrix
DONOR_UPD = {"id": 999999, "userId": 999999, "nicNo": "PROBE", "dateOfBirth": "1990-01-01",
             "gender": "male", "bloodGroup": "O+", "isActive": True}
MR_UPD = {"id": 999999, "donationId": 999999, "hospitalId": 999999,
          "hivResult": "Negative", "hepatitisBResult": "Negative", "hepatitisCResult": "Negative",
          "malariaResult": "Negative", "syphilisResult": "Negative", "screeningStatus": "Passed"}
ANN_UPD = {"id": 999999, "title": "probe", "content": "x", "category": "News", "isActive": True}
BR_UPD = {"id": 999999, "userId": 999999, "hospitalId": 999999, "patientName": "probe",
          "bloodGroup": "O+", "unitsRequired": 1, "urgency": "low"}
DON_UPD = {"id": 999999, "donorId": 999999, "hospitalId": 999999, "bloodGroup": "O+",
           "unitsDonated": 1, "status": "pending", "remarks": "probe"}
USR_UPD = {"userId": 999999, "username": "probe", "email": "probe@x.local", "userRoleId": 1}
ROL_UPD = {"id": 999999, "name": "probe_role"}
PRM_UPD = {"id": 999999, "name": "probe.perm"}
HOS_UPD = {"id": 999999, "name": "probe", "isVerified": False}

DENY = [
    ("donations.update",     "PUT", "/api/donation/Update",          DON_UPD, "user"),
    ("blood_requests.update","PUT", "/api/bloodrequest/update",      BR_UPD,  "donor"),
    ("blood_requests.update","PUT", "/api/bloodrequest/update",      BR_UPD,  "user"),
    ("donors.update",        "PUT", "/api/donor/update",             DONOR_UPD, "donor"),
    ("donors.update",        "PUT", "/api/donor/update",             DONOR_UPD, "user"),
    ("medical_records.update","PUT","/api/medicalrecord/update",     MR_UPD,  "donor"),
    ("medical_records.update","PUT","/api/medicalrecord/update",     MR_UPD,  "user"),
    ("announcements.update", "PUT", "/api/announcement/999999",      ANN_UPD, "staff"),
    ("announcements.update", "PUT", "/api/announcement/999999",      ANN_UPD, "donor"),
    ("announcements.update", "PUT", "/api/announcement/999999",      ANN_UPD, "user"),
    ("users.update",         "PUT", "/api/user/update",              USR_UPD, "staff"),
    ("roles.update",         "PUT", "/api/role/edit",                ROL_UPD, "staff"),
    ("permissions.manage",   "PUT", "/api/permission/edit",          PRM_UPD, "staff"),
    ("permissions.manage",   "PUT", "/api/permission/edit",          PRM_UPD, "donor"),
    ("permissions.manage",   "PUT", "/api/permission/edit",          PRM_UPD, "user"),
    ("hospitals.update",     "PUT", "/api/hospital/update",          HOS_UPD, "staff"),
]
for perm, method, path, body, role in DENY:
    c, r = call(method, path, body, token=TOKENS[role])
    check(f"J deny {perm} | {role} → 403", c == 403,
          f"http={c} msg={(r.get('message') or r.get('_raw') or '')[:50]}")

# ---- report
fails = [x for x in checks if not x[1]]
total_ms = sum(x[5] for x in log)
print(f"\n{len(checks)-len(fails)}/{len(checks)} checks passed | {len(log)} requests | {round(total_ms,1)} ms total")
if fails:
    print("\nFAILED:")
    for name, _, detail in fails:
        print("  ", name, "|", detail)
sys.exit(1 if fails else 0)
