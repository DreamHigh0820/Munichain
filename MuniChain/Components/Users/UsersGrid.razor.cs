using Domain.Services.Database;
using Microsoft.AspNetCore.Components;
using Shared.Helpers.Pagination;
using Shared.Models.Users;
using System.Linq.Expressions;

namespace UI.Components.Users
{
    public partial class UsersGrid
    {
        #region ComponentFields
        [Inject]
        public IUserService UserService { get; set; }
        [Parameter]
        public bool IsConfirmed { get; set; }

        [Parameter]
        public bool IsAdmin { get; set; }
        public bool loading { get; set; }
        public ILogger<UsersGrid> Logger { get; set; }
        [Parameter] public EventCallback<User> UserEdit { get; set; }
        [Parameter] public bool ShowRole { get; set; } = true;
        [Parameter] public bool ShowExport { get; set; } = true;
        [Parameter] public Expression<Func<User, bool>> filter { get; set; } = null;
        #endregion

        #region GridFields
        private List<User> UsersList { get; set; }
        public PaginationDTO SearchParams = new PaginationDTO()
        {
            RecordsPerPage = 10,
            CurrentPage = 1,
            SortField = "DisplayName",
            SortOrder = "DESC",
            SearchColumns = new string[] { "DisplayName", "Bio", "City", "JobTitle" }
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
                await LoadUsersForGrid();
            }
            catch (Exception)
            {
                //Logger.LogError(ex.Message + "," + ex.StackTrace);
            }
        }
        public async Task OnUserEdit(User user)
        {
            await UserEdit.InvokeAsync(user);
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
            await LoadUsersForGrid();
        }
        public async Task OnPageChange(Dictionary<string, dynamic> parameters)
        {
            SearchParams.CurrentPage = parameters["CurrentPage"];
            CurrentPageDouble = parameters["CurrentPageDouble"];
            SearchParams.RecordsPerPage = parameters["RecordsPerPage"];
            await LoadUsersForGrid();
        }
        public async Task OnRecordsPerPageChange(Dictionary<string, dynamic> parameters)
        {
            SearchParams.CurrentPage = parameters["CurrentPage"];
            CurrentPageDouble = parameters["CurrentPageDouble"];
            SearchParams.RecordsPerPage = parameters["RecordsPerPage"];
            await LoadUsersForGrid();
        }
        public async Task OnSearchGeneric(Dictionary<string, dynamic> parameters)
        {
            SearchParams.CurrentPage = parameters["CurrentPage"];
            CurrentPageDouble = parameters["CurrentPageDouble"];
            SearchParams.RecordsPerPage = parameters["RecordsPerPage"];
            SearchParams.SearchText = parameters["SearchText"];
            await LoadUsersForGrid();
        }
        public async Task LoadUsersForGrid()
        {
            try
            {
                PaginatedResponse<User> paginatedResponse = await UserService.SearchPaged(SearchParams, IsConfirmed, filter, ShowRole);
                UsersList = paginatedResponse.Records;
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