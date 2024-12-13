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

        public string GetUserFilePath()
        {
            return _configuration["FileSettings:UserFilePath"]!;
        }

        public string GetEmbeddedUserFileContent()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "InvestmentPortfolio.DataBase.UploadedFiles.UserData.json";

            using(var stream = assembly.GetManifestResourceStream(resourceName))
            using(var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }


}
