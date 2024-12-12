using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace InvestmentPortfolio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VaultController : ControllerBase
    {


        [HttpPost("upload")]
        public async Task<IActionResult> UploadExcelFile(IFormFile file,string fileName)
        {
            if(file == null || file.Length == 0)
            {
                return BadRequest("Please upload a valid file.");
            }

            // Define the folder path where the file will be saved
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), $"UploadedFiles/{fileName}");

            // Ensure the folder exists
            if(!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // Construct the file path
            string filePath = Path.Combine(folderPath, file.FileName);

            // Save the file to the folder
            using(var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Ok(new { Message = "File uploaded successfully", FilePath = filePath });
        }



        [HttpGet("files")]
        public IActionResult GetAllFiles()
        {
            // Define the root directory for uploaded files
            string rootDirectory = Path.Combine(Directory.GetCurrentDirectory(), "UploadedFiles");

            if(!Directory.Exists(rootDirectory))
            {
                return NotFound("No files found.");
            }

            // Get all files from the root directory and subdirectories recursively
            var allFiles = GetFilesRecursive(rootDirectory);

            return Ok(allFiles);
        }

        private List<object> GetFilesRecursive(string directoryPath)
        {
            var allFiles = new List<object>();

            // Regex to extract dates from file names
            var regex = new Regex(@"\d{2}-\d{2}-\d{4}");

            // Get all files in the current directory
            var files = Directory.GetFiles(directoryPath)
                .Select(filePath =>
                {
                    var fileName = Path.GetFileName(filePath);
                    var matches = regex.Matches(fileName);

                    // Extract start and end dates if they exist in the file name
                    string startDate = matches.Count > 0 ? matches[0].Value : null!;
                    string endDate = matches.Count > 1 ? matches[1].Value : null!;

                    return new
                    {
                        FolderName = Path.GetFileName(directoryPath), // Folder name (parent folder)
                        FileName = fileName,                          // File name
                        FilePath = filePath,                          // Full file path
                        StartDate = startDate,                        // Extracted start date
                        EndDate = endDate                             // Extracted end date
                    };
                })
                .ToList();

            allFiles.AddRange(files);

            // Recursively get files from subdirectories
            var directories = Directory.GetDirectories(directoryPath);
            foreach(var subDirectory in directories)
            {
                allFiles.AddRange(GetFilesRecursive(subDirectory)); // Recurse into subdirectory
            }

            return allFiles;
        }



        [HttpGet("download/{fileName}")]
        public IActionResult DownloadFile(string fileName)
        {
            // Define the root directory for UploadedFiles
            string rootDirectory = Path.Combine(Directory.GetCurrentDirectory(), "UploadedFiles");

            // Get all files in the root directory and its subdirectories
            var allFiles = GetFilesRecursive(rootDirectory);

            // Find the file path that matches the requested file name
            var matchingFile = allFiles
                .Cast<dynamic>()
                .FirstOrDefault(file => file.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase));

            if(matchingFile == null)
            {
                return NotFound("File not found.");
            }

            string filePath = matchingFile.FilePath;

            // Read the file bytes and return it for download
            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/octet-stream", fileName);
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
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "UploadedFiles/Stocks");
            var filePath = Path.Combine(folderPath, fileName);

            if(!System.IO.File.Exists(filePath))
            {
                return NotFound("File not found.");
            }

            return new PhysicalFileResult(filePath, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }




        [HttpDelete("files/delete/{fileName}")]
        public IActionResult DeleteFile(string fileName)
        {
            // Define the folder path where files are stored
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "UploadedFiles/Stocks");
            var filePath = Path.Combine(folderPath, fileName);

            if(!System.IO.File.Exists(filePath))
            {
                return NotFound("File not found.");
            }

            // Delete the file
            System.IO.File.Delete(filePath);
            return Ok("File deleted successfully.");
        }



    }
}
