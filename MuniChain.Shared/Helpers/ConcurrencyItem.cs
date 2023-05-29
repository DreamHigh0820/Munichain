namespace Shared.Helpers
{
    public class ConcurrencyItem
    {
        public string BaseModelName { get; set; }
        public string Prop { get; set; }
        public string MaturityDate { get; set; }
        public object Original { get; set; }
        public ChangeEventType? EventType { get; set; }
        public object Updated { get; set; }

        public object FromDB { get; set; }
        public bool IsUpdatedChecked { get; set; }
        public bool IsFromDBChecked { get; set; }
        public bool IsUpdatedDecimal
        {
            get
            {
                return Updated != null && decimal.TryParse(Updated.ToString(), out _);
            }
        }
        public bool IsFromDBDecimal
        {
            get
            {
                return FromDB != null && decimal.TryParse(FromDB.ToString(), out _);
            }
        }
        public bool IsFromDBDateTime
        {
            get
            {
                return FromDB != null && DateTime.TryParse(FromDB.ToString(), out _);
            }
        }
        public bool IsUpdatedDateTime
        {
            get
            {
                return Updated != null && DateTime.TryParse(Updated.ToString(), out _);
            }
        }
    }

    public enum ChangeEventType
    {
        Modified,
        Added,
        Removed
    }
}
