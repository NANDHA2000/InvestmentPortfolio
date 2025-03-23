using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace InvestmentPortfolio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SchemeCodeController : ControllerBase
    {

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

        private async Task<List<Transaction>> ProcessExcelFile(IFormFile file)
        {
            var transactions = new List<Transaction>();

            // Fetch all scheme data from API
            var schemeData = await FetchSchemeData();

            //var data = await FetchSchemeData1("Quant%20Small%20Cap%20Fund%20Direct%20Plan%20Growth");

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


                        //if(data1.)

                        //  var data2 = FetchSchemeData();


                        // int data2 = 1234;

                        //var normalizedSchemeName = NormalizeString(schemeName);
                        //var matchingScheme = schemeData
                        //.FirstOrDefault(s => NormalizeString(s.schemeName).Equals(schemeName, StringComparison.OrdinalIgnoreCase));


                        //var schemeCode = matchingScheme?.schemeCode ?? 0;

                        // Find the matching scheme code
                        // var schemeCode = schemeData.FirstOrDefault(s => s.schemeName.Equals(schemeName, StringComparison.OrdinalIgnoreCase))?.schemeCode.ToString() ?? "Not Found";



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


/*        //private string NormalizeString(string input)
        //{
        //    if(string.IsNullOrWhiteSpace(input)) return string.Empty;

        //    // Convert to lowercase, remove extra spaces, and common noise words
        //    var noiseWords = new[] { "Fund", "Plan", "-", "Direct", "Growth" };
        //    return string.Join(" ",
        //        input.Split(' ', StringSplitOptions.RemoveEmptyEntries)
        //             .Where(word => !noiseWords.Contains(word, StringComparer.OrdinalIgnoreCase))
        //             .Select(word => word.Trim().ToLower()));
        //}

        private string NormalizeString(string input)
        {
            if(string.IsNullOrWhiteSpace(input)) return string.Empty;

            // Step 1: Remove unnecessary characters (hyphens, extra spaces)
            input = input.Replace("-", " ").Trim();

            // Step 2: Replace common patterns to standardize
            input = input.Replace("Direct Growth", "Direct Plan Growth", StringComparison.OrdinalIgnoreCase);

            // Step 3: Define noise words to remove
            var noiseWords = new[] { "Fund", "Plan", "DirectPlan", "Growth" };

            // Step 4: Normalize the input string
            return string.Join(" ",
                input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries) // Split by spaces
                     .Where(word => !noiseWords.Contains(word, StringComparer.OrdinalIgnoreCase)) // Remove noise words
                     .Select(word => word.Trim()) // Trim extra spaces
            ).ToLower(); // Normalize to lowercase
        }*/




        private async Task<List<Scheme>> FetchSchemeData()
        {
            using(var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync("https://api.mfapi.in/mf");
                if(response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<Scheme>>(json);
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
                    return JsonSerializer.Deserialize<List<Scheme>>(json);
                }
                return null;
            }
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
