USE LifeLinkDb;
GO

DECLARE @FullName NVARCHAR(150) = 'Your Name';
DECLARE @Email NVARCHAR(256) = 'your-admin-email@example.com';
DECLARE @PasswordHash NVARCHAR(500) = 'PASTE_YOUR_GENERATED_HASH_HERE';
DECLARE @PhoneNumber NVARCHAR(20) = 'YOUR_PHONE_NUMBER';

IF EXISTS (SELECT 1 FROM Users WHERE Email = @Email)
BEGIN
    PRINT 'User with this email already exists.';
END
ELSE
BEGIN
    INSERT INTO Users
    (
        FullName,
        Email,
        PasswordHash,
        PhoneNumber,
        Role,
        IsActive
    )
    VALUES
    (
        @FullName,
        @Email,
        @PasswordHash,
        @PhoneNumber,
        'Admin',
        1
    );

    PRINT 'Admin account created successfully.';
END;
GO