using InvestmentPortfolio.Service.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace InvestmentPortfolio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VaultController : ControllerBase
    {

        private readonly IVaultService _vaultService;

        public VaultController(IVaultService vaultService)
        {
            _vaultService = vaultService;
        }


        [HttpPost("upload")]
        public async Task<IActionResult> UploadExcelFile(IFormFile file,string fileName)
        {
            if(file == null || file.Length == 0)
            {
                return BadRequest("Please upload a valid file.");
            }

            var filePath = _vaultService.UploadExcelFile(file, fileName);

            return Ok(new { Message = "File uploaded successfully", FilePath = filePath });
        }



        [HttpGet("files")]
        public IActionResult GetAllFiles()
        {

            var allFiles = _vaultService.GetAllFiles();
            return Ok(allFiles);
        }


        [HttpGet("download/{fileName}")]
        public IActionResult DownloadFile(string fileName)
        {

            var matchingFile = _vaultService.DownloadFile(fileName);

            if(matchingFile == null)
            {
                return NotFound("File not found.");
            }

            string filePath = matchingFile.FirstOrDefault(fileName);

            // Read the file bytes and return it for download
            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/octet-stream", fileName);
        }


        [HttpDelete("files/delete/{fileName}")]
        public IActionResult DeleteFile(string fileName)
        {
            // Call the service method to delete the file
            var result = _vaultService.DeleteFile(fileName);

            // Check the result and return appropriate HTTP responses
            if(result == "File not found.")
            {
                return NotFound(new { message = result });
            }
            else if(result.StartsWith("Error"))
            {
                return StatusCode(500, new { message = result });
            }

            return Ok(new { message = result });
        }



        /* [HttpGet("files/view/{fileName}")]
         public IActionResult ViewFile(string fileName)
         {
             // Define the folder path where files are stored
             string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "UploadedFiles/Stocks");
             var filePath = Path.Combine(folderPath, fileName);

             if(!System.IO.File.Exists(filePath))
             {
                 return NotFound("File not found.");
             }

             // Returning the file path to be opened in the browser
             return new PhysicalFileResult(filePath, "application/octet-stream");
         }*/


        [HttpGet("files/view/{fileName}")]
        [Produces("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
        [ProducesResponseType(typeof(FileResult), 200)]
        [ProducesResponseType(404)]
        public IActionResult ViewFile(string fileName)
        {

            string folderPath = @"D:\Publish\InvestmentPortfolio\Backend\UploadedFiles\Stocks\Stocks_PnL_Report_3948949075_01-12-2021_26-11-2024.xlsx";
            //string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "UploadedFiles/Stocks");
            //var filePath = Path.Combine(folderPath, fileName);

            if(!System.IO.File.Exists(folderPath))
            {
                return NotFound("File not found.");
            }

            var data = new PhysicalFileResult(folderPath, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            return data;
        }








    }
}
