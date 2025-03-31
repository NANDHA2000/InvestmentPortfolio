CREATE   FUNCTION fn_GetRankedStockHoldings()
RETURNS TABLE
AS
RETURN (
    WITH RankedData AS (
        SELECT 
            sh.StockName,
            sh.ISIN,
            SUM(sh.Quantity) AS Quantity,
            MIN(sh.BuyDate) AS BuyDate,
            MAX(sh.SellDate) AS SellDate,
            SUM(sh.BuyValue) AS BuyValue,
            SUM(sh.SellValue) AS SellValue,
            SUM(sh.BuyPrice) AS BuyPrice,
            SUM(sh.SellPrice) AS SellPrice,
            SUM(sh.RealisedPL) AS RealisedPL,
            ROW_NUMBER() OVER (ORDER BY sh.ISIN) AS RowNum,
            COUNT(*) OVER () AS TotalRows
        FROM StockHoldings sh
        INNER JOIN PersonalDetails pd
            ON sh.PersonalDetailsId = pd.PersonalDetailsId
        WHERE pd.InvestmentTypeId = 1
        GROUP BY sh.StockName, sh.ISIN
    )
    SELECT StockName, ISIN, Quantity, BuyDate, SellDate, BuyValue, SellValue, BuyPrice, SellPrice, RealisedPL
    FROM RankedData
    WHERE RowNum > 1 AND RowNum < TotalRows
);