using Dapper;
using InvestmentPortfolio.Model.Models;
using InvestmentPortfolio.Repository.IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace InvestmentPortfolio.Repository.Repository
{
    public class InvestmentRepository : IInvestmentRepository
    {
        private readonly IConfiguration _configuration;

        public InvestmentRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

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


        public async Task SaveStockDataAsync(dynamic stockData)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                // Deserialize JSON to strongly-typed object
                var data = JsonSerializer.Deserialize<Stocks>(JsonSerializer.Serialize(stockData));

                // Ensure PersonalDetails exist
                if(data?.PersonalDetails == null)
                {
                    throw new ArgumentNullException(nameof(data.PersonalDetails), "Personal details are required.");
                }

                // 1. Insert PersonalDetails and get the ID
                var parameters = new DynamicParameters();
                parameters.Add("@Name", data.PersonalDetails.Name);
                parameters.Add("@UniqueClientCode", data.PersonalDetails.UniqueClientCode);
                parameters.Add("@PLStatement", data.PersonalDetails.PLStatement);
                parameters.Add("@PersonalDetailId", dbType: DbType.Int32, direction: ParameterDirection.Output);

                await connection.ExecuteAsync(
                    "sp_InsertPersonalDetails",
                    parameters,
                    transaction,
                    commandType: CommandType.StoredProcedure
                );

                var personalDetailId = parameters.Get<int>("@PersonalDetailId");
                var uniqueClientCode = data.PersonalDetails.UniqueClientCode;

                // 2. Insert ProfitAndLoss (if exists)
                if(data.ProfitAndLoss != null)
                {
                    await connection.ExecuteAsync(
                        "sp_InsertProfitAndLoss",
                        new
                        {
                            UniqueClientCode = data.PersonalDetails.UniqueClientCode, // ✅ Corrected parameter
                            RealisedPL = ConvertToDecimal(data.ProfitAndLoss.RealisedPL),
                            UnRealisedPL = ConvertToDecimal(data.ProfitAndLoss.UnRealisedPL)
                        },
                        transaction,
                        commandType: CommandType.StoredProcedure
                    );
                }

                // 3. Insert Charges (if exists)
                if(data.Charges != null)
                {
                    await connection.ExecuteAsync(
                        "sp_InsertCharges",
                        new
                        {
                            UniqueClientCode = data.PersonalDetails.UniqueClientCode,
                            ExchangeTransactionCharges = ConvertToDecimal(data.Charges.ExchangeTransactionCharges),
                            SEBICharges = ConvertToDecimal(data.Charges.SEBICharges),
                            STT = ConvertToDecimal(data.Charges.STT),
                            StampDuty = ConvertToDecimal(data.Charges.StampDuty),
                            IPFTCharges = ConvertToDecimal(data.Charges.IPFTCharges),
                            Brokerage = ConvertToDecimal(data.Charges.Brokerage),
                            DPCharges = ConvertToDecimal(data.Charges.DPCharges),
                            TotalGST = ConvertToDecimal(data.Charges.TotalGST),
                            Total = ConvertToDecimal(data.Charges.Total)
                        },
                        transaction,
                        commandType: CommandType.StoredProcedure
                    );
                }

                // 4. Insert Holdings (if exists)
                if(data.Holdings != null)
                {
                    foreach(var holding in data.Holdings)
                    {
                        await connection.ExecuteAsync(
                            "sp_InsertHoldings",
                            new
                            {
                                UniqueClientCode = data.PersonalDetails.UniqueClientCode,
                                StockName = holding.StockName,
                                ISIN = holding.ISIN,
                                Quantity = ConvertToInt(holding.Quantity),
                                BuyDate = ParseNullableDate(holding.BuyDate),
                                BuyPrice = ConvertToDecimal(holding.BuyPrice),
                                BuyValue = ConvertToDecimal(holding.BuyValue),
                                SellDate = ParseNullableDate(holding.SellDate),
                                SellPrice = ConvertToDecimal(holding.SellPrice),
                                SellValue = ConvertToDecimal(holding.SellValue),
                                RealisedPL = ConvertToDecimal(holding.RealisedPL)
                            },
                            transaction,
                            commandType: CommandType.StoredProcedure
                        );
                    }
                }

                await transaction.CommitAsync();
            }
            catch(Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        // Helper Methods for Safe Conversion
        private static decimal? ConvertToDecimal(string? value)
        {
            return decimal.TryParse(value, out var result) ? result : null;
        }

        private static int? ConvertToInt(string? value)
        {
            return int.TryParse(value, out var result) ? result : null;
        }

        private static DateTime? ParseNullableDate(string? dateValue)
        {
            return DateTime.TryParse(dateValue, out var result) ? result : null;
        }


    }
}
