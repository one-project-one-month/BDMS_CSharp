-- =============================================
-- BDMS — Blood Donation Management System
-- Microsoft SQL Server Database Creation Script
-- =============================================

USE master;
GO

IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = 'BDMS')
    CREATE DATABASE BDMS;
GO

USE BDMS;
GO

-- =============================================
-- 1. ROLES (no FK dependencies)
-- =============================================
CREATE TABLE Roles (
    id              INT IDENTITY(1,1)   PRIMARY KEY,
    name            NVARCHAR(100)       NOT NULL,
    created_at      DATETIME2           NOT NULL DEFAULT GETDATE(),
    updated_at      DATETIME2           NOT NULL DEFAULT GETDATE(),

    CONSTRAINT UQ_Roles_Name UNIQUE (name)
);
GO

-- =============================================
-- 2. PERMISSIONS (no FK dependencies)
-- =============================================
CREATE TABLE Permissions (
    id              INT IDENTITY(1,1)   PRIMARY KEY,
    name            NVARCHAR(100)       NOT NULL,
    created_at      DATETIME2           NOT NULL DEFAULT GETDATE(),
    updated_at      DATETIME2           NOT NULL DEFAULT GETDATE(),

    CONSTRAINT UQ_Permissions_Name UNIQUE (name)
);
GO

-- =============================================
-- 3. ROLE_PERMISSIONS (depends on Roles, Permissions)
-- =============================================
CREATE TABLE Role_Permissions (
    role_id         INT                 NOT NULL,
    permission_id   INT                 NOT NULL,
    created_at      DATETIME2           NOT NULL DEFAULT GETDATE(),
    updated_at      DATETIME2           NOT NULL DEFAULT GETDATE(),

    CONSTRAINT PK_RolePermissions          PRIMARY KEY (role_id, permission_id),
    CONSTRAINT FK_RolePermissions_Role     FOREIGN KEY (role_id)       REFERENCES Roles(id),
    CONSTRAINT FK_RolePermissions_Perm     FOREIGN KEY (permission_id) REFERENCES Permissions(id)
);
GO

-- =============================================
-- 4. HOSPITALS (no FK dependencies)
-- =============================================
CREATE TABLE Hospitals (
    id              INT IDENTITY(1,1)   PRIMARY KEY,
    name            NVARCHAR(255)       NOT NULL,
    address         NVARCHAR(MAX)       NULL,
    phone           NVARCHAR(20)        NULL,
    email           NVARCHAR(255)       NULL,
    is_active       BIT                 NOT NULL DEFAULT 1,
    is_verified     BIT                 NOT NULL DEFAULT 0,
    created_at      DATETIME2           NOT NULL DEFAULT GETDATE(),
    updated_at      DATETIME2           NOT NULL DEFAULT GETDATE(),
    deleted_at      DATETIME2           NULL,

    CONSTRAINT UQ_Hospitals_Phone UNIQUE (phone),
    CONSTRAINT UQ_Hospitals_Email UNIQUE (email)
);
GO

-- =============================================
-- 5. USERS (depends on Roles, Hospitals)
-- =============================================
CREATE TABLE Users (
    id              INT IDENTITY(1,1)   PRIMARY KEY,
    role_id         INT                 NOT NULL,
    hospital_id     INT                 NULL,
    user_name       NVARCHAR(100)       NOT NULL,
    email           NVARCHAR(255)       NOT NULL,
    password        NVARCHAR(255)       NOT NULL,
    is_active       BIT                 NOT NULL DEFAULT 1,
    created_at      DATETIME2           NOT NULL DEFAULT GETDATE(),
    updated_at      DATETIME2           NOT NULL DEFAULT GETDATE(),
    deleted_at      DATETIME2           NULL,

    CONSTRAINT UQ_Users_Email       UNIQUE (email),
    CONSTRAINT FK_Users_Role        FOREIGN KEY (role_id)     REFERENCES Roles(id),
    CONSTRAINT FK_Users_Hospital    FOREIGN KEY (hospital_id) REFERENCES Hospitals(id)
);
GO

