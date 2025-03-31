CREATE   PROCEDURE sp_GetInvestmentDetails
    @InvestmentTypeId INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @JsonOutput NVARCHAR(MAX);

    -- If InvestmentTypeId = 1, fetch from PersonalDetails and StockHoldings
    IF @InvestmentTypeId = 1
    BEGIN
        SELECT @JsonOutput = (
            SELECT 
                (SELECT Name, Mobile, PanNumber 
                 FROM PersonalDetails 
                 WHERE InvestmentTypeId = @InvestmentTypeId
                 FOR JSON PATH) AS PersonalDetails,

                (SELECT * FROM dbo.fn_GetRankedStockHoldings()
                 FOR JSON PATH) AS StockHoldings

            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER, INCLUDE_NULL_VALUES
        );
    END

    -- If InvestmentTypeId = 2, fetch from PersonalDetails, HoldingSummary, and Holdings
    ELSE IF @InvestmentTypeId = 2
    BEGIN
        SELECT @JsonOutput = (
            SELECT 
                (SELECT Name, Mobile, PanNumber 
                 FROM PersonalDetails 
                 WHERE InvestmentTypeId = @InvestmentTypeId
                 FOR JSON PATH) AS PersonalDetails,

                (SELECT TotalInvestments, CurrentPortfolioValue, ProfitAndLoss, ProfitAndLossPercentage, XIRR 
                 FROM HoldingSummary 
                 WHERE PersonalDetailsId IN (
                       SELECT PersonalDetailsId 
                       FROM PersonalDetails 
                       WHERE InvestmentTypeId = @InvestmentTypeId
                 )
                 FOR JSON PATH) AS HoldingSummary,

                (SELECT SchemeName, AMC, Category, SubCategory, FolioNo, Source, Unit, InvestedValue, CurrentValue, Returns, XIRR 
                 FROM Holdings 
                 WHERE HoldingSummaryId IN (
                       SELECT HoldingSummaryId 
                       FROM HoldingSummary 
                       WHERE PersonalDetailsId IN (
                             SELECT PersonalDetailsId 
                             FROM PersonalDetails 
                             WHERE InvestmentTypeId = @InvestmentTypeId
                       )
                 )
                 FOR JSON PATH) AS Holdings

            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER, INCLUDE_NULL_VALUES
        );
    END

    -- If InvestmentTypeId is invalid, return a message
    ELSE
    BEGIN
        SET @JsonOutput = '{"Message": "Invalid InvestmentTypeId"}';
    END

    -- Output JSON result
    SELECT @JsonOutput AS JsonResult;
END;