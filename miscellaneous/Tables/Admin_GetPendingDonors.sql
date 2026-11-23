CREATE OR ALTER PROCEDURE dbo.usp_Admin_GetPendingDonors
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        DP.Id AS DonorId,
        U.Id AS UserId,
        U.FullName,
        U.Email,
        U.PhoneNumber,
        DP.BloodGroup,
        C.Name AS City,
        C.State,
        DP.Latitude,
        DP.Longitude,
        DP.IsAvailable,
        DP.IsVerified,
        DP.VerificationStatus,
        DP.RejectionReason,
        DP.CreatedAt AS RegisteredAt
    FROM dbo.DonorProfiles DP
    INNER JOIN dbo.Users U
        ON DP.UserId = U.Id
    INNER JOIN dbo.Cities C
        ON DP.CityId = C.Id
    WHERE
        U.Role = 'Donor'
        AND U.IsActive = 1
        AND DP.VerificationStatus = 'Pending'
    ORDER BY DP.CreatedAt ASC;
END;
GO