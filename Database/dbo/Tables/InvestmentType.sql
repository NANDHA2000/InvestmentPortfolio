CREATE TABLE [dbo].[InvestmentType] (
    [InvestmentTypeId]   INT           NOT NULL,
    [InvestmentTypeName] NVARCHAR (15) NULL,
    [IsActive]           BIT           NULL,
    CONSTRAINT [PK_InvestmentType] PRIMARY KEY CLUSTERED ([InvestmentTypeId] ASC)
);

