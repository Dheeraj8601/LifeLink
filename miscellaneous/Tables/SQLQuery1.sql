
/* ==========================================
   LifeLink Database
   SQL Server - Database First
   ========================================== */

IF DB_ID('LifeLinkDb') IS NULL
BEGIN
    CREATE DATABASE LifeLinkDb;
END
GO

USE LifeLinkDb;
GO

/* ==========================================
   1. Cities
   ========================================== */

CREATE TABLE Cities
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    State NVARCHAR(100) NOT NULL,

    CONSTRAINT UQ_Cities_Name_State
        UNIQUE (Name, State)
);
GO

/* ==========================================
   2. Users
   ========================================== */

CREATE TABLE Users
(
    Id INT IDENTITY(1,1) PRIMARY KEY,

    FullName NVARCHAR(150) NOT NULL,

    Email NVARCHAR(256) NOT NULL,

    PasswordHash NVARCHAR(500) NOT NULL,

    PhoneNumber NVARCHAR(20) NOT NULL,

    Role NVARCHAR(20) NOT NULL,

    IsActive BIT NOT NULL
        CONSTRAINT DF_Users_IsActive DEFAULT 1,

    CreatedAt DATETIME2 NOT NULL
        CONSTRAINT DF_Users_CreatedAt
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT UQ_Users_Email
        UNIQUE (Email),

    CONSTRAINT CK_Users_Role
        CHECK (Role IN ('Donor', 'Requester', 'Admin'))
);
GO

/* ==========================================
   3. DonorProfiles
   ========================================== */

CREATE TABLE DonorProfiles
(
    Id INT IDENTITY(1,1) PRIMARY KEY,

    UserId INT NOT NULL,

    CityId INT NOT NULL,

    BloodGroup VARCHAR(3) NOT NULL,

    Latitude DECIMAL(9,6) NULL,

    Longitude DECIMAL(9,6) NULL,

    IsAvailable BIT NOT NULL
        CONSTRAINT DF_DonorProfiles_IsAvailable
        DEFAULT 1,

    IsVerified BIT NOT NULL
        CONSTRAINT DF_DonorProfiles_IsVerified
        DEFAULT 0,

    LastDonationDate DATE NULL,

    CreatedAt DATETIME2 NOT NULL
        CONSTRAINT DF_DonorProfiles_CreatedAt
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_DonorProfiles_Users
        FOREIGN KEY (UserId)
        REFERENCES Users(Id),

    CONSTRAINT FK_DonorProfiles_Cities
        FOREIGN KEY (CityId)
        REFERENCES Cities(Id),

    CONSTRAINT UQ_DonorProfiles_UserId
        UNIQUE (UserId),

    CONSTRAINT CK_DonorProfiles_BloodGroup
        CHECK
        (
            BloodGroup IN
            (
                'A+', 'A-',
                'B+', 'B-',
                'AB+', 'AB-',
                'O+', 'O-'
            )
        ),

    CONSTRAINT CK_DonorProfiles_Latitude
        CHECK (Latitude IS NULL OR Latitude BETWEEN -90 AND 90),

    CONSTRAINT CK_DonorProfiles_Longitude
        CHECK (Longitude IS NULL OR Longitude BETWEEN -180 AND 180)
);
GO

/* ==========================================
   4. BloodRequests
   ========================================== */

CREATE TABLE BloodRequests
(
    Id INT IDENTITY(1,1) PRIMARY KEY,

    RequesterId INT NOT NULL,

    CityId INT NOT NULL,

    BloodGroup VARCHAR(3) NOT NULL,

    HospitalName NVARCHAR(200) NOT NULL,

    HospitalAddress NVARCHAR(500) NOT NULL,

    Latitude DECIMAL(9,6) NULL,

    Longitude DECIMAL(9,6) NULL,

    UnitsRequired INT NOT NULL,

    Urgency NVARCHAR(20) NOT NULL,

    Description NVARCHAR(1000) NULL,

    Status NVARCHAR(30) NOT NULL
        CONSTRAINT DF_BloodRequests_Status
        DEFAULT 'Open',

    CreatedAt DATETIME2 NOT NULL
        CONSTRAINT DF_BloodRequests_CreatedAt
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_BloodRequests_Users
        FOREIGN KEY (RequesterId)
        REFERENCES Users(Id),

    CONSTRAINT FK_BloodRequests_Cities
        FOREIGN KEY (CityId)
        REFERENCES Cities(Id),

    CONSTRAINT CK_BloodRequests_BloodGroup
        CHECK
        (
            BloodGroup IN
            (
                'A+', 'A-',
                'B+', 'B-',
                'AB+', 'AB-',
                'O+', 'O-'
            )
        ),

    CONSTRAINT CK_BloodRequests_Units
        CHECK (UnitsRequired > 0),

    CONSTRAINT CK_BloodRequests_Urgency
        CHECK (Urgency IN ('Normal', 'Urgent', 'Critical')),

    CONSTRAINT CK_BloodRequests_Status
        CHECK
        (
            Status IN
            (
                'Open',
                'PartiallyFulfilled',
                'Fulfilled',
                'Cancelled'
            )
        ),

    CONSTRAINT CK_BloodRequests_Latitude
        CHECK (Latitude IS NULL OR Latitude BETWEEN -90 AND 90),

    CONSTRAINT CK_BloodRequests_Longitude
        CHECK (Longitude IS NULL OR Longitude BETWEEN -180 AND 180)
);
GO

