CREATE TABLE [dbo].[Charges] (
    [ChargesId]                  INT             IDENTITY (1, 1) NOT NULL,
    [PersonalDetailsId]          INT             NOT NULL,
    [ExchangeTransactionCharges] DECIMAL (18, 2) NULL,
    [SebiCharges]                DECIMAL (18, 2) NULL,
    [STT]                        DECIMAL (18, 2) NULL,
    [StampDuty]                  DECIMAL (18, 2) NULL,
    [IpftCharges]                DECIMAL (18, 2) NULL,
    [Brokerage]                  DECIMAL (18, 2) NULL,
    [DPCharges]                  DECIMAL (18, 2) NULL,
    [TotalGST]                   DECIMAL (18, 2) NULL,
    [Total]                      DECIMAL (18, 2) NULL,
    PRIMARY KEY CLUSTERED ([ChargesId] ASC),
    FOREIGN KEY ([PersonalDetailsId]) REFERENCES [dbo].[PersonalDetails] ([PersonalDetailsId])
);

