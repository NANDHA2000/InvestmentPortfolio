CREATE TABLE [dbo].[Transactions] (
    [TransactionsId]    INT             IDENTITY (1, 1) NOT NULL,
    [SchemeName]        NVARCHAR (100)  NULL,
    [TransactionType]   NVARCHAR (20)   NULL,
    [Units]             DECIMAL (18, 2) NULL,
    [NAV]               DECIMAL (18, 2) NULL,
    [Amount]            BIGINT          NULL,
    [Date]              DATE            NULL,
    [PersonalDetailsId] INT             NOT NULL,
    PRIMARY KEY CLUSTERED ([TransactionsId] ASC),
    FOREIGN KEY ([PersonalDetailsId]) REFERENCES [dbo].[PersonalDetails] ([PersonalDetailsId])
);

