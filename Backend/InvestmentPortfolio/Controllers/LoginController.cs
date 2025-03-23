using InvestmentPortfolio.Model.Models;
using InvestmentPortfolio.Service.IService;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace InvestmentPortfolio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = false)]
    public class LoginController : ControllerBase
    {
        private readonly IAuthService _authService;

        public LoginController(IAuthService authService)
        {
            _authService = authService;
        }

        string jsonFilePath = @"D:\InvestmentPortpolio\Backend\Data\UserData.json";

        #region Login

        [HttpPost("Login")]
        public async Task<IActionResult> Login(User user)
        {
            try
            {
                var isValidUser = await _authService.ValidateUser(user);

                if(isValidUser)
                {
                    return Ok(new { success = true, message = "Login successful" });
                }
                else
                {
                    return Ok(new { success = false, message = "Invalid username or password" });
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        #endregion


        #region Register
        [HttpPost("Register")]
        public async Task<IActionResult> Register(User user)
        {
            try
            {
                var users = new List<User>();

                if(System.IO.File.Exists(jsonFilePath))
                {
                    var jsonData = await System.IO.File.ReadAllTextAsync(jsonFilePath);

                    if(!string.IsNullOrWhiteSpace(jsonData))
                    {
                        try
                        {
                            users = JsonSerializer.Deserialize<List<User>>(jsonData) ?? new List<User>();
                        }
                        catch(JsonException)
                        {
                            // Handle invalid JSON format
                            return BadRequest(new { success = false, message = "Invalid JSON format in the file." });
                        }
                    }
                }

                // Check if email already exists
                if(users.Any(u => u.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase)))
                {
                    return BadRequest(new { success = false, message = "Email already exists. Please use a different email." });
                }

                users.Add(user);

                // Serialize the stock data list to JSON
                var updatedJsonData = JsonSerializer.Serialize(users, new JsonSerializerOptions
                {
                    WriteIndented = true // For readable JSON format
                });

                // Write JSON data to the specified file path
                await System.IO.File.WriteAllTextAsync(jsonFilePath, updatedJsonData);

                return Ok(new { success = true, message = "User registered successfully." });

            }
            catch(Exception ex)
            {

                Console.WriteLine($"An error occurred: {ex.Message}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
        #endregion


        #region NavBar
        [HttpGet]
        [Route("GetNavBar")]
        public async Task<IActionResult> GetNavBar()
        {

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Data\\Navbar.json");

            var jsonData = await System.IO.File.ReadAllTextAsync(filePath);

            var navBarData = JsonSerializer.Deserialize<List<NavBar>>(jsonData);

            return Ok(navBarData);

        } 
        #endregion

    }
}
