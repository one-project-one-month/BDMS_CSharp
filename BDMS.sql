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
-- 1. HOSPITALS (no FK dependencies)
-- =============================================
CREATE TABLE Hospitals (
    id              INT IDENTITY(1,1)   PRIMARY KEY,
    hospital_name   NVARCHAR(255)       NOT NULL,
    address         NVARCHAR(500)       NULL,
    phone           NVARCHAR(20)        NULL,
    email           NVARCHAR(255)       NULL,
    is_verified     BIT                 NOT NULL DEFAULT 0,
    is_deleted      BIT                 NOT NULL DEFAULT 0,
    created_at      DATETIME2           NOT NULL DEFAULT GETDATE(),
    updated_at      DATETIME2           NOT NULL DEFAULT GETDATE()
);
GO

-- =============================================
-- 2. USERS (depends on Hospitals)
-- =============================================
CREATE TABLE Users (
    id              INT IDENTITY(1,1)   PRIMARY KEY,
    hospital_id     INT                 NULL,
    user_name       NVARCHAR(100)       NOT NULL,
    email           NVARCHAR(255)       NOT NULL,
    password        NVARCHAR(255)       NOT NULL,
    role            NVARCHAR(20)        NOT NULL,
    is_active       BIT                 NOT NULL DEFAULT 1,
    is_deleted      BIT                 NOT NULL DEFAULT 0,
    created_at      DATETIME2           NOT NULL DEFAULT GETDATE(),
    updated_at      DATETIME2           NOT NULL DEFAULT GETDATE(),

    CONSTRAINT UQ_Users_Email       UNIQUE (email),
    CONSTRAINT FK_Users_Hospital    FOREIGN KEY (hospital_id) REFERENCES Hospitals(id),
    CONSTRAINT CK_Users_Role        CHECK (role IN ('admin', 'donor', 'staff', 'user'))
);
GO

-- =============================================
-- 3. DONORS (depends on Users)
-- =============================================
CREATE TABLE Donors (
    id                  INT IDENTITY(1,1)   PRIMARY KEY,
    user_id             INT                 NOT NULL,
    blood_group         NVARCHAR(5)         NOT NULL,
    gender              NVARCHAR(10)        NOT NULL,
    address             NVARCHAR(500)       NULL,
    date_of_birth       DATE                NOT NULL,
    last_donation_date  DATE                NULL,
    total_donations     INT                 NOT NULL DEFAULT 0,
    medical_notes       NVARCHAR(MAX)       NULL,
    emergency_consent   BIT                 NOT NULL DEFAULT 0,
    emergency_phone     NVARCHAR(20)        NULL,
    is_deleted          BIT                 NOT NULL DEFAULT 0,
    created_at          DATETIME2           NOT NULL DEFAULT GETDATE(),
    updated_at          DATETIME2           NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_Donors_User           FOREIGN KEY (user_id) REFERENCES Users(id),
    CONSTRAINT CK_Donors_BloodGroup     CHECK (blood_group IN ('A+','A-','B+','B-','AB+','AB-','O+','O-')),
    CONSTRAINT CK_Donors_Gender         CHECK (gender IN ('male','female','other'))
);
GO

-- =============================================
-- 4. ANNOUNCEMENTS (depends on Users)
-- =============================================
CREATE TABLE Announcements (
    id              INT IDENTITY(1,1)   PRIMARY KEY,
    user_id         INT                 NOT NULL,
    title           NVARCHAR(255)       NOT NULL,
    content         NVARCHAR(MAX)       NULL,
    is_active       BIT                 NOT NULL DEFAULT 1,
    is_deleted      BIT                 NOT NULL DEFAULT 0,
    created_at      DATETIME2           NOT NULL DEFAULT GETDATE(),
    updated_at      DATETIME2           NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_Announcements_User    FOREIGN KEY (user_id) REFERENCES Users(id)
);
GO

