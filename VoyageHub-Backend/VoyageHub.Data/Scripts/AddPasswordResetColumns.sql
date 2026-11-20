USE AngularAuthDb;
GO

IF COL_LENGTH('dbo.USERS', 'PasswordResetToken') IS NULL
BEGIN
    ALTER TABLE dbo.USERS
    ADD PasswordResetToken NVARCHAR(255) NULL;
END
GO

IF COL_LENGTH('dbo.USERS', 'PasswordResetTokenExpires') IS NULL
BEGIN
    ALTER TABLE dbo.USERS
    ADD PasswordResetTokenExpires DATETIME2 NULL;
END
GO

