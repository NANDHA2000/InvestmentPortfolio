namespace InvestmentPortfolio.Model.Models
{
    public class StockData
    {
        public string? Stockname { get; set; }
        public string? ISIN { get; set; }
        public string? Quantity { get; set; }
        public string? Buyprice { get; set; }
        public string? Buyvalue { get; set; }
        public string? Sellprice { get; set; }
        public string? Sellvalue { get; set; }
        public string? RealisedPL { get; set; }
        //public string? RealisedPnLPercentage { get; set; }
        public string? Buydate { get; set; }
        public string? Selldate { get; set; }
    }

}
