CREATE TABLE [dbo].[Files] (
    [FileId]      INT             IDENTITY (1, 1) NOT NULL,
    [FileName]    NVARCHAR (255)  NOT NULL,
    [FileContent] VARBINARY (MAX) NOT NULL,
    [UploadDate]  DATETIME        DEFAULT (getdate()) NOT NULL,
    [FileTypeId]  INT             NOT NULL,
    [FileType]    NVARCHAR (100)  NULL,
    PRIMARY KEY CLUSTERED ([FileId] ASC),
    FOREIGN KEY ([FileTypeId]) REFERENCES [dbo].[FileTypes] ([FileTypeId])
);

