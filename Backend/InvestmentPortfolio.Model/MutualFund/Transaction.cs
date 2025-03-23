using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvestmentPortfolio.Model.Models
{
    public class Transaction
    {
        public int SchemeCode { get; set; }
        public string? SchemeName { get; set; }
        public string? TransactionType { get; set; }
        public decimal Units { get; set; }
        public decimal NAV { get; set; }
        public decimal Amount { get; set; }
        public string? Date { get; set; }
    }
}
