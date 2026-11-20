-- ====================================================================
-- VoyageHub: Enterprise Corporate Travel & Expense Management Database
-- Database: AngularAuthDb
-- ====================================================================

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'AngularAuthDb')
BEGIN
    CREATE DATABASE AngularAuthDb;
END
GO

USE AngularAuthDb;
GO

-- 1. Drop existing VoyageHub tables in reverse foreign key order if needed
IF OBJECT_ID('dbo.SUPPORT_TICKETS', 'U') IS NOT NULL DROP TABLE dbo.SUPPORT_TICKETS;
IF OBJECT_ID('dbo.AUDIT_LOGS', 'U') IS NOT NULL DROP TABLE dbo.AUDIT_LOGS;
IF OBJECT_ID('dbo.EXPENSES', 'U') IS NOT NULL DROP TABLE dbo.EXPENSES;
IF OBJECT_ID('dbo.STAYS', 'U') IS NOT NULL DROP TABLE dbo.STAYS;
IF OBJECT_ID('dbo.ITINERARY_SEGMENTS', 'U') IS NOT NULL DROP TABLE dbo.ITINERARY_SEGMENTS;
IF OBJECT_ID('dbo.APPROVAL_STEPS', 'U') IS NOT NULL DROP TABLE dbo.APPROVAL_STEPS;
IF OBJECT_ID('dbo.TRAVEL_REQUESTS', 'U') IS NOT NULL DROP TABLE dbo.TRAVEL_REQUESTS;
IF OBJECT_ID('dbo.TRAVEL_POLICIES', 'U') IS NOT NULL DROP TABLE dbo.TRAVEL_POLICIES;
IF OBJECT_ID('dbo.USERS', 'U') IS NOT NULL DROP TABLE dbo.USERS;
IF OBJECT_ID('dbo.DEPARTMENTS', 'U') IS NOT NULL DROP TABLE dbo.DEPARTMENTS;
GO

-- 2. Create DEPARTMENTS Table
CREATE TABLE dbo.DEPARTMENTS (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(150) NOT NULL UNIQUE
);
GO

-- 3. Create USERS Table
CREATE TABLE dbo.USERS (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(150) NOT NULL,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(500) NOT NULL,
    Role NVARCHAR(50) NOT NULL DEFAULT 'Employee', -- Employee, Manager, Finance, Admin
    DepartmentId UNIQUEIDENTIFIER NULL,
    ManagerId UNIQUEIDENTIFIER NULL,
    AvatarUrl NVARCHAR(500) NULL,
    PasswordResetToken NVARCHAR(255) NULL,
    PasswordResetTokenExpires DATETIME2 NULL,
    CreatedAt DATETIME2 DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_USERS_DEPARTMENTS FOREIGN KEY (DepartmentId) REFERENCES dbo.DEPARTMENTS(Id) ON DELETE SET NULL,
    CONSTRAINT FK_USERS_MANAGERS FOREIGN KEY (ManagerId) REFERENCES dbo.USERS(Id)
);
GO

-- 4. Create TRAVEL_POLICIES Table
CREATE TABLE dbo.TRAVEL_POLICIES (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    DepartmentId UNIQUEIDENTIFIER NOT NULL UNIQUE,
    MaxBudget DECIMAL(18,2) NOT NULL DEFAULT 3000.00,
    IntlBudgetLimit DECIMAL(18,2) NOT NULL DEFAULT 3000.00,
    DomesticLimit DECIMAL(18,2) NOT NULL DEFAULT 800.00,
    AllowedClass NVARCHAR(50) NOT NULL DEFAULT 'ECONOMY+',
    HardEnforcement BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_TRAVEL_POLICIES_DEPARTMENTS FOREIGN KEY (DepartmentId) REFERENCES dbo.DEPARTMENTS(Id) ON DELETE CASCADE
);
GO

-- 5. Create TRAVEL_REQUESTS Table
CREATE TABLE dbo.TRAVEL_REQUESTS (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    RequestCode NVARCHAR(50) NOT NULL UNIQUE, -- e.g. #TR-2048
    EmployeeId UNIQUEIDENTIFIER NOT NULL,
    Destination NVARCHAR(200) NOT NULL,
    Purpose NVARCHAR(500) NOT NULL,
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    EstimatedBudget DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    TravelClass NVARCHAR(50) NOT NULL DEFAULT 'Economy (Standard)',
    Justification NVARCHAR(MAX) NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Draft', -- Draft, In Review, Manager Review, Finance Review, Approved, Completed, Rejected
    PolicyCompliance NVARCHAR(50) NOT NULL DEFAULT 'Standard', -- Standard, Flagged
    CreatedAt DATETIME2 DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_TRAVEL_REQUESTS_USERS FOREIGN KEY (EmployeeId) REFERENCES dbo.USERS(Id) ON DELETE CASCADE
);
GO