-- =============================================
-- 6. DONORS (depends on Users)
-- =============================================
CREATE TABLE Donors (
    id                  INT IDENTITY(1,1)   PRIMARY KEY,
    user_id             INT                 NOT NULL,
    nic_no              NVARCHAR(50)        NOT NULL,
    date_of_birth       DATE                NOT NULL,
    gender              NVARCHAR(10)        NOT NULL,
    blood_group         NVARCHAR(5)         NOT NULL,
    last_donation_date  DATE                NULL,
    remarks             NVARCHAR(MAX)       NULL,
    emergency_contact   NVARCHAR(255)       NULL,
    emergency_phone     NVARCHAR(20)        NULL,
    address             NVARCHAR(MAX)       NULL,
    is_active           BIT                 NOT NULL DEFAULT 1,
    created_at          DATETIME2           NOT NULL DEFAULT GETDATE(),
    updated_at          DATETIME2           NOT NULL DEFAULT GETDATE(),
    deleted_at          DATETIME2           NULL,

    CONSTRAINT UQ_Donors_UserId           UNIQUE (user_id),
    CONSTRAINT UQ_Donors_NicNo            UNIQUE (nic_no),
    CONSTRAINT FK_Donors_User             FOREIGN KEY (user_id) REFERENCES Users(id),
    CONSTRAINT CK_Donors_BloodGroup       CHECK (blood_group IN ('A+','A-','B+','B-','AB+','AB-','O+','O-')),
    CONSTRAINT CK_Donors_Gender           CHECK (gender IN ('male','female','other'))
);
GO

-- =============================================
-- 7. ANNOUNCEMENTS (no FK dependencies per diagram)
-- =============================================
CREATE TABLE Announcements (
    id              INT IDENTITY(1,1)   PRIMARY KEY,
    title           NVARCHAR(255)       NOT NULL,
    content         NVARCHAR(MAX)       NULL,
    is_active       BIT                 NOT NULL DEFAULT 1,
    expired_at      DATE                NULL,
    created_at      DATE                NOT NULL DEFAULT CAST(GETDATE() AS DATE),
    updated_at      DATE                NOT NULL DEFAULT CAST(GETDATE() AS DATE)
);
GO

-- =============================================
-- 8. CERTIFICATES (depends on Users)
-- =============================================
CREATE TABLE Certificates (
    id                          INT IDENTITY(1,1)   PRIMARY KEY,
    user_id                     INT                 NOT NULL,
    certificate_title           NVARCHAR(255)       NULL,
    certificate_description     NVARCHAR(500)       NULL,
    certificate_data            NVARCHAR(MAX)       NULL,
    created_at                  DATE                NOT NULL DEFAULT CAST(GETDATE() AS DATE),
    updated_at                  DATE                NOT NULL DEFAULT CAST(GETDATE() AS DATE),

    CONSTRAINT FK_Certificates_User FOREIGN KEY (user_id) REFERENCES Users(id)
);
GO

-- =============================================
-- 9. BLOOD_REQUESTS (depends on Users, Hospitals)
-- =============================================
CREATE TABLE Blood_Requests (
    id                  INT IDENTITY(1,1)   PRIMARY KEY,
    user_id             INT                 NOT NULL,
    hospital_id         INT                 NOT NULL,
    patient_name        NVARCHAR(255)       NOT NULL,
    blood_group         NVARCHAR(5)         NOT NULL,
    units_required      INT                 NOT NULL DEFAULT 1,
    contact_phone       NVARCHAR(20)        NULL,
    urgency             NVARCHAR(10)        NOT NULL DEFAULT 'low',
    required_date       DATE                NULL,
    status              NVARCHAR(20)        NOT NULL DEFAULT 'pending',
    reason              NVARCHAR(MAX)       NULL,
    approved_by         INT                 NULL,
    approved_at         DATETIME2           NULL,
    created_at          DATETIME2           NOT NULL DEFAULT GETDATE(),
    updated_at          DATETIME2           NOT NULL DEFAULT GETDATE(),
    deleted_at          DATETIME2           NULL,

    CONSTRAINT FK_BloodRequests_User        FOREIGN KEY (user_id)     REFERENCES Users(id),
    CONSTRAINT FK_BloodRequests_Hospital    FOREIGN KEY (hospital_id) REFERENCES Hospitals(id),
    CONSTRAINT FK_BloodRequests_ApprovedBy  FOREIGN KEY (approved_by) REFERENCES Users(id),
    CONSTRAINT CK_BloodRequests_BloodGroup  CHECK (blood_group IN ('A+','A-','B+','B-','AB+','AB-','O+','O-')),
    CONSTRAINT CK_BloodRequests_Urgency     CHECK (urgency IN ('low','medium','high','critical')),
    CONSTRAINT CK_BloodRequests_Status      CHECK (status IN ('pending','cancelled','approved','rejected','fulfilled'))
);
GO

