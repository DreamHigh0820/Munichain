using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using UI.Components.Other;
using Shared.Helpers;
using Shared.Models.AppComponents;
using Shared.Models.DealComponents;
using Shared.Models.Users;
using System.Text.RegularExpressions;
using Hangfire;

namespace UI.Components.Deal
{
    public partial class Participants
    {
        [CascadingParameter]
        public Error? Error { get; set; }
        [Parameter]
        public DealModel dealInformation { get; set; }
        [Parameter]
        public bool IsPublicView { get; set; }

        private List<DealParticipant> participants = new();
        private List<Firm> firmParticipants = new();
        private DealParticipant currentSelectedParticipant = new();
        private DealParticipant oldSelectedParticipant = new();
        private PermissionsModal currentSelectedParticipantPermissionsModal = new();
        private bool ShowPermissionsPopup { get; set; } = false;
        private List<string> currentUserPermissions = new();

        public string emailAddress;
        public string role;

        private bool loading { get; set; } = true;
        private User user;
        private List<DealParticipant> origParticipants = new();

        protected override async Task OnInitializedAsync()
        {
            var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
            user = authState.ToUser();

            telemetry.Context.User.Id = user.Id;
            telemetry.TrackPageView("Participants");

            loading = true;
            try
            {
                await LoadComponent();
            }
            catch (Exception ex)
            {
                Error?.ProcessError("Failed to get participants by deal ID", ex, user, dealInformation);
                return;
            }

            loading = false;
        }

        private async Task LoadComponent()
        {
            origParticipants = new List<DealParticipant>();
            var dealId = dealInformation.IsMasterCopy ? dealInformation.Id : dealInformation.HistoryDealID;
            participants = (await dealParticipantService.GetParticipantsByDealId(dealId))
                                                        .OrderByDescending(i => i.UserId == dealInformation.CreatedBy)
                                                        .ThenByDescending(x => x.DealPermissions.Contains("Deal.Admin"))
                                                        .ThenByDescending(r => r.CreatedBy == user.Id)
                                                        .ThenBy(d => d.DealPermissions.Count)
                                                        .ToList();
            await ReloadFirmParticipants();
            currentUserPermissions = participants.FirstOrDefault(x => x.UserId == user.Id)?.DealPermissions;

            foreach (var participant in participants)
            {
                origParticipants.Add((DealParticipant)participant.Clone());
            }
        }

        private static bool IsValid(string email)
        {
            string regex = @"^[^@\s]+@[^@\s]+\.(com|net|org|gov)$";

            return Regex.IsMatch(email, regex, RegexOptions.IgnoreCase);
        }

        public async Task AddParticipant(string email)
        {
            if (string.IsNullOrEmpty(role)) { toastService.ShowError("Participant role is required."); return; }
            if (string.IsNullOrEmpty(email) || !IsValid(email)) { toastService.ShowError("Participant email is invalid."); return; }
            
            try
            {
                var participant = await userService.GetUserByEmail(email);
                var exists = await dealParticipantService.ParticipantExistsOnDeal(email, dealInformation.Id);

                if (exists)
                {
                    toastService.ShowError("Participant already exists on this deal!");
                }
                else
                {
                    DealParticipant person = new DealParticipant()
                    {
                        DealId = dealInformation.Id,
                        IsPublic = false,
                        Id = Guid.NewGuid().ToString(),
                        EmailAddress = email,
                        DisplayName = participant?.DisplayName == null ? null : participant.DisplayName,
                        UserId = participant?.Id == null ? null : participant.Id,
                        Role = role,
                        CreatedBy = user.Id,
                        DateAddedUTC = DateTime.Now,
                        DealPermissions = new List<string> { PermissionExtensions.DealNone, PermissionExtensions.ExpendituresNone, PermissionExtensions.PerformanceNone }
                    };
                    await dealParticipantService.CreateParticipant(person);
                    BackgroundJobs.Enqueue(() => notificationService.ParticipantAddedNotification(user, dealInformation, person));
                    participants.Add(person);
                }
                
                await LoadComponent();

                emailAddress = "";
                role = "";
            }
            catch (Exception ex)
            {
                Error?.ProcessError("Failed to add participants to deal", ex, user, dealInformation);
                return;
            }
        }

        public async Task RemoveParticipant(DealParticipant participant)
        {
            if (participant.UserId == dealInformation.CreatedBy)
            {
                // User tried to delete created by
            }
            else
            {
                try
                {
                    await dealParticipantService.DeleteParticipant(participant);
                    participants.Remove(participant);

                    await ReloadFirmParticipants();
                    BackgroundJobs.Enqueue(() => notificationService.ParticipantRemovedNotification(user, dealInformation, participant));
                }
                catch (Exception ex)
                {
                    Error?.ProcessError("Failed to remove participant from deal", ex, user, dealInformation);
                    return;
                }
            }
        }

