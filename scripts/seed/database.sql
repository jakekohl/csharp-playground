-- Users table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE [dbo].[Users] (
    [UserID]         INT                IDENTITY (1, 1) NOT NULL,
    [Created]        DATETIMEOFFSET (7) CONSTRAINT [DEFAULT_Users_Created] DEFAULT (getdate()) NOT NULL,
    [CreatedBy]      INT                NULL,
    [LastModified]   DATETIMEOFFSET (7) NULL,
    [LastModifiedBy] INT                NULL,
    [DisplayName]    VARCHAR (50)       NULL,
    [Email]          VARCHAR (50)       NULL,
    [InActive]       DATETIMEOFFSET (7) NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([UserID] ASC)
);
END;