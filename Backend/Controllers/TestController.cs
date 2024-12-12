using InvestmentPortfolio.Constants;
using InvestmentPortfolio.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System.Text.Json;
using System.Text.Json.Serialization;
using static InvestmentPortfolio.Controllers.MutualFundController2;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace InvestmentPortfolio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MutualFundController1 : ControllerBase
    {

        string jsonFilePath = @"D:\Publish\InvestmentPortfolio\Backend\Data\DayBydayMfPerformance.json";


        private readonly HttpClient _httpClient;

        public MutualFundController1(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }


        [HttpGet("GetData")]
        public async Task<IActionResult> GetData() 
        {
            //var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Data\\UserData.json");

            var jsonContent = await System.IO.File.ReadAllTextAsync(jsonFilePath);

            var users = JsonSerializer.Deserialize<List<Transaction>>(jsonContent);

            var groupedTransactions = users
                .GroupBy(t => t.SchemeCode)
                .Select(group => new
                {
                    SchemeCode = group.Key,
                    SchemeName = group.First().SchemeName,
                    SchemeReturns = group.Select(t => new
                    {
                        Date = t.Date,
                        NAV = t.NAV,
                        CurrentValue = t.Units * t.NAV,
                        ReturnPercentage = ((t.Units * t.NAV) - t.Amount) / t.Amount * 100,
                        DayReturn = t.Amount - (t.Units * t.NAV)
                    }).ToList()
                });

            return Ok(groupedTransactions);
        }

        [HttpPost("uploadExcel")]
        public async Task<IActionResult> UploadExcel(IFormFile file)
        {

            if(file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            // Process the uploaded file
            var transactions = await ProcessExcelFile(file);

            // Calculate returns based on the transactions
            var returns = await GetReturns(transactions);

          // var dataArray = returns.Value.data;

            var jsonData = JsonSerializer.Serialize(returns);

            System.IO.File.WriteAllTextAsync(jsonFilePath, jsonData);

            // Return the returns as JSON
            return Ok(returns);
        }

        private async Task<List<Transaction>> ProcessExcelFile(IFormFile file)
        {
            var transactions = new List<Transaction>();

            // Fetch all scheme data from API
            var schemeData = await FetchSchemeData();


            if(schemeData == null)
            {
                throw new Exception("Failed to fetch scheme data from the API.");
            }

            using(var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using(var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets[0]; // Assuming first worksheet
                    var rowCount = worksheet.Dimension.End.Row;

                    // Start reading from row 15 (as specified)
                    for(int row = 15; row <= rowCount; row++)
                    {
                        var schemeNameFromExcel = worksheet.Cells[row, 1].Text.Trim();
                        var schemeName = worksheet.Cells[row, 1].Text.Trim().Replace(" ", "%20");
                        int SchemeCode = 0;
                        var schemeDatas = await FetchSchemeData1(schemeName);

                        foreach(var scheme in schemeDatas)
                        {
                            SchemeCode = scheme.SchemeCode;
                            break;
                        }


                        var transaction = new Transaction
                        {
                            SchemeCode = SchemeCode,
                            SchemeName = schemeNameFromExcel,
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

        private async Task<List<Scheme>> FetchSchemeData()
        {
            using(var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync("https://api.mfapi.in/mf");
                if(response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<Scheme>>(json)!;
                }
                return null;
            }
        }

        private async Task<List<Scheme>> FetchSchemeData1(string SchemeName)
        {
            using(var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync($"https://api.mfapi.in/mf/search?q={SchemeName}");
                if(response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<Scheme>>(json)!;
                }
                return null;
            }
        }

        private async Task<List<object>> GetReturns(List<Transaction> transactions)
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
                        throw new Exception($"Failed to fetch NAV data for SchemeCode {SchemeCode}");
                    }
                }
            }

            return returns;
        }


        private List<object> CalculateReturns(int SchemeCode, List<NavData> navData, List<Transaction> transactions)
        {
            var result = new List<object>();

            // Group transactions by SchemeName
            var groupedTransactions = transactions
                .Where(t => t.SchemeCode == SchemeCode)
                .GroupBy(t => t.SchemeName)
                .ToList();

            foreach(var schemeGroup in groupedTransactions)
            {
                var schemeReturns = new List<object>();

                var orderedTransactions = schemeGroup
                .OrderBy(t => DateTime.Parse(t.Date)) // Order by Date for each group
                .ToList();

                // Find the first purchase transaction date (start date for the scheme)
                var firstPurchaseTransaction = orderedTransactions
                    .Where(t => t.TransactionType == "PURCHASE")
                    .FirstOrDefault();

                if(firstPurchaseTransaction == null || !DateTime.TryParse(firstPurchaseTransaction.Date, out DateTime schemeStartDate))
                    continue; // If no purchase transactions found, skip the scheme

                // Filter NAV data to include only records after the scheme's start date
                var filteredNavData = navData
                    .Where(nav => DateTime.Parse(nav.Date) >= schemeStartDate)
                    .OrderByDescending(nav => DateTime.Parse(nav.Date)) // Sort in descending order (from last to first)
                    .ToList();


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


                foreach(var nav in filteredNavData)
                {
                    decimal currentUnits = totalUnitsPurchased - totalUnitsRedeemed;
                    decimal currentValue = currentUnits * nav.Nav;

                    //previousValue = currentValue;

                    dayReturn = previousValue - currentValue; // Calculate dayReturn (currentValue - previousValue)


/*                    DateTime currentDate = DateTime.Now;

                    // Calculate one day minus the current date
                    string previousDate = currentDate.AddDays(-1).ToString("dd-MM-yyyy");*/


                    schemeReturns.Add(new
                    {
                        //Date = DateTime.Parse(nav.Date).AddDays(1).ToString("dd-MM-yyyy"),
                        Date = nav.Date,
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

            return result;
        }


        private List<NavData> ParseNavData(string data)
        {
            var navList = new List<NavData>();
            var jsonData = JObject.Parse(data);
            var navArray = jsonData["data"];

            foreach(var item in navArray!)
            {
                navList.Add(new NavData
                {
                    Date = item["date"]!.ToString(),
                    Nav = decimal.Parse(item["nav"]!.ToString())
                });
            }

            return navList;
        }



        // Supporting classes
        public class Scheme
        {
            [JsonPropertyName("schemeCode")]
            public int SchemeCode { get; set; }

            [JsonPropertyName("schemeName")]
            public string SchemeName { get; set; }
        }

        public class Transaction
        {
            public int SchemeCode { get; set; }
            public string SchemeName { get; set; }
            public string TransactionType { get; set; }
            public decimal Units { get; set; }
            public decimal NAV { get; set; }
            public decimal Amount { get; set; }
            public string Date { get; set; }
        }
    }
}