-- =============================================
-- 10. DONATIONS (depends on Donors, Blood_Requests, Hospitals, Users)
-- =============================================
CREATE TABLE Donations (
    id                  INT IDENTITY(1,1)   PRIMARY KEY,
    donor_id            INT                 NOT NULL,
    hospital_id         INT                 NOT NULL,
    blood_request_id    INT                 NULL,
    created_by          INT                 NOT NULL,
    blood_group         NVARCHAR(5)         NOT NULL,
    units_donated       INT                 NULL,
    donation_date       DATE                NULL,
    status              NVARCHAR(20)        NOT NULL DEFAULT 'pending',
    approved_by         INT                 NULL,
    approved_at         DATETIME2           NULL,
    remarks             NVARCHAR(MAX)       NULL,
    created_at          DATETIME2           NOT NULL DEFAULT GETDATE(),
    updated_at          DATETIME2           NOT NULL DEFAULT GETDATE(),
    deleted_at          DATETIME2           NULL,

    CONSTRAINT FK_Donations_Donor           FOREIGN KEY (donor_id)         REFERENCES Donors(id),
    CONSTRAINT FK_Donations_Hospital        FOREIGN KEY (hospital_id)      REFERENCES Hospitals(id),
    CONSTRAINT FK_Donations_BloodRequest    FOREIGN KEY (blood_request_id) REFERENCES Blood_Requests(id),
    CONSTRAINT FK_Donations_CreatedBy       FOREIGN KEY (created_by)       REFERENCES Users(id),
    CONSTRAINT FK_Donations_ApprovedBy      FOREIGN KEY (approved_by)      REFERENCES Users(id),
    CONSTRAINT CK_Donations_BloodGroup      CHECK (blood_group IN ('A+','A-','B+','B-','AB+','AB-','O+','O-')),
    CONSTRAINT CK_Donations_Status          CHECK (status IN ('pending','cancelled','approved','screening','rejected','completed'))
);
GO

-- =============================================
-- 11. APPOINTMENTS (depends on Users, Hospitals, Donations, Blood_Requests)
-- =============================================
CREATE TABLE Appointments (
    id                  INT IDENTITY(1,1)   PRIMARY KEY,
    user_id             INT                 NOT NULL,
    hospital_id         INT                 NOT NULL,
    donation_id         INT                 NULL,
    blood_request_id    INT                 NULL,
    appointment_date    DATE                NOT NULL,
    appointment_time    TIME                NOT NULL,
    status              NVARCHAR(20)        NOT NULL DEFAULT 'scheduled',
    remarks             NVARCHAR(MAX)       NULL,
    created_at          DATETIME2           NOT NULL DEFAULT GETDATE(),
    updated_at          DATETIME2           NOT NULL DEFAULT GETDATE(),
    deleted_at          DATETIME2           NULL,

    CONSTRAINT FK_Appointments_User             FOREIGN KEY (user_id)          REFERENCES Users(id),
    CONSTRAINT FK_Appointments_Hospital         FOREIGN KEY (hospital_id)      REFERENCES Hospitals(id),
    CONSTRAINT FK_Appointments_Donation         FOREIGN KEY (donation_id)      REFERENCES Donations(id),
    CONSTRAINT FK_Appointments_BloodRequest     FOREIGN KEY (blood_request_id) REFERENCES Blood_Requests(id),
    CONSTRAINT CK_Appointments_Status           CHECK (status IN ('scheduled','confirmed','cancelled','completed'))
);
GO

