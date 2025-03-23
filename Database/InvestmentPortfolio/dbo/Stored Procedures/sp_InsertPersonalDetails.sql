CREATE   PROCEDURE sp_InsertPersonalDetails
    @Name NVARCHAR(100),
    @UniqueClientCode NVARCHAR(50) NULL,
	@Mobile BIGINT NULL,
	@PanNumber BIGINT NULL,
    @PLStatement NVARCHAR(255) NULL,
    @PersonalDetailId INT OUTPUT
AS
BEGIN
    INSERT INTO PersonalDetails (Name, UniqueClientCode,Mobile,PanNumber, PLStatement,IsActive)
    VALUES (@Name, @UniqueClientCode,@Mobile,@PanNumber, @PLStatement,1);
    
    SET @PersonalDetailId = SCOPE_IDENTITY();
END