        public async Task ReloadFirmParticipants()
        {
            var firmIds = (await firmService.GetFirmsByUserEmail(participants.Select(x => x.EmailAddress).ToArray())).Select(x => x.FirmId);
            firmParticipants = await firmService.GetByIds(firmIds.ToList());
            StateHasChanged();
        }

        public async Task SaveParticipants()
        {
            try
            {

                Dictionary<string, List<ConcurrencyItem>> variances = new();
                foreach (var participant in origParticipants)
                {
                    var updated = participants.FirstOrDefault(x => x.Id == participant.Id);
                    List<ConcurrencyItem> parDiffs = updated.ConcurrencyCompare(participant, addlRules: new[] { "Role" });
                    foreach (var variance in parDiffs.Distinct().ToList())
                    {
                        if (variances.ContainsKey(participant.Id))
                        {
                            variances.FirstOrDefault(x => x.Key == participant.Id).Value.Append(variance);
                        }
                        else
                        {
                            variances.Add(participant.Id, parDiffs);
                        }
                    }
                }

                await dealParticipantService.UpdateParticipants(participants);

                if (variances.Any())
                {
                    await notificationService.ParticipantModified(user, dealInformation, participants, variances);
                }
            }
            catch (DbUpdateException)
            {
                toastService.ShowError("Someone has updated the deal after you have opened it. Please refresh");
                return;
            }
            catch (Exception ex)
            {
                Error?.ProcessError("Failed to save participants to deal", ex, user, dealInformation);
                return;
            }

            toastService.ShowSuccess("Successfully saved participants");
            return;
        }

        private async Task OnPermissionsOpen(DealParticipant participant)
        {
            currentSelectedParticipant = participant;
            var dealPermission = currentSelectedParticipant.DealPermissions.Where(x => !x.Contains("Deal.Admin")).FirstOrDefault(x => x.Contains("Deal."));
            var expenditurePermission = currentSelectedParticipant.DealPermissions.FirstOrDefault(x => x.Contains("Expenditures."));
            var performancePermission = currentSelectedParticipant.DealPermissions.FirstOrDefault(x => x.Contains("Performance."));

            currentSelectedParticipantPermissionsModal = new PermissionsModal()
            {
                DealPermission = string.IsNullOrEmpty(dealPermission) ? PermissionExtensions.DealNone : dealPermission,
                ExpenditurePermission = string.IsNullOrEmpty(expenditurePermission) ? PermissionExtensions.ExpendituresNone : expenditurePermission,
                PerformancePermission = string.IsNullOrEmpty(performancePermission) ? PermissionExtensions.PerformanceNone : performancePermission,
                IsAdmin = currentSelectedParticipant.DealPermissions.Exists(x => x.Contains("Deal.Admin")),
            };

            //set my compare data for notification changes
            if (currentSelectedParticipantPermissionsModal.IsAdmin)
            {
                oldSelectedParticipant.DealPermissions = new List<string> { "Deal.Admin" };
            }
            else
            {
                oldSelectedParticipant.DealPermissions = new List<string> { currentSelectedParticipantPermissionsModal.DealPermission.ToString(), currentSelectedParticipantPermissionsModal.PerformancePermission.ToString(), currentSelectedParticipantPermissionsModal.ExpenditurePermission.ToString() };
            }
            ShowPermissionsPopup = true;
        }

        private async Task OnPermissionsSave(DealParticipant participant)
        {
            try
            {
                if (!currentSelectedParticipantPermissionsModal.IsPermissionsModalValuesValid(currentUserPermissions))
                {
                    ShowPermissionsPopup = false;
                    toastService.ShowError("Invalid input.");
                    await emailService.ErrorEmail("Invalid input on permission save", currentUser: user);
                    return;
                }

                if (currentSelectedParticipantPermissionsModal.IsAdmin)
                {
                    participant.DealPermissions = new List<string> { "Deal.Admin" };
                }
                else
                {
                    participant.DealPermissions = new List<string>
                    {
                        currentSelectedParticipantPermissionsModal.DealPermission?.ToString() ?? "Deal.None",
                        currentSelectedParticipantPermissionsModal.PerformancePermission?.ToString() ?? "Performance.None",
                        currentSelectedParticipantPermissionsModal.ExpenditurePermission?.ToString() ?? "Expenditures.None"
                    };
                }

                var oldVariances = oldSelectedParticipant.DealPermissions.Except(participant.DealPermissions).ToList();
                var newVariances = participant.DealPermissions.Except(oldSelectedParticipant.DealPermissions).ToList();

                await dealParticipantService.UpdateParticipantPermissions(participant);
                BackgroundJobs.Enqueue(() => notificationService.ParticipantPermissionsModified(user, dealInformation, newVariances, oldVariances, participant));

                ShowPermissionsPopup = false;
            }
            catch (DbUpdateException)
            {
                toastService.ShowError("Someone has updated the deal after you have opened it. Please refresh");
                return;
            }
            catch (Exception ex)
            {
                Error?.ProcessError("Failed to save permissions", ex, user, dealInformation);
                return;
            }
        }
    }
}
