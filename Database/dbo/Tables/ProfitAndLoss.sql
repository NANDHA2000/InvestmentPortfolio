CREATE TABLE [dbo].[ProfitAndLoss] (
    [ProfitAndLossId]   INT             IDENTITY (1, 1) NOT NULL,
    [PersonalDetailsId] INT             NOT NULL,
    [RealisedPL]        DECIMAL (18, 2) NOT NULL,
    [UnRealisedPL]      DECIMAL (18, 2) NOT NULL,
    PRIMARY KEY CLUSTERED ([ProfitAndLossId] ASC),
    FOREIGN KEY ([PersonalDetailsId]) REFERENCES [dbo].[PersonalDetails] ([PersonalDetailsId])
);

