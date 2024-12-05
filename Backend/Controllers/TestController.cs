using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace YourProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MutualFundController1 : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public MutualFundController1(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Define a class to hold the response from the external API
        public class NavData
        {
            public string Date { get; set; }
            public decimal Nav { get; set; }
        }

        // Transaction class to store investment actions (PURCHASE/REDEEM)
        public class Transaction
        {
            public int SchemeCode { get; set; }
            public string? TransactionType { get; set; }
            public decimal Units { get; set; }
            public decimal NAV { get; set; }
            public decimal Amount { get; set; }
            public string Date { get; set; }
        }

        // Example of transactions (you can add more transactions here)
        private readonly List<Transaction> _transactions = new List<Transaction>
        {
            new Transaction { SchemeCode = 120621, TransactionType = "PURCHASE", Units = 14.02m, NAV = 214.05m, Amount = 2999m, Date = "13 Sep 2024" },
            new Transaction { SchemeCode = 120621, TransactionType = "REDEEM", Units = 2.47m, NAV = 202.21m, Amount = 500m, Date = "25 Nov 2024" },
            new Transaction { SchemeCode = 120621, TransactionType = "PURCHASE", Units = 23.93m, NAV = 208.91m, Amount = 4999m, Date = "09 Jul 2024" },
            new Transaction { SchemeCode = 120621, TransactionType = "REDEEM", Units = 2.38m, NAV = 210.36m, Amount = 500m, Date = "28 Aug 2024" },
            new Transaction { SchemeCode = 120621, TransactionType = "PURCHASE", Units = 175.41m, NAV = 188.12m, Amount = 32998m, Date = "15 May 2024" },
            new Transaction { SchemeCode = 120621, TransactionType = "REDEEM", Units = 2.52m, NAV = 198m, Amount = 500m, Date = "10 Jun 2024" }
        };

        [HttpGet("getReturns")]
        public async Task<IActionResult> GetReturns()
        {
            var apiUrl = "https://api.mfapi.in/mf/120621"; // The URL of the external API

            // Send a GET request to the external API
            var response = await _httpClient.GetAsync(apiUrl);

            // Check if the response is successful
            if(response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                var navData = ParseNavData(data);

                // Calculate the returns for each transaction based on the NAV data
                var returns = CalculateReturns(navData);

                // Return the returns as JSON
                return Ok(returns);
            }
            else
            {
                // Return an error if the API call fails
                return StatusCode((int)response.StatusCode, "Failed to fetch NAV data");
            }
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

        // Calculate the returns for each investment based on the NAV data

        private List<object> CalculateReturns(List<NavData> navData)
        {
            var result = new List<object>();

            // Group transactions by SchemeCode
            var groupedTransactions = _transactions
                .GroupBy(t => t.SchemeCode)
                .ToList();

            foreach(var schemeGroup in groupedTransactions)
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
                    SchemeCode = schemeGroup.Key,
                    SchemeReturns = schemeReturns
                });
            }

            return result;
        }






    }
}
