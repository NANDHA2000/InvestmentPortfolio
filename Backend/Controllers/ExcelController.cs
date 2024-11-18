using InvestmentPortpolio.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OfficeOpenXml;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;

[ApiController]
[Route("api/[controller]")]
public class ExcelController : ControllerBase
{
    string jsonFilePath = @"D:\InvestmentPortpolio\InvestmentPortpolio\Data\Data.json";


    [HttpGet("Test")]
    public async Task<string> Testing()
    {
        return "Test";
    }


    #region InvestmentData
    [HttpGet("InvestmentData")]
    public async Task<IActionResult> GetInvestmentData()
    {

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Data\\StockData.json");

        var jsonData = await System.IO.File.ReadAllTextAsync(filePath);

        var stockDataList = JsonSerializer.Deserialize<List<StockData>>(jsonData);

        return Ok(stockDataList);
    }
    #endregion


    #region ReadStockData
    [HttpPost("ReadStockData")]
    public IActionResult ReadStockData(IFormFile file)
    {
        int sheetNumber = 1;

        if(file == null || file.Length == 0)
            return BadRequest("Invalid file.");

        var stockDataList = new List<Dictionary<string, string>>();

        // Define the headers you want to extract from the Groww "Stocks P&L" report
        var requiredHeaders = new List<string>
                {
                    "Stock name",
                    "ISIN",
                    "Quantity",
                    "Buy date",
                    "Buy price",
                    "Buy value",
                    "Sell date",
                    "Sell price",
                    "Sell value",
                    "Realised P&L",
                    "Remark"
                };

        try
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using(var stream = file.OpenReadStream())
            {
                using(var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets[sheetNumber - 1]; // Sheets are 0-indexed

                    // Extract headers from row 25 (A25:K25)
                    var headers = new List<string>();
                    for(int col = 1; col <= 11; col++)  // Columns A to K (1 to 11)
                    {
                        var headerCell = worksheet.Cells[25, col]; // Row 25 (headers row)
                        if(!string.IsNullOrWhiteSpace(headerCell.Text))
                        {
                            headers.Add(headerCell.Text.Trim());
                        }
                    }

                    // Extract data starting from row 26 (data starts from row 26)
                    for(int row = 26; row <= worksheet.Dimension.End.Row; row++)
                    {
                        var stockData = new Dictionary<string, string>();

                        // Read each column (A to K) and check against required headers
                        for(int col = 1; col <= 11; col++)
                        {
                            var header = headers[col - 1];
                            var valueCell = worksheet.Cells[row, col].Text.Trim();

                            //var formattedHeader = header.Replace(" ", string.Empty);
                            var formattedHeader = header.Replace(" ", string.Empty).Replace("&", string.Empty);

                            // Only add key-value pair if the header is in the required list and the value exists
                            if(requiredHeaders.Contains(header) && !string.IsNullOrEmpty(valueCell))
                            {
                                stockData[formattedHeader] = valueCell;

                            }
                        }

                        // Add the stock data only if it contains at least one valid entry
                        if(stockData.Count > 0)
                        {
                            stockDataList.Add(stockData);
                        }
                    }
                }
            }

            // Return the extracted data as JSON
            var filteredData = stockDataList
           .Where(item => item.ContainsKey("ISIN") && !string.IsNullOrEmpty(item["ISIN"]))
            .ToList();

            var jsonData = JsonSerializer.Serialize(filteredData);

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Data\\StockData.json");

            System.IO.File.WriteAllTextAsync(filePath, jsonData);

            return Ok(filteredData);
        }
        catch(System.Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    #endregion


    #region SaveToJsonFileAsync
    private async Task SaveToJsonFileAsync(List<StockData> stockDataList)
    {
        try
        {
            // Serialize the stock data list to JSON
            var jsonData = JsonSerializer.Serialize(stockDataList, new JsonSerializerOptions
            {
                WriteIndented = true // For readable JSON format
            });

            // Write JSON data to the specified file path
            await System.IO.File.WriteAllTextAsync(jsonFilePath, jsonData);
        }
        catch(Exception ex)
        {
            // Handle any exceptions here (logging, re-throwing, etc.)
            Console.WriteLine($"Error saving to JSON file: {ex.Message}");
        }
    }
    #endregion

    #region upload
    /*    [HttpPost("upload")]
    public async Task<IActionResult> UploadExcel(IFormFile file)
    {
        if(file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        var stockDataList = new List<StockData>();

        try
        {
            using(var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);

                using(var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets[2]; // Read the first worksheet
                    if(worksheet == null) return BadRequest("No worksheet found.");

                    for(int row = 2; row <= worksheet.Dimension.End.Row; row++) // Assuming first row is headers
                    {
                        // Use TryParse for safe parsing of numeric values
                        var stockData = new StockData
                        {
                            StockName = worksheet.Cells[row, 1].Text,
                            ISIN = worksheet.Cells[row, 2].Text,
                            Quantity = int.TryParse(worksheet.Cells[row, 3].Text, out var quantity) ? quantity : 0, // Default to 0 if parsing fails
                            AvgBuyPrice = decimal.TryParse(worksheet.Cells[row, 4].Text, out var avgBuyPrice) ? avgBuyPrice : 0m,
                            BuyValue = decimal.TryParse(worksheet.Cells[row, 5].Text, out var buyValue) ? buyValue : 0m,
                            AvgSellPrice = decimal.TryParse(worksheet.Cells[row, 6].Text, out var avgSellPrice) ? avgSellPrice : 0m,
                            SellValue = decimal.TryParse(worksheet.Cells[row, 7].Text, out var sellValue) ? sellValue : 0m,
                            RealisedPnL = decimal.TryParse(worksheet.Cells[row, 8].Text, out var realisedPnL) ? realisedPnL : 0m,
                            RealisedPnLPercentage = worksheet.Cells[row, 9].Text,
                            BuyDate = worksheet.Cells[row, 10].Text,
                            SellDate = worksheet.Cells[row, 11].Text
                        };
                        stockDataList.Add(stockData);
                    }
                }
            }

            // Save the data to JSON file
            await SaveToJsonFileAsync(stockDataList);

            return Ok(stockDataList);
        }
        catch(Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    } */
    #endregion







}
