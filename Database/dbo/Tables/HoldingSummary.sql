CREATE TABLE [dbo].[HoldingSummary] (
    [HoldingSummaryId]        INT             IDENTITY (1, 1) NOT NULL,
    [CurrentPortfolioValue]   NVARCHAR (100)  NULL,
    [ProfitAndLoss]           NVARCHAR (20)   NULL,
    [ProfitAndLossPercentage] NVARCHAR (20)   NULL,
    [XIRR]                    DECIMAL (18, 2) NULL,
    [PersonalDetailsId]       INT             NOT NULL,
    [TotalInvestments]        DECIMAL (18, 2) NULL,
    PRIMARY KEY CLUSTERED ([HoldingSummaryId] ASC),
    FOREIGN KEY ([PersonalDetailsId]) REFERENCES [dbo].[PersonalDetails] ([PersonalDetailsId])
);

