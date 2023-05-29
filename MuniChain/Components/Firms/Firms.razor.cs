using Shared.Models.AppComponents;
using Syncfusion.Blazor.Grids;

namespace UI.Components.Firms
{
    public partial class Firms
    {
        SfGrid<Firm> FirmsList;

        public async Task GoToSelectedFirm(RowSelectEventArgs<Firm> args)
        {
            var record = await FirmsList.GetSelectedRecordsAsync();
            var value = record.First();
            navigationManager.NavigateTo($"firm/{value.Id}");
            return;
        }
    }
}