-- 6. Create APPROVAL_STEPS Table
CREATE TABLE dbo.APPROVAL_STEPS (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    RequestId UNIQUEIDENTIFIER NOT NULL,
    ApproverId UNIQUEIDENTIFIER NULL,
    Level NVARCHAR(50) NOT NULL, -- Manager, Finance
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending', -- Pending, Approved, Rejected
    Comment NVARCHAR(MAX) NULL,
    DecidedAt DATETIME2 NULL,
    CONSTRAINT FK_APPROVAL_STEPS_REQUESTS FOREIGN KEY (RequestId) REFERENCES dbo.TRAVEL_REQUESTS(Id) ON DELETE CASCADE,
    CONSTRAINT FK_APPROVAL_STEPS_USERS FOREIGN KEY (ApproverId) REFERENCES dbo.USERS(Id)
);
GO

-- 7. Create ITINERARY_SEGMENTS Table
CREATE TABLE dbo.ITINERARY_SEGMENTS (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    RequestId UNIQUEIDENTIFIER NOT NULL,
    Mode NVARCHAR(50) NOT NULL DEFAULT 'Flight', -- Flight, Train, Car
    Carrier NVARCHAR(150) NULL, -- e.g. LUFTHANSA LH123
    Origin NVARCHAR(100) NOT NULL,
    Destination NVARCHAR(100) NOT NULL,
    DepartureTime DATETIME2 NOT NULL,
    ArrivalTime DATETIME2 NULL,
    ConfirmationNumber NVARCHAR(100) NULL,
    Cost DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    CONSTRAINT FK_ITINERARY_SEGMENTS_REQUESTS FOREIGN KEY (RequestId) REFERENCES dbo.TRAVEL_REQUESTS(Id) ON DELETE CASCADE
);
GO

-- 8. Create STAYS Table
CREATE TABLE dbo.STAYS (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    RequestId UNIQUEIDENTIFIER NOT NULL,
    HotelName NVARCHAR(200) NOT NULL,
    CheckIn DATE NOT NULL,
    CheckOut DATE NOT NULL,
    Nights INT NOT NULL DEFAULT 1,
    ConfirmationNumber NVARCHAR(100) NULL,
    Cost DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    CONSTRAINT FK_STAYS_REQUESTS FOREIGN KEY (RequestId) REFERENCES dbo.TRAVEL_REQUESTS(Id) ON DELETE CASCADE
);
GO

-- 9. Create EXPENSES Table
CREATE TABLE dbo.EXPENSES (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    RequestId UNIQUEIDENTIFIER NOT NULL,
    Category NVARCHAR(50) NOT NULL, -- Airfare, Hotel, Meals, Transport
    Amount DECIMAL(18,2) NOT NULL,
    ProofPath NVARCHAR(500) NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending', -- Pending, Approved, Rejected
    SubmittedAt DATETIME2 DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_EXPENSES_REQUESTS FOREIGN KEY (RequestId) REFERENCES dbo.TRAVEL_REQUESTS(Id) ON DELETE CASCADE
);
GO

-- 10. Create AUDIT_LOGS Table
CREATE TABLE dbo.AUDIT_LOGS (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    EntityType NVARCHAR(50) NOT NULL, -- POLICY, APPROVAL, ACCESS, REQUEST
    EntityId UNIQUEIDENTIFIER NULL,
    Action NVARCHAR(100) NOT NULL,
    ActorId UNIQUEIDENTIFIER NULL,
    ActorName NVARCHAR(150) NOT NULL,
    ChangeDetail NVARCHAR(MAX) NOT NULL,
    Severity NVARCHAR(20) NOT NULL DEFAULT 'LOW', -- LOW, MEDIUM, HIGH
    Timestamp DATETIME2 DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_AUDIT_LOGS_USERS FOREIGN KEY (ActorId) REFERENCES dbo.USERS(Id) ON DELETE SET NULL
);
GO

