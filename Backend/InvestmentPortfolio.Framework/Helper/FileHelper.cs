using Microsoft.Extensions.Configuration;
using System.Reflection;


namespace InvestmentPortfolio.Framework.Helper
{
    public class FileHelper
    {
        private readonly IConfiguration _configuration;

        public FileHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GetUserFilePath(string fileName)
        {
            string directoryPath = _configuration["FileSettings:BaseDirectory"]!;
            string filePath = Path.Combine(directoryPath, $"{fileName}.json");

            if(File.Exists(filePath))
            {
                return filePath;
            }

            throw new FileNotFoundException($"File '{fileName}.json' not found in directory '{directoryPath}'");
        }

        public string GetFilePath(string fileName)
        {
            string baseDirectory = _configuration["FileSettings:BaseDirectory"]!;
            string[] files = Directory.GetFiles(baseDirectory, $"{fileName}.json", SearchOption.AllDirectories);

            if(files.Length > 0)
            {
                return files[0]; // Return the first matching file
            }

            throw new FileNotFoundException($"File '{fileName}.json' not found in directory '{baseDirectory}' or its subdirectories.");
        }


    }
}
