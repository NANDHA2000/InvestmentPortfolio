CREATE TABLE [dbo].[StockHoldings] (
    [StockHoldingId]    INT             IDENTITY (1, 1) NOT NULL,
    [PersonalDetailsId] INT             NOT NULL,
    [StockName]         NVARCHAR (100)  NULL,
    [ISIN]              NVARCHAR (20)   NULL,
    [Quantity]          INT             NULL,
    [BuyDate]           DATE            NULL,
    [BuyPrice]          DECIMAL (18, 2) NULL,
    [BuyValue]          DECIMAL (18, 2) NULL,
    [SellDate]          DATE            NULL,
    [SellPrice]         DECIMAL (18, 2) NULL,
    [SellValue]         DECIMAL (18, 2) NULL,
    [RealisedPL]        DECIMAL (18, 2) NULL,
    PRIMARY KEY CLUSTERED ([StockHoldingId] ASC),
    FOREIGN KEY ([PersonalDetailsId]) REFERENCES [dbo].[PersonalDetails] ([PersonalDetailsId])
);

