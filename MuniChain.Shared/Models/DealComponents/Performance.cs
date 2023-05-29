using System.ComponentModel;

namespace Shared.Models.DealComponents
{
    public class Performance
    {
        public object Clone()
        {
            return MemberwiseClone();
        }

        public string Id { get; set; }
        [DisplayName("Net Interest Cost Percentage")]
        public int? NIC { get; set; }
        [DisplayName("True Interest Cost Percentage")]
        public int? TIC { get; set; }
        [DisplayName("Gross Spread/Bond")]
        public decimal? GrossSpread { get; set; }
        [DisplayName("Takedown/Bond")]
        public decimal? Takedown { get; set; }
        [DisplayName("Total Retail Participation Par")]
        public decimal? TotalRetailPar { get; set; }
        [DisplayName("Total Institutional Participation Par")]
        public decimal? TotalInstitutionalPar { get; set; }
        [DisplayName("Par Amount of Bonds")]
        public decimal? ParAmountBonds { get; set; }
        [DisplayName("Reoffering Premium")]
        public decimal? ReofferingPremium { get; set; }
        [DisplayName("Gross Production")]
        public decimal? GrossProduction { get; set; }
        [DisplayName("Total Underwriter's Discount")]
        public decimal? TotalUnderwriterDiscount { get; set; }
        public decimal? Bid { get; set; }
        [DisplayName("Bid Percentage")]
        public decimal? BidPercentage { get; set; }
        [DisplayName("Bond Year Dollars")]
        public decimal? BondYearDollars { get; set; }
        [DisplayName("Average Life")]
        public int? AverageLife { get; set; }
        [DisplayName("Average Coupon")]
        public int? AverageCoupon { get; set; }
        public string? DealModelId { get; set; }
        public List<TopAccount> TopAccountList { get; set; } = new();
        public byte[] RowVersion { get; set; }
        public DateTime CreatedDateUTC { get; set; }
        public string? HistoryPerformanceID { get; set; }
    }

    public class TopAccount
    {
        public object Clone()
        {
            return MemberwiseClone();
        }
        public string Id { set; get; }
        [DisplayName("Account Name")]
        public string? AccountName { get; set; }
        [DisplayName("Par Amount")]
        public decimal? ParAmount { get; set; }
        [DisplayName("Maturity Date")]
        public DateTime? MaturityDateUTC { get; set; }
        [DisplayName("Performance ID")]
        public string? PerformanceId { get; set; }
        public DateTime CreatedDateUTC { get; set; }
        public string? HistoryTopAccountID { get; set; }
    }
}