-- =============================================
-- 12. MEDICAL_RECORDS (1:1 with Donations, depends on Hospitals, Users)
-- =============================================
CREATE TABLE Medical_Records (
    id                  INT IDENTITY(1,1)   PRIMARY KEY,
    donation_id         INT                 NOT NULL,
    hospital_id         INT                 NOT NULL,
    hemoglobin_level    DECIMAL(4,2)        NULL,
    hiv_result          NVARCHAR(15)        NULL,
    hepatitis_b_result  NVARCHAR(15)        NULL,
    hepatitis_c_result  NVARCHAR(15)        NULL,
    malaria_result      NVARCHAR(15)        NULL,
    syphilis_result     NVARCHAR(15)        NULL,
    screening_status    NVARCHAR(10)        NULL,
    screening_notes     NVARCHAR(MAX)       NULL,
    screened_by         INT                 NULL,
    screening_at        DATETIME2           NULL,
    created_at          DATETIME2           NOT NULL DEFAULT GETDATE(),
    updated_at          DATETIME2           NOT NULL DEFAULT GETDATE(),
    deleted_at          DATETIME2           NULL,

    CONSTRAINT UQ_MedicalRecords_Donation       UNIQUE (donation_id),
    CONSTRAINT FK_MedicalRecords_Donation       FOREIGN KEY (donation_id)  REFERENCES Donations(id),
    CONSTRAINT FK_MedicalRecords_Hospital       FOREIGN KEY (hospital_id)  REFERENCES Hospitals(id),
    CONSTRAINT FK_MedicalRecords_ScreenedBy     FOREIGN KEY (screened_by)  REFERENCES Users(id),
    CONSTRAINT CK_MedicalRecords_HIV            CHECK (hiv_result IN ('positive','negative','inconclusive')),
    CONSTRAINT CK_MedicalRecords_HepB           CHECK (hepatitis_b_result IN ('positive','negative','inconclusive')),
    CONSTRAINT CK_MedicalRecords_HepC           CHECK (hepatitis_c_result IN ('positive','negative','inconclusive')),
    CONSTRAINT CK_MedicalRecords_Malaria        CHECK (malaria_result IN ('positive','negative','inconclusive')),
    CONSTRAINT CK_MedicalRecords_Syphilis       CHECK (syphilis_result IN ('positive','negative','inconclusive')),
    CONSTRAINT CK_MedicalRecords_ScreenStatus   CHECK (screening_status IN ('pending','failed','passed'))
);
GO

-- =============================================
-- 13. BLOOD_INVENTORIES (depends on Donations, Hospitals, Blood_Requests)
-- =============================================
CREATE TABLE Blood_Inventories (
    id              INT IDENTITY(1,1)   PRIMARY KEY,
    donation_id     INT                 NOT NULL,
    hospital_id     INT                 NOT NULL,
    blood_group     NVARCHAR(5)         NOT NULL,
    units           INT                 NOT NULL DEFAULT 0,
    collected_at    DATE                NULL,
    expired_at      DATE                NULL,
    status          NVARCHAR(15)        NOT NULL DEFAULT 'available',
    request_id      INT                 NULL,
    created_at      DATETIME2           NOT NULL DEFAULT GETDATE(),
    updated_at      DATETIME2           NOT NULL DEFAULT GETDATE(),
    deleted_at      DATETIME2           NULL,

    CONSTRAINT UQ_BloodInventories_Donation     UNIQUE (donation_id),
    CONSTRAINT FK_BloodInventories_Donation     FOREIGN KEY (donation_id) REFERENCES Donations(id),
    CONSTRAINT FK_BloodInventories_Hospital     FOREIGN KEY (hospital_id) REFERENCES Hospitals(id),
    CONSTRAINT FK_BloodInventories_Request      FOREIGN KEY (request_id)  REFERENCES Blood_Requests(id),
    CONSTRAINT CK_BloodInventories_BloodGroup   CHECK (blood_group IN ('A+','A-','B+','B-','AB+','AB-','O+','O-')),
    CONSTRAINT CK_BloodInventories_Status       CHECK (status IN ('available','used','expired'))
);
GO