-- 11. Create SUPPORT_TICKETS Table
IF OBJECT_ID('dbo.SUPPORT_TICKETS', 'U') IS NULL
CREATE TABLE dbo.SUPPORT_TICKETS (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TicketCode NVARCHAR(50) NOT NULL UNIQUE,
    Category NVARCHAR(100) NOT NULL,
    Priority NVARCHAR(50) NOT NULL DEFAULT 'Medium',
    RequestCode NVARCHAR(50) NULL,
    RequestId UNIQUEIDENTIFIER NULL,
    Subject NVARCHAR(300) NOT NULL,
    Description NVARCHAR(MAX) NOT NULL,
    CallbackNumber NVARCHAR(50) NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Open',
    AssignedAgent NVARCHAR(150) NULL DEFAULT 'Meera Sen (Corporate Travel Desk)',
    ResolutionNotes NVARCHAR(MAX) NULL,
    UserId UNIQUEIDENTIFIER NULL,
    CreatedAt DATETIME2 DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_SUPPORT_TICKETS_USERS FOREIGN KEY (UserId) REFERENCES dbo.USERS(Id) ON DELETE SET NULL,
    CONSTRAINT FK_SUPPORT_TICKETS_REQUESTS FOREIGN KEY (RequestId) REFERENCES dbo.TRAVEL_REQUESTS(Id) ON DELETE NO ACTION
);
GO

-- ====================================================================
-- SEED DATA (Aligning with VoyageHub Mock UI Templates)
-- ====================================================================

DECLARE @DeptSales UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';
DECLARE @DeptEng UNIQUEIDENTIFIER   = '22222222-2222-2222-2222-222222222222';
DECLARE @DeptHR UNIQUEIDENTIFIER    = '33333333-3333-3333-3333-333333333333';
DECLARE @DeptExec UNIQUEIDENTIFIER  = '44444444-4444-4444-4444-444444444444';

INSERT INTO dbo.DEPARTMENTS (Id, Name) VALUES
(@DeptSales, 'Sales & Marketing'),
(@DeptEng, 'Engineering'),
(@DeptHR, 'Human Resources'),
(@DeptExec, 'Executive Leadership');

-- Policies for departments
INSERT INTO dbo.TRAVEL_POLICIES (Id, DepartmentId, MaxBudget, IntlBudgetLimit, DomesticLimit, AllowedClass, HardEnforcement) VALUES
(NEWID(), @DeptSales, 3000.00, 3000.00, 800.00, 'ECONOMY+', 1),
(NEWID(), @DeptExec, 99999.00, 99999.00, 2500.00, 'BUSINESS', 1),
(NEWID(), @DeptEng, 3500.00, 3500.00, 1000.00, 'ECONOMY+', 1),
(NEWID(), @DeptHR, 2000.00, 2000.00, 600.00, 'ECONOMY', 1);

-- Demo user BCrypt hash (VoyageDemo2026!)
DECLARE @DefaultHash NVARCHAR(500) = '$2a$11$MPV/7YdSwWOjQgfArQTqcecQGYvcVeEdn.o5fpUjos8hyeQhxfV/6';

DECLARE @UserAlex UNIQUEIDENTIFIER   = 'A1111111-AAAA-AAAA-AAAA-AAAAAAAAAAAA';
DECLARE @UserMarcus UNIQUEIDENTIFIER = 'B2222222-BBBB-BBBB-BBBB-BBBBBBBBBBBB';
DECLARE @UserDavid UNIQUEIDENTIFIER  = 'C3333333-CCCC-CCCC-CCCC-CCCCCCCCCCCC';
DECLARE @UserSarah UNIQUEIDENTIFIER  = 'D4444444-DDDD-DDDD-DDDD-DDDDDDDDDDDD';
DECLARE @UserLisa UNIQUEIDENTIFIER   = 'E5555555-EEEE-EEEE-EEEE-EEEEEEEEEEEE';
DECLARE @UserAdmin UNIQUEIDENTIFIER  = 'F6666666-FFFF-FFFF-FFFF-FFFFFFFFFFFF';

