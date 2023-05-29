
using Microsoft.AspNetCore.Components;

namespace UI.Components.Other
{
    public partial class PagedList<TItem>
    {
        [Parameter]
        public List<TItem> ListQuery { get; set; }
        [Parameter]
        public RenderFragment<TItem> ItemDisplay { get; set; }
        [Parameter]
        public int ItemsPerPage { get; set; } = 7;

        private int CurrentPage = 1;
        private List<TItem> CurrentDisplay;
        private int TotalCount;

        protected override void OnParametersSet()
        {
            UpdateDisplay();
            TotalCount = ListQuery.Count();
        }

        private void UpdateDisplay()
        {
            CurrentDisplay = ListQuery.Skip((CurrentPage - 1) * ItemsPerPage).Take(ItemsPerPage).ToList();
        }

        private bool AtLastPage()
        {
            return CurrentPage >= TotalPages();
        }

        private int TotalPages()
        {
            return Convert.ToInt32(Math.Ceiling(TotalCount / Convert.ToDecimal(ItemsPerPage)));
        }

        private void MoveFirst()
        {
            CurrentPage = 1;
            UpdateDisplay();
        }

        private void MoveBack()
        {
            CurrentPage--;
            UpdateDisplay();
        }

        private void MoveNext()
        {
            CurrentPage++;
            UpdateDisplay();
        }

        private void MoveLast()
        {
            CurrentPage = TotalPages();
            UpdateDisplay();
        }
    }
}
