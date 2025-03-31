CREATE   PROCEDURE sp_InsertFile
    @FileName NVARCHAR(255),
    @FileContent VARBINARY(MAX),
	@FileType NVARCHAR(100),
	@FileTypeId int
AS
BEGIN
    INSERT INTO Files (FileName, FileContent,FileType, UploadDate,FileTypeId)
    VALUES (@FileName, @FileContent,@FileType, GETDATE(),@FileTypeId);
END;