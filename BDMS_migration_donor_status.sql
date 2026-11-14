-- Migration: Donors.status (pending | approved), for DBs created before this feature.
-- Existing donor rows are already active, so backfill 'approved'.
-- Run once against the target database (local + deployed).

IF COL_LENGTH('Donors', 'status') IS NULL
BEGIN
    ALTER TABLE Donors ADD status NVARCHAR(20) NOT NULL DEFAULT 'approved';
END
GO

IF COL_LENGTH('Donors', 'status') IS NOT NULL AND OBJECT_ID('CK_Donors_Status') IS NULL
BEGIN
    ALTER TABLE Donors ADD CONSTRAINT CK_Donors_Status CHECK (status IN ('pending','approved'));
END
GO
