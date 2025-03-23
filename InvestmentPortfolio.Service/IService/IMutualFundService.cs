using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvestmentPortfolio.Service.IService
{
    public interface IMutualFundService
    {
        Task<object> UploadExcel(IFormFile file);
        Task<List<MutualFundAnalysis>> GetTopFundsAsync();
    }
}
