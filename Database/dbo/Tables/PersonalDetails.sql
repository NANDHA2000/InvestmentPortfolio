CREATE TABLE [dbo].[PersonalDetails] (
    [PersonalDetailsId] INT            IDENTITY (1, 1) NOT NULL,
    [Name]              NVARCHAR (100) NOT NULL,
    [UniqueClientCode]  NVARCHAR (20)  NULL,
    [Mobile]            BIGINT         NULL,
    [PanNumber]         BIGINT         NULL,
    [PLStatement]       NVARCHAR (255) NULL,
    [IsActive]          BIT            DEFAULT ((1)) NULL,
    PRIMARY KEY CLUSTERED ([PersonalDetailsId] ASC)
);

