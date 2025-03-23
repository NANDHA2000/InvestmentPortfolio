namespace InvestmentPortfolio.Model.Models
{
    public class MutualFund
    {
        public PersonalDetails PersonalDetails { get; set; }
        public HoldingSummary HoldingSummary { get; set; }
        //public string HoldingsDate { get; set; }
        public List<Holding> Holdings { get; set; }
    }

    public class PersonalDetails
    {
        public string Name { get; set; }
        public string MobileNumber { get; set; }
        public string PAN { get; set; }
    }

    public class HoldingSummary
    {
        public string TotalInvestments { get; set; }
        public string CurrentPortfolioValue { get; set; }
        public string ProfitLoss { get; set; }
        public string ProfitLossPercentage { get; set; }
        public string XIRR { get; set; }
    }

    public class Holding
    {
        public string SchemeName { get; set; }
        public string AMC { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public string FolioNo { get; set; }
        public string Source { get; set; }
        public string Units { get; set; }
        public string InvestedValue { get; set; }
        public string CurrentValue { get; set; }
        public string Returns { get; set; }
        public string XIRR { get; set; }
    }


}

