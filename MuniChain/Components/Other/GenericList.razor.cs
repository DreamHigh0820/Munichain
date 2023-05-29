using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Shared.Helpers.Pagination;

namespace UI.Components.Other
{
    public class GenericListBase<T> : ComponentBase
    {
        [Parameter] public RenderFragment NullTemplate { get; set; }
        [Parameter] public RenderFragment EmptyTemplate { get; set; }
        [Parameter] public RenderFragment<T> ElementTemplate { get; set; }
        [Parameter] public RenderFragment WholeListTemplate { get; set; }
        [Parameter] public List<T> List { get; set; }
        [Parameter] public PaginationDTO PaginationDTO { get; set; }
        [Parameter] public double CurrentPageDouble { get; set; }
        [Parameter] public int CurrentPage { get; set; }
        [Parameter] public string TableName { get; set; }
        [Parameter] public int TotalPages { get; set; }
        [Parameter] public int TotalRecords { get; set; }
        [Parameter] public int RecordsPerPage { get; set; } = 10;
        [Parameter] public string CurrentSortField { get; set; }
        [Parameter] public string CurrentSortOrder { get; set; }
        [Parameter] public bool IsShowExport { get; set; } = false;
        [Parameter] public EventCallback<Dictionary<string, dynamic>> PageChange { get; set; }
        [Parameter] public EventCallback<Dictionary<string, dynamic>> RecordsPerPageChange { get; set; }
        [Parameter] public EventCallback<Dictionary<string, dynamic>> SearchGeneric { get; set; }
        [Parameter] public EventCallback<Dictionary<string, dynamic>> ExcelExportGeneric { get; set; }
        public int CurrentRecordsPerPage { get; set; }
        public int PrevRecordsPerPage { get; set; }
        public int FirstRecordNoShowing => (CurrentPage - 1) * RecordsPerPage + 1;
        public int LastRecordNoShowing => (CurrentPage - 1) * RecordsPerPage + RecordsPerPage;
        public string TotalRecordsShowing => string.Format("{0:n0}", TotalRecords);
        public async Task OnPageChange(int page)
        {
            CurrentPageDouble = page;
            CurrentPage = page;

            var parameters = new Dictionary<string, dynamic>();
            parameters.Add("CurrentPage", CurrentPage);
            parameters.Add("RecordsPerPage", RecordsPerPage);
            parameters.Add("CurrentPageDouble", CurrentPageDouble);
            await PageChange.InvokeAsync(parameters);
        }
        public async Task RecordsPerPageChangeInternal(ChangeEventArgs e)
        {
            //default and page not changed atleast once
            if (CurrentPageDouble == 0)
                CurrentPageDouble = CurrentPage;

            int changedVal = int.Parse(e.Value.ToString());
            PrevRecordsPerPage = RecordsPerPage;
            CurrentRecordsPerPage = changedVal;
            //if (CurrentRecordsPerPage > PrevRecordsPerPage)
            //{
            //    CurrentPageDouble = (Double)((Double)CurrentPage / (Double)((Double)CurrentRecordsPerPage / (Double)PrevRecordsPerPage));
            //    CurrentPage = int.Parse(Math.Ceiling(CurrentPageDouble).ToString());
            //}
            //if (CurrentRecordsPerPage < PrevRecordsPerPage)
            //{
            //    CurrentPageDouble = (Double)((Double)CurrentPageDouble * (Double)((Double)PrevRecordsPerPage / (Double)CurrentRecordsPerPage));
            //    CurrentPage = int.Parse(Math.Ceiling(CurrentPageDouble).ToString());
            //}

            CurrentPage = (CurrentPage - 1) * PrevRecordsPerPage / CurrentRecordsPerPage + 1;
            RecordsPerPage = CurrentRecordsPerPage;

            var parameters = new Dictionary<string, dynamic>();
            parameters.Add("CurrentPage", CurrentPage);
            parameters.Add("RecordsPerPage", RecordsPerPage);
            parameters.Add("CurrentPageDouble", CurrentPageDouble);
            await RecordsPerPageChange.InvokeAsync(parameters);
        }
        public async Task SearchGenericInternal()
        {
            CurrentPage = 1;

            var parameters = new Dictionary<string, dynamic>();
            parameters.Add("CurrentPage", CurrentPage);
            parameters.Add("SearchText", PaginationDTO.SearchText);
            parameters.Add("CurrentPageDouble", CurrentPageDouble);
            parameters.Add("RecordsPerPage", RecordsPerPage);
            await SearchGeneric.InvokeAsync(parameters);
        }
        public async Task ExcelExportGenericInternal()
        {
            var parameters = new Dictionary<string, dynamic>();
            parameters.Add("CurrentPageDouble", CurrentPageDouble);
            parameters.Add("RecordsPerPage", TotalRecords);
            parameters.Add("SearchText", PaginationDTO.SearchText);
            await ExcelExportGeneric.InvokeAsync(parameters);
        }
        public async Task OnChangeSearchBox(ChangeEventArgs e)
        {
            var oldVal = PaginationDTO.SearchText;
            var newVal = e.Value.ToString();
            PaginationDTO.SearchText = newVal;
            if (!string.IsNullOrEmpty(oldVal) && string.IsNullOrEmpty(newVal))//cleared
            {
                await SearchGenericInternal();
            }
        }
        public async Task OnKeyDown(KeyboardEventArgs e)
        {
            if (e.Code == "Enter" || e.Code == "NumpadEnter")
            {
                await SearchGenericInternal();
            }
        }
    }
}
