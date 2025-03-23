namespace InvestmentPortfolio.Model.Models
{
    public class Stocks
    {
        public StockPersonalDetails? PersonalDetails { get; set; }
        public ProfitAndLoss? ProfitAndLoss { get; set; }
        public Charges? Charges { get; set; }
        public List<StockHolding>? Holdings { get; set; }
    }

    public class StockPersonalDetails
    {
        public string? Name { get; set; }
        public string? UniqueClientCode { get; set; }
        public string? PLStatement { get; set; }
    }

    public class ProfitAndLoss
    {
        public string? RealisedPL { get; set; }
        public string? UnRealisedPL { get; set; }
    }

    public class Charges
    {
        public string? ExchangeTransactionCharges { get; set; }
        public string? SEBICharges { get; set; }
        public string? STT { get; set; }
        public string? StampDuty { get; set; }
        public string? IPFTCharges { get; set; }
        public string? Brokerage { get; set; }
        public string? DPCharges { get; set; }
        public string? TotalGST { get; set; }
        public string? Total { get; set; }

    }

    public class StockHolding
    {
        public string? StockName { get; set; }
        public string? ISIN { get; set; }
        public string? Quantity { get; set; }
        public string? BuyDate { get; set; }
        public string? BuyPrice { get; set; }
        public string? BuyValue { get; set; }
        public string? SellDate { get; set; }
        public string? SellPrice { get; set; }
        public string? SellValue { get; set; }
        public string? RealisedPL { get; set; }
    }
}
