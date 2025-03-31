CREATE PROCEDURE sp_ValidateUser
    @Email NVARCHAR(100),
    @Password NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM Users WHERE Email = @Email AND PasswordHash = @Password AND IsActive = 1)
        SELECT 1 AS IsValidUser;
    ELSE
        SELECT 0 AS IsValidUser;
END