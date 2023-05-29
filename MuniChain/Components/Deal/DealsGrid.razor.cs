using ClosedXML.Excel;
using Domain.Services.Database;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Shared.Helpers;
using Shared.Helpers.Pagination;
using Shared.Models.DealComponents;
using UI;

namespace UI.Components.Deal
{
    public partial class DealsGrid
    {
        #region ComponentFields
        [Inject]
        public IJSRuntime JS { get; set; }
        [Inject]
        public IDealService DealService { get; set; }
        [Parameter]
        public bool IsPublishedOnly { get; set; }
        [Parameter]
        public string FirmID { get; set; }
        [Parameter]
        public string UserId { get; set; }
        [Parameter]
        public bool ShowStatus { get; set; } = true;
        public bool loading { get; set; }
        public ILogger<DealsGrid> Logger { get; set; }
        #endregion

        #region GridFields
        private List<DealModel> DealList { get; set; }
        public PaginationDTO SearchParams = new PaginationDTO()
        {
            RecordsPerPage = 10,
            CurrentPage = 1,
            SortField = "SaleDateUTC",
            SortOrder = "DESC",
            SearchColumns = new string[] { "OfferingType", "Issuer", "IssuerURL", "Description" }
        };//Ascending
        [Parameter] public string TableName { get; set; }
        public double CurrentPageDouble { get; set; }
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
        public int FirstRecordNoShowing => (SearchParams.CurrentPage - 1) * SearchParams.RecordsPerPage + 1;
        public int LastRecordNoShowing => (SearchParams.CurrentPage - 1) * SearchParams.RecordsPerPage + SearchParams.RecordsPerPage;
        public string TotalRecordsShowing => string.Format("{0:n0}", TotalRecords);
        public int CurrentRecordsPerPage { get; set; }
        public int PrevRecordsPerPage { get; set; }
        #endregion
        protected override async Task OnInitializedAsync()
        {
            try
            {
                loading = true;
                var result = await ProtectedSessionStore.GetAsync<PaginationDTO>(GetType().FullName);
                if (result.Value is not null)
                    SearchParams = result.Value;
                await LoadDealsForGrid();
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
            await LoadDealsForGrid();
        }
        public async Task OnPageChange(Dictionary<string, dynamic> parameters)
        {
            SearchParams.CurrentPage = parameters["CurrentPage"];
            SearchParams.RecordsPerPage = parameters["RecordsPerPage"];
            CurrentPageDouble = parameters["CurrentPageDouble"];
            await LoadDealsForGrid();
        }
        public async Task OnRecordsPerPageChange(Dictionary<string, dynamic> parameters)
        {
            SearchParams.CurrentPage = parameters["CurrentPage"];
            SearchParams.RecordsPerPage = parameters["RecordsPerPage"];

            if (TotalRecords < SearchParams.RecordsPerPage)
            {
                SearchParams.CurrentPage = 1;
            }
            CurrentPageDouble = parameters["CurrentPageDouble"];
            await LoadDealsForGrid();
        }
        public async Task OnSearchGeneric(Dictionary<string, dynamic> parameters)
        {
            SearchParams.CurrentPage = parameters["CurrentPage"];
            SearchParams.RecordsPerPage = parameters["RecordsPerPage"];
            SearchParams.SearchText = parameters["SearchText"];
            CurrentPageDouble = parameters["CurrentPageDouble"];
            await LoadDealsForGrid();
        }
        public async Task OnGridExcelExport(Dictionary<string, dynamic> parameters)
        {
            PaginationDTO srchExcel = SearchParams.Clone();
            srchExcel.RecordsPerPage = TotalRecords;
            try
            {
                //First Search, sort and then paginate
                PaginatedResponse<DealModel> paginatedResponse = await GetPaginatedResponse(srchExcel);
                List<DealModel> list = paginatedResponse.Records;
                string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                string fileName = DateTime.Now.ToString("ddMMyyyyyHHmmss") + "_Deals.xlsx";
                using (var workbook = new XLWorkbook())
                {
                    IXLWorksheet worksheet =
                    workbook.Worksheets.Add("Deals");
                    worksheet.Cell(1, 1).Value = "SaleDate";
                    worksheet.Cell(1, 2).Value = "Size";
                    worksheet.Cell(1, 3).Value = "Issuer";
                    worksheet.Cell(1, 4).Value = "State";
                    worksheet.Cell(1, 5).Value = "OfferingType";
                    worksheet.Cell(1, 6).Value = "Status";
                    for (int index = 1; index <= list.Count; index++)
                    {
                        worksheet.Cell(index + 1, 1).Value = list[index - 1].SaleDateUTC;
                        worksheet.Cell(index + 1, 1).Style.DateFormat.Format = "yyyy-MM-dd";//"yyyy-MM-dd HH:mm:ss";
                        worksheet.Cell(index + 1, 1).WorksheetColumn().AdjustToContents();
                        worksheet.Cell(index + 1, 1).Style.Alignment.WrapText = false;//set wrap text

                        worksheet.Cell(index + 1, 2).Value = list[index - 1].Size;
                        worksheet.Cell(index + 1, 2).WorksheetColumn().AdjustToContents();
                        worksheet.Cell(index + 1, 2).Style.Alignment.WrapText = false;//set wrap text

                        worksheet.Cell(index + 1, 3).Value = list[index - 1].Issuer;
                        worksheet.Cell(index + 1, 3).WorksheetColumn().AdjustToContents();
                        worksheet.Cell(index + 1, 3).Style.Alignment.WrapText = false;//set wrap text

                        worksheet.Cell(index + 1, 4).Value = list[index - 1].State;
                        worksheet.Cell(index + 1, 4).WorksheetColumn().AdjustToContents();
                        worksheet.Cell(index + 1, 4).Style.Alignment.WrapText = false;//set wrap text

                        worksheet.Cell(index + 1, 5).Value = list[index - 1].OfferingType;
                        worksheet.Cell(index + 1, 5).WorksheetColumn().AdjustToContents();
                        worksheet.Cell(index + 1, 5).Style.Alignment.WrapText = false;//set wrap text

                        worksheet.Cell(index + 1, 6).Value = list[index - 1].Status;
                        worksheet.Cell(index + 1, 6).WorksheetColumn().AdjustToContents();
                        worksheet.Cell(index + 1, 6).Style.Alignment.WrapText = false;//set wrap text
                    }
                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        var content = stream.ToArray();
                        await JS.SaveAs(fileName, content);
                        //await JS.InvokeAsync<object>("exampleJsFunctions.saveAsFile", fileName, content);
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        public async Task LoadDealsForGrid()
        {
            try
            {
                loading = true;
                await InvokeAsync(StateHasChanged);
                PaginatedResponse<DealModel> paginatedResponse = await GetPaginatedResponse(SearchParams);
                DealList = paginatedResponse?.Records;
                TotalPages = paginatedResponse.TotalPages;
                TotalRecords = paginatedResponse.TotalRecords;
                await ProtectedSessionStore.SetAsync(GetType().FullName, SearchParams);
            }
            catch (Exception)
            {

            }

            loading = false;
            await InvokeAsync(StateHasChanged);

        }

        private async Task<PaginatedResponse<DealModel>> GetPaginatedResponse(PaginationDTO srchParams)
        {
            return await DealService.SearchPaged(srchParams, IsPublishedOnly, UserId, FirmID);
        }
        #endregion

    }
}
