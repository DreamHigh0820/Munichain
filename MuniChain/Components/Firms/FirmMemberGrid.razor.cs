using Domain.Services.Database;
using Microsoft.AspNetCore.Components;
using Shared.Helpers.Pagination;
using Shared.Models.AppComponents;
using Shared.Models.Users;
using UI;

namespace UI.Components.Firms
{
    public partial class FirmMemberGrid
    {
        #region ComponentFields
        [Inject]
        public IFirmsService FirmService { get; set; }
        [Inject]
        public IUserService UserService { get; set; }
        public bool loading { get; set; }
        [Parameter]
        public string FirmId { get; set; }
        [Parameter]
        public bool IsAdmin { get; set; }
        [Parameter] public EventCallback<User> RemoveFirmMember { get; set; }
        #endregion

        #region GridFields
        private List<User> UserList { get; set; }
        public PaginationDTO SearchParams = new PaginationDTO()
        {
            RecordsPerPage = 10,
            CurrentPage = 1,
            SortField = "EmailAddress",
            SortOrder = "DESC",
            SearchColumns = new string[] { "EmailAddress" }
        };//Ascending
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
                loading = true;
                await LoadMembersForGrid();
                loading = false;
            }
            catch (Exception)
            {
            }
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
            await LoadMembersForGrid();
        }
        public async Task OnPageChange(Dictionary<string, dynamic> parameters)
        {
            SearchParams.CurrentPage = parameters["CurrentPage"];
            CurrentPageDouble = parameters["CurrentPageDouble"];
            SearchParams.RecordsPerPage = parameters["RecordsPerPage"];
            await LoadMembersForGrid();
        }
        public async Task OnRecordsPerPageChange(Dictionary<string, dynamic> parameters)
        {
            SearchParams.CurrentPage = parameters["CurrentPage"];
            CurrentPageDouble = parameters["CurrentPageDouble"];
            SearchParams.RecordsPerPage = parameters["RecordsPerPage"];
            await LoadMembersForGrid();
        }
        public async Task OnSearchGeneric(Dictionary<string, dynamic> parameters)
        {
            SearchParams.CurrentPage = parameters["CurrentPage"];
            CurrentPageDouble = parameters["CurrentPageDouble"];
            SearchParams.RecordsPerPage = parameters["RecordsPerPage"];
            SearchParams.SearchText = parameters["SearchText"];
            await LoadMembersForGrid();
        }
        public async Task OnRemoveFirmMember(User user)
        {
            await RemoveFirmMember.InvokeAsync(user);
        }
        public async Task LoadMembersForGrid()
        {
            try
            {
                PaginatedResponse<FirmMember> paginatedResponse = await FirmService.SearchPaged(SearchParams, FirmId);
                var users = await UserService.BatchGetUsersByEmail(paginatedResponse.Records.Select(x => x.EmailAddress).ToArray());

                foreach (var firmMember in paginatedResponse.Records.Where(p => !users.Any(p2 => p2.Email == p.EmailAddress)))
                {
                    users.Add(new User()
                    {
                        Id = null,
                        Email = firmMember.EmailAddress
                    });
                }

                UserList = users;
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
