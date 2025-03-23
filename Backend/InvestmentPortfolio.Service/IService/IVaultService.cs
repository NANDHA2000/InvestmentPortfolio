using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvestmentPortfolio.Service.IService
{
    public interface IVaultService
    {
        Task<string> UploadExcelFile(IFormFile file, string fileName);
        List<object> GetAllFiles();
        List<string> DownloadFile(string fileName);
        string DeleteFile(string fileName);
    }
}
