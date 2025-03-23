using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using static InvestmentPortfolio.Controllers.MutualFundController2;

namespace InvestmentPortfolio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Test1Controller : ControllerBase
    {

        private readonly HttpClient _httpClient;

        public Test1Controller(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }


        private readonly List<string> _targetSchemeNames = new List<string>
        {
               "ICICI Prudential Infrastructure Direct Growth",
               "Quant Small Cap Fund Direct Plan Growth",
               "Parag Parikh Flexi Cap Fund Direct Growth"
        };


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

        


        // No changes needed in the helper method
        private bool IsValidDecimal(object value)
        {
            if(value == null) return false;
            return decimal.TryParse(value.ToString(), out _);
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
                    Date = item["date"].ToString(), // Keeping Date as a string for now
                    Nav = decimal.Parse(item["nav"].ToString())
                });
            }

            return navList;
        }

        // Calculate Returns
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

                        dayReturn = currentValue - previousValue; // Corrected dayReturn calculation

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
        }

    }
}
