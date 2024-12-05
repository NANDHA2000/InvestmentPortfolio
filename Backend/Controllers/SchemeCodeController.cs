using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System.Text.Json;

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
                        var schemeName = worksheet.Cells[row, 1].Text.Trim();

                        var normalizedSchemeName = NormalizeString(schemeName);
                        var matchingScheme = schemeData
                            .FirstOrDefault(s => NormalizeString(s.schemeName).Equals(normalizedSchemeName, StringComparison.OrdinalIgnoreCase));

                        var schemeCode = matchingScheme?.schemeCode.ToString() ?? "Not Found";

                        // Find the matching scheme code
                        // var schemeCode = schemeData.FirstOrDefault(s => s.schemeName.Equals(schemeName, StringComparison.OrdinalIgnoreCase))?.schemeCode.ToString() ?? "Not Found";

                        var transaction = new Transaction
                        {
                            SchemeCode = schemeCode,
                            SchemeName = schemeName,
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


        private string NormalizeString(string input)
        {
            if(string.IsNullOrWhiteSpace(input)) return string.Empty;

            // Convert to lowercase, remove extra spaces, and common noise words
            var noiseWords = new[] { "Fund", "Plan", "-", "Direct", "Growth" };
            return string.Join(" ",
                input.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                     .Where(word => !noiseWords.Contains(word, StringComparer.OrdinalIgnoreCase))
                     .Select(word => word.Trim().ToLower()));
        }

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

        // Supporting classes
        public class Scheme
        {
            public int schemeCode { get; set; }
            public string schemeName { get; set; }
        }

        public class Transaction
        {
            public string SchemeCode { get; set; }
            public string SchemeName { get; set; }
            public string TransactionType { get; set; }
            public decimal Units { get; set; }
            public decimal NAV { get; set; }
            public decimal Amount { get; set; }
            public string Date { get; set; }
        }

    }
}
