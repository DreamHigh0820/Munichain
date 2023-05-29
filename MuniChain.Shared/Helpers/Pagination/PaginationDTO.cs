namespace Shared.Helpers.Pagination
{
    public class PaginationDTO
    {
        public int CurrentPage { get; set; } = 1;
        public int RecordsPerPage { get; set; } = 10;
        public string SortField { get; set; }
        public string SortOrder { get; set; } = "DESC";
        public string SearchText { get; set; } = string.Empty;
        public string SearchGridName { get; set; } = string.Empty;
        public string[] SearchColumns { get; set; }
    }
}
