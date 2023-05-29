using Microsoft.AspNetCore.Components;
using UI.Components.Other;
using Shared.Helpers;
using Shared.Models.AppComponents;
using Shared.Models.Users;
using System.Text.RegularExpressions;

namespace UI.Components.Firms
{
    public partial class FirmAdmin
    {
        [CascadingParameter]
        public Error? Error { get; set; }
        Firm firm;
        private string firmAdminEmail;
        private FirmMemberGrid grid;
        private bool loading { get; set; } = true;
        private Tuple<int, decimal?> deals;
        private List<string> firmMemberEmails = new();
        User user;
        private bool AddUserVisibility { get; set; } = false;

        private bool Visibility { get; set; } = false;

        private void OnAddClick()
        {
            if (!firmMemberEmails.Any())
                firmMemberEmails.Add("");
            Visibility = true;
        }
        private async Task InviteButtonClick()
        {
            try
            {
                await AddMemberToFirm(firm, firmMemberEmails);
            }
            catch (Exception ex)
            {
                Error?.ProcessError("Failed to add user to firm", ex, null, null);
                return;
            }

            firmMemberEmails = new();
        }
        private async Task AddFirmAdmin()
        {
            try
            {
                await AddMemberToFirm(firm, new List<string> { firmAdminEmail }, true);
            }
            catch (Exception ex)
            {
                Error?.ProcessError("Failed to add user to firm", ex, null, null);
                return;
            }
        }

        protected override async Task OnInitializedAsync()
        {
            loading = true;
            user = (await auth.GetAuthenticationStateAsync()).ToUser();
            firm = await firmsService.GetFirmWhereUserIsAdmin(user.Email);
            if (firm == null)
            {
                navigationManager.NavigateTo("/404", true);
                return;
            }
            deals = await dealsService.GetDealsSumByFirmID(firm.Id, false);

            loading = false;
        }

        private static bool IsValid(List<string> emails)
        {
            string regex = @"^[^@\s]+@[^@\s]+\.(com|net|org|gov)$";
            var validList = false;

            foreach (var email in emails)
            {
                validList = Regex.IsMatch(email, regex, RegexOptions.IgnoreCase);
                if (!validList)
                {
                    return false;
                }
            }

            return validList;

        }

        static decimal? SumFloats(params decimal?[] values)
        {
            decimal total = 0;
            for (var i = 0; i < values.Length; i++)
            {
                total += values[i] ?? 0;
            }
            return total;
        }

        public async Task AddMemberToFirm(Firm firm, List<string> firmMemberEmails, bool isAdmin = false)
        {
            if (firmMemberEmails.Any())
            {
                return;
            }

            firmMemberEmails = firmMemberEmails.Select(s => s.Trim()).ToList();
            if (!IsValid(firmMemberEmails))
            {
                toastService.ShowError("Invalid email inputs.");
                return;
            }

            List<FirmMember> usersToAdd = new List<FirmMember>();
            var users = await userService.BatchGetUsersByEmail(firmMemberEmails.ToArray());

            foreach (var email in firmMemberEmails)
            {
                var user = users.FirstOrDefault(x => x.Email == email);

                var member = new FirmMember()
                {
                    Id = Guid.NewGuid().ToString(),
                    EmailAddress = email,
                    FirmId = firm?.Id,
                    UserId = user?.Id == null ? null : user.Id,
                    IsAdmin = isAdmin
                };
                usersToAdd.Add(member);
            }

            try
            {
                var areUsersInFirmAlready = await firmsService.GetFirmsByUserEmail(firmMemberEmails.ToArray());
                if (areUsersInFirmAlready.Any())
                {
                    Visibility = false;
                    string error = "";
                    foreach (var user in areUsersInFirmAlready)
                    {
                        if (areUsersInFirmAlready.Last() == user)
                        {
                            error += user.EmailAddress;
                        }
                        else
                        {
                            error += user.EmailAddress + ",";
                        }
                    }
                    if (areUsersInFirmAlready.Count == 1)
                    {
                        toastService.ShowError(error + " is already in a firm.");
                    }
                    else
                    {
                        toastService.ShowError(error + " are already a part of firms.");
                    }
                }
                else
                {
                    await firmsService.AddMembersToFirm(usersToAdd);
                    foreach (var member in usersToAdd)
                    {
                        await notificationService.FirmMemberAddedToFirmNotification(user, firm, member);
                    }
                    navigationManager.NavigateTo("/firm/admin", true);
                    return;
                }
            }
            catch (Exception ex)
            {
                Error?.ProcessError("Failed to add member to firm", ex, user, null);
                return;
            }
        }

        public async Task RemoveFirmMember(User userToRemove)
        {
            try
            {
                loading = true;
                await InvokeAsync(() => StateHasChanged());
                await firmsService.DeleteFirmMember(userToRemove.Email);
                await grid.LoadMembersForGrid();
                firm = await firmsService.GetFirmWhereUserIsAdmin(user.Email);
            }
            catch (Exception ex)
            {
                Error?.ProcessError("Failed to delete member of firm", ex, user, null);
                return;
            }
            await InvokeAsync(() => StateHasChanged());
            loading = false;
        }
    }
}

