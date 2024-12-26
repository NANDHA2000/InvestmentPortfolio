using InvestmentPortfolio.Repository.IRepository;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace InvestmentPortfolio.Repository.Repository
{
    public class InvestmentRepository : IInvestmentRepository
    {

        public async Task<T> GetInvestmentDataAsync<T>(string filePath)
        {
            var jsonData = await System.IO.File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<T>(jsonData);
        }

        public async Task<string> SaveUploadedFileAsync(IFormFile file)
        {
            var filePath = Path.GetTempFileName();
            using(var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return filePath;
        }

        public async Task<bool> WriteJsonDataAsync<T>(T data, string filePath)
        {
            var jsonData = JsonSerializer.Serialize(data);
            await System.IO.File.WriteAllTextAsync(filePath, jsonData);
            return true;
        }
    }
}
