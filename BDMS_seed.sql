-- =============================================
-- BDMS — Seed Data (1–2 records per table)
-- Run this AFTER BDMS.sql has been executed
-- =============================================

USE BDMS;
GO

-- =============================================
-- NOTE: Roles & Permissions already seeded by BDMS.sql
-- We reference them by subquery below
-- =============================================

-- =============================================
-- 1. HOSPITALS
-- =============================================
INSERT INTO Hospitals (name, address, phone, email, is_active, is_verified) VALUES
('Bangkok General Hospital',  '2 Soi Soonvijai 7, New Petchburi Rd, Bangkok 10310', '026103000', 'info@bgh.co.th',    1, 1),
('Bumrungrad International',  '33 Sukhumvit 3, Wattana, Bangkok 10110',              '026673000', 'contact@bumrungrad.com', 1, 1);
GO

-- =============================================
-- 2. USERS  (admin & staff already seeded; add 2 more: a donor-role user + a hospital staff)
-- =============================================
INSERT INTO Users (role_id, hospital_id, user_name, email, password) VALUES
(
  (SELECT id FROM Roles WHERE name = 'donor'),
  NULL,
  'Somchai Jaidee',
  'somchai.j@email.com',
  '$2a$11$Kq8H1zXvYmN3pL9wRtUeOeABCDEF1234567890abcdefABCDEF12'
),
(
  (SELECT id FROM Roles WHERE name = 'staff'),
  (SELECT id FROM Hospitals WHERE email = 'info@bgh.co.th'),
  'Niran Saengchai',
  'niran.s@bgh.co.th',
  '$2a$11$Kq8H1zXvYmN3pL9wRtUeOeABCDEF1234567890abcdefABCDEF99'
);
GO

-- =============================================
-- 3. DONORS  (depends on Users)
-- =============================================
INSERT INTO Donors (user_id, nic_no, date_of_birth, gender, blood_group, last_donation_date,
                    emergency_contact, emergency_phone, address) VALUES
(
  (SELECT id FROM Users WHERE email = 'somchai.j@email.com'),
  '1100600123456',
  '1990-05-14',
  'male',
  'O+',
  '2025-11-20',
  'Malee Jaidee',
  '0812345678',
  '45/3 Rama IV Rd, Khlong Toei, Bangkok 10110'
),
(
  (SELECT id FROM Users WHERE email = 'niran.s@bgh.co.th'),
  '1100600654321',
  '1985-08-30',
  'male',
  'A+',
  NULL,
  'Sunisa Saengchai',
  '0898765432',
  '12 Lat Phrao Rd, Bangkok 10230'
);
GO

-- =============================================
-- 4. ANNOUNCEMENTS  (no FK dependencies)
-- =============================================
INSERT INTO Announcements (title, content, is_active, expired_at) VALUES
(
  'World Blood Donor Day - June 14',
  'Join us on June 14 for World Blood Donor Day. Donation drives will be held at all partner hospitals across Bangkok. Walk-ins welcome.',
  1,
  '2026-06-15'
),
(
  'Urgent: O- Blood Stock Critical',
  'Our O-negative inventory is critically low. All eligible donors are urged to visit the nearest hospital as soon as possible.',
  1,
  '2026-05-20'
);
GO

-- =============================================
-- 5. BLOOD_REQUESTS  (depends on Users, Hospitals)
-- =============================================
INSERT INTO Blood_Requests
  (user_id, hospital_id, blood_request_code, patient_name, blood_group,
   units_required, contact_phone, urgency, required_date, status, reason) VALUES
(
  (SELECT id FROM Users WHERE email = 'somchai.j@email.com'),
  (SELECT id FROM Hospitals WHERE email = 'info@bgh.co.th'),
  'BR-2026-0001',
  'Pranee Sukjai',
  'O+',
  2,
  '026103000',
  'high',
  '2026-05-10',
  'approved',
  'Emergency surgery - trauma patient'
),
(
  (SELECT id FROM Users WHERE email = 'niran.s@bgh.co.th'),
  (SELECT id FROM Hospitals WHERE email = 'contact@bumrungrad.com'),
  'BR-2026-0002',
  'David Lim',
  'B+',
  1,
  '026673000',
  'medium',
  '2026-05-15',
  'pending',
  'Scheduled cardiac procedure'
);
GO

-- =============================================
-- 6. DONATIONS  (depends on Donors, Hospitals, Blood_Requests, Users)
-- =============================================
INSERT INTO Donations
  (donor_id, hospital_id, blood_request_id, created_by, donation_code,
   blood_group, units_donated, donation_date, status) VALUES
(
  (SELECT id FROM Donors WHERE nic_no = '1100600123456'),
  (SELECT id FROM Hospitals WHERE email = 'info@bgh.co.th'),
  (SELECT id FROM Blood_Requests WHERE blood_request_code = 'BR-2026-0001'),
  (SELECT id FROM Users WHERE email = 'niran.s@bgh.co.th'),
  'DON-2026-0001',
  'O+',
  1,
  '2026-05-02',
  'completed'
),
(
  (SELECT id FROM Donors WHERE nic_no = '1100600654321'),
  (SELECT id FROM Hospitals WHERE email = 'contact@bumrungrad.com'),
  NULL,
  (SELECT id FROM Users WHERE email = 'niran.s@bgh.co.th'),
  'DON-2026-0002',
  'A+',
  1,
  '2026-05-02',
  'pending'
);
GO