-- Insert Users (Rohan reports to Priya)
INSERT INTO dbo.USERS (Id, Name, Email, PasswordHash, Role, DepartmentId, ManagerId, AvatarUrl) VALUES
(@UserMarcus, 'Priya Nair', 'priya.nair@voyagehub.in', @DefaultHash, 'Manager', @DeptSales, NULL, 'https://images.unsplash.com/photo-1573496359142-b8d87734a5a2?auto=format&fit=crop&w=120&h=120&q=80'),
(@UserDavid, 'Vikram Malhotra', 'vikram.malhotra@voyagehub.in', @DefaultHash, 'Finance', @DeptSales, NULL, 'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?auto=format&fit=crop&w=120&h=120&q=80'),
(@UserAdmin, 'Ananya Verma', 'admin@voyagehub.in', @DefaultHash, 'Admin', @DeptExec, NULL, 'https://images.unsplash.com/photo-1580489944761-15a19d654956?auto=format&fit=crop&w=120&h=120&q=80'),
(@UserAlex, 'Rohan Sharma', 'rohan.sharma@voyagehub.in', @DefaultHash, 'Employee', @DeptSales, @UserMarcus, 'https://images.unsplash.com/photo-1506794778202-cad84cf45f1d?auto=format&fit=crop&w=120&h=120&q=80'),
(@UserSarah, 'Sarah Chen', 'sarah@voyagehub.io', @DefaultHash, 'Employee', @DeptEng, @UserMarcus, 'https://images.unsplash.com/photo-1494790108377-be9c29b29330?auto=format&fit=crop&w=120&h=120&q=80'),
(@UserLisa, 'Lisa Ray', 'lisa@voyagehub.io', @DefaultHash, 'Employee', @DeptSales, @UserMarcus, 'https://images.unsplash.com/photo-1534528741775-53994a69daeb?auto=format&fit=crop&w=120&h=120&q=80');

-- Insert Travel Requests
DECLARE @ReqBerlin UNIQUEIDENTIFIER = 'AA010101-0000-0000-0000-000000000001';
DECLARE @ReqLisbon UNIQUEIDENTIFIER = 'AA020202-0000-0000-0000-000000000002';
DECLARE @ReqAustin UNIQUEIDENTIFIER = 'AA030303-0000-0000-0000-000000000003';
DECLARE @ReqMadrid UNIQUEIDENTIFIER = 'AA040404-0000-0000-0000-000000000004';
DECLARE @ReqCopen  UNIQUEIDENTIFIER = 'AA050505-0000-0000-0000-000000000005';
DECLARE @ReqSF     UNIQUEIDENTIFIER = 'AA060606-0000-0000-0000-000000000006';
DECLARE @ReqNYC    UNIQUEIDENTIFIER = 'AA070707-0000-0000-0000-000000000007';

INSERT INTO dbo.TRAVEL_REQUESTS (Id, RequestCode, EmployeeId, Destination, Purpose, StartDate, EndDate, EstimatedBudget, TravelClass, Justification, Status, PolicyCompliance, CreatedAt) VALUES
(@ReqBerlin, '#TR-2048', @UserAlex, 'Berlin, Germany', 'Q3 Sales Conference', '2024-06-18', '2024-06-22', 2410.00, 'Economy (Standard)', 'Annual European partner summit and client alignment sessions.', 'In Review', 'Flagged', '2024-06-01T10:00:00'),
(@ReqLisbon, '#TR-2041', @UserAlex, 'Lisbon, Portugal', 'Tech Summit 2024', '2024-07-02', '2024-07-09', 1860.00, 'Economy (Standard)', 'Presenting keynote and customer networking.', 'Approved', 'Standard', '2024-05-15T09:30:00'),
(@ReqAustin, '#TR-2035', @UserAlex, 'Austin, TX', 'Client Workshop', '2024-05-10', '2024-05-12', 640.00, 'Economy (Standard)', 'Onsite strategic client workshop.', 'Completed', 'Standard', '2024-05-01T14:15:00'),
(@ReqMadrid, '#TR-2029', @UserAlex, 'Madrid, Spain', 'Regional Training', '2024-04-11', '2024-04-15', 2120.00, 'Economy (Standard)', 'EMEA cross-functional team training.', 'Rejected', 'Standard', '2024-04-01T11:00:00'),
(@ReqCopen,  '#TR-2022', @UserAlex, 'Copenhagen, DK', 'Nordic Client Review', '2024-03-04', '2024-03-08', 1940.00, 'Economy (Standard)', 'Quarterly business review with Nordic accounts.', 'Draft', 'Standard', '2024-02-20T08:00:00'),
(@ReqSF,     '#TR-2055', @UserSarah, 'San Francisco, CA', 'AWS Re:Invent', '2024-08-12', '2024-08-16', 3150.00, 'Economy+', 'Cloud architecture training and vendor summit.', 'In Review', 'Standard', '2024-06-03T16:20:00'),
(@ReqNYC,    '#TR-2031', @UserLisa, 'New York, NY', 'Product Launch NYC', '2024-05-20', '2024-05-24', 980.00, 'Economy (Standard)', 'Media press tour and flagship launch.', 'Approved', 'Standard', '2024-05-10T12:00:00');

