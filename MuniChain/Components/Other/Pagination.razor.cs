using Microsoft.AspNetCore.Components;

namespace UI.Components.Other
{
    public class PaginationBase : ComponentBase
    {
        [Parameter] public int CurrentPage { get; set; } = 1;
        [Parameter] public int TotalPages { get; set; }
        [Parameter] public int Radius { get; set; } = 3;
        [Parameter] public EventCallback<int> PageChange { get; set; }
        public List<LinkModel> Links;

        public async Task SelectedPageInternal(LinkModel link)
        {
            if (link.Page == CurrentPage)
            {
                return;
            }

            if (!link.Enabled)
            {
                return;
            }

            CurrentPage = link.Page;
            await PageChange.InvokeAsync(link.Page);
        }

        protected override void OnParametersSet()
        {
            LoadPages();
            base.OnParametersSet();
        }

        public void LoadPages()
        {
            Links = new List<LinkModel>();
            var isPreviousPageLinkEnabled = CurrentPage != 1;
            var previousPage = CurrentPage - 1;
            Links.Add(new LinkModel(previousPage, isPreviousPageLinkEnabled, "Previous"));

            for (int i = 1; i <= TotalPages; i++)
            {
                if (i >= CurrentPage - Radius && i <= CurrentPage + Radius)
                {
                    Links.Add(new LinkModel(i) { Active = CurrentPage == i });
                }
            }

            var isNextPageLinkEnabled = CurrentPage != TotalPages;
            var nextPage = CurrentPage + 1;
            Links.Add(new LinkModel(nextPage, isNextPageLinkEnabled, "Next"));
        }
    }
    public class LinkModel
    {
        public LinkModel(int page)
            : this(page, true) { }

        public LinkModel(int page, bool enabled)
            : this(page, enabled, page.ToString())
        { }

        public LinkModel(int page, bool enabled, string text)
        {
            Page = page;
            Enabled = enabled;
            Text = text;
        }

        public string Text { get; set; }
        public int Page { get; set; }
        public bool Enabled { get; set; } = true;
        public bool Active { get; set; } = false;
    }
}
