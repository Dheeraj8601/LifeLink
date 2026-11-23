USE LifeLinkDb;
GO

/* Add VerificationStatus */
IF COL_LENGTH('dbo.DonorProfiles', 'VerificationStatus') IS NULL
BEGIN
    ALTER TABLE dbo.DonorProfiles
    ADD VerificationStatus NVARCHAR(20) NOT NULL
        CONSTRAINT DF_DonorProfiles_VerificationStatus
        DEFAULT 'Pending';
END
GO

/* Add RejectionReason */
IF COL_LENGTH('dbo.DonorProfiles', 'RejectionReason') IS NULL
BEGIN
    ALTER TABLE dbo.DonorProfiles
    ADD RejectionReason NVARCHAR(500) NULL;
END
GO

/* Synchronize existing data */
UPDATE dbo.DonorProfiles
SET VerificationStatus =
    CASE
        WHEN IsVerified = 1 THEN 'Verified'
        ELSE 'Pending'
    END;
GO

/* Add allowed-status constraint */
IF NOT EXISTS
(
    SELECT 1
    FROM sys.check_constraints
    WHERE name = 'CK_DonorProfiles_VerificationStatus'
)
BEGIN
    ALTER TABLE dbo.DonorProfiles
    ADD CONSTRAINT CK_DonorProfiles_VerificationStatus
    CHECK
    (
        VerificationStatus IN ('Pending', 'Verified', 'Rejected')
    );
END
GO