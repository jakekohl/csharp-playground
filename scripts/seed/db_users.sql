DECLARE @UserCount int = (SELECT COUNT(*) FROM Users);

IF @UserCount = 0
BEGIN
INSERT INTO Users (DisplayName, Email, InActive, CreatedBy, LastModified, LastModifiedBy)
VALUES ('Jake Kohl', 'jake.kohl@test.com', NULL, 1, GETDATE(), 1),
('John Doe', 'john.doe@test.com', GETDATE(), 1, GETDATE(), 1),
('Jane Smith', 'jane.smith@test.com', NULL, 1, GETDATE(), 1),
('Alice Johnson', 'alice.johnson@test.com', NULL, 1, GETDATE(), 1),
('Bob Brown', 'bob.brown@test.com', NULL, 1, GETDATE(), 1);
END
GO