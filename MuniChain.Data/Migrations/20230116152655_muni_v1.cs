using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    public partial class muni_v1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotificationPreferences");

            migrationBuilder.DropColumn(
                name: "DealName",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "DealPermission",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "ExpenditurePermission",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "PerformancePermission",
                table: "Notifications");

            migrationBuilder.CreateTable(
                name: "UserNotificationPreference",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DealArchived = table.Column<bool>(type: "bit", nullable: false),
                    DealMadePublic = table.Column<bool>(type: "bit", nullable: false),
                    DealUpdated = table.Column<bool>(type: "bit", nullable: false),
                    DealUpdatedMultiple = table.Column<bool>(type: "bit", nullable: false),
                    DealUpdatedAudit = table.Column<bool>(type: "bit", nullable: false),
                    DealPartAdded = table.Column<bool>(type: "bit", nullable: false),
                    DealPartRemoved = table.Column<bool>(type: "bit", nullable: false),
                    ParticipantAdd = table.Column<bool>(type: "bit", nullable: false),
                    ParticipantRemove = table.Column<bool>(type: "bit", nullable: false),
                    ParticipantPermissionUpdate = table.Column<bool>(type: "bit", nullable: false),
                    ParticipantAddedAsAdmin = table.Column<bool>(type: "bit", nullable: false),
                    ParticipantRoleUpdated = table.Column<bool>(type: "bit", nullable: false),
                    ParticipantPublic = table.Column<bool>(type: "bit", nullable: false),
                    ParticipantPrivate = table.Column<bool>(type: "bit", nullable: false),
                    ParticipantModified = table.Column<bool>(type: "bit", nullable: false),
                    Saved = table.Column<bool>(type: "bit", nullable: false),
                    Opened = table.Column<bool>(type: "bit", nullable: false),
                    Sent = table.Column<bool>(type: "bit", nullable: false),
                    Uploaded = table.Column<bool>(type: "bit", nullable: false),
                    DocumentCommentedOn = table.Column<bool>(type: "bit", nullable: false),
                    DocumentUploadedForUser = table.Column<bool>(type: "bit", nullable: false),
                    DocumentUploadedForSigner = table.Column<bool>(type: "bit", nullable: false),
                    DocumentUploadedForReviewer = table.Column<bool>(type: "bit", nullable: false),
                    DocumentParticipantAdded = table.Column<bool>(type: "bit", nullable: false),
                    DocumentParticipantRemoved = table.Column<bool>(type: "bit", nullable: false),
                    DocumentVisibilityChangedToParticipants = table.Column<bool>(type: "bit", nullable: false),
                    DocumentVisibilityChangedToPublic = table.Column<bool>(type: "bit", nullable: false),
                    DocumentVisibilityChanged = table.Column<bool>(type: "bit", nullable: false),
                    DocumentAnnotationAdded = table.Column<bool>(type: "bit", nullable: false),
                    DocumentAnnotationChanged = table.Column<bool>(type: "bit", nullable: false),
                    DocumentAnnotationCommentAdded = table.Column<bool>(type: "bit", nullable: false),
                    Signed = table.Column<bool>(type: "bit", nullable: false),
                    Shared = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    Commented = table.Column<bool>(type: "bit", nullable: false),
                    Completed = table.Column<bool>(type: "bit", nullable: false),
                    Declined = table.Column<bool>(type: "bit", nullable: false),
                    Revoked = table.Column<bool>(type: "bit", nullable: false),
                    Reassigned = table.Column<bool>(type: "bit", nullable: false),
                    Expired = table.Column<bool>(type: "bit", nullable: false),
                    ExpenditureAdded = table.Column<bool>(type: "bit", nullable: false),
                    ExpenditureChanged = table.Column<bool>(type: "bit", nullable: false),
                    PerformanceAdded = table.Column<bool>(type: "bit", nullable: false),
                    PerformanceUpdated = table.Column<bool>(type: "bit", nullable: false),
                    PerformanceTopAccountAdded = table.Column<bool>(type: "bit", nullable: false),
                    PerformanceTopAccountChanged = table.Column<bool>(type: "bit", nullable: false),
                    PerformanceTopAccountMarginAmountChanged = table.Column<bool>(type: "bit", nullable: false),
                    PerformanceTopAccountMarginDateChanged = table.Column<bool>(type: "bit", nullable: false),
                    FirmMemberAddedToFirm = table.Column<bool>(type: "bit", nullable: false),
                    MessageBoardComment = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserNotificationPreference", x => x.ID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserNotificationPreference");

            migrationBuilder.AddColumn<string>(
                name: "DealName",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DealPermission",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExpenditurePermission",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PerformancePermission",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "NotificationPreferences",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DocumentComments = table.Column<bool>(type: "bit", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationPreferences", x => x.Id);
                });
        }
    }
}
