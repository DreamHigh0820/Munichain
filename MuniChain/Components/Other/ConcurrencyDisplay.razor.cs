using Microsoft.AspNetCore.Components;
using Shared.Helpers;

namespace UI.Components.Other
{
    public partial class ConcurrencyDisplay : ComponentBase
    {
        [Parameter]
        public List<ConcurrencyItem> LstConcurrencyItem { get; set; } = new List<ConcurrencyItem>();
        [Parameter]
        public bool IsShowConcurrencyItem { get; set; }
        [Parameter]
        public object UpdatedObj { get; set; }
        [Parameter] public EventCallback<bool> MergeSubmit { get; set; }
        private void FromDBChange(ChangeEventArgs args, ConcurrencyItem item)
        {
            if ((bool)args.Value)
            {
                item.IsUpdatedChecked = false;
            }
        }
        private async Task MergeSubmitInternal()
        {
            foreach (var item in LstConcurrencyItem)
            {
                if (item.IsFromDBChecked)
                {
                    UpdatedObj.SetPropertyByName(item.Prop, item.FromDB);
                }
            }
            await MergeSubmit.InvokeAsync(true);
        }
    }
}
