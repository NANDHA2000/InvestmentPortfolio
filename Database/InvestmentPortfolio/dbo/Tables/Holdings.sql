CREATE TABLE [dbo].[Holdings] (
    [HoldingId]        INT             IDENTITY (1, 1) NOT NULL,
    [HoldingSummaryId] INT             NULL,
    [SchemeName]       NVARCHAR (100)  NULL,
    [AMC]              NVARCHAR (100)  NULL,
    [Category]         NVARCHAR (50)   NULL,
    [SubCategory]      NVARCHAR (50)   NULL,
    [FolioNo]          BIGINT          NULL,
    [Source]           NVARCHAR (20)   NULL,
    [Unit]             DECIMAL (18, 2) NULL,
    [InvestedValue]    DECIMAL (18, 2) NULL,
    [CurrentValue]     DECIMAL (18, 2) NULL,
    [Returns]          DECIMAL (18, 2) NULL,
    [XIRR]             DECIMAL (18, 2) NULL,
    PRIMARY KEY CLUSTERED ([HoldingId] ASC),
    FOREIGN KEY ([HoldingSummaryId]) REFERENCES [dbo].[HoldingSummary] ([HoldingSummaryId])
);

