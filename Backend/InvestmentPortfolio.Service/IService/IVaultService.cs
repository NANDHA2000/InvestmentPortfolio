﻿using InvestmentPortfolio.Model.Models;
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
        Task<string> UploadFileAsync(IFormFile file, string fileName);
        Task<List<dynamic>> GetAllFilesAsync();
        Task<FileModel> DownloadFileAsync(int fileId);
        Task<string> DeleteFileAsync(int fileId);
        Task<(byte[] FileContent, string FileType)> ViewFileAsync(int fileId);

        /*Task<string> UploadExcelFile(IFormFile file, string fileName);
        List<object> GetAllFiles();
        List<string> DownloadFile(string fileName);
        string DeleteFile(string fileName);*/
    }
}
