CREATE OR ALTER PROCEDURE dbo.usp_Admin_RejectDonor
    @DonorId INT,
    @RejectionReason NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;

    IF NULLIF(LTRIM(RTRIM(@RejectionReason)), '') IS NULL
    BEGIN
        THROW 50002, 'Rejection reason is required.', 1;
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.DonorProfiles
        WHERE Id = @DonorId
          AND VerificationStatus = 'Pending'
    )
    BEGIN
        THROW 50003, 'Donor does not exist or is not pending verification.', 1;
    END;

    UPDATE dbo.DonorProfiles
    SET
        IsVerified = 0,
        VerificationStatus = 'Rejected',
        RejectionReason = @RejectionReason,
        IsAvailable = 0
    WHERE Id = @DonorId
      AND VerificationStatus = 'Pending';

    SELECT
        Id AS DonorId,
        UserId,
        IsVerified,
        VerificationStatus,
        RejectionReason,
        IsAvailable
    FROM dbo.DonorProfiles
    WHERE Id = @DonorId;
END;
GO