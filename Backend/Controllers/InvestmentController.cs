using ExcelDataReader;
using InvestmentPortfolio.Model.Models;
using InvestmentPortfolio.Service.IService;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Text.Json;


namespace InvestmentPortfolio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi =false)]
    public class InvestmentController : ControllerBase
    {
        private readonly IInvestmentService _investmentService;

        public InvestmentController(IInvestmentService investmentService)
        {
            _investmentService = investmentService;
        }

        #region Get Invested Details
        [HttpGet("GetInvestedDetails")]
        public async Task<IActionResult> GetInvestmentData(string investmentName)
        {
            var result = await _investmentService.GetInvestmentDetailsAsync(investmentName);

            if(result != null)
                return Ok(result);

            return NotFound("Invalid investment name or no data found.");
        }
        #endregion

        #region Add Investment Details


        [HttpPost("UploadGrowwReport")]
        public async Task<IActionResult> UploadGrowwReport(IFormFile file, string fileName)
        {
            try
            {
                var result = await _investmentService.ProcessGrowwReportAsync(file, fileName);

                if(result.success)
                    return Ok(new { success = true, message = result.message });

                return BadRequest(new { success = false, message = result.message });
            }
            catch(Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /*[HttpPost("AddInvestmentDetails")]
        public IActionResult UploadGrowwReport(IFormFile file, string fileName)
        {

            bool IsExist = false;
            if(file == null || file.Length == 0 && fileName == null || fileName.Length == 0)
                return BadRequest(MessageConstant.NoFileData);

            try
            {
                var filePath = Path.GetTempFileName();

                // Save the file to a temporary location
                using(var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                var dataTable = ReadGrowwReport(filePath);  // Read and process the Excel file



                if(fileName == NamingConstant.Stocks)
                {
                    
                    var stockResult = ProcessGrowwStockData(dataTable);
                    var jsonData = JsonSerializer.Serialize(stockResult);
                    var jsonfilePath = Path.Combine(Directory.GetCurrentDirectory(), LocationConstant.StocksData);
                    IsExist = System.IO.File.WriteAllTextAsync(jsonfilePath, jsonData).IsCompleted;
                }

                else if(fileName == NamingConstant.MutualFund)
                {
                    var result = ProcessGrowwReportData(dataTable); // Process the data into a structured format
                    var jsonData = JsonSerializer.Serialize(result);
                    var jsonfilePath = Path.Combine(Directory.GetCurrentDirectory(), LocationConstant.MutualFundData);
                    IsExist = System.IO.File.WriteAllTextAsync(jsonfilePath, jsonData).IsCompleted;
                }

                if(IsExist)
                {
                    return Ok(new { success = true, message = MessageConstant.FileDataAdded });
                }
                else
                {
                    return Ok(new { success = false, message = MessageConstant.Error });
                }

            }
            catch(Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        } */

        #endregion

        #region Read Excel file
        private DataTable ReadGrowwReport(string filePath)
        {
            try
            {
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                using var stream = System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read);
                using var reader = ExcelReaderFactory.CreateReader(stream);

                var result = reader.AsDataSet();
                if(result.Tables.Count == 0)
                    throw new Exception("The uploaded file does not contain any data.");

                return result.Tables[0];
            }
            catch(Exception ex)
            {
                throw new Exception($"Failed to read the Excel file: {ex.Message}");
            }
        } 
        #endregion


        #region MF
        // Updated method to process and structure the data
        private object ProcessGrowwReportData(DataTable table)
        {
            var report = new
            {
                PersonalDetails = new
                {
                    Name = table.Rows[3]["Column1"]?.ToString(),
                    MobileNumber = table.Rows[4]["Column1"]?.ToString(),
                    PAN = table.Rows[5]["Column1"]?.ToString()
                },
                HoldingSummary = new
                {
                    TotalInvestments = table.Rows[13]["Column0"]?.ToString(),
                    CurrentPortfolioValue = table.Rows[13]["Column1"]?.ToString(),
                    ProfitLoss = table.Rows[13]["Column2"]?.ToString(),
                    ProfitLossPercentage = table.Rows[13]["Column3"]?.ToString(),
                    XIRR = table.Rows[13]["Column4"]?.ToString()
                },
                HoldingsDate = table.Rows[18]["Column0"]?.ToString(),
                Holdings = GetHoldings(table)
            };

            return report;
        }

        //Extract holdings details from the table
        private List<object> GetHoldings(DataTable table)
        {
            var holdings = new List<object>();

            // Adjust the starting index (19) based on where holdings start
            for(int i = 19; i < table.Rows.Count; i++)
            {
                var row = table.Rows[i];

                var holding = new Dictionary<string, object>();

                // Add non-empty fields to the dictionary
                if(!string.IsNullOrWhiteSpace(row["Column0"]?.ToString()))
                    holding["SchemeName"] = row["Column0"]?.ToString()!;
                if(!string.IsNullOrWhiteSpace(row["Column1"]?.ToString()))
                    holding["AMC"] = row["Column1"]?.ToString()!;
                if(!string.IsNullOrWhiteSpace(row["Column2"]?.ToString()))
                    holding["Category"] = row["Column2"]?.ToString()!;
                if(!string.IsNullOrWhiteSpace(row["Column3"]?.ToString()))
                    holding["SubCategory"] = row["Column3"]?.ToString()!;
                if(!string.IsNullOrWhiteSpace(row["Column4"]?.ToString()))
                    holding["FolioNo"] = row["Column4"]?.ToString()!;
                if(!string.IsNullOrWhiteSpace(row["Column5"]?.ToString()))
                    holding["Source"] = row["Column5"]?.ToString()!;
                if(!string.IsNullOrWhiteSpace(row["Column6"]?.ToString()))
                    holding["Units"] = row["Column6"]?.ToString()!;
                if(!string.IsNullOrWhiteSpace(row["Column7"]?.ToString()))
                    holding["InvestedValue"] = row["Column7"]?.ToString()!;
                if(!string.IsNullOrWhiteSpace(row["Column8"]?.ToString()))
                    holding["CurrentValue"] = row["Column8"]?.ToString()!;
                if(!string.IsNullOrWhiteSpace(row["Column9"]?.ToString()))
                    holding["Returns"] = row["Column9"]?.ToString()!;
                if(!string.IsNullOrWhiteSpace(row["Column10"]?.ToString()))
                    holding["XIRR"] = row["Column10"]?.ToString()!;

                holdings.Add(holding);
            }

            return holdings;
        }


        #endregion


        #region Stocks
        private object ProcessGrowwStockData(DataTable table)
        {
            var report = new
            {
                PersonalDetails = new
                {
                    Name = table.Rows[0]["Column1"]?.ToString(),
                    UniqueClientCode = table.Rows[1]["Column1"]?.ToString(),
                    PLStatement = table.Rows[3]["Column0"]?.ToString(),
                },
                ProfitAndLoss = new
                {
                    RealisedPL = table.Rows[8]["Column1"]?.ToString(),
                    UnRealisedPL = table.Rows[9]["Column1"]?.ToString(),
                },
                Charges = new
                {
                    ExchangeTransactionCharges = table.Rows[12]["Column1"]?.ToString(),
                    SEBICharges = table.Rows[13]["Column1"]?.ToString(),
                    STT = table.Rows[14]["Column1"]?.ToString(),
                    StampDuty = table.Rows[15]["Column1"]?.ToString(),
                    IPFTCharges = table.Rows[16]["Column1"]?.ToString(),
                    Brokerage = table.Rows[17]["Column1"]?.ToString(),
                    DPCharges = table.Rows[18]["Column1"]?.ToString(),
                    TotalGST = table.Rows[19]["Column1"]?.ToString(),
                    Total = table.Rows[20]["Column1"]?.ToString(),
                },
                Holdings = GetStockHoldings(table)
            };

            return report;
        }

        private List<object> GetStockHoldings(DataTable table)
        {
            var holdings = new List<object>();

            // Adjust the starting index based on where holdings start
            for(int i = 25; i < table.Rows.Count; i++)
            {
                var row = table.Rows[i];
                // Check if the row contains valid data for stock holdings
                if(string.IsNullOrWhiteSpace(row["Column0"]?.ToString()) ||
                    row["Column0"]?.ToString()!.Contains("Disclaimer", StringComparison.OrdinalIgnoreCase) == true ||
                    row["Column0"]?.ToString()!.Contains("This report is provided", StringComparison.OrdinalIgnoreCase) == true ||
                    row["Column0"]?.ToString()!.Contains("Groww Invest Tech Private Limited", StringComparison.OrdinalIgnoreCase) == true)
                {
                    // Skip unwanted or disclaimer rows
                    continue;
                }

                var holding = new Dictionary<string, object>();

                // Add non-empty fields to the dictionary
                if(!string.IsNullOrWhiteSpace(row["Column0"]?.ToString()))
                    holding["StockName"] = row["Column0"]?.ToString()!;
                if(!string.IsNullOrWhiteSpace(row["Column1"]?.ToString()))
                    holding["ISIN"] = row["Column1"]?.ToString()!;
                if(!string.IsNullOrWhiteSpace(row["Column2"]?.ToString()))
                    holding["Quantity"] = row["Column2"]?.ToString()!;
                if(!string.IsNullOrWhiteSpace(row["Column3"]?.ToString()))
                    holding["BuyDate"] = row["Column3"]?.ToString()!;
                if(!string.IsNullOrWhiteSpace(row["Column4"]?.ToString()))
                    holding["BuyPrice"] = row["Column4"]?.ToString()!;
                if(!string.IsNullOrWhiteSpace(row["Column5"]?.ToString()))
                    holding["BuyValue"] = row["Column5"]?.ToString()!;
                if(!string.IsNullOrWhiteSpace(row["Column6"]?.ToString()))
                    holding["SellDate"] = row["Column6"]?.ToString()!;
                if(!string.IsNullOrWhiteSpace(row["Column7"]?.ToString()))
                    holding["SellPrice"] = row["Column7"]?.ToString()!;
                if(!string.IsNullOrWhiteSpace(row["Column8"]?.ToString()))
                    holding["SellValue"] = row["Column8"]?.ToString()!;
                if(!string.IsNullOrWhiteSpace(row["Column9"]?.ToString()))
                    holding["RealisedPL"] = row["Column9"]?.ToString()!;
                if(!string.IsNullOrWhiteSpace(row["Column10"]?.ToString()))
                    holding["Remarks"] = row["Column10"]?.ToString()!;

                holdings.Add(holding);
            }

            return holdings;
        }
        #endregion

    }
}
