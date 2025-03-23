using InvestmentPortfolio.Service.IService;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace InvestmentPortfolio.Service.Service
{
    public class VaultService:IVaultService
    {

        public async Task<string> UploadExcelFile(IFormFile file, string fileName) 
        {
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

            return filePath;
        }


        public List<object> GetAllFiles() 
        {
            // Define the root directory for uploaded files
            string rootDirectory = Path.Combine(Directory.GetCurrentDirectory(), "UploadedFiles");

            // Get all files from the root directory and subdirectories recursively
            var allFiles = GetFilesRecursive(rootDirectory);

            return allFiles;
        }

        public List<string> DownloadFile(string fileName)
        {
            // Define the root directory for UploadedFiles
            string rootDirectory = Path.Combine(Directory.GetCurrentDirectory(), "UploadedFiles");

            // Get all files in the root directory and its subdirectories
            var allFiles = GetFilesRecursive(rootDirectory);

            // Filter files that match the requested file name
            var matchingFiles = allFiles
                .Cast<dynamic>() // Cast to dynamic to access properties of anonymous types
                .Where(file => file.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase))
                .Select(file => file.FilePath) // Select the file path
                .Cast<string>() // Ensure the result is a List<string>
                .ToList();

            return matchingFiles;
        }


        public string DeleteFile(string fileName) 
        {
            try
            {
                string rootDirectory = Path.Combine(Directory.GetCurrentDirectory(), "UploadedFiles");

                // Get all files in the root directory and its subdirectories
                var allFiles = GetFilesRecursive(rootDirectory);

                // Filter files that match the requested file name
                var matchingFiles = allFiles
                    .Cast<dynamic>() // Cast to dynamic to access properties of anonymous types
                    .Where(file => file.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase))
                    .Select(file => file.FilePath) // Select the file path
                    .Cast<string>() // Ensure the result is a List<string>
                    .ToList();
                foreach(var filePath in matchingFiles)
                {
                    if(File.Exists(filePath))
                    {
                        // Delete the file
                        File.Delete(filePath);
                    }
                    else
                    {
                        return "File not found.";
                    }
                }

                return "File(s) deleted successfully.";
            }
            catch(Exception ex)
            {
                // Handle any exceptions that occur during file deletion
                return $"Error deleting file: {ex.Message}";
            }

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
    }
}