-- =============================================
-- 7. APPOINTMENTS  (depends on Users, Hospitals, Donations, Blood_Requests)
-- =============================================
INSERT INTO Appointments
  (user_id, hospital_id, donation_id, blood_request_id,
   appointment_date, appointment_time, status, remarks) VALUES
(
  (SELECT id FROM Users WHERE email = 'somchai.j@email.com'),
  (SELECT id FROM Hospitals WHERE email = 'info@bgh.co.th'),
  (SELECT id FROM Donations WHERE donation_code = 'DON-2026-0001'),
  (SELECT id FROM Blood_Requests WHERE blood_request_code = 'BR-2026-0001'),
  '2026-05-02',
  '09:00:00',
  'completed',
  'Donor arrived on time. No complications.'
),
(
  (SELECT id FROM Users WHERE email = 'niran.s@bgh.co.th'),
  (SELECT id FROM Hospitals WHERE email = 'contact@bumrungrad.com'),
  (SELECT id FROM Donations WHERE donation_code = 'DON-2026-0002'),
  NULL,
  '2026-05-07',
  '14:00:00',
  'scheduled',
  'First-time donation screening appointment.'
);
GO

-- =============================================
-- 8. MEDICAL_RECORDS  (1:1 with Donations)
-- =============================================
INSERT INTO Medical_Records
  (donation_id, hospital_id, hemoglobin_level,
   hiv_result, hepatitis_b_result, hepatitis_c_result,
   malaria_result, syphilis_result,
   screening_status, screening_notes,
   screened_by, screening_at) VALUES
(
  (SELECT id FROM Donations WHERE donation_code = 'DON-2026-0001'),
  (SELECT id FROM Hospitals WHERE email = 'info@bgh.co.th'),
  14.20,
  'negative', 'negative', 'negative', 'negative', 'negative',
  'passed',
  'All screening tests passed. Donor fit for donation.',
  (SELECT id FROM Users WHERE email = 'niran.s@bgh.co.th'),
  '2026-05-02 08:45:00'
),
(
  (SELECT id FROM Donations WHERE donation_code = 'DON-2026-0002'),
  (SELECT id FROM Hospitals WHERE email = 'contact@bumrungrad.com'),
  13.80,
  'negative', 'negative', 'negative', 'negative', 'negative',
  'pending',
  'Awaiting lab confirmation.',
  NULL,
  NULL
);
GO

-- =============================================
-- 9. BLOOD_INVENTORIES  (depends on Donations, Hospitals, Blood_Requests)
-- =============================================
INSERT INTO Blood_Inventories
  (donation_id, hospital_id, blood_group, units,
   collected_at, expired_at, status, request_id) VALUES
(
  (SELECT id FROM Donations WHERE donation_code = 'DON-2026-0001'),
  (SELECT id FROM Hospitals WHERE email = 'info@bgh.co.th'),
  'O+',
  1,
  '2026-05-02',
  '2026-06-02',
  'used',
  (SELECT id FROM Blood_Requests WHERE blood_request_code = 'BR-2026-0001')
),
(
  (SELECT id FROM Donations WHERE donation_code = 'DON-2026-0002'),
  (SELECT id FROM Hospitals WHERE email = 'contact@bumrungrad.com'),
  'A+',
  1,
  '2026-05-02',
  '2026-06-02',
  'available',
  NULL
);
GO

-- =============================================
-- 10. CERTIFICATES  (depends on Users)
-- =============================================
INSERT INTO Certificates
  (user_id, certificate_title, certificate_description, certificate_data) VALUES
(
  (SELECT id FROM Users WHERE email = 'somchai.j@email.com'),
  'Blood Donation Certificate - May 2026',
  'Awarded to Somchai Jaidee for completing a successful blood donation on 2 May 2026.',
  '{"donor":"Somchai Jaidee","date":"2026-05-02","hospital":"Bangkok General Hospital","bloodGroup":"O+","units":1}'
),
(
  (SELECT id FROM Users WHERE email = 'niran.s@bgh.co.th'),
  'Staff Appreciation Certificate - Q1 2026',
  'Awarded to Niran Saengchai for outstanding contribution to the blood donation program.',
  '{"recipient":"Niran Saengchai","period":"Q1 2026","hospital":"Bangkok General Hospital"}'
);
GO

-- =============================================
-- VERIFY: Quick row counts
-- =============================================
SELECT 'Hospitals'         AS [Table], COUNT(*) AS [Rows] FROM Hospitals
UNION ALL
SELECT 'Users',             COUNT(*) FROM Users
UNION ALL
SELECT 'Donors',            COUNT(*) FROM Donors
UNION ALL
SELECT 'Announcements',     COUNT(*) FROM Announcements
UNION ALL
SELECT 'Blood_Requests',    COUNT(*) FROM Blood_Requests
UNION ALL
SELECT 'Donations',         COUNT(*) FROM Donations
UNION ALL
SELECT 'Appointments',      COUNT(*) FROM Appointments
UNION ALL
SELECT 'Medical_Records',   COUNT(*) FROM Medical_Records
UNION ALL
SELECT 'Blood_Inventories', COUNT(*) FROM Blood_Inventories
UNION ALL
SELECT 'Certificates',      COUNT(*) FROM Certificates;
GO
