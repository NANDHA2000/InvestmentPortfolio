using InvestmentPortfolio.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace InvestmentPortfolio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {

        string jsonFilePath = @"D:\InvestmentPortpolio\Backend\Data\UserData.json";

        #region Login

        [HttpPost("Login")]
        public async Task<IActionResult> Login(User user)
        {
            try
            {

                var jsonContent = await System.IO.File.ReadAllTextAsync(jsonFilePath);

                var users = JsonSerializer.Deserialize<List<User>>(jsonContent);

                if(users == null)
                {
                    Console.WriteLine("No users found in the JSON file.");
                    return Ok(new { success = false, message = "No users found in the JSON file." });
                }

                // Check if the email and password pair exists
                var userExists = users.Exists(users => users.Email == user.Email && users.Password == user.Password);

                if(userExists)
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
                return BadRequest(new { success = false, ex.Message });
            }
        }

        #endregion


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

    }
}
