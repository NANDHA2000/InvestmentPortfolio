-- 3. Insert into Charges
CREATE   PROCEDURE sp_InsertCharges
    @PersonalDetailsId INT,
    @ExchangeTransactionCharges DECIMAL(18,2) = 0,
    @SEBICharges DECIMAL(18,2) = 0,
    @STT DECIMAL(18,2) = 0,
    @StampDuty DECIMAL(18,2) = 0,
    @IPFTCharges DECIMAL(18,2) = 0,
    @Brokerage DECIMAL(18,2) = 0,
    @DPCharges DECIMAL(18,2) = 0,
    @TotalGST DECIMAL(18,2) = 0,
    @Total DECIMAL(18,2) = 0
AS
BEGIN
    INSERT INTO Charges (PersonalDetailsId, ExchangeTransactionCharges, SEBICharges, STT, StampDuty, IPFTCharges, Brokerage, DPCharges, TotalGST, Total)
    VALUES (@PersonalDetailsId, @ExchangeTransactionCharges, @SEBICharges, @STT, @StampDuty, @IPFTCharges, @Brokerage, @DPCharges, @TotalGST, @Total);
END