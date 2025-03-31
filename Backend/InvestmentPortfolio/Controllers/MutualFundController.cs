using InvestmentPortfolio.Model.Models;
using InvestmentPortfolio.Model.MutualFund;
using InvestmentPortfolio.Service.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System.Net.Http;
using System.Text.Json;
using static InvestmentPortfolio.Controllers.MutualFundController1;


namespace InvestmentPortfolio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MutualFundController : ControllerBase
    {

        private readonly IMutualFundService _mutualFundService;
        private readonly IVaultService _vaultService;

        public MutualFundController(IMutualFundService mutualFundService, IVaultService vaultService)
        {

            _mutualFundService = mutualFundService;
            _vaultService = vaultService;
        }


        [HttpGet("GetSchemeNames")]
        public IActionResult GetSchemeNames()
        {
            var schemeNames = new List<string>();

            try
            {
                // Define the JSON file path
                var jsonFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "DayBydayMfPerformance.json");

                // Read the JSON file
                if(!System.IO.File.Exists(jsonFilePath))
                {
                    return NotFound("JSON file not found.");
                }

                string jsonInput = System.IO.File.ReadAllText(jsonFilePath);

                var jsonArray = System.Text.Json.JsonSerializer.Deserialize<List<List<Model.Models.Scheme>>>(jsonInput);

                // Log deserialized data for debugging
                if(jsonArray == null)
                {
                    Console.WriteLine("Deserialization returned null.");
                }
                else
                {
                    Console.WriteLine($"Deserialized JSON: {System.Text.Json.JsonSerializer.Serialize(jsonArray)}");
                }

                // Extract SchemeNames
                if(jsonArray != null)
                {
                    foreach(var innerList in jsonArray)
                    {
                        foreach(var scheme in innerList)
                        {
                            schemeNames.Add(scheme.SchemeName);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                return BadRequest($"Error parsing JSON: {ex.Message}");
            }

            // Check if schemeNames is empty
            if(schemeNames.Count == 0)
            {
                Console.WriteLine("No scheme names found in the JSON.");
            }

            // Return the list of SchemeNames
            return Ok(schemeNames);
        }




        [HttpGet("GetData")]
        public async Task<IActionResult> GetData([FromQuery] string schemeName, [FromQuery] string? fromDate, [FromQuery] string? toDate)
        {

            if (string.IsNullOrEmpty(fromDate) && string.IsNullOrEmpty(toDate))
            { 
                   DateTime dateTime = DateTime.Now;
                   fromDate = dateTime.AddDays(-30).ToString("yyyy-MM-dd");
                   toDate = dateTime.ToString("yyyy-MM-dd");
            }
            // Path to the JSON file
            var jsonFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "DayBydayMfPerformance.json");

            // Read the JSON content
            if(!System.IO.File.Exists(jsonFilePath))
            {
                return NotFound("JSON file not found.");
            }

            var jsonContent = await System.IO.File.ReadAllTextAsync(jsonFilePath);

            // Deserialize JSON content into a list of lists
            var schemesList = System.Text.Json.JsonSerializer.Deserialize<List<List<Model.Models.Scheme>>>(jsonContent);

            if(schemesList == null || !schemesList.Any())
            {
                return NotFound("No data found in the JSON file.");
            }

            // Flatten the list if needed
            var schemes = schemesList.SelectMany(s => s).ToList();

            // Filter schemes based on schemeName if provided
            if(!string.IsNullOrEmpty(schemeName))
            {
                schemes = schemes.Where(s => s.SchemeName.Equals(schemeName, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if(!schemes.Any())
            {
                return NotFound("No matching schemes found.");
            }

            // Filter SchemeReturns based on fromDate and toDate
            if(!string.IsNullOrEmpty(fromDate) && DateTime.TryParse(fromDate, out DateTime from))
            {
                schemes.ForEach(s =>
                {
                    s.SchemeReturns = s.SchemeReturns.Where(r => DateTime.TryParse(r.Date, out DateTime date) && date >= from).ToList();
                });
            }

            if(!string.IsNullOrEmpty(toDate) && DateTime.TryParse(toDate, out DateTime to))
            {
                schemes.ForEach(s =>
                {
                    s.SchemeReturns = s.SchemeReturns.Where(r => DateTime.TryParse(r.Date, out DateTime date) && date <= to).ToList();
                });
            }

            // If no SchemeReturns left after filtering, return an appropriate response
            if(!schemes.Any(s => s.SchemeReturns.Any()))
            {
                return NotFound("No data found for the specified date range.");
            }

            return Ok(schemes);
        }

        [HttpGet("DayPerformanceMF")]
        public async Task<IActionResult> DayPerformanceMF()
        {

            var responseData = string.IsNullOrEmpty;
                //_vaultService.GetAllFiles();

/*            if(responseData == null || !responseData.Any())
            {
                return BadRequest("Invalid JSON input.");
            }*/

            string StrResult = System.Text.Json.JsonSerializer.Serialize(responseData);

            // Deserialize the JSON input
            var fileDetails = JsonConvert.DeserializeObject<List<Model.MutualFund.FileDetail>>(StrResult);

            if(fileDetails == null || fileDetails.Count == 0)
            {
                return BadRequest("No file details found in the JSON.");
            }

            var allReturns = new List<object>(); // To store returns for all files

            // Loop through each file in the list
            foreach(var fileDetail in fileDetails)
            {

                if(fileDetail.FolderName != "MF_DayPerformance")
                {
                    continue;
                }

                if(!System.IO.File.Exists(fileDetail.FilePath))
                {
                    return NotFound($"File not found at path: {fileDetail.FilePath}");
                }

                // Read the file into a stream
                using var fileStream = new FileStream(fileDetail.FilePath, FileMode.Open, FileAccess.Read);
                var formFile = new FormFile(fileStream, 0, fileStream.Length, "file", fileDetail.FileName);

                // Call the UploadExcel method for each file
                var result = await _mutualFundService.UploadExcel(formFile);
                allReturns.Add(result);

            }
            return Ok(allReturns);
        }
    }
}
