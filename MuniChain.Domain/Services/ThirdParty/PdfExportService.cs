using Shared.Models.DealComponents;
using TheArtOfDev.HtmlRenderer.PdfSharp;

namespace Domain.Services.ThirdParty
{
    public class PdfExportService
    {
        public static MemoryStream CreatePdf(ExportSettings settings, DealModel dealInformation, List<string> advisorFirmNames, List<string> bondCounselFirmNames, List<DealExpenditure> dealExpenditures)
        {
            string html = "";

            if (!settings.Deal && !settings.Series && !settings.Scale)
            {
                return null;
            }

            if (settings.Deal || settings.Series || settings.Scale)
            {
                html = $@"
                <div style='padding-bottom: 15px; border-bottom: 2px solid #059669;'>
                    <a href='https://www.munichain.com' target='_blank' rel='noopener'><img src='https://www.munichain.com/munichain-logo.png' style='width: 150px;' /></a>
                </div>
                <div style='font-size: 24px; color: #111827; margin-top: 15px; margin-bottom: 5px;'><b>{dealInformation.Issuer}</b></div>
                <div style='font-size: 18px; white-space: pre-line; color: #1f2937;'>{dealInformation.Description}</div>
                <div style='border: 1px solid #d1d5db; margin-top: 15px; padding: 10px;'>
                    <div style='font-size: 16px; color: #1f2937;'>Deal Overview</div>
                    <div style='border-bottom: 1px solid #e5e7eb; margin-top: 5px; margin-bottom: 5px;'></div>
                    <div>
                        <table style='table-layout: fixed; width: 100%;'>
                            <tr>
                                <td style='padding-top: 5px; padding-bottom: 5px;'>
                                    <div style='font-size: 10px; color: #1f2937;'>Issuer</div>
                                    <div>{dealInformation.Issuer}</div>
                                </td>
                                <td style='padding-top: 5px; padding-bottom: 5px;'>
                                    <div style='font-size: 10px; color: #1f2937;'>Issue Size</div>
                                    <div>{dealInformation.FormattedCurrencySize}</div>
                                </td>
                                <td style='padding-top: 5px; padding-bottom: 5px;'>
                                    <div style='font-size: 10px; color: #1f2937;'>Offering Type</div>
                                    <div>{dealInformation.OfferingType}</div>
                                </td>
                            </tr>
                            <tr>
                                <td style='padding-top: 5px; padding-bottom: 5px;'>
                                    <div style='font-size: 10px; color: #1f2937;'>Issuer URL</div>
                                    <div>{dealInformation.IssuerURL}</div>
                                </td>
                                <td style='padding-top: 5px; padding-bottom: 5px;'>
                                    <div style='font-size: 10px; color: #1f2937;'>Issuer State</div>
                                    <div>{dealInformation.State}</div>
                                </td>
                                <td>
                                    <div style='font-size: 10px; color: #1f2937;'>Sale Date</div>
                                    <div>{(dealInformation.SaleDateUTC == null ? "TBA" : dealInformation.SaleDateUTC.Value.Date.ToShortDateString())}</div>
                                </td>
                            </tr>
                            <tr>
                                <td style='padding-top: 5px; padding-bottom: 5px;'>
                                    <div style='font-size: 10px; color: #1f2937;'>Municipal Advisor Firm</div>
                                    <div>{string.Join(", ", advisorFirmNames)}</div>
                                </td>
                                <td style='padding-top: 5px; padding-bottom: 5px;'>
                                    <div style='font-size: 10px; color: #1f2937;'>Bond Counsel Firm</div>
                                    <div>{string.Join(", ", bondCounselFirmNames)}</div>
                                </td>
                                <td>
                                    <div style='font-size: 10px; color: #1f2937;'>CUSIP6</div>
                                    <div>{dealInformation.CUSIP6}</div>
                                </td>
                            </tr>
                        <table>
                    </div>
                </div>
            ";

            }
            // Multi line string

            if (settings.Series || settings.Scale)
            {
                foreach (var series in dealInformation.Series)
                {
                    html += $@"
                    <div style='border: 1px solid #d1d5db; margin-top: 15px; padding: 10px;'>
                        <div style='font-size: 16px; color: #1f2937;'>Series: {series.Name}</div>
                        <div style='border-bottom: 1px solid #e5e7eb; margin-top: 5px; margin-bottom: 5px;'></div>
                        <div>
                            <table style='table-layout: fixed; width: 100%;'>
                                <tr>
                                    <td style='padding-top: 5px; padding-bottom: 5px;'>
                                        <div style='font-size: 10px; color: #1f2937;'>Series Size</div>
                                        <div>{series.Size}</div>
                                    </td>
                                    <td style='padding-top: 5px; padding-bottom: 5px;'>
                                        <div style='font-size: 10px; color: #1f2937;'>Purpose</div>
                                        <div>{series.Purpose}</div>
                                    </td>
                                    <td style='padding-top: 5px; padding-bottom: 5px;'>
                                        <div style='font-size: 10px; color: #1f2937;'>Source of Repayment</div>
                                        <div>{series.SourceOfRepayment}</div>
                                    </td>
                                </tr>
                                <tr>
                                    <td style='padding-top: 5px; padding-bottom: 5px;'>
                                        <div style='font-size: 10px; color: #1f2937;'>Sale Time</div>
                                        <div>{(series.SaleTimeUTC == null ? "TBA" : series.SaleTimeUTC.Value.ToShortTimeString())}</div>
                                    </td>
                                    <td style='padding-top: 5px; padding-bottom: 5px;'>
                                        <div style='font-size: 10px; color: #1f2937;'>Time Zone</div>
                                        <div>{series.TimeZone}</div>
                                    </td>
                                    <td>
                                        <div style='font-size: 10px; color: #1f2937;'>Dated Date</div>
                                        <div>{series.DatedDateUTC}</div>
                                    </td>
                                </tr>
                                <tr>
                                    <td style='padding-top: 5px; padding-bottom: 5px;'>
                                        <div style='font-size: 10px; color: #1f2937;'>Tax Status</div>
                                        <div>{series.TaxStatus}</div>
                                    </td>
                                    <td style='padding-top: 5px; padding-bottom: 5px;'>
                                        <div style='font-size: 10px; color: #1f2937;'>Security Type</div>
                                        <div>{series.SecurityType}</div>
                                    </td>
                                    <td>
                                        <div style='font-size: 10px; color: #1f2937;'>Lead Manager</div>
                                        <div>{series.LeadManager}</div>
                                    </td>
                                </tr>
                                <tr>
                                    <td style='padding-top: 5px; padding-bottom: 5px;'>
                                        <div style='font-size: 10px; color: #1f2937;'>ESG Certified Type</div>
                                        <div>{series.ESGCertifiedType}</div>
                                    </td>
                                    <td style='padding-top: 5px; padding-bottom: 5px;'>
                                        <div style='font-size: 10px; color: #1f2937;'>ESG Verifier</div>
                                        <div>{series.ESGVerifier}</div>
                                    </td>
                                    <td>
                                        <div style='font-size: 10px; color: #1f2937;'>Bank Qualified</div>
                                        <div>{series.IsBankQualified}</div>
                                    </td>
                                </tr>
                            <table>
                        </div>
                        <div>
                            <table style='table-layout: fixed; width: 100%;'>
                                <tr>
                                    <td style='padding-top: 5px; padding-bottom: 5px;'>
                                        <div style='font-size: 10px; color: #1f2937;'>Moody's Rating</div>
                                        <div>{series.MoodysRating}</div>
                                    </td>
                                    <td style='padding-top: 5px; padding-bottom: 5px;'>
                                        <div style='font-size: 10px; color: #1f2937;'>S&P Rating</div>
                                        <div>{series.SPRating}</div>
                                    </td>
                                    <td style='padding-top: 5px; padding-bottom: 5px;'>
                                        <div style='font-size: 10px; color: #1f2937;'>Fitch Rating</div>
                                        <div>{series.FitchRating}</div>
                                    </td>
                                    <td style='padding-top: 5px; padding-bottom: 5px;'>
                                        <div style='font-size: 10px; color: #1f2937;'>Kroll Rating</div>
                                        <div>{series.KrollRating}</div>
                                    </td>
                                </tr>
                            </table>
                        </div>
                        <div style='padding-top: 5px; padding-bottom: 5px;'>
                            <div style='font-size: 10px; color: #1f2937;'>Series Description</div>
                            <div style='white-space: pre-line;'>{series.Description}</div>
                        </div>
                        <div style='border: 1px solid #e5e7eb; margin-top: 10px;'>
                            <table style='table-layout: fixed; width: 100%;'>
                                <thead>
                                    <tr>
                                        <th style='padding-top: 5px; padding-bottom: 5px; padding-left: 5px; border-bottom: 1px solid #e5e7eb;'>
                                            <div style='font-size: 10px; color: #1f2937; text-align: left;'>Maturity Date</div>
                                        </th>
                                        <th style='padding-top: 5px; padding-bottom: 5px; border-bottom: 1px solid #e5e7eb;'>
                                            <div style='font-size: 10px; color: #1f2937; text-align: left;'>Par</div>
                                        </th>
                                        <th style='padding-top: 5px; padding-bottom: 5px; border-bottom: 1px solid #e5e7eb;'>
                                            <div style='font-size: 10px; color: #1f2937; text-align: left;'>Coupon</div>
                                        </th>
                                        <th style='padding-top: 5px; padding-bottom: 5px; border-bottom: 1px solid #e5e7eb;'>
                                            <div style='font-size: 10px; color: #1f2937; text-align: left;'>Yield</div>
                                        </th>
                                        <th style='padding-top: 5px; padding-bottom: 5px; border-bottom: 1px solid #e5e7eb;'>
                                            <div style='font-size: 10px; color: #1f2937; text-align: left;'>Dollar Price</div>
                                        </th>
                                        <th style='padding-top: 5px; padding-bottom: 5px; border-bottom: 1px solid #e5e7eb;'>
                                            <div style='font-size: 10px; color: #1f2937; text-align: left;'>CUSIP9</div>
                                        </th>
                                    </tr>
                                </thead>
                                <tbody>
                    ";

                    if (settings.Scale)
                    {
                        foreach (var maturity in series?.Maturities)
                        {
                            html += $@"
                                <tr>
                                    <td style='padding-top: 5px; padding-bottom: 5px; padding-left: 5px;'>
                                        <div>{(maturity.MaturityDateUTC == null ? "TBA" : maturity.MaturityDateUTC.Value.Date.ToShortDateString())}</div>
                                    </td>
                                    <td style='padding-top: 5px; padding-bottom: 5px;'>
                                        <div>{(maturity.Par == null ? "" : $"${maturity.Par}")}</div>
                                    </td>
                                    <td style='padding-top: 5px; padding-bottom: 5px;'>
                                        <div>{(maturity.Coupon == null ? "" : $"{maturity.Coupon}%")}</div>
                                    </td>
                                    <td style='padding-top: 5px; padding-bottom: 5px;'>
                                        <div>{(maturity.Yield == null ? "" : $"{maturity.Yield}%")}</div>
                                    </td>
                                    <td style='padding-top: 5px; padding-bottom: 5px;'>
                                        <div>{(maturity.Price == null ? "" : $"${maturity.Price}")}</div>
                                    </td>
                                    <td style='padding-top: 5px; padding-bottom: 5px;'>
                                        <div>{(maturity.CUSIP9LastThree == null ? "" : maturity.CUSIP9LastThree)}</div>
                                    </td>
                                </tr>
                            ";
                        }
                    }
                }

            }

            if (settings.Expenditures)
            {
                // Begin Expenditures
                html += $@"
                <div style='border: 1px solid #d1d5db; margin-top: 15px; padding: 10px;'>
                    <div style='font-size: 16px; color: #1f2937;'>Expenditures</div>
                    <div style='border-bottom: 1px solid #e5e7eb; margin-top: 5px; margin-bottom: 5px;'></div>
                    <div style='border: 1px solid #e5e7eb; margin-top: 10px;'>
                        <table style='table-layout: fixed; width: 100%;'>
                            <thead>
                                <tr>
                                    <th style='padding-top: 5px; padding-bottom: 5px; padding-left: 5px; border-bottom: 1px solid #e5e7eb;'>
                                        <div style='font-size: 10px; color: #1f2937; text-align: left;'>Expenditure Name</div>
                                    </th>
                                    <th style='padding-top: 5px; padding-bottom: 5px; border-bottom: 1px solid #e5e7eb;'>
                                        <div style='font-size: 10px; color: #1f2937; text-align: left;'>Amount</div>
                                    </th>
                                </tr>
                            </thead>
                            <tbody>
            ";
                foreach (var item in dealExpenditures.Where(e => e.IsDeleted == false && e.Value != null))
                {
                    html += $@"
                    <tr>
                ";
                    if (item.IsOther)
                    {
                        html += $@"
                        <td style='padding-top: 5px; padding-bottom: 5px; padding-left: 5px;'>
                            <div>{(item.Name == null ? "TBA" : item.Name)}</div>
                        </td>
                    ";
                    }
                    else
                    {
                        html += $@"
                        <td style='padding-top: 5px; padding-bottom: 5px; padding-left: 5px;'>
                            <div>{(item.DisplayName == null ? "TBA" : item.DisplayName)}</div>
                        </td>
                    ";
                    }
                    html += $@"
                        <td style='padding-top: 5px; padding-bottom: 5px; padding-left: 5px;'>
                            <div>{(item.Value == null ? "TBA" : $"${string.Format("{0:C0}", item.Value)}")}</div>
                        </td>
                    </tr>
                ";
                }
                html += $@"
                            </tbody>
                        </table>
                    </div>
                </div>
            ";
            }

            if (settings.Performance)
            {
                // Begin Performance
                html += $@"
                <div style='border: 1px solid #d1d5db; margin-top: 15px; padding: 10px;'>
                    <div style='font-size: 16px; color: #1f2937;'>Performance</div>
                    <div style='border-bottom: 1px solid #e5e7eb; margin-top: 5px; margin-bottom: 5px;'></div>
                    <div>
                        <table style='table-layout: fixed; width: 100%;'>
                            <tr>
                                <td style='padding-top: 5px; padding-bottom: 5px;'>
                                    <div style='font-size: 10px; color: #1f2937;'>N.I.C. (Net Interest Cost)</div>
                                    <div>{(dealInformation?.Performance?.NIC == null ? "" : $"{dealInformation?.Performance?.NIC}%")}</div>
                                </td>
                                <td style='padding-top: 5px; padding-bottom: 5px;'>
                                    <div style='font-size: 10px; color: #1f2937;'>T.I.C. (True Interest Cost)</div>
                                    <div>{(dealInformation?.Performance?.TIC == null ? "" : $"{dealInformation?.Performance?.TIC}%")}</div>
                                </td>
                                <td style='padding-top: 5px; padding-bottom: 5px;'>
                                    <div style='font-size: 10px; color: #1f2937;'>Gross Spread/Bond</div>
                                    <div>{(dealInformation?.Performance?.GrossSpread == null ? "" : $"${dealInformation?.Performance?.GrossSpread}")}</div>
                                </td>
                            </tr>
                            <tr>
                                <td style='padding-top: 5px; padding-bottom: 5px;'>
                                    <div style='font-size: 10px; color: #1f2937;'>Takedown/Bond</div>
                                    <div>{(dealInformation?.Performance?.Takedown == null ? "" : $"${dealInformation?.Performance?.Takedown}")}</div>
                                </td>
                                <td style='padding-top: 5px; padding-bottom: 5px;'>
                                    <div style='font-size: 10px; color: #1f2937;'>Total Retail Participation Par</div>
                                    <div>{(dealInformation?.Performance?.TotalRetailPar == null ? "" : $"${dealInformation?.Performance?.TotalRetailPar}")}</div>
                                </td>
                                <td style='padding-top: 5px; padding-bottom: 5px;'>
                                    <div style='font-size: 10px; color: #1f2937;'>Total Institutional Participation Par</div>
                                    <div>{(dealInformation?.Performance?.TotalInstitutionalPar == null ? "" : $"${dealInformation?.Performance?.TotalInstitutionalPar}")}</div>
                                </td>
                            </tr>
                            <tr>
                                <td style='padding-top: 5px; padding-bottom: 5px;'>
                                    <div style='font-size: 10px; color: #1f2937;'>Par Amount of Bonds</div>
                                    <div>{(dealInformation?.Performance?.ParAmountBonds == null ? "" : $"${dealInformation?.Performance?.ParAmountBonds}")}</div>
                                </td>
                                <td style='padding-top: 5px; padding-bottom: 5px;'>
                                    <div style='font-size: 10px; color: #1f2937;'>Reoffering Premium (or Discount)</div>
                                    <div>{(dealInformation?.Performance?.ReofferingPremium == null ? "" : $"${dealInformation?.Performance?.ReofferingPremium}")}</div>
                                </td>
                                <td style='padding-top: 5px; padding-bottom: 5px;'>
                                    <div style='font-size: 10px; color: #1f2937;'>Gross Production</div>
                                    <div>{(dealInformation?.Performance?.GrossProduction == null ? "" : $"${dealInformation?.Performance?.GrossProduction}")}</div>
                                </td>
                            </tr>
                            <tr>
                                <td style='padding-top: 5px; padding-bottom: 5px;'>
                                    <div style='font-size: 10px; color: #1f2937;'>Total Underwriter's Discount</div>
                                    <div>{(dealInformation?.Performance?.TotalUnderwriterDiscount == null ? "" : $"${dealInformation?.Performance?.TotalUnderwriterDiscount}")}</div>
                                </td>
                                <td style='padding-top: 5px; padding-bottom: 5px;'>
                                    <div style='font-size: 10px; color: #1f2937;'>Bid</div>
                                    <div>{(dealInformation?.Performance?.Bid == null ? "" : $"${dealInformation?.Performance?.Bid}")}</div>
                                </td>
                                <td style='padding-top: 5px; padding-bottom: 5px;'>
                                    <div style='font-size: 10px; color: #1f2937;'>Bid</div>
                                    <div>{(dealInformation?.Performance?.BidPercentage == null ? "" : $"{dealInformation?.Performance?.BidPercentage}%")}</div>
                                </td>
                            </tr>
                            <tr>
                                <td style='padding-top: 5px; padding-bottom: 5px;'>
                                    <div style='font-size: 10px; color: #1f2937;'>Bond Year Dollars</div>
                                    <div>{dealInformation?.Performance?.BondYearDollars}</div>
                                </td>
                                <td style='padding-top: 5px; padding-bottom: 5px;'>
                                    <div style='font-size: 10px; color: #1f2937;'>Average Life</div>
                                    <div>{dealInformation?.Performance?.AverageLife}</div>
                                </td>
                                <td style='padding-top: 5px; padding-bottom: 5px;'>
                                    <div style='font-size: 10px; color: #1f2937;'>Average Coupon</div>
                                    <div>{dealInformation?.Performance?.AverageCoupon}</div>
                                </td>
                            </tr>
                        </table>
                    </div>
                    <div style='border: 1px solid #e5e7eb; margin-top: 10px;'>
                        <table style='table-layout: fixed; width: 100%;'>
                            <thead>
                                <tr>
                                    <th style='padding-top: 5px; padding-bottom: 5px; padding-left: 5px; border-bottom: 1px solid #e5e7eb;'>
                                        <div style='font-size: 10px; color: #1f2937; text-align: left;'>Top Accounts</div>
                                    </th>
                                    <th style='padding-top: 5px; padding-bottom: 5px; border-bottom: 1px solid #e5e7eb;'>
                                        <div style='font-size: 10px; color: #1f2937; text-align: left;'>Par Amount</div>
                                    </th>
                                    <th style='padding-top: 5px; padding-bottom: 5px; border-bottom: 1px solid #e5e7eb;'>
                                        <div style='font-size: 10px; color: #1f2937; text-align: left;'>Maturity Date</div>
                                    </th>
                                </tr>
                            </thead>
                            <tbody>
            ";
                foreach (var topAccount in dealInformation?.Performance?.TopAccountList)
                {
                    html += $@"
                    <tr>
                        <td style='padding-top: 5px; padding-bottom: 5px; padding-left: 5px;'>
                            <div>{(topAccount.AccountName == null ? "TBA" : topAccount.AccountName)}</div>
                        </td>
                        <td style='padding-top: 5px; padding-bottom: 5px;'>
                            <div>{(topAccount.ParAmount == null ? "" : $"${topAccount.ParAmount}")}</div>
                        </td>
                        <td style='padding-top: 5px; padding-bottom: 5px;'>
                            <div>{(topAccount.MaturityDateUTC == null ? "" : topAccount.MaturityDateUTC.Value.Date.ToShortDateString())}</div>
                        </td>
                    </tr>
                ";
                }
                html += $@"
                            </tbody>
                        </table>
                    </div>
                </div>
            ";
                // End Performance
            }

            using (MemoryStream ms = new MemoryStream())
            {
                var pdf = PdfGenerator.GeneratePdf(html, PdfSharpCore.PageSize.A4);
                pdf.Save(ms);
                return ms;
            }
        }
    }
}
