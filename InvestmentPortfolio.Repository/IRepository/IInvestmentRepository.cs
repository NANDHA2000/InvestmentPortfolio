﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvestmentPortfolio.Repository.IRepository
{
    public interface IInvestmentRepository
    {
        Task<T> GetInvestmentDataAsync<T>(string filePath);
        Task<string> SaveUploadedFileAsync(IFormFile file);
        Task<bool> WriteJsonDataAsync<T>(T data, string filePath);
    }
}