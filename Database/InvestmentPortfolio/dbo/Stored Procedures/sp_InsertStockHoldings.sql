-- 4. Insert into Holdings
CREATE   PROCEDURE sp_InsertStockHoldings
    @PersonalDetailsId INT,
    @StockName NVARCHAR(100) = NULL,
    @ISIN NVARCHAR(20) = NULL,
    @Quantity INT = NULL,
    @BuyDate DATE = NULL,
    @BuyPrice DECIMAL(18,2) = NULL,
    @BuyValue DECIMAL(18,2) = NULL,
    @SellDate DATE = NULL,
    @SellPrice DECIMAL(18,2) = NULL,
    @SellValue DECIMAL(18,2) = NULL,
    @RealisedPL DECIMAL(18,2) = NULL
AS
BEGIN
    INSERT INTO StockHoldings (PersonalDetailsId, StockName, ISIN, Quantity, BuyDate, BuyPrice, BuyValue, SellDate, SellPrice, SellValue, RealisedPL)
    VALUES (@PersonalDetailsId, @StockName, @ISIN, @Quantity, @BuyDate, @BuyPrice, @BuyValue, @SellDate, @SellPrice, @SellValue, @RealisedPL);
END