-- =============================================
-- TRIGGERS: Auto-update updated_at on all tables
-- =============================================

CREATE TRIGGER trg_Roles_UpdatedAt ON Roles AFTER UPDATE AS
    UPDATE Roles SET updated_at = GETDATE() WHERE id IN (SELECT id FROM inserted);
GO

CREATE TRIGGER trg_Permissions_UpdatedAt ON Permissions AFTER UPDATE AS
    UPDATE Permissions SET updated_at = GETDATE() WHERE id IN (SELECT id FROM inserted);
GO

CREATE TRIGGER trg_RolePermissions_UpdatedAt ON Role_Permissions AFTER UPDATE AS
    UPDATE Role_Permissions SET updated_at = GETDATE()
    WHERE role_id IN (SELECT role_id FROM inserted)
      AND permission_id IN (SELECT permission_id FROM inserted);
GO

CREATE TRIGGER trg_Hospitals_UpdatedAt ON Hospitals AFTER UPDATE AS
    UPDATE Hospitals SET updated_at = GETDATE() WHERE id IN (SELECT id FROM inserted);
GO

CREATE TRIGGER trg_Users_UpdatedAt ON Users AFTER UPDATE AS
    UPDATE Users SET updated_at = GETDATE() WHERE id IN (SELECT id FROM inserted);
GO

CREATE TRIGGER trg_Donors_UpdatedAt ON Donors AFTER UPDATE AS
    UPDATE Donors SET updated_at = GETDATE() WHERE id IN (SELECT id FROM inserted);
GO

CREATE TRIGGER trg_Announcements_UpdatedAt ON Announcements AFTER UPDATE AS
    UPDATE Announcements SET updated_at = CAST(GETDATE() AS DATE) WHERE id IN (SELECT id FROM inserted);
GO

CREATE TRIGGER trg_Certificates_UpdatedAt ON Certificates AFTER UPDATE AS
    UPDATE Certificates SET updated_at = CAST(GETDATE() AS DATE) WHERE id IN (SELECT id FROM inserted);
GO

CREATE TRIGGER trg_BloodRequests_UpdatedAt ON Blood_Requests AFTER UPDATE AS
    UPDATE Blood_Requests SET updated_at = GETDATE() WHERE id IN (SELECT id FROM inserted);
GO

CREATE TRIGGER trg_Donations_UpdatedAt ON Donations AFTER UPDATE AS
    UPDATE Donations SET updated_at = GETDATE() WHERE id IN (SELECT id FROM inserted);
GO

CREATE TRIGGER trg_Appointments_UpdatedAt ON Appointments AFTER UPDATE AS
    UPDATE Appointments SET updated_at = GETDATE() WHERE id IN (SELECT id FROM inserted);
GO

CREATE TRIGGER trg_MedicalRecords_UpdatedAt ON Medical_Records AFTER UPDATE AS
    UPDATE Medical_Records SET updated_at = GETDATE() WHERE id IN (SELECT id FROM inserted);
GO

CREATE TRIGGER trg_BloodInventories_UpdatedAt ON Blood_Inventories AFTER UPDATE AS
    UPDATE Blood_Inventories SET updated_at = GETDATE() WHERE id IN (SELECT id FROM inserted);
GO


-- =============================================
-- SEED DATA: Roles
-- =============================================
INSERT INTO Roles (name) VALUES
('admin'),
('staff'),
('donor'),
('user');
GO

