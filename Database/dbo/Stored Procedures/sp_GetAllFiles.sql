CREATE   PROCEDURE sp_GetAllFiles
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        f.FileId, 
        f.FileName, 
        ft.FileTypeName, 
        f.FileContent, 
        f.UploadDate 
    FROM Files f
    LEFT JOIN FileTypes ft ON f.FileTypeId = ft.FileTypeId
    ORDER BY f.UploadDate DESC;
END;