using Hangfire;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using Shared.Helpers;
using Shared.Models.DealComponents;
using Shared.Models.Enums;
using Shared.Models.Users;
using Shared.Validators;
using Syncfusion.Blazor.Inputs;
using Syncfusion.Blazor.Navigations;
using System.Collections.Generic;
using UI.Components.Other;

namespace UI.Components.Deal
{
    public partial class Information
    {
        [CascadingParameter]
        public Error? Error { get; set; }
        
        [Parameter]
        public DealViewResponse DealView { get; set; }
        [Parameter]
        public User user { get; set; }
        [Parameter]
        public EventCallback<(string, bool, bool)> ReloadDeal { get; set; }
        public DealModel dealInformationOriginal { get; set; }
        public DealModel dealInformation
        {
            get
            {
                return DealView?.DealGranted;
            }
        }
        private EditForm dealForm;
        private bool isSaved = false;
        private bool SaveError = false;
        #region DropDownOptions
        public DateTime MinVal { get; set; } = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 15, 05, 30, 00);
        public DateTime MaxVal { get; set; } = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 15, 20, 00, 00);
        public string[] EnumValues = Enum.GetNames(typeof(States));
        public string[] CallOptionEnumValues = Enum.GetNames(typeof(CallFeatureOption));
        public List<string> ESGVerifierOptions = new List<string>() { "Kestrel", "Sustainalytics", "BAMGreen Star", "FirstEnvironment", "EY", "Self", "Other" };
        public List<string> OfferingTypes = new List<string>() { "Competitive", "Negotiated", "Private Placement" };
        public List<string> SeriesPurpose = new List<string>() { "Airports", "Appropriation", "Assisted Living Facilities", "Bond Anticipation Notes", "Bridge & Tunnel", "Building", "Capital Appreciation Bond", "Certificate of Obligation", "Charter School District", "County Government", "Dormitory Authority", "Economic Development", "Education", "Electric & Hydro Power", "Environmental", "Equipment Lease", "Escrow", "General Improvement", "Government Building", "Health Care", "Higher Education", "Highway", "Hospital", "Housing", "Industrial Development", "Law Enforcement", "Lease Revenue Bonds", "Multi Family Housing", "Municipal Utility District", "Non-Profit", "Parks & Recreation", "Pension Obligation", "Personal Income Tax", "Power", "Primary Education", "Prison", "Private University", "Public Improvement", "Public University", "Recreational Facilities", "Sales Tax", "Sanitation", "Secondary Education", "Single Family Housing", "Student Loans", "Tax Increment", "Tobacco Settlement", "Transportation", "University", "Utilities", "Various Purpose", "Water/Sewer/Gas", "501(c)(3)", "Other" };
        public List<string> SeriesSourceOfRepayment = new List<string>() { "Appropriation", "Certificate of Participation", "Double Barreled", "G.O. (Limited Tax)", "G.O. (Unlimited Tax)", "Revenue" };
        public List<string> MoodysWithSecurityTypeNote = new List<string>() { "Aaa", "Aa", "A", "MIG 1", "MIG 2", "MIG 3", "VMIG 1", "VMIG 2", "VMIG 3", "SG", "P1", "P2", "P3", "Prime -1", "Prime -2", "Prime -3", "Not Prime", "NR" };
        public List<string> MoodysWithoutSecurityTypeNote = new List<string>() { "Aaa", "Aa1", "Aa2", "Aa3", "A1", "A2", "A3", "Baa1", "Baa2", "Baa3", "Ba1", "Ba2", "Ba3", "B1", "B2", "B3", "Caa1", "Caa2", "Caa3", "Ca", "C", "D", "NR" };
        public List<string> SPWithSecurityTypeNote = new List<string>() { "AAA", "AA", "A", "A-1", "A-1+", "A-2", "A-3", "B", "C", "D", "SD", "SP-1", "SP-1+", "SP-2", "SP-3", "NR" };
        public List<string> SPWithoutSecurityTypeNote = new List<string>() { "AAA", "AA+", "AA", "AA-", "A+", "A", "A-", "BBB+", "BBB", "BBB-", "BB+", "BB", "BB-", "B+", "B", "B-", "CCC+", "CCC", "CCC-", "CC", "C", "D", "NR" };
        public List<string> FitchWithSecurityTypeNote = new List<string>() { "AAA", "AA", "A", "F1", "F1+", "F2", "F3", "WD", "NR" };
        public List<string> FitchWithoutSecurityTypeNote = new List<string>() { "AAA", "AA+", "AA", "AA-", "A+", "A", "A-", "BBB+", "BBB", "BBB-", "BB+", "BB", "BB-", "B+", "B", "B-", "CCC+", "CCC", "CCC-", "CC+", "CC", "CC-", "DDD", "NR" };
        public List<string> KrollWithSecurityTypeNote = new List<string>() { "K1+", "K1", "K2", "K3", "B", "C", "D", "NR" };
        public List<string> KrollWithoutSecurityTypeNote = new List<string>() { "AAA", "AA+", "AA", "AA-", "A+", "A", "A-", "BBB+", "BBB", "BBB-", "BB+", "BB", "BB-", "B+", "B", "B-", "CCC+", "CCC", "CCC-", "CC", "C", "D", "NR" };
        #endregion
        public DateTime? CallDate;
        public bool AddCallPopup = false;
        public List<Maturity> MaturitiesToCall = new();
        public string CallOption;
        public decimal CallPrice;
        public List<Maturity> editMaturitiesInTerm = new();


        #region CustomValue
        SfTextBox CustomValue;
        IssuerSelect issuerSelectRef;
        private bool IsCustomValueVisible { get; set; } = false;
        private bool IsShowConcurrencyItem { get; set; } = false;
        private List<ConcurrencyItem> LstConcurrencyItem = new List<ConcurrencyItem>();
        private List<string> ObjectToAddTo;
        private object ObjectToChange;
        private string PropToChange;
        private List<string> dealValidationErrors = new();
        private bool ValidationErrorPopup = false;
        private bool EditTermPopup = false;
        private bool EditIssuer = false;
        private bool ConfirmDialogVisible { get; set; } = false;

        #endregion

        #region Custom Input for Dropdown
        private void CustomValueChange(Microsoft.AspNetCore.Components.ChangeEventArgs change, List<string> values, Series series, string prop)
        {
            // Select input changed, List of values needs to get updated after custom input
            ObjectToChange = series;
            PropToChange = prop;
            ObjectToAddTo = values;

            // If custom input show modal
            if (change.Value.ToString().Equals("Other"))
            {
                IsCustomValueVisible = true;
            }
            // If non-Other input just set property normally
            else
            {
                ObjectToChange.SetPropertyByName(prop, change.Value);
            }
        }

        private async Task CloseCustomInput()
        {
            // Add custom input to list of dropdown options
            ObjectToAddTo.Add(CustomValue.Value);
            // Set property to custom input value
            ObjectToChange.SetPropertyByName(PropToChange, CustomValue.Value);
            // Close
            IsCustomValueVisible = false;
            await Task.Delay(0);
        }
        #endregion

        protected override async Task OnInitializedAsync()
        {
            dealInformationOriginal = dealInformation.Clone();
        }

        public async Task onCreated(string id)
        {
            await JsRuntime.InvokeVoidAsync("stopScroll", id);
        }

        public async Task OnClicked(ClickEventArgs Args, Series series)
        {
            if (Args.Item.Text == "Add")
            {
                var maturity = new Maturity() { Id = Guid.NewGuid().ToString(), GlobalMaturityID = Guid.NewGuid().ToString(), SeriesId = series.Id };
                series.Maturities.Add(maturity);
                return;
            }
            else if (Args.Item.Text == "Add Call")
            {
                CallDate = null; CallOption = null;
                MaturitiesToCall = series.Maturities.Where(x => x.IsChecked).ToList();
                AddCallPopup = true;
            }
            else if (Args.Item.Text == "Term")
            {
                var checkedMaturities = series.Maturities.Where(x => x.IsChecked).ToList();
                var termId = Guid.NewGuid().ToString();
                foreach (var maturity in checkedMaturities)
                {
                    maturity.IsTermed = true;
                    maturity.TermId = termId;
                }

                var termedMaturities = series.Maturities.TermMaturities();

                if (termedMaturities.Any(x => x.Key.Count < 2 && x.Value == true))
                {
                    toastService.ShowError("Invalid term input");
                    foreach (var maturity in checkedMaturities)
                    {
                        maturity.IsChecked = false;
                        maturity.IsTermed = false;
                        maturity.TermId = null;
                    }
                    return;
                }
            }

            series.Maturities.ForEach(x => x.IsChecked = false);
        }

        private async Task SplitTerm(List<Maturity> maturities, Shared.Models.DealComponents.Series series)
        {
            maturities.ForEach(x => x.IsTermed = false);
            maturities.ForEach(x => x.TermId = null);
            return;
        }

        private async Task EditTerm(List<Maturity> maturities)
        {
            editMaturitiesInTerm = maturities;
            EditTermPopup = true;
        }

        public async Task RefreshGrid()
        {
            StateHasChanged();
        }

        public void AddCall()
        {
            var checkedMaturities = MaturitiesToCall.Where(x => x.IsChecked).ToList();
            checkedMaturities.ForEach(x => x.CallDateUTC = CallDate);
            checkedMaturities.ForEach(x => x.CallType = CallOption);
            checkedMaturities.ForEach(x => x.CallPrice = CallPrice);
            AddCallPopup = false;
        }

        public void AddCallToTerm(List<Maturity> maturities)
        {
            CallDate = null; CallOption = null;
            MaturitiesToCall = maturities.ToList();
            AddCallPopup = true;
        }

        public void AddSeries()
        {
            var series = new Series()
            {
                Id = Guid.NewGuid().ToString(),
                GlobalSeriesID = Guid.NewGuid().ToString(),
                CreatedDateUTC = DateTime.Now
            };
            dealInformation.Series.Add(series);
        }

        public async Task Submit(bool isMergeSubmit)
        {
            try
            {
                await HandleUpdate(isMergeSubmit);
                if (!IsShowConcurrencyItem)
                {
                    toastService.ShowSuccess(heading: "Saved", message: "Deal has been successfully saved");
                }
            }
            catch (Exception ex)
            {
                Error?.ProcessError("There was a system error trying to save this deal, please try again.", ex, user, dealInformation);
                return;
            }

        }

        public void InvalidSubmit()
        {
            SaveError = true;
        }

        private void EditIssuerClicked()
        {
            EditIssuer = !EditIssuer;
            StateHasChanged();
        }

        public async Task AutoFill(string value, Series series, Maturity maturity, string prop)
        {
            var maturityList = series.Maturities.Where(x => !x.IsTermed).OrderBy(x => x.MaturityDateUTC).ToList();
            var isAutoFill = value.Contains(';');
            if (prop == "par")
            {
                if (isAutoFill)
                {
                    var splitString = value.Split(';');
                    if (splitString.Length == 2 && splitString.All(x => decimal.TryParse(x, out _)))
                    {
                        var strVal = decimal.TryParse(splitString[0], out var val);
                        var strFill = decimal.TryParse(splitString[1], out var fill);

                        var indexOfMaturity = maturityList.IndexOf(maturity);
                        var endOfAutoFill = indexOfMaturity + fill;
                        if (endOfAutoFill > maturityList.Count)
                        {
                            endOfAutoFill = maturityList.Count;
                        }

                        for (var i = indexOfMaturity; i < endOfAutoFill; i++)
                        {
                            var nextMaturity = maturityList.ElementAt(i);
                            nextMaturity.Par = val;
                        }
                    }
                }
                else
                {
                    try
                    {
                        decimal.TryParse(value, out decimal result);
                        maturity.Par = result;
                    }
                    catch (Exception e)
                    {

                    }
                }
            }
            else if (prop == "coupon")
            {
                if (isAutoFill)
                {
                    var splitString = value.Split(';');
                    if (splitString.Length == 2 && splitString.All(x => decimal.TryParse(x, out _)))
                    {
                        var strVal = decimal.TryParse(splitString[0], out var val);
                        var strFill = decimal.TryParse(splitString[1], out var fill);

                        var indexOfMaturity = maturityList.IndexOf(maturity);
                        var endOfAutoFill = indexOfMaturity + fill;
                        if (endOfAutoFill > maturityList.Count)
                        {
                            endOfAutoFill = maturityList.Count;
                        }

                        for (var i = indexOfMaturity; i < endOfAutoFill; i++)
                        {
                            maturityList.ElementAt(i).Coupon = val;
                        }
                    }
                }
                else
                {
                    try
                    {
                        decimal.TryParse(value, out decimal result);
                        maturity.Coupon = result;
                    }
                    catch (Exception e)
                    {

                    }
                }
            }
            else if (prop == "yield")
            {
                if (isAutoFill)
                {
                    var splitString = value.Split(';');
                    if (splitString.Length == 2 && splitString.All(x => decimal.TryParse(x, out _)))
                    {
                        var strVal = decimal.TryParse(splitString[0], out var val);
                        var strFill = decimal.TryParse(splitString[1], out var fill);

                        var indexOfMaturity = maturityList.IndexOf(maturity);
                        var endOfAutoFill = indexOfMaturity + fill;
                        if (endOfAutoFill > maturityList.Count)
                        {
                            endOfAutoFill = maturityList.Count;
                        }

                        for (var i = indexOfMaturity; i < endOfAutoFill; i++)
                        {
                            maturityList.ElementAt(i).Yield = val;
                        }
                    }
                }
                else
                {
                    try
                    {
                        decimal.TryParse(value, out decimal result);
                        maturity.Yield = result;
                    }
                    catch (Exception e)
                    {

                    }
                }
            }
        }

        public async Task HandleUpdate(bool isMergeSubmit)
        {
            // Try to update, check for concurrency
            Tuple<DealModel, bool> retVal = await dealService.Update(user, dealInformation, isMergeSubmit);
            DealModel fromDB = retVal.Item1;
            if (retVal.Item2)
            {
                List<ConcurrencyItem> diffs = dealInformation.ConcurrencyCompare(fromDB);

                if (diffs.Any())
                {
                    //Show PopUP
                    LstConcurrencyItem = diffs;
                    IsShowConcurrencyItem = true;
                    StateHasChanged();
                }
                return;
            }
            else
            {
                isSaved = true;
                await ReloadDeal.InvokeAsync((dealInformation.Id, false, false));

                // Create the Notifications after navigating
                List<ConcurrencyItem> variances = dealInformation.ConcurrencyCompare(dealInformationOriginal);
                BackgroundJobs.Enqueue(() => notificationService.DealModifiedNotification(user, dealInformation, variances));

                return;
            }
        }

        private async Task OnBeforeInternalNavigation(LocationChangingContext context)
        {
            if (isSaved)
            {
                return;
            }

            if (dealForm == null)
            {
                return;
            }
            bool busy = dealForm.EditContext.IsModified();

            if (busy)
            {
                var isConfirmed = await JsRuntime.InvokeAsync<bool>("confirm", new object[] { "Are you sure you want to navigate away from this page?" });

                if (!isConfirmed)
                {
                    context.PreventNavigation();
                }
            }
        }

        private async Task Publish(bool confirmed, bool canPublish)
        {
            if (!canPublish)
            {
                return;
            }

            ConfirmDialogVisible = true;
            if (confirmed)
            {
                ConfirmDialogVisible = false;
                await Publish();
            }
        }

        private async Task Publish()
        {
            try
            {
                if (dealInformation.Validate()?.Any() == true)
                {
                    dealValidationErrors = dealInformation.Validate();
                    ValidationErrorPopup = true;
                    return;
                }

                dealInformation.Status = "Published";

                // Can't publish maturities without publishing the series
                foreach (var series in dealInformation.Series)
                {
                    if (series.IsPublished == false)
                    {
                        series.IsPublishedMaturities = false;
                    }
                }

                await dealService.ChangeStatus(user, dealInformation);
                await ReloadDeal.InvokeAsync((dealInformation.Id, true, false));

                BackgroundJobs.Enqueue(() => notificationService.DealPublishNotification(user, dealInformation));

                return;
            }
            catch (DbUpdateException ex)
            {
                toastService.ShowError("Someone has updated the deal after you have opened it. Please refresh");
                return;
            }
            catch (Exception ex)
            {
                Error?.ProcessError("Failed to change deal status", ex, user, dealInformation);
                return;
            }
        }

        private async Task ShowErrorsWhenPublish()
        {
            dealValidationErrors = dealInformation.Validate();
            ValidationErrorPopup = true;
        }
    }


}