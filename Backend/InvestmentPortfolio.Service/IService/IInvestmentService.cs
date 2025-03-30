using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvestmentPortfolio.Service.IService
{
    public interface IInvestmentService
    {

        Task<object> GetInvestmentDetailsAsync(string investmentName);
        Task<string> GetInvestmentDetailsAsync(int investmentTypeId);
        Task<(bool success, string message)> ProcessGrowwReportAsync(IFormFile file, string fileName);
    }
}
