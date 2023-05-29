using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    public partial class NotificationprefCleanup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DealArchived",
                table: "UserNotificationPreference");

            migrationBuilder.DropColumn(
                name: "DealMadePublic",
                table: "UserNotificationPreference");

            migrationBuilder.DropColumn(
                name: "DealPartAdded",
                table: "UserNotificationPreference");

            migrationBuilder.DropColumn(
                name: "DealPartRemoved",
                table: "UserNotificationPreference");

            migrationBuilder.DropColumn(
                name: "DealUpdated",
                table: "UserNotificationPreference");

            migrationBuilder.DropColumn(
                name: "DealUpdatedAudit",
                table: "UserNotificationPreference");

            migrationBuilder.DropColumn(
                name: "DealUpdatedMultiple",
                table: "UserNotificationPreference");

            migrationBuilder.DropColumn(
                name: "DocumentCommentedOn",
                table: "UserNotificationPreference");

            migrationBuilder.DropColumn(
                name: "DocumentParticipantAdded",
                table: "UserNotificationPreference");

            migrationBuilder.DropColumn(
                name: "DocumentParticipantRemoved",
                table: "UserNotificationPreference");

            migrationBuilder.DropColumn(
                name: "DocumentUploadedForReviewer",
                table: "UserNotificationPreference");

            migrationBuilder.DropColumn(
                name: "DocumentUploadedForSigner",
                table: "UserNotificationPreference");

            migrationBuilder.DropColumn(
                name: "DocumentUploadedForUser",
                table: "UserNotificationPreference");

            migrationBuilder.DropColumn(
                name: "DocumentVisibilityChanged",
                table: "UserNotificationPreference");

            migrationBuilder.DropColumn(
                name: "DocumentVisibilityChangedToParticipants",
                table: "UserNotificationPreference");

            migrationBuilder.DropColumn(
                name: "DocumentVisibilityChangedToPublic",
                table: "UserNotificationPreference");

            migrationBuilder.DropColumn(
                name: "ExpenditureAdded",
                table: "UserNotificationPreference");

            migrationBuilder.DropColumn(
                name: "ExpenditureChanged",
                table: "UserNotificationPreference");

            migrationBuilder.DropColumn(
                name: "FirmMemberAddedToFirm",
                table: "UserNotificationPreference");

            migrationBuilder.DropColumn(
                name: "ParticipantAdd",
                table: "UserNotificationPreference");

            migrationBuilder.DropColumn(
                name: "ParticipantAddedAsAdmin",
                table: "UserNotificationPreference");

            migrationBuilder.DropColumn(
                name: "ParticipantModified",
                table: "UserNotificationPreference");

            migrationBuilder.DropColumn(
                name: "ParticipantPermissionUpdate",
                table: "UserNotificationPreference");

            migrationBuilder.DropColumn(
                name: "ParticipantPrivate",
                table: "UserNotificationPreference");

            migrationBuilder.DropColumn(
                name: "ParticipantPublic",
                table: "UserNotificationPreference");

            migrationBuilder.RenameColumn(
                name: "PerformanceUpdated",
                table: "UserNotificationPreference",
                newName: "Performance");

            migrationBuilder.RenameColumn(
                name: "PerformanceTopAccountMarginDateChanged",
                table: "UserNotificationPreference",
                newName: "Participant");

            migrationBuilder.RenameColumn(
                name: "PerformanceTopAccountMarginAmountChanged",
                table: "UserNotificationPreference",
                newName: "Firm");

            migrationBuilder.RenameColumn(
                name: "PerformanceTopAccountChanged",
                table: "UserNotificationPreference",
                newName: "Expenditure");

            migrationBuilder.RenameColumn(
                name: "PerformanceTopAccountAdded",
                table: "UserNotificationPreference",
                newName: "DocumentParticipant");

            migrationBuilder.RenameColumn(
                name: "PerformanceAdded",
                table: "UserNotificationPreference",
                newName: "DocumentCommented");

            migrationBuilder.RenameColumn(
                name: "ParticipantRoleUpdated",
                table: "UserNotificationPreference",
                newName: "Document");

            migrationBuilder.RenameColumn(
                name: "ParticipantRemove",
                table: "UserNotificationPreference",
                newName: "Deal");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Performance",
                table: "UserNotificationPreference",
                newName: "PerformanceUpdated");

            migrationBuilder.RenameColumn(
                name: "Participant",
                table: "UserNotificationPreference",
                newName: "PerformanceTopAccountMarginDateChanged");

            migrationBuilder.RenameColumn(
                name: "Firm",
                table: "UserNotificationPreference",
                newName: "PerformanceTopAccountMarginAmountChanged");

            migrationBuilder.RenameColumn(
                name: "Expenditure",
                table: "UserNotificationPreference",
                newName: "PerformanceTopAccountChanged");

            migrationBuilder.RenameColumn(
                name: "DocumentParticipant",
                table: "UserNotificationPreference",
                newName: "PerformanceTopAccountAdded");

            migrationBuilder.RenameColumn(
                name: "DocumentCommented",
                table: "UserNotificationPreference",
                newName: "PerformanceAdded");

            migrationBuilder.RenameColumn(
                name: "Document",
                table: "UserNotificationPreference",
                newName: "ParticipantRoleUpdated");

            migrationBuilder.RenameColumn(
                name: "Deal",
                table: "UserNotificationPreference",
                newName: "ParticipantRemove");

            migrationBuilder.AddColumn<bool>(
                name: "DealArchived",
                table: "UserNotificationPreference",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DealMadePublic",
                table: "UserNotificationPreference",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DealPartAdded",
                table: "UserNotificationPreference",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DealPartRemoved",
                table: "UserNotificationPreference",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DealUpdated",
                table: "UserNotificationPreference",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DealUpdatedAudit",
                table: "UserNotificationPreference",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DealUpdatedMultiple",
                table: "UserNotificationPreference",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DocumentCommentedOn",
                table: "UserNotificationPreference",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DocumentParticipantAdded",
                table: "UserNotificationPreference",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DocumentParticipantRemoved",
                table: "UserNotificationPreference",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DocumentUploadedForReviewer",
                table: "UserNotificationPreference",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DocumentUploadedForSigner",
                table: "UserNotificationPreference",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DocumentUploadedForUser",
                table: "UserNotificationPreference",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DocumentVisibilityChanged",
                table: "UserNotificationPreference",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DocumentVisibilityChangedToParticipants",
                table: "UserNotificationPreference",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DocumentVisibilityChangedToPublic",
                table: "UserNotificationPreference",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ExpenditureAdded",
                table: "UserNotificationPreference",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ExpenditureChanged",
                table: "UserNotificationPreference",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "FirmMemberAddedToFirm",
                table: "UserNotificationPreference",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ParticipantAdd",
                table: "UserNotificationPreference",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ParticipantAddedAsAdmin",
                table: "UserNotificationPreference",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ParticipantModified",
                table: "UserNotificationPreference",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ParticipantPermissionUpdate",
                table: "UserNotificationPreference",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ParticipantPrivate",
                table: "UserNotificationPreference",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ParticipantPublic",
                table: "UserNotificationPreference",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
