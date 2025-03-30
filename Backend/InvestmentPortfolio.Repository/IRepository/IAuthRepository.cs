using InvestmentPortfolio.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvestmentPortfolio.Repository.IRepository
{
    public interface IAuthRepository
    {
        Task<bool> RegisterUser(User user);
        Task<bool> ValidateUser(string email, string password);
    }
}
