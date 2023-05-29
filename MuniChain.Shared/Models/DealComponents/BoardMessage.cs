namespace Shared.Models.DealComponents
{
    public class BoardMessage
    {
        public string? Id { get; set; }
        public string? DealId { get; set; }
        public string? Message { get; set; }
        public DateTime? DateGivenUTC { get; set; }
        public string? GivenByName { get; set; }
        public string? GivenByUserId { get; set; }
    }
}
