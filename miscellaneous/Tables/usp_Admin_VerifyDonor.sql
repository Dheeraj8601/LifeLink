CREATE OR ALTER PROCEDURE dbo.usp_Admin_VerifyDonor
    @DonorId INT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.DonorProfiles
        WHERE Id = @DonorId
          AND VerificationStatus = 'Pending'
    )
    BEGIN
        THROW 50001, 'Donor does not exist or is not pending verification.', 1;
    END;

    UPDATE dbo.DonorProfiles
    SET
        IsVerified = 1,
        VerificationStatus = 'Verified',
        RejectionReason = NULL
    WHERE Id = @DonorId
      AND VerificationStatus = 'Pending';

    SELECT
        Id AS DonorId,
        UserId,
        IsVerified,
        VerificationStatus,
        RejectionReason
    FROM dbo.DonorProfiles
    WHERE Id = @DonorId;
END;
GO