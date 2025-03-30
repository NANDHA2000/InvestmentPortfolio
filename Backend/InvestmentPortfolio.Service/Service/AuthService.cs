using InvestmentPortfolio.Framework.Helper;
using InvestmentPortfolio.Model.Models;
using InvestmentPortfolio.Repository.IRepository;
using InvestmentPortfolio.Service.IService;
using System.Text.Json;


namespace InvestmentPortfolio.Service.Service
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly FileHelper _fileHelper;
        private readonly string fileName = "UserData";

        public AuthService(FileHelper fileHelper, IAuthRepository authRepository)
        {
            _fileHelper = fileHelper;
            _authRepository = authRepository;

        }

        public async Task<bool> ValidateUser(string email, string password)
        {
            return await _authRepository.ValidateUser(email, password);
        }


        public async Task<bool> RegisterUser(User user)
        {
            return await _authRepository.RegisterUser(user);
        }

        /*public async Task<bool> ValidateUser(User user)
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
        }*/
    }
}
