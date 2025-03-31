CREATE   PROCEDURE sp_GetFileForView
    @FileId INT
AS
BEGIN
    SET NOCOUNT ON;

        SELECT 
        CAST(FileContent AS VARBINARY(MAX)) AS FileContent, 
        CAST(FileType AS NVARCHAR(100)) AS FileType
    FROM Files
    WHERE FileId = @FileId;
END;