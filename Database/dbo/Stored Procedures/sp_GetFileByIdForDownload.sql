CREATE   PROCEDURE sp_GetFileByIdForDownload
    @FileId INT
AS
BEGIN
        SELECT 
        FileContent, 
        FileName, 
        COALESCE(FileType, 'application/octet-stream') AS FileType
    FROM Files
    WHERE FileId = @FileId;
END;