CREATE PROCEDURE sp_GetFileById
    @Id INT
AS
BEGIN
    SELECT FileContent FROM Files WHERE FileId = @Id;
END;