-- =============================================
-- 5. CERTIFICATES (depends on Users)
-- =============================================
CREATE TABLE Certificates (
    id                          INT IDENTITY(1,1)   PRIMARY KEY,
    user_id                     INT                 NOT NULL,
    certificate_type            NVARCHAR(50)        NOT NULL,
    certificate_description     NVARCHAR(500)       NULL,
    certificate_date            DATE                NOT NULL,
    validity_date               DATE                NULL,
    is_deleted                  BIT                 NOT NULL DEFAULT 0,
    created_at                  DATETIME2           NOT NULL DEFAULT GETDATE(),
    updated_at                  DATETIME2           NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_Certificates_User     FOREIGN KEY (user_id) REFERENCES Users(id),
    CONSTRAINT CK_Certificates_Type     CHECK (certificate_type IN ('donor','volunteer','organizer','emergency'))
);
GO

-- =============================================
-- 6. BLOOD_INVENTORY (depends on Hospitals)
-- =============================================
CREATE TABLE Blood_Inventory (
    id                  INT IDENTITY(1,1)   PRIMARY KEY,
    hospital_id         INT                 NOT NULL,
    blood_group         NVARCHAR(5)         NOT NULL,
    units_available     INT                 NOT NULL DEFAULT 0,
    units_requested     INT                 NOT NULL DEFAULT 0,
    units_used          INT                 NOT NULL DEFAULT 0,
    expiry_date         DATE                NULL,
    is_deleted          BIT                 NOT NULL DEFAULT 0,
    created_at          DATETIME2           NOT NULL DEFAULT GETDATE(),
    updated_at          DATETIME2           NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_BloodInventory_Hospital       FOREIGN KEY (hospital_id) REFERENCES Hospitals(id),
    CONSTRAINT CK_BloodInventory_BloodGroup     CHECK (blood_group IN ('A+','A-','B+','B-','AB+','AB-','O+','O-'))
);
GO

-- =============================================
-- 7. BLOOD_REQUESTS (depends on Users, Hospitals)
-- =============================================
CREATE TABLE Blood_Requests (
    id                  INT IDENTITY(1,1)   PRIMARY KEY,
    user_id             INT                 NOT NULL,
    hospital_id         INT                 NOT NULL,
    patient_name        NVARCHAR(255)       NOT NULL,
    blood_group         NVARCHAR(5)         NOT NULL,
    units_requested     INT                 NOT NULL DEFAULT 1,
    contact_phone       NVARCHAR(20)        NULL,
    urgency             NVARCHAR(10)        NOT NULL DEFAULT 'low',
    status              NVARCHAR(20)        NOT NULL DEFAULT 'pending',
    notes               NVARCHAR(MAX)       NULL,
    approved_by         INT                 NULL,
    is_deleted          BIT                 NOT NULL DEFAULT 0,
    created_at          DATETIME2           NOT NULL DEFAULT GETDATE(),
    updated_at          DATETIME2           NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_BloodRequests_User        FOREIGN KEY (user_id)     REFERENCES Users(id),
    CONSTRAINT FK_BloodRequests_Hospital    FOREIGN KEY (hospital_id) REFERENCES Hospitals(id),
    CONSTRAINT FK_BloodRequests_ApprovedBy  FOREIGN KEY (approved_by) REFERENCES Users(id),
    CONSTRAINT CK_BloodRequests_BloodGroup  CHECK (blood_group IN ('A+','A-','B+','B-','AB+','AB-','O+','O-')),
    CONSTRAINT CK_BloodRequests_Urgency     CHECK (urgency IN ('low','medium','high','critical')),
    CONSTRAINT CK_BloodRequests_Status      CHECK (status IN ('pending','cancelled','approved','rejected','fulfilled'))
);
GO