-- =============================================
-- SEED DATA: Permissions
-- =============================================
INSERT INTO Permissions (name) VALUES
('users.view'),
('users.create'),
('users.update'),
('users.delete'),
('donors.view'),
('donors.create'),
('donors.update'),
('donors.delete'),
('hospitals.view'),
('hospitals.create'),
('hospitals.update'),
('hospitals.delete'),
('blood_requests.view'),
('blood_requests.create'),
('blood_requests.update'),
('blood_requests.approve'),
('blood_requests.delete'),
('donations.view'),
('donations.create'),
('donations.update'),
('donations.approve'),
('donations.delete'),
('appointments.view'),
('appointments.create'),
('appointments.update'),
('appointments.delete'),
('medical_records.view'),
('medical_records.create'),
('medical_records.update'),
('medical_records.delete'),
('blood_inventories.view'),
('blood_inventories.create'),
('blood_inventories.update'),
('blood_inventories.delete'),
('announcements.view'),
('announcements.create'),
('announcements.update'),
('announcements.delete'),
('certificates.view'),
('certificates.create'),
('certificates.update'),
('certificates.delete'),
('roles.view'),
('roles.create'),
('roles.update'),
('roles.delete'),
('permissions.view'),
('permissions.manage');
GO

-- =============================================
-- SEED DATA: Role_Permissions
-- Admin gets ALL permissions
-- =============================================
INSERT INTO Role_Permissions (role_id, permission_id)
SELECT r.id, p.id
FROM Roles r
CROSS JOIN Permissions p
WHERE r.name = 'admin';
GO

-- Staff gets operational permissions (no role/permission management)
INSERT INTO Role_Permissions (role_id, permission_id)
SELECT r.id, p.id
FROM Roles r
CROSS JOIN Permissions p
WHERE r.name = 'staff'
  AND p.name IN (
    'users.view',
    'donors.view', 'donors.create', 'donors.update',
    'hospitals.view',
    'blood_requests.view', 'blood_requests.create', 'blood_requests.update', 'blood_requests.approve',
    'donations.view', 'donations.create', 'donations.update', 'donations.approve',
    'appointments.view', 'appointments.create', 'appointments.update',
    'medical_records.view', 'medical_records.create', 'medical_records.update',
    'blood_inventories.view', 'blood_inventories.create', 'blood_inventories.update',
    'announcements.view',
    'certificates.view', 'certificates.create'
  );
GO

-- Donor gets self-service permissions
INSERT INTO Role_Permissions (role_id, permission_id)
SELECT r.id, p.id
FROM Roles r
CROSS JOIN Permissions p
WHERE r.name = 'donor'
  AND p.name IN (
    'donors.view',
    'blood_requests.view',
    'donations.view',
    'appointments.view', 'appointments.create', 'appointments.update',
    'medical_records.view',
    'announcements.view',
    'certificates.view'
  );
GO

-- User gets read-only / minimal permissions
INSERT INTO Role_Permissions (role_id, permission_id)
SELECT r.id, p.id
FROM Roles r
CROSS JOIN Permissions p
WHERE r.name = 'user'
  AND p.name IN (
    'blood_requests.view', 'blood_requests.create',
    'announcements.view',
    'appointments.view', 'appointments.create'
  );
GO

-- =============================================
-- SEED DATA: Sample Users (password is a placeholder hash)
-- =============================================
INSERT INTO Users (role_id, hospital_id, user_name, email, password) VALUES
((SELECT id FROM Roles WHERE name = 'admin'), NULL, 'System Admin',   'admin@bdms.com',    'hashed_password_here'),
((SELECT id FROM Roles WHERE name = 'staff'), NULL, 'Staff Member',   'staff@bdms.com',    'hashed_password_here'),
((SELECT id FROM Roles WHERE name = 'donor'), NULL, 'John Donor',     'john@example.com',  'hashed_password_here'),
((SELECT id FROM Roles WHERE name = 'user'),  NULL, 'Jane User',      'jane@example.com',  'hashed_password_here');
GO