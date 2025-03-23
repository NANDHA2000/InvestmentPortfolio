CREATE   PROCEDURE sp_InsertHoldingSummary
    @PersonalDetailId INT,
    @CurrentPortfolioValue NVARCHAR(100) NULL,
	@TotalInvestments DECIMAL(18,2) NULL,
	@ProfitAndLoss DECIMAL(18,2) NULL,
	@ProfitAndLossPercentage DECIMAL(18,2) NULL,
	@XIRR DECIMAL(18,2) NULL,
    @HoldingSummaryId INT OUTPUT
AS
BEGIN

    INSERT INTO HoldingSummary 
	(PersonalDetailsId,CurrentPortfolioValue,TotalInvestments, ProfitAndLoss, ProfitAndLossPercentage, 
	XIRR)
    VALUES (@PersonalDetailId,@CurrentPortfolioValue,@TotalInvestments, @ProfitAndLoss, @ProfitAndLossPercentage, 
	@XIRR);

	SET @HoldingSummaryId = SCOPE_IDENTITY();


	
END