-- =============================================
-- 8. DONATIONS (depends on Donors, Blood_Requests, Hospitals)
-- =============================================
CREATE TABLE Donations (
    id              INT IDENTITY(1,1)   PRIMARY KEY,
    donor_id        INT                 NOT NULL,
    request_id      INT                 NULL,
    hospital_id     INT                 NOT NULL,
    donation_code   NVARCHAR(50)        NULL,
    status          NVARCHAR(20)        NOT NULL DEFAULT 'pending',
    donation_date   DATE                NULL,
    approved_at     DATETIME2           NULL,
    is_deleted      BIT                 NOT NULL DEFAULT 0,
    created_at      DATETIME2           NOT NULL DEFAULT GETDATE(),
    updated_at      DATETIME2           NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_Donations_Donor       FOREIGN KEY (donor_id)    REFERENCES Donors(id),
    CONSTRAINT FK_Donations_Request     FOREIGN KEY (request_id)  REFERENCES Blood_Requests(id),
    CONSTRAINT FK_Donations_Hospital    FOREIGN KEY (hospital_id) REFERENCES Hospitals(id),
    CONSTRAINT CK_Donations_Status      CHECK (status IN ('pending','matched','completed','failed','cancelled'))
);
GO

-- =============================================
-- 9. DONOR_REQUEST_MATCHES (M:N junction — Donors ⇄ Blood_Requests)
-- =============================================
CREATE TABLE Donor_Request_Matches (
    id              INT IDENTITY(1,1)   PRIMARY KEY,
    donor_id        INT                 NOT NULL,
    request_id      INT                 NOT NULL,
    status          NVARCHAR(20)        NOT NULL DEFAULT 'pending',
    notified_at     DATETIME2           NULL,
    responded_at    DATETIME2           NULL,
    is_deleted      BIT                 NOT NULL DEFAULT 0,
    created_at      DATETIME2           NOT NULL DEFAULT GETDATE(),
    updated_at      DATETIME2           NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_DonorRequestMatches_Donor      FOREIGN KEY (donor_id)   REFERENCES Donors(id),
    CONSTRAINT FK_DonorRequestMatches_Request    FOREIGN KEY (request_id) REFERENCES Blood_Requests(id),
    CONSTRAINT CK_DonorRequestMatches_Status     CHECK (status IN ('pending','accepted','declined','expired'))
);
GO

-- =============================================
-- 10. APPOINTMENTS (depends on Donors, Hospitals, Blood_Requests)
-- =============================================
CREATE TABLE Appointments (
    id                  INT IDENTITY(1,1)   PRIMARY KEY,
    donor_id            INT                 NOT NULL,
    hospital_id         INT                 NOT NULL,
    request_id          INT                 NULL,
    status              NVARCHAR(20)        NOT NULL DEFAULT 'scheduled',
    appointment_type    NVARCHAR(20)        NOT NULL,
    appointment_date    DATETIME2           NOT NULL,
    is_deleted          BIT                 NOT NULL DEFAULT 0,
    created_at          DATETIME2           NOT NULL DEFAULT GETDATE(),
    updated_at          DATETIME2           NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_Appointments_Donor        FOREIGN KEY (donor_id)    REFERENCES Donors(id),
    CONSTRAINT FK_Appointments_Hospital     FOREIGN KEY (hospital_id) REFERENCES Hospitals(id),
    CONSTRAINT FK_Appointments_Request      FOREIGN KEY (request_id)  REFERENCES Blood_Requests(id),
    CONSTRAINT CK_Appointments_Status       CHECK (status IN ('scheduled','confirmed','completed','cancelled','no_show')),
    CONSTRAINT CK_Appointments_Type         CHECK (appointment_type IN ('donation','screening','checkup'))
);
GO

