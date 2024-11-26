using ExcelDataReader;
using InvestmentPortpolio.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Text.Json;

namespace InvestmentPortfolio.Controllers
{
    public class ExcelReaderController : ControllerBase
    {


        [HttpPost("upload")]
        public IActionResult UploadGrowwReport(IFormFile file)
        {
            if(file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            try
            {
                var filePath = Path.GetTempFileName();

                // Save the file to a temporary location
                using(var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                // Read and process the Excel file
                var dataTable = ReadGrowwReport(filePath);

               // var result = ProcessGrowwReportData(dataTable); // Process the data into a structured format

                var stockResult = ProcessGrowwStockData(dataTable);

               // var jsonData = JsonSerializer.Serialize(result);

                var jsonfilePath = Path.Combine(Directory.GetCurrentDirectory(), "Data\\MutualFundData.json");

                System.IO.File.WriteAllTextAsync(filePath, jsonfilePath);

                return Ok(stockResult);
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"An error occurred while processing the file: {ex.Message}");
            }
        }

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

        // Extract holdings details from the table
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


        private object ProcessGrowwStockData(DataTable table)
        {
            var report = new
            {
                PersonalDetails = new
                {
                    Name = table.Rows[2]["Column1"]?.ToString(),
                    Email = table.Rows[3]["Column1"]?.ToString(),
                    MobileNumber = table.Rows[4]["Column1"]?.ToString(),
                    PAN = table.Rows[5]["Column1"]?.ToString()
                },
                StockSummary = new
                {
                    TotalInvestments = table.Rows[10]["Column1"]?.ToString(),
                    CurrentPortfolioValue = table.Rows[10]["Column2"]?.ToString(),
                    ProfitLoss = table.Rows[10]["Column3"]?.ToString(),
                    ProfitLossPercentage = table.Rows[10]["Column4"]?.ToString()
                },
                Holdings = GetStockHoldings(table)
            };

            return report;
        }

        private List<object> GetStockHoldings(DataTable table)
        {
            var holdings = new List<object>();

            // Adjust the starting index based on where holdings start
            for(int i = 15; i < table.Rows.Count; i++)
            {
                var row = table.Rows[i];

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
                    holding["RealisedP&L"] = row["Column9"]?.ToString()!;
                if(!string.IsNullOrWhiteSpace(row["Column10"]?.ToString()))
                    holding["Remarks"] = row["Column10"]?.ToString()!;

                holdings.Add(holding);
            }

            return holdings;


            //var holding = new
            //{
            //    SchemeName = row["Column0"]?.ToString(),
            //    AMC = row["Column1"]?.ToString(),
            //    Category = row["Column2"]?.ToString(),
            //    SubCategory = row["Column3"]?.ToString(),
            //    FolioNo = row["Column4"]?.ToString(),
            //    Source = row["Column5"]?.ToString(),
            //    Units = row["Column6"]?.ToString(),
            //    // Units = row["Column6"] != DBNull.Value ? Convert.ToDecimal(row["Column6"]) : 0,
            //    InvestedValue = row["Column7"].ToString(),
            //    // InvestedValue = row["Column7"] != DBNull.Value ? Convert.ToDecimal(row["Column7"]) : 0,
            //    CurrentValue = row["Column8"].ToString(),
            //    //CurrentValue = row["Column8"] != DBNull.Value ? Convert.ToDecimal(row["Column8"]) : 0,
            //    // Returns = row["Column9"] != DBNull.Value ? Convert.ToDecimal(row["Column9"]) : 0,
            //    Returns = row["Column9"].ToString(),
            //    XIRR = row["Column10"]?.ToString()
            //};

            //// New method to process and structure the data
            //private object ProcessGrowwReportData(DataTable table)
            //{
            //    var report = new
            //    {
            //        PersonalDetails = new
            //        {
            //            Name = table.Rows[3]["Column1"]?.ToString(),
            //            MobileNumber = table.Rows[4]["Column1"]?.ToString(),
            //            PAN = table.Rows[5]["Column1"]?.ToString()
            //        },
            //        HoldingSummary = new
            //        {
            //            TotalInvestments = table.Rows[10]["Column1"]?.ToString(),
            //            CurrentPortfolioValue = table.Rows[10]["Column2"]?.ToString(),
            //            ProfitLoss = table.Rows[10]["Column3"]?.ToString(),
            //            ProfitLossPercentage = table.Rows[10]["Column4"]?.ToString(),
            //            XIRR = table.Rows[10]["Column5"]?.ToString()
            //        },
            //        HoldingsDate = table.Rows[18]["Column0"]?.ToString(),
            //        Holdings = GetHoldings(table)
            //    };

            //    return report;
            //}

            //// Extract holdings details from the table
            //private List<object> GetHoldings(DataTable table)
            //{
            //    var holdings = new List<object>();

            //    for(int i = 19; i < table.Rows.Count; i++)
            //    {
            //        var row = table.Rows[i];

            //        var holding = new
            //        {
            //            SchemeName = row["Column0"]?.ToString(),
            //            AMC = row["Column1"]?.ToString(),
            //            Category = row["Column2"]?.ToString(),
            //            SubCategory = row["Column3"]?.ToString(),
            //            FolioNo = row["Column4"]?.ToString(),
            //            Source = row["Column5"]?.ToString(),
            //            Units = row["Column6"]?.ToString(),
            //           // Units = row["Column6"] != DBNull.Value ? Convert.ToDecimal(row["Column6"]) : 0,
            //            InvestedValue = row["Column7"].ToString(),
            //           // InvestedValue = row["Column7"] != DBNull.Value ? Convert.ToDecimal(row["Column7"]) : 0,
            //            CurrentValue = row["Column8"].ToString(),
            //            //CurrentValue = row["Column8"] != DBNull.Value ? Convert.ToDecimal(row["Column8"]) : 0,
            //           // Returns = row["Column9"] != DBNull.Value ? Convert.ToDecimal(row["Column9"]) : 0,
            //            Returns = row["Column9"].ToString() ,
            //            XIRR = row["Column10"]?.ToString()
            //        };

            //        holdings.Add(holding);
            //    }

            //    return holdings;
            //}


            /*[HttpPost("upload")]
            public IActionResult UploadGrowwReport(IFormFile file)
            {
                if(file == null || file.Length == 0)
                    return BadRequest("No file uploaded.");

                try
                {
                    var filePath = Path.GetTempFileName();

                    // Save the file to a temporary location
                    using(var stream = new FileStream(filePath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }

                    // Read and process the Excel file
                    var dataTable = ReadGrowwReport(filePath);
                    var result = ConvertDataTableToList(dataTable); // Convert to JSON-serializable format

                    //// Apply filtering if filterColumn and filterValue are provided
                    //if(!string.IsNullOrEmpty(filterColumn) && !string.IsNullOrEmpty(filterValue))
                    //{
                    //    result = result
                    //        .Where(row => row.ContainsKey(filterColumn) && row[filterColumn]?.ToString() == filterValue)
                    //        .ToList();
                    //}


                    return Ok(result);
                }
                catch(Exception ex)
                {
                    return StatusCode(500, $"An error occurred while processing the file: {ex.Message}");
                }
            }

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


            private List<Dictionary<string, object>> ConvertDataTableToList(DataTable table)
            {
                var result = new List<Dictionary<string, object>>();

                foreach(DataRow row in table.Rows)
                {
                    var dict = new Dictionary<string, object>();
                    foreach(DataColumn column in table.Columns)
                    {
                        dict[column.ColumnName] = row[column];
                    }
                    result.Add(dict);
                }

                return result;
            }*/

        }
    }
}