-- Approval Steps for #TR-2048 (Berlin)
INSERT INTO dbo.APPROVAL_STEPS (Id, RequestId, ApproverId, Level, Status, Comment, DecidedAt) VALUES
(NEWID(), @ReqBerlin, @UserMarcus, 'Manager', 'Pending', NULL, NULL),
(NEWID(), @ReqBerlin, @UserDavid, 'Finance', 'Pending', NULL, NULL);

-- Approval Steps for #TR-2041 (Lisbon - Approved)
INSERT INTO dbo.APPROVAL_STEPS (Id, RequestId, ApproverId, Level, Status, Comment, DecidedAt) VALUES
(NEWID(), @ReqLisbon, @UserMarcus, 'Manager', 'Approved', 'Approved for team expansion.', '2024-05-16T11:00:00'),
(NEWID(), @ReqLisbon, @UserDavid, 'Finance', 'Approved', 'Budget verified against Q2 budget.', '2024-05-17T14:30:00');

-- Approval Steps for #TR-2055 (SF - Pending)
INSERT INTO dbo.APPROVAL_STEPS (Id, RequestId, ApproverId, Level, Status, Comment, DecidedAt) VALUES
(NEWID(), @ReqSF, @UserMarcus, 'Manager', 'Pending', NULL, NULL);

-- Itinerary Segments for #TR-2048 (Berlin)
INSERT INTO dbo.ITINERARY_SEGMENTS (Id, RequestId, Mode, Carrier, Origin, Destination, DepartureTime, ArrivalTime, ConfirmationNumber, Cost) VALUES
(NEWID(), @ReqBerlin, 'Flight', 'LUFTHANSA LH123', 'LHR', 'BER', '2024-06-18T09:45:00', '2024-06-18T12:30:00', '#AJK29L', 420.00),
(NEWID(), @ReqBerlin, 'Flight', 'LUFTHANSA LH124', 'BER', 'LHR', '2024-06-22T17:15:00', '2024-06-22T20:00:00', '#AJK29L', 380.00);

-- Stays for #TR-2048 (Berlin)
INSERT INTO dbo.STAYS (Id, RequestId, HotelName, CheckIn, CheckOut, Nights, ConfirmationNumber, Cost) VALUES
(NEWID(), @ReqBerlin, 'Grand Hyatt Berlin', '2024-06-18', '2024-06-22', 4, '#HY-202931', 1240.00);

-- Expenses for #TR-2048 & #TR-2031
INSERT INTO dbo.EXPENSES (Id, RequestId, Category, Amount, ProofPath, Status, SubmittedAt) VALUES
(NEWID(), @ReqBerlin, 'Airfare', 1240.00, 'receipt_berlin_airfare.pdf', 'Pending', '2024-06-23T09:00:00'),
(NEWID(), @ReqNYC,    'Meals',   85.20,   'receipt_nyc_dinner.png',     'Pending', '2024-05-25T19:30:00'),
(NEWID(), @ReqAustin, 'Transport', 145.00, 'receipt_uber_austin.pdf',   'Approved', '2024-05-13T10:00:00');

-- Audit Logs (Matching PDF Page 10)
INSERT INTO dbo.AUDIT_LOGS (Id, EntityType, EntityId, Action, ActorId, ActorName, ChangeDetail, Severity, Timestamp) VALUES
(NEWID(), 'POLICY', NULL, 'Update Limit', @UserAdmin, 'Admin User', 'Sales Intl. Limit: $2,500 -> $3,000', 'LOW', '2024-06-05T14:22:10'),
(NEWID(), 'APPROVAL', @ReqBerlin, 'Override Applied', @UserMarcus, 'M. Reynolds', 'Force-approved #TR-2048 (Policy Exception)', 'MEDIUM', '2024-06-05T12:45:01'),
(NEWID(), 'ACCESS', NULL, 'Failed Login', NULL, '192.168.1.42', '3 consecutive failures for alex@voyagehub.io', 'HIGH', '2024-06-04T09:15:22');
GO

PRINT 'VoyageHub Schema & Seed Data populated successfully!';
GO