/* ==========================================
   5. RequestResponses
   ========================================== */

CREATE TABLE RequestResponses
(
    Id INT IDENTITY(1,1) PRIMARY KEY,

    BloodRequestId INT NOT NULL,

    DonorId INT NOT NULL,

    Status NVARCHAR(20) NOT NULL
        CONSTRAINT DF_RequestResponses_Status
        DEFAULT 'Pending',

    RespondedAt DATETIME2 NOT NULL
        CONSTRAINT DF_RequestResponses_RespondedAt
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_RequestResponses_BloodRequests
        FOREIGN KEY (BloodRequestId)
        REFERENCES BloodRequests(Id),

    CONSTRAINT FK_RequestResponses_DonorProfiles
        FOREIGN KEY (DonorId)
        REFERENCES DonorProfiles(Id),

    CONSTRAINT CK_RequestResponses_Status
        CHECK (Status IN ('Pending', 'Accepted', 'Rejected', 'Withdrawn')),

    CONSTRAINT UQ_RequestResponses_Request_Donor
        UNIQUE (BloodRequestId, DonorId)
);
GO

/* ==========================================
   6. Notifications
   ========================================== */

CREATE TABLE Notifications
(
    Id INT IDENTITY(1,1) PRIMARY KEY,

    UserId INT NOT NULL,

    BloodRequestId INT NULL,

    Title NVARCHAR(200) NOT NULL,

    Message NVARCHAR(1000) NOT NULL,

    IsRead BIT NOT NULL
        CONSTRAINT DF_Notifications_IsRead
        DEFAULT 0,

    CreatedAt DATETIME2 NOT NULL
        CONSTRAINT DF_Notifications_CreatedAt
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_Notifications_Users
        FOREIGN KEY (UserId)
        REFERENCES Users(Id),

    CONSTRAINT FK_Notifications_BloodRequests
        FOREIGN KEY (BloodRequestId)
        REFERENCES BloodRequests(Id)
);
GO

/* ==========================================
   7. AIInteractionLogs
   ========================================== */

CREATE TABLE AIInteractionLogs
(
    Id INT IDENTITY(1,1) PRIMARY KEY,

    UserId INT NOT NULL,

    FeatureName NVARCHAR(100) NOT NULL,

    ModelName NVARCHAR(100) NOT NULL,

    Status NVARCHAR(20) NOT NULL,

    DurationMs INT NULL,

    CreatedAt DATETIME2 NOT NULL
        CONSTRAINT DF_AIInteractionLogs_CreatedAt
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_AIInteractionLogs_Users
        FOREIGN KEY (UserId)
        REFERENCES Users(Id),

    CONSTRAINT CK_AIInteractionLogs_Status
        CHECK (Status IN ('Success', 'Failed')),

    CONSTRAINT CK_AIInteractionLogs_Duration
        CHECK (DurationMs IS NULL OR DurationMs >= 0)
);
GO

/* ==========================================
   8. Indexes
   ========================================== */

CREATE INDEX IX_DonorProfiles_BloodGroup_City_Availability
ON DonorProfiles(BloodGroup, CityId, IsAvailable, IsVerified);
GO

CREATE INDEX IX_BloodRequests_Status_City
ON BloodRequests(Status, CityId);
GO

CREATE INDEX IX_RequestResponses_BloodRequestId
ON RequestResponses(BloodRequestId);
GO

CREATE INDEX IX_Notifications_UserId_IsRead
ON Notifications(UserId, IsRead);
GO

CREATE INDEX IX_AIInteractionLogs_UserId_CreatedAt
ON AIInteractionLogs(UserId, CreatedAt DESC);
GO

/* ==========================================
   9. Seed Cities
   ========================================== */

INSERT INTO Cities (Name, State)
VALUES
    ('Udupi', 'Karnataka'),
    ('Mangaluru', 'Karnataka'),
    ('Bengaluru', 'Karnataka'),
    ('Mysuru', 'Karnataka'),
    ('Hubballi', 'Karnataka'),
    ('Shivamogga', 'Karnataka'),
    ('Chikkaballapur', 'Karnataka');
GO

/* ==========================================
   10. Verify
   ========================================== */

SELECT * FROM Cities;

SELECT
    TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;
GO
