CREATE TABLE [dbo].[FileTypes] (
    [FileTypeId]   INT            IDENTITY (1, 1) NOT NULL,
    [FileTypeName] NVARCHAR (100) NOT NULL,
    [IsActive]     BIT            NULL,
    PRIMARY KEY CLUSTERED ([FileTypeId] ASC),
    UNIQUE NONCLUSTERED ([FileTypeName] ASC)
);

