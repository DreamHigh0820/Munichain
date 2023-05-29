using System.ComponentModel;

namespace Shared.Models.DealComponents
{


    public class Permissions
    {
        public enum Deal
        {
            [Description("Deal.None")]
            None = 0,
            [Description("Deal.Read")]
            Read,
            [Description("Deal.Write")]
            Write,
            [Description("Deal.Admin")]
            Admin
        }

        public enum Expenditures
        {
            [Description("Expenditures.None")]
            None = 0,
            [Description("Expenditures.Read")]
            Read,
            [Description("Expenditures.Write")]
            Write,
        }

        public enum Performance
        {
            [Description("Performance.None")]
            None = 0,
            [Description("Performance.Read")]
            Read,
            [Description("Performance.Write")]
            Write,
        }
    }
}
