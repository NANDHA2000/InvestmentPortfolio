using ExcelDataReader;
using InvestmentPortfolio.Framework.Constants;
using InvestmentPortfolio.Model.Models;
using InvestmentPortfolio.Repository.IRepository;
using InvestmentPortfolio.Service.IService;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvestmentPortfolio.Service.Service
{
    public class InvestmentService : IInvestmentService
    {

        private readonly IInvestmentRepository _investmentRepository;
        private readonly IVaultService _vaultService;

        public InvestmentService(IInvestmentRepository investmentRepository, IVaultService vaultService)
        {
            _investmentRepository = investmentRepository;
            _vaultService = vaultService;
        }


        public async Task<object> GetInvestmentDetailsAsync(string investmentName)
        {
            if(investmentName == NamingConstant.Stocks)
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), LocationConstant.StocksData);
                return await _investmentRepository.GetInvestmentDataAsync<Stocks>(filePath);
            }
            else if(investmentName == NamingConstant.MutualFund)
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), LocationConstant.MutualFundData);
                return await _investmentRepository.GetInvestmentDataAsync<MutualFund>(filePath);
            }

            return null; // Or handle unknown investmentName cases appropriately
        }


        public async Task<(bool success, string message)> ProcessGrowwReportAsync(IFormFile file, string fileName)
        {
            if(file == null || file.Length == 0 || string.IsNullOrEmpty(fileName))
                return (false, MessageConstant.NoFileData);

            try
            {
                // Save uploaded file to a temporary location
                var tempFilePath = await _investmentRepository.SaveUploadedFileAsync(file);

                // Read and process the Excel file
                var dataTable = ReadGrowwReport(tempFilePath);

                if(fileName == NamingConstant.Stocks)
                {
                    var stockResult = ProcessGrowwStockData(dataTable);
                    //var jsonFilePath = Path.Combine(Directory.GetCurrentDirectory(), LocationConstant.StocksData);
                    //await _vaultService.UploadExcelFile(file, fileName);
                    //await _investmentRepository.WriteJsonDataAsync(stockResult, jsonFilePath);
                    await _investmentRepository.SaveStockDataAsync(stockResult);
                }
                else if(fileName == NamingConstant.MutualFund)
                {
                    var mutualFundResult = ProcessGrowwReportData(dataTable);
                    //var jsonFilePath = Path.Combine(Directory.GetCurrentDirectory(), LocationConstant.MutualFundData);
                    //await _vaultService.UploadExcelFile(file, fileName);
                    //await _investmentRepository.WriteJsonDataAsync(mutualFundResult, jsonFilePath);
                    await _investmentRepository.SaveMutualFundDataAsync(mutualFundResult);
                }
                else if(fileName == NamingConstant.MF_DayPerformance) 
                {
                    await _vaultService.UploadExcelFile(file, fileName);
                }
                else
                {
                    return (false, MessageConstant.InvalidFileName);
                }

                return (true, MessageConstant.FileDataAdded);
            }
            catch(Exception ex)
            {
                throw new Exception($"Error processing file: {ex.Message}");
            }
        }

        public async Task<string> GetInvestmentDetailsAsync(int investmentTypeId)
        {
            // Call the repository to get investment details
            return await _investmentRepository.GetInvestmentDetailsAsync(investmentTypeId);
        }



        #region Read Excel file
        private DataTable ReadGrowwReport(string filePath)
        {
            try
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
                using var reader = ExcelReaderFactory.CreateReader(stream);

                var result = reader.AsDataSet();
                if(result.Tables.Count == 0)
                    throw new Exception("The uploaded file does not contain any data.");

                return result.Tables[0];
            }
            catch(Exception ex)
            {
                throw new Exception($"Failed to read the Excel file: {ex.Message}");
            }
        }
        #endregion


        #region MF
        // Updated method to process and structure the data
        private object ProcessGrowwReportData(DataTable table)
        {
            var report = new
            {
                PersonalDetails = new
                {
                    Name = table.Rows[3]["Column1"]?.ToString(),
                    MobileNumber = table.Rows[4]["Column1"]?.ToString(),
                    PAN = table.Rows[5]["Column1"]?.ToString()
                },
                HoldingSummary = new
                {
                    TotalInvestments = table.Rows[13]["Column0"]?.ToString(),
                    CurrentPortfolioValue = table.Rows[13]["Column1"]?.ToString(),
                    ProfitLoss = table.Rows[13]["Column2"]?.ToString(),
                    ProfitLossPercentage = table.Rows[13]["Column3"]?.ToString(),
                    XIRR = table.Rows[13]["Column4"]?.ToString()
                },
                HoldingsDate = table.Rows[18]["Column0"]?.ToString(),
                Holdings = GetHoldings(table)
            };

            return report;
        }

        //Extract holdings details from the table
        private List<object> GetHoldings(DataTable table)
        {
            var holdings = new List<object>();

            // Adjust the starting index (19) based on where holdings start
            for(int i = 19; i < table.Rows.Count; i++)
            {
                var row = table.Rows[i];

                var holding = new Dictionary<string, object>();

                // Add non-empty fields to the dictionary
                if(!string.IsNullOrWhiteSpace(row["Column0"]?.ToString()))
                    holding["SchemeName"] = row["Column0"]?.ToString()!;
                if(!string.IsNullOrWhiteSpace(row["Column1"]?.ToString()))
                    holding["AMC"] = row["Column1"]?.ToString()!;
                if(!string.IsNullOrWhiteSpace(row["Column2"]?.ToString()))
                    holding["Category"] = row["Column2"]?.ToString()!;
                if(!string.IsNullOrWhiteSpace(row["Column3"]?.ToString()))
                    holding["SubCategory"] = row["Column3"]?.ToString()!;
                if(!string.IsNullOrWhiteSpace(row["Column4"]?.ToString()))
                    holding["FolioNo"] = row["Column4"]?.ToString()!;
                if(!string.IsNullOrWhiteSpace(row["Column5"]?.ToString()))
                    holding["Source"] = row["Column5"]?.ToString()!;
                if(!string.IsNullOrWhiteSpace(row["Column6"]?.ToString()))
                    holding["Units"] = row["Column6"]?.ToString()!;
                if(!string.IsNullOrWhiteSpace(row["Column7"]?.ToString()))
                    holding["InvestedValue"] = row["Column7"]?.ToString()!;
                if(!string.IsNullOrWhiteSpace(row["Column8"]?.ToString()))
                    holding["CurrentValue"] = row["Column8"]?.ToString()!;
                if(!string.IsNullOrWhiteSpace(row["Column9"]?.ToString()))
                    holding["Returns"] = row["Column9"]?.ToString()!;
                if(!string.IsNullOrWhiteSpace(row["Column10"]?.ToString()))
                    holding["XIRR"] = row["Column10"]?.ToString()!;

                holdings.Add(holding);
            }

            return holdings;
        }


        #endregion


        #region Stocks
        private object ProcessGrowwStockData(DataTable table)
        {
            var report = new
            {
                PersonalDetails = new
                {
                    Name = table.Rows[0]["Column1"]?.ToString(),
                    UniqueClientCode = table.Rows[1]["Column1"]?.ToString(),
                    PLStatement = table.Rows[3]["Column0"]?.ToString(),
                },
                ProfitAndLoss = new
                {
                    RealisedPL = table.Rows[8]["Column1"]?.ToString(),
                    UnRealisedPL = table.Rows[9]["Column1"]?.ToString(),
                },
                Charges = new
                {
                    ExchangeTransactionCharges = table.Rows[12]["Column1"]?.ToString(),
                    SEBICharges = table.Rows[13]["Column1"]?.ToString(),
                    STT = table.Rows[14]["Column1"]?.ToString(),
                    StampDuty = table.Rows[15]["Column1"]?.ToString(),
                    IPFTCharges = table.Rows[16]["Column1"]?.ToString(),
                    Brokerage = table.Rows[17]["Column1"]?.ToString(),
                    DPCharges = table.Rows[18]["Column1"]?.ToString(),
                    TotalGST = table.Rows[19]["Column1"]?.ToString(),
                    Total = table.Rows[20]["Column1"]?.ToString(),
                },
                Holdings = GetStockHoldings(table)
            };

            return report;
        }

        private List<object> GetStockHoldings(DataTable table)
        {
            var holdings = new List<object>();

            // Adjust the starting index based on where holdings start
            for(int i = 25; i < table.Rows.Count; i++)
            {
                var row = table.Rows[i];
                // Check if the row contains valid data for stock holdings
                if(string.IsNullOrWhiteSpace(row["Column0"]?.ToString()) ||
                    row["Column0"]?.ToString()!.Contains("Disclaimer", StringComparison.OrdinalIgnoreCase) == true ||
                    row["Column0"]?.ToString()!.Contains("This report is provided", StringComparison.OrdinalIgnoreCase) == true ||
                    row["Column0"]?.ToString()!.Contains("Groww Invest Tech Private Limited", StringComparison.OrdinalIgnoreCase) == true)
                {
                    // Skip unwanted or disclaimer rows
                    continue;
                }

                var holding = new Dictionary<string, object>();

                // Add non-empty fields to the dictionary
                if(!string.IsNullOrWhiteSpace(row["Column0"]?.ToString()))
                    holding["StockName"] = row["Column0"]?.ToString()!;
                if(!string.IsNullOrWhiteSpace(row["Column1"]?.ToString()))
                    holding["ISIN"] = row["Column1"]?.ToString()!;
                if(!string.IsNullOrWhiteSpace(row["Column2"]?.ToString()))
                    holding["Quantity"] = row["Column2"]?.ToString()!;
                if(!string.IsNullOrWhiteSpace(row["Column3"]?.ToString()))
                    holding["BuyDate"] = row["Column3"]?.ToString()!;
                if(!string.IsNullOrWhiteSpace(row["Column4"]?.ToString()))
                    holding["BuyPrice"] = row["Column4"]?.ToString()!;
                if(!string.IsNullOrWhiteSpace(row["Column5"]?.ToString()))
                    holding["BuyValue"] = row["Column5"]?.ToString()!;
                if(!string.IsNullOrWhiteSpace(row["Column6"]?.ToString()))
                    holding["SellDate"] = row["Column6"]?.ToString()!;
                if(!string.IsNullOrWhiteSpace(row["Column7"]?.ToString()))
                    holding["SellPrice"] = row["Column7"]?.ToString()!;
                if(!string.IsNullOrWhiteSpace(row["Column8"]?.ToString()))
                    holding["SellValue"] = row["Column8"]?.ToString()!;
                if(!string.IsNullOrWhiteSpace(row["Column9"]?.ToString()))
                    holding["RealisedPL"] = row["Column9"]?.ToString()!;
                if(!string.IsNullOrWhiteSpace(row["Column10"]?.ToString()))
                    holding["Remarks"] = row["Column10"]?.ToString()!;

                holdings.Add(holding);
            }

            return holdings;
        }
        #endregion
    }
}
