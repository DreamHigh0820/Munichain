using Shared.Helpers;
using Shared.Models.Enums;
using Shared.Validators;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Models.DealComponents
{
    public class DealModel
    {
        public string Id { get; set; }
        [DateTimeValidator]
        [DisplayName("Sale Date")]
        public DateTime? SaleDateUTC { get; set; }
        public string? CreatedBy { get; set; }
        [DisplayName("Offering Type")]
        public string? OfferingType { get; set; }
        [Required(ErrorMessage = "The Issuer field is mandatory.")]
        public string? Issuer { get; set; }
        [IssuerUrlValidator(ErrorMessage = "The Issuer URL must be a valid URL. (ex: https://www.munichain.com)")]
        [DisplayName("Issuer Website")]
        public string? IssuerURL { get; set; }
        [NotMapped]
        public string FormattedCurrencySize
        {
            get
            {
                if (Size == null)
                {
                    return "";
                }
                else if (Size.ToString().EndsWith("00"))
                {
                    return string.Format("{0:C0}", Size.Value);
                }
                else
                {
                    return string.Format("{0:C2}", Size.Value);
                }
            }
        }
        [Required(ErrorMessage = "The Deal Size field is mandatory.")]
        [DisplayName("Issuer Size")]
        public decimal? Size { get; set; }
        [Required(ErrorMessage = "The Deal State field is mandatory.")]
        [DisplayName("Issuer State")]
        public States? State { get; set; }
        public string? Description { get; set; }
        [StringLength(6, MinimumLength = 6)]
        public string? CUSIP6 { get; set; }
        public string? Status { get; set; }
        public string? OldStatus { get; set; }
        public bool? IsLocked { get; set; }
        public byte[] RowVersion { get; set; }
        public Performance? Performance { get; set; }
        public List<Series> Series { get; set; } = new();
        [DisplayName("Created Date")]
        public DateTime CreatedDateUTC { get; set; }
        public string LastModifiedBy { get; set; }
        public string LastModifiedByDisplayName { get; set; }
        [NotMapped]
        public bool IsMasterCopy { get { return HistoryDealID == null; } }
        [DisplayName("Last Modified Date")]
        public DateTime LastModifiedDateTimeUTC { get; set; }
        [NotMapped]
        public bool HasBeenPublished { get; set; }
        [NotMapped]
        public bool IsLatestPublished { get; set; }
        [DisplayName("History Deal ID")]
        public string? HistoryDealID { get; set; }
    }
    public class Series
    {
        public string Id { get; set; }
        [DisplayName("Series Name")]
        public string? Name { get; set; }
        [DisplayName("Created Date")]
        public DateTime CreatedDateUTC { get; set; }
        [NotMapped]
        public string FormattedCurrencySize
        {
            get
            {
                if (Size == null)
                {
                    return "";
                }
                else if (Size.ToString().EndsWith("00"))
                {
                    return string.Format("{0:C0}", Size.Value);
                }
                else
                {
                    return string.Format("{0:C2}", Size.Value);
                }
            }
        }
        public decimal? Size { get; set; }
        [DisplayName("Series Description")]
        public string? Description { get; set; }
        [DisplayName("Sale Time")]
        public DateTime? SaleTimeUTC { get; set; }
        [DateTimeValidator]
        [DisplayName("Dated Date")]
        public DateTime? DatedDateUTC { get; set; }
        [DateTimeValidator]
        [DisplayName("Settlement Date")]
        public DateTime? SettlementDateUTC { get; set; }
        [DateTimeValidator]
        [DisplayName("Delivery Date")]
        public DateTime? DeliveryDateUTC { get; set; }
        [DisplayName("Time Zone")]
        public string? TimeZone { get; set; }
        [DisplayName("Tax Status")]
        public string? TaxStatus { get; set; }
        [DisplayName("Security Type")]
        public string? SecurityType { get; set; }
        [DisplayName("Lead Manager")]
        public string? LeadManager { get; set; }
        public string? Purpose { get; set; }
        [DisplayName("Source of Repayment")]
        public string? SourceOfRepayment { get; set; }
        [DisplayName("Bank Qualified")]
        public bool? IsBankQualified { get; set; }
        [DisplayName("ERP Status")]
        public bool? IsERP { get; set; }
        [DisplayName("Moody's Rating")]
        public string? MoodysRating { get; set; }
        [DisplayName("S&P Rating")]
        public string? SPRating { get; set; }
        [DisplayName("Fitch Rating")]
        public string? FitchRating { get; set; }
        [DisplayName("Kroll Rating")]
        public string? KrollRating { get; set; }
        [DisplayName("ESG Certified Type")]
        public string? ESGCertifiedType { get; set; }
        [DisplayName("ESG Verifier")]
        public string? ESGVerifier { get; set; }
        public string? DealModelId { get; set; }
        [DisplayName("Published Status")]
        public bool IsPublished { get; set; }
        [DisplayName("Maturities Publish Status")]
        public bool IsPublishedMaturities { get; set; }
        [DisplayName("History Series ID")]
        public string? HistorySeriesID { get; set; }
        [DisplayName("Global Series ID")]
        public string? GlobalSeriesID { get; set; }

        public List<Maturity>? Maturities { get; set; } = new();
    }

    public class Maturity
    {
        public string? Id { get; set; }
        [NotMapped]
        public bool IsChecked { get; set; } = false;
        public bool IsTermed { get; set; } = false;
        public string? TermId { get; set; }
        [DisplayName("Maturity Date")]
        public DateTime? MaturityDateUTC { get; set; }
        public decimal? Par { get; set; }
        [NotMapped]
        public string? ParAutoFill
        {
            get
            {
                return string.Format("{0:N2}", Par);
            }
        }

        public decimal? Price { get; set; }
        public decimal? Coupon { get; set; }
        [NotMapped]
        public string? CouponAutoFill
        {
            get
            {
                return string.Format("{0:N2}", Coupon);
            }
        }
        public decimal? Yield { get; set; }
        [NotMapped]
        public string? YieldAutoFill
        {
            get
            {
                return string.Format("{0:N2}", Yield);
            }
        }
        [DisplayName("Dollar Yield")]
        public decimal? DollarYield { get; set; }
        [DisplayName("Yield Denominator")]
        public string? YieldDenom { get; set; }
        [StringLength(3, MinimumLength = 3, ErrorMessage = "Must be exactly 3 characters")]
        [DisplayName("Last Three of CUSIP")]
        public string? CUSIP9LastThree { get; set; }
        public string? SeriesId { get; set; }
        [DisplayName("Created Date")]
        public DateTime CreatedDateUTC { get; set; }
        [DisplayName("Call Date")]
        public DateTime? CallDateUTC { get; set; }
        [DisplayName("Call Price")]
        public decimal? CallPrice { get; set; }
        [DisplayName("Call Type")]
        public string? CallType { get; set; }
        [DisplayName("History Maturity ID")]
        public string? HistoryMaturityID { get; set; }
        [DisplayName("Global Maturity ID")]
        public string? GlobalMaturityID { get; set; }
    }
}
