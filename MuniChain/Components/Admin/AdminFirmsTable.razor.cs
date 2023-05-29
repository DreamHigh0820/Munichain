using Shared.Models.AppComponents;
using Syncfusion.Blazor.Popups;
using UI.Components.Firms;

namespace UI.Components.Admin
{
    public partial class AdminFirmsTable
    {
        private bool AddFirmVisibility { get; set; } = false;
        private Firm firmToEdit = new();
        private Firm firmToDelete = new();
        private string firmAdminEmails;
        private FirmsGrid grid;

        private void OnAddFirmClick(bool edit, Firm firm = null)
        {
            if (edit)
            {
                firmToEdit = (Firm)firm.Clone();
                firmAdminEmails = firm.FirmAdminEmails != null ? string.Join(",", firm.FirmAdminEmails) : "";
            }
            // Add new firm
            else
            {
                firmToEdit = new() { Id = Guid.NewGuid().ToString() };
                firmAdminEmails = "";
            }

            AddFirmVisibility = true;
        }

        private async Task SaveFirm()
        {
            await firmsService.Update(firmToEdit);
            await firmsService.UpdateFirmAdmins(firmAdminEmails, firmToEdit.Id);
            await grid.LoadFirmsForGrid();
            AddFirmVisibility = false;
        }
        private void OverlayClick(OverlayModalClickEventArgs args)
        {
            AddFirmVisibility = false;
        }

        private bool ShowDeletePopup { get; set; } = false;
        private void OnDeleteFirmClick(Firm firm)
        {
            firmToDelete = (Firm)firm.Clone();
            ShowDeletePopup = true;
        }
        private async Task DeleteFirm()
        {
            await firmsService.Delete(firmToDelete.Id);
            await grid.LoadFirmsForGrid();
            ShowDeletePopup = false;
            StateHasChanged();
        }
    }
}
