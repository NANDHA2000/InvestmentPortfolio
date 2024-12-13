using InvestmentPortfolio.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvestmentPortfolio.Service.IService
{
    public interface IAuthService
    {

        Task<bool> ValidateUser(User user);
    }
}
