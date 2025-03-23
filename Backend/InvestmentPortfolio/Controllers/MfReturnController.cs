using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;

namespace InvestmentPortfolio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class MutualFundController2 : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public MutualFundController2(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        //// Define a class to hold the response from the external API
        public class NavData
        {
            public string Date { get; set; }
            public decimal Nav { get; set; }
        }

        // Transaction class to store investment actions (PURCHASE/REDEEM)
        public class Transaction
        {
            public int SchemeCode { get; set; }
            public string? SchemeName { get; set; }
            public string? TransactionType { get; set; }
            public decimal Units { get; set; }
            public decimal NAV { get; set; }
            public decimal Amount { get; set; }
            public string Date { get; set; }
        }

        // Endpoint to upload an Excel file and process it
        [HttpPost("uploadExcel")]
        public async Task<IActionResult> UploadExcel(IFormFile file)
        {
            if(file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            // Process the uploaded file
            var transactions = await ProcessExcelFile(file);

            // Call the GetReturns method with the transactions
            //var returns = await GetReturns(transactions);

            // Return the returns as JSON
            return Ok(transactions);
        }

        private readonly List<string> _targetSchemeNames = new List<string>
        {
               "ICICI Prudential Infrastructure Direct Growth",
               "Quant Small Cap Fund Direct Plan Growth",
               "Parag Parikh Flexi Cap Fund Direct Growth"
        };

        // Process the uploaded Excel file
        private async Task<List<Transaction>> ProcessExcelFile(IFormFile file)
        {
            var transactions = new List<Transaction>();

            using(var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using(var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets[0]; // Assuming first worksheet
                    var rowCount = worksheet.Dimension.End.Row;

                    // Start reading from row 2 (assuming row 1 has headers)
                    for(int row = 15; row <= rowCount; row++)
                    {
                        var transaction = new Transaction
                        {
                            SchemeCode = 123456, // You mentioned to skip this in the request
                            SchemeName = worksheet.Cells[row, 1].Text.Trim(), // You mentioned to skip this in the request
                            TransactionType = worksheet.Cells[row, 2].Text.Trim(),
                            Units = decimal.TryParse(worksheet.Cells[row, 3].Text.Trim(), out var units) ? units : 0,
                            NAV = decimal.TryParse(worksheet.Cells[row, 4].Text.Trim(), out var nav) ? nav : 0,
                            Amount = decimal.TryParse(worksheet.Cells[row, 5].Text.Trim(), out var amount) ? amount : 0,
                            Date = worksheet.Cells[row, 6].Text.Trim()
                        };

                        transactions.Add(transaction);
                    }
                }
            }

            return transactions;
        }


        /*[HttpPost("getReturns")]
        public async Task<IActionResult> GetReturns(List<Transaction> transactions)
        {
            object returns = "";
            //int SchemeCode = 0;
            foreach(var transaction in transactions)
            {
                int SchemeCode = transaction.SchemeCode;


                if(SchemeCode > 0)
                {
                    var apiUrl = $"https://api.mfapi.in/mf/{SchemeCode}"; // The URL of the external API

                    // Send a GET request to the external API
                    var response = await _httpClient.GetAsync(apiUrl);

                    // Check if the response is successful
                    if(response.IsSuccessStatusCode)
                    {
                        var data = await response.Content.ReadAsStringAsync();
                        var navData = ParseNavData(data);

                        // Calculate the returns for each transaction based on the NAV data
                        returns = CalculateReturns(SchemeCode, navData, transactions);

                        // Return the returns as JSON
                        //return Ok(returns);
                    }
                    else
                    {
                        // Return an error if the API call fails
                        return StatusCode((int)response.StatusCode, "Failed to fetch NAV data");
                    }
                }

            }
            return Ok(returns);

        }*/


        [HttpPost("getReturns")]
        public async Task<IActionResult> GetReturns(List<Transaction> transactions)
        {
            var returns = new List<object>();  // Store the returns for each transaction
            var processedSchemeCodes = new HashSet<int>();  // Set to keep track of processed SchemeCodes

            foreach(var transaction in transactions)
            {
                int SchemeCode = transaction.SchemeCode;

                // Skip processing if the SchemeCode has already been processed
                if(processedSchemeCodes.Contains(SchemeCode))
                {
                    continue;  // Skip this iteration and move to the next transaction
                }

                if(SchemeCode > 0)
                {
                    var apiUrl = $"https://api.mfapi.in/mf/{SchemeCode}"; // The URL of the external API

                    // Send a GET request to the external API
                    var response = await _httpClient.GetAsync(apiUrl);

                    // Check if the response is successful
                    if(response.IsSuccessStatusCode)
                    {
                        var data = await response.Content.ReadAsStringAsync();
                        var navData = ParseNavData(data);

                        // Calculate the returns for each transaction based on the NAV data
                        var transactionReturns = CalculateReturns(SchemeCode, navData, transactions);
                        returns.Add(transactionReturns);  // Add calculated returns to the list

                        // Mark this SchemeCode as processed
                        processedSchemeCodes.Add(SchemeCode);
                    }
                    else
                    {
                        // Return an error if the API call fails
                        return StatusCode((int)response.StatusCode, "Failed to fetch NAV data");
                    }
                }
            }

            return Ok(returns);
        }





        // Parse the NAV data from the API response
        private List<NavData> ParseNavData(string data)
        {
            var navList = new List<NavData>();
            var jsonData = JObject.Parse(data);
            var navArray = jsonData["data"];

            foreach(var item in navArray)
            {
                navList.Add(new NavData
                {
                    Date = item["date"].ToString(),
                    Nav = decimal.Parse(item["nav"].ToString())
                });
            }

            return navList;
        }

        private List<object> CalculateReturns(int SchemeCode, List<NavData> navData, List<Transaction> transactions)
        {
            var result = new List<object>();

            // Group transactions by SchemeName
            var groupedTransactions = transactions
                .Where(t => t.SchemeCode == SchemeCode) // Filter transactions by SchemeCode
                .GroupBy(t => t.SchemeName)
                .ToList();

            foreach(var schemeGroup in groupedTransactions)
            {
                if(_targetSchemeNames.Contains(schemeGroup.Key!))
                {
                    var schemeReturns = new List<object>();

                    // Track total units purchased and redeemed
                    decimal totalUnitsPurchased = 0;
                    decimal totalUnitsRedeemed = 0;
                    decimal totalAmountInvested = 0;
                    decimal totalAmountRedeemed = 0;

                    foreach(var transaction in schemeGroup)
                    {
                        if(transaction.TransactionType == "PURCHASE")
                        {
                            totalUnitsPurchased += transaction.Units;
                            totalAmountInvested += transaction.Amount;
                        }
                        else if(transaction.TransactionType == "REDEEM")
                        {
                            totalUnitsRedeemed += transaction.Units;
                            totalAmountRedeemed += transaction.Amount;
                        }
                    }

                    // Calculate the return for each NAV date
                    decimal previousValue = 0; // Initialize for dayReturn calculation
                    decimal dayReturn = 0;

                    foreach(var nav in navData)
                    {
                        decimal currentUnits = totalUnitsPurchased - totalUnitsRedeemed;
                        decimal currentValue = currentUnits * nav.Nav;

                        dayReturn = previousValue - currentValue; // Calculate dayReturn (currentValue - previousValue)

                        schemeReturns.Add(new
                        {
                            Date = DateTime.Parse(nav.Date).AddDays(1).ToString("dd-MM-yyyy"),
                            NAV = nav.Nav,
                            CurrentValue = currentValue,
                            ReturnPercentage = (currentValue - totalAmountInvested) / totalAmountInvested * 100,
                            DayReturn = dayReturn
                        });

                        previousValue = currentValue; // Update previousValue for the next calculation
                    }

                    result.Add(new
                    {
                        SchemeName = schemeGroup.Key,
                        SchemeReturns = schemeReturns
                    });
                }
            }

            return result;
        }


        // Calculate the returns for each investment based on the NAV data
        /* private List<object> CalculateReturns(int SchemeCode, List<NavData> navData, List<Transaction> transactions)
         {


             var result = new List<object>();

             // Group transactions by SchemeCode
 *//*            var groupedTransactions = transactions
                 .GroupBy(t => t.SchemeCode)
                 .ToList();*//*

             var groupedTransactions = transactions
             .GroupBy(t => t.SchemeName) // Group by SchemeName
             .ToList();



             foreach(var schemeGroup in groupedTransactions)
                 {

                 if(_targetSchemeNames.Contains(schemeGroup.Key!)) 
                 {
                     var schemeReturns = new List<object>();

                     // Track total units purchased and redeemed
                     decimal totalUnitsPurchased = 0;
                     decimal totalUnitsRedeemed = 0;
                     decimal totalAmountInvested = 0;
                     decimal totalAmountRedeemed = 0;

                     foreach(var transaction in schemeGroup)
                     {
                         if(transaction.TransactionType == "PURCHASE")
                         {
                             totalUnitsPurchased += transaction.Units;
                             totalAmountInvested += transaction.Amount;
                         }
                         else if(transaction.TransactionType == "REDEEM")
                         {
                             totalUnitsRedeemed += transaction.Units;
                             totalAmountRedeemed += transaction.Amount;
                         }
                     }

                     // Calculate the return for each NAV date
                     decimal previousValue = 0; // Initialize for dayReturn calculation
                     decimal dayReturn = 0;

                     // Process the rest of the NAV data
                     foreach(var nav in navData)
                     {
                         decimal currentUnits = totalUnitsPurchased - totalUnitsRedeemed;
                         decimal currentValue = currentUnits * nav.Nav;

                         dayReturn = previousValue - currentValue; // Calculate dayReturn (previousValue - currentValue)

                         schemeReturns.Add(new
                         {
                             Date = DateTime.Parse(nav.Date).AddDays(1).ToString("dd-MM-yyyy"),
                             NAV = nav.Nav,
                             CurrentValue = currentValue,
                             ReturnPercentage = ((currentValue - totalAmountInvested) / totalAmountInvested) * 100,
                             DayReturn = dayReturn
                         });

                         previousValue = currentValue; // Update previousValue for the next calculation
                     }

                     result.Add(new
                     {
                         SchemeName = schemeGroup.Key,
                         SchemeReturns = schemeReturns
                     });
                 }

                 }


             return result;
         }*/

    }
}
