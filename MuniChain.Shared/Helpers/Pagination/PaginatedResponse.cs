namespace Shared.Helpers.Pagination
{
    public class PaginatedResponse<T>
    {
        public List<T> Records { get; set; }
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
    }
}
