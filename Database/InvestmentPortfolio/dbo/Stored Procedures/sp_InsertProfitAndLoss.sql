-- 2. Insert into ProfitAndLoss
CREATE   PROCEDURE sp_InsertProfitAndLoss
    @PersonalDetailsId INT,
    @RealisedPL DECIMAL(18, 2),
    @UnRealisedPL DECIMAL(18, 2)
AS
BEGIN
    INSERT INTO ProfitAndLoss (PersonalDetailsId, RealisedPL, UnRealisedPL)
    VALUES (@PersonalDetailsId, @RealisedPL, @UnRealisedPL);
END