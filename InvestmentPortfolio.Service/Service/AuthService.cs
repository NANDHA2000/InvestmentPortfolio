using InvestmentPortfolio.Framework.Helper;
using InvestmentPortfolio.Model.Models;
using InvestmentPortfolio.Service.IService;
using System.Text.Json;


namespace InvestmentPortfolio.Service.Service
{
    public class AuthService : IAuthService
    {

        private readonly FileHelper _fileHelper;
        private readonly string fileName = "UserData";

        public AuthService(FileHelper fileHelper)
        {
            _fileHelper = fileHelper;
        }

        public async Task<bool> ValidateUser(User user)
        {
            try
            {
                var filePath = _fileHelper.GetUserFilePath(fileName);
                var jsonContent = await File.ReadAllTextAsync(filePath);
                var users = JsonSerializer.Deserialize<List<User>>(jsonContent);

                if(users == null)
                {
                    Console.WriteLine("No users found in the JSON file.");
                    return false;
                }

                // Check if the email and password pair exists
                return users.Exists(u => u.Email == user.Email && u.Password == user.Password);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw;
            }
        }
    }
}
