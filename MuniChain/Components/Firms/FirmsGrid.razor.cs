using Domain.Services.Database;
using Microsoft.AspNetCore.Components;
using Shared.Helpers.Pagination;
using Shared.Models.AppComponents;
using System.Linq.Expressions;

namespace UI.Components.Firms
{
    public partial class FirmsGrid
    {
        #region ComponentFields
        [Inject]
        public IFirmsService FirmService { get; set; }
        [Parameter]
        public bool IsConfirmed { get; set; }

        [Parameter]
        public bool IsAdmin { get; set; }
        [Parameter]
        public bool IncludeMembers { get; set; } = false;
        public bool loading { get; set; }
        public ILogger<FirmsGrid> Logger { get; set; }
        [Parameter] public bool ShowExport { get; set; } = true;
        [Parameter] public EventCallback<Firm> FirmEdit { get; set; }
        [Parameter] public EventCallback<Firm> FirmDelete { get; set; }
        [Parameter] public Expression<Func<Firm, bool>> filter { get; set; } = null;
        #endregion

        #region GridFields
        private List<Firm> FirmList { get; set; }
        public PaginationDTO SearchParams = new PaginationDTO()
        {
            RecordsPerPage = 10,
            CurrentPage = 1,
            SortField = "Name",
            SortOrder = "DESC",
            SearchColumns = new string[] { "Name" }
        };
        [Parameter] public string TableName { get; set; }
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
        public int FirstRecordNoShowing => (SearchParams.CurrentPage - 1) * SearchParams.RecordsPerPage + 1;
        public int LastRecordNoShowing => (SearchParams.CurrentPage - 1) * SearchParams.RecordsPerPage + SearchParams.RecordsPerPage;
        public string TotalRecordsShowing => string.Format("{0:n0}", TotalRecords);
        public int CurrentRecordsPerPage { get; set; }
        public int PrevRecordsPerPage { get; set; }
        public double CurrentPageDouble { get; set; }
        #endregion
        protected override async Task OnInitializedAsync()
        {
            try
            {
                await LoadFirmsForGrid();
                loading = false;
            }
            catch (Exception)
            {
                //Logger.LogError(ex.Message + "," + ex.StackTrace);
            }
        }
        public async Task OnFirmEdit(Firm firm)
        {
            await FirmEdit.InvokeAsync(firm);
        }
        public async Task OnFirmDelete(Firm firm)
        {
            await FirmDelete.InvokeAsync(firm);
        }

        #region GridBinding
        public string SortIndicator(string sortField)
        {
            if (sortField.Equals(SearchParams.SortField, StringComparison.OrdinalIgnoreCase))
            {
                return SearchParams.SortOrder.Equals("ASC", StringComparison.OrdinalIgnoreCase) ? Constants.GridUpArrowIndicator : Constants.GridDownArrowIndicator;
            }
            return string.Empty;
        }
        public async Task Sort(string sortField)
        {
            if (sortField.Equals(SearchParams.SortField, StringComparison.OrdinalIgnoreCase))
            {
                SearchParams.SortOrder = SearchParams.SortOrder.Equals("ASC", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC";
            }
            else
            {
                SearchParams.SortField = sortField;
                SearchParams.SortOrder = "ASC";
            }
            await LoadFirmsForGrid();
        }
        public async Task OnPageChange(Dictionary<string, dynamic> parameters)
        {
            SearchParams.CurrentPage = parameters["CurrentPage"];
            CurrentPageDouble = parameters["CurrentPageDouble"];
            SearchParams.RecordsPerPage = parameters["RecordsPerPage"];
            await LoadFirmsForGrid();
        }
        public async Task OnRecordsPerPageChange(Dictionary<string, dynamic> parameters)
        {
            SearchParams.CurrentPage = parameters["CurrentPage"];
            CurrentPageDouble = parameters["CurrentPageDouble"];
            SearchParams.RecordsPerPage = parameters["RecordsPerPage"];
            await LoadFirmsForGrid();
        }
        public async Task OnSearchGeneric(Dictionary<string, dynamic> parameters)
        {
            SearchParams.CurrentPage = parameters["CurrentPage"];
            CurrentPageDouble = parameters["CurrentPageDouble"];
            SearchParams.RecordsPerPage = parameters["RecordsPerPage"];
            SearchParams.SearchText = parameters["SearchText"];
            await LoadFirmsForGrid();
        }
        public async Task LoadFirmsForGrid()
        {
            try
            {
                PaginatedResponse<Firm> paginatedResponse = await FirmService.SearchPaged(SearchParams, IsConfirmed, filter, IncludeMembers);
                FirmList = paginatedResponse.Records;
                TotalPages = paginatedResponse.TotalPages;
                TotalRecords = paginatedResponse.TotalRecords;
            }
            catch (Exception)
            {
                //Logger.LogError(ex.Message + "," + ex.StackTrace);
            }
        }
        #endregion
    }
}