CREATE   PROCEDURE sp_RegisterUser
    @Name NVARCHAR(100),
    @Email NVARCHAR(100),
    @Password NVARCHAR(255),
    @RoleId INT,
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    -- Check if email already exists
    IF EXISTS (SELECT 1 FROM Users WHERE Email = @Email)
    BEGIN
        RAISERROR ('Email already exists. Please use a different email.', 16, 1);
        RETURN;
    END

    -- Insert user data
    INSERT INTO Users (Name, Email, PasswordHash, RoleId, IsActive, CreatedOn, CreatedBy)
    VALUES (@Name, @Email, @Password, @RoleId, 1, GETDATE(), @CreatedBy);
END