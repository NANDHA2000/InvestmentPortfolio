using InvestmentPortfolio.Model.Models;
using InvestmentPortfolio.Service.IService;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System.Net.Http.Json;


namespace InvestmentPortfolio.Service.Service
{

    public class MutualFundService:IMutualFundService
    {
        string jsonFilePath = @"D:\Publish\InvestmentPortfolio\Backend\Data\DayBydayMfPerformance.json";
        private readonly HttpClient _httpClient;


        public MutualFundService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<object> UploadExcel(IFormFile file)
        {

            if(file == null || file.Length == 0)
            {
                return "No file uploaded.";
            }


            // Process the uploaded file
            var transactions = await ProcessExcelFile(file);

            // Calculate returns based on the transactions
            var returns = await GetReturns(transactions);

            // var dataArray = returns.Value.data;

            var jsonData = System.Text.Json.JsonSerializer.Serialize(returns);

            _ = File.WriteAllTextAsync(jsonFilePath, jsonData);

            // Return the returns as JSON
            return returns;
        }


        public async Task<List<MutualFundAnalysis>> GetTopFundsAsync()
        {
            // Fetch all available schemes
            var allSchemes = await GetAllSchemesAsync();
            var analyzedFunds = new List<MutualFundAnalysis>();

            foreach(var scheme in allSchemes)
            {
                var fundDetails = await GetSchemeDetailsAsync(scheme.SchemeCode);
                if(fundDetails != null)
                {
                    var analysis = AnalyzeFund(fundDetails);
                    analyzedFunds.Add(analysis);
                }
            }

            // Sort by returns and risk level, then take top 2
            return analyzedFunds
                .OrderByDescending(f => f.ExpectedReturns)
                .ThenBy(f => f.RiskLevel)
                .Take(2)
                .ToList();
        }

        private async Task<List<MutualFundScheme>> GetAllSchemesAsync()
        {
            // Fetch all schemes
            var response = await _httpClient.GetFromJsonAsync<List<MutualFundScheme>>("https://api.mfapi.in/mf");
            return response ?? new List<MutualFundScheme>();
        }

        private async Task<MutualFundDetails?> GetSchemeDetailsAsync(int schemeCode)
        {
            // Fetch details for a specific scheme
            var response = await _httpClient.GetFromJsonAsync<MutualFundDetails>($"https://api.mfapi.in/mf/{schemeCode}");
            return response;
        }

        private MutualFundAnalysis AnalyzeFund(MutualFundDetails fundDetails)
        {
            // Example logic for analysis
            var expectedReturns = fundDetails.Returns != null && fundDetails.Returns.ContainsKey("1Y")
                ? fundDetails.Returns["1Y"]
                : 0;

            return new MutualFundAnalysis
            {
                SchemeName = fundDetails.SchemeName,
                FundType = fundDetails.FundType,
                RiskLevel = fundDetails.Risk ?? "Unknown",
                ExpectedReturns = expectedReturns
            };
        }



        #region ProcessExcelFile
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
                        var schemeDatas = await FetchSchemeData(schemeName);

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
        #endregion


        #region FetchSchemeData
        private async Task<List<Scheme>?> FetchSchemeData()
        {
            using(var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync("https://api.mfapi.in/mf");
                if(response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return System.Text.Json.JsonSerializer.Deserialize<List<Scheme>>(json)!;
                }
                return null;
            }
        }

        private async Task<List<Scheme>> FetchSchemeData(string SchemeName)
        {
            using(var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync($"https://api.mfapi.in/mf/search?q={SchemeName}");
                if(response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return System.Text.Json.JsonSerializer.Deserialize<List<Scheme>>(json)!;
                }
                return null;
            }
        } 
        #endregion


        #region GetReturns
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
        #endregion


        #region CalculateReturns
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
                .OrderBy(t => DateTime.Parse(t.Date!)) // Order by Date for each group
                .ToList();

                // Find the first purchase transaction date (start date for the scheme)
                var firstPurchaseTransaction = orderedTransactions
                    .Where(t => t.TransactionType == "PURCHASE")
                    .FirstOrDefault();

                if(firstPurchaseTransaction == null || !DateTime.TryParse(firstPurchaseTransaction.Date, out DateTime schemeStartDate))
                    continue; // If no purchase transactions found, skip the scheme

                // Filter NAV data to include only records after the scheme's start date
                var filteredNavData = navData
                    .Where(nav => DateTime.Parse(nav.Date!) >= schemeStartDate)
                    .OrderByDescending(nav => DateTime.Parse(nav.Date!)) // Sort in descending order (from last to first)
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


                    DateTime currentDate = DateTime.Now;

                    // Calculate one day minus the current date
                    string previousDate = currentDate.AddDays(-1).ToString("dd-MM-yyyy");


                    schemeReturns.Add(new
                    {
                        Date = DateTime.Parse(nav.Date).AddDays(1).ToString("dd-MM-yyyy"),
                        //Date = nav.Date,
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
        #endregion


        #region ParseNavData
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
        #endregion
    }
}


public class MutualFundScheme
{
    public int SchemeCode { get; set; }
    public string SchemeName { get; set; }
}

public class MutualFundDetails
{
    public string SchemeName { get; set; }
    public string FundType { get; set; }
    public string Risk { get; set; }
    public Dictionary<string, double>? Returns { get; set; } // e.g., {"1Y": 10.5, "3Y": 15.2}
}

public class MutualFundAnalysis
{
    public string SchemeName { get; set; }
    public string FundType { get; set; }
    public string RiskLevel { get; set; }
    public double ExpectedReturns { get; set; }
}