-- =============================================
-- 11. MEDICAL_RECORDS (1:1 with Donations)
-- =============================================
CREATE TABLE Medical_Records (
    id                  INT IDENTITY(1,1)   PRIMARY KEY,
    donation_id         INT                 NOT NULL,
    hemoglobin_level    DECIMAL(5,2)        NULL,
    hiv_test            NVARCHAR(15)        NULL,
    blood_group         NVARCHAR(5)         NULL,
    hepatitis_b_result  NVARCHAR(15)        NULL,
    hepatitis_c_result  NVARCHAR(15)        NULL,
    syphilis_result     NVARCHAR(15)        NULL,
    malaria_result      NVARCHAR(15)        NULL,
    screening_status    NVARCHAR(10)        NULL,
    screening_notes     NVARCHAR(MAX)       NULL,
    screening_by        NVARCHAR(255)       NULL,
    screening_date      DATE                NULL,
    is_deleted          BIT                 NOT NULL DEFAULT 0,
    created_at          DATETIME2           NOT NULL DEFAULT GETDATE(),
    updated_at          DATETIME2           NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_MedicalRecords_Donation       FOREIGN KEY (donation_id) REFERENCES Donations(id),
    CONSTRAINT UQ_MedicalRecords_Donation       UNIQUE (donation_id),
    CONSTRAINT CK_MedicalRecords_BloodGroup     CHECK (blood_group IN ('A+','A-','B+','B-','AB+','AB-','O+','O-')),
    CONSTRAINT CK_MedicalRecords_HIV            CHECK (hiv_test IN ('positive','negative','inconclusive')),
    CONSTRAINT CK_MedicalRecords_HepB           CHECK (hepatitis_b_result IN ('positive','negative','inconclusive')),
    CONSTRAINT CK_MedicalRecords_HepC           CHECK (hepatitis_c_result IN ('positive','negative','inconclusive')),
    CONSTRAINT CK_MedicalRecords_Syphilis       CHECK (syphilis_result IN ('positive','negative','inconclusive')),
    CONSTRAINT CK_MedicalRecords_Malaria        CHECK (malaria_result IN ('positive','negative','inconclusive')),
    CONSTRAINT CK_MedicalRecords_ScreenStatus   CHECK (screening_status IN ('passed','failed'))
);
GO

-- =============================================
-- TRIGGER: Auto-update updated_at on all tables
-- =============================================

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
    UPDATE Announcements SET updated_at = GETDATE() WHERE id IN (SELECT id FROM inserted);
GO
CREATE TRIGGER trg_Certificates_UpdatedAt ON Certificates AFTER UPDATE AS
    UPDATE Certificates SET updated_at = GETDATE() WHERE id IN (SELECT id FROM inserted);
GO
CREATE TRIGGER trg_BloodInventory_UpdatedAt ON Blood_Inventory AFTER UPDATE AS
    UPDATE Blood_Inventory SET updated_at = GETDATE() WHERE id IN (SELECT id FROM inserted);
GO
CREATE TRIGGER trg_BloodRequests_UpdatedAt ON Blood_Requests AFTER UPDATE AS
    UPDATE Blood_Requests SET updated_at = GETDATE() WHERE id IN (SELECT id FROM inserted);
GO
CREATE TRIGGER trg_Donations_UpdatedAt ON Donations AFTER UPDATE AS
    UPDATE Donations SET updated_at = GETDATE() WHERE id IN (SELECT id FROM inserted);
GO
CREATE TRIGGER trg_DonorRequestMatches_UpdatedAt ON Donor_Request_Matches AFTER UPDATE AS
    UPDATE Donor_Request_Matches SET updated_at = GETDATE() WHERE id IN (SELECT id FROM inserted);
GO
CREATE TRIGGER trg_Appointments_UpdatedAt ON Appointments AFTER UPDATE AS
    UPDATE Appointments SET updated_at = GETDATE() WHERE id IN (SELECT id FROM inserted);
GO
CREATE TRIGGER trg_MedicalRecords_UpdatedAt ON Medical_Records AFTER UPDATE AS
    UPDATE Medical_Records SET updated_at = GETDATE() WHERE id IN (SELECT id FROM inserted);
GO