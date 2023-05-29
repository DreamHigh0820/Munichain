using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppointmentData",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsAllDay = table.Column<bool>(type: "bit", nullable: false),
                    CategoryColor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsReadonly = table.Column<bool>(type: "bit", nullable: false),
                    RecurrenceRule = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecurrenceID = table.Column<int>(type: "int", nullable: true),
                    RecurrenceException = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartTimezone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EndTimezone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DealModelId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppointmentData", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BoardMessages",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DealId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateGivenUTC = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GivenByName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GivenByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoardMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChatMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FromUserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FromUserDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ToConversationId = table.Column<int>(type: "int", nullable: false),
                    DateSentUTC = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Conversations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MemberIds = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MemberEmails = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MemberDisplayNames = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateCreatedUTC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConversationReadByMembers = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conversations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DealExpenditure",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DealModelId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsOther = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DealExpenditure", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "DealParticipants",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DealId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsPublic = table.Column<bool>(type: "bit", nullable: true),
                    DateAddedUTC = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DealPermissions = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DealParticipants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Deals",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SaleDateUTC = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OfferingType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Issuer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IssuerURL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Size = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    State = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CUSIP6 = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsLocked = table.Column<bool>(type: "bit", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedDateUTC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastModifiedByDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastModifiedDateTimeUTC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HistoryDealID = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DealId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedByFullName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PublicDocumentViewSettings = table.Column<int>(type: "int", nullable: true),
                    UserPermissions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDateTimeUTC = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Firm",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FirmType = table.Column<int>(type: "int", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    State = table.Column<int>(type: "int", nullable: true),
                    Confirmed = table.Column<bool>(type: "bit", nullable: true),
                    Bio = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Firm", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NotificationPreferences",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DocumentComments = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationPreferences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DealId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DealName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DealSize = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DealDesc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DealState = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DealParticipant = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DealRole = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OldRole = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DealPermission = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DocumentId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DocumentName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DocumentType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DocParticipant = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DocumentStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExpenditureField = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExpenditureValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TopAccount = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TopAccountParAmount = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TopAccountMaturityDateUTC = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FirmName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FirmMember = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActionBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NotifyTo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PropertyChanged = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OldObject = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewObject = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExpenditurePermission = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PerformancePermission = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Action = table.Column<int>(type: "int", nullable: true),
                    ActionDateTimeUTC = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsUserRead = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Reference",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DealId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FirmId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GivenBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateGivenUTC = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reference", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JobTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StateValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AreasOfExpertise = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Bio = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Confirmed = table.Column<bool>(type: "bit", nullable: true),
                    TimeZone = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Performance",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NIC = table.Column<int>(type: "int", nullable: true),
                    TIC = table.Column<int>(type: "int", nullable: true),
                    GrossSpread = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Takedown = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TotalRetailPar = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TotalInstitutionalPar = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ParAmountBonds = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ReofferingPremium = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    GrossProduction = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TotalUnderwriterDiscount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Bid = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    BidPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    BondYearDollars = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AverageLife = table.Column<int>(type: "int", nullable: true),
                    AverageCoupon = table.Column<int>(type: "int", nullable: true),
                    DealModelId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedDateUTC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HistoryPerformanceID = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Performance", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Performance_Deals_DealModelId",
                        column: x => x.DealModelId,
                        principalTable: "Deals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Series",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDateUTC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Size = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SaleTimeUTC = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DatedDateUTC = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SettlementDateUTC = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveryDateUTC = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TimeZone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TaxStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LeadManager = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Purpose = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SourceOfRepayment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsBankQualified = table.Column<bool>(type: "bit", nullable: true),
                    IsERP = table.Column<bool>(type: "bit", nullable: true),
                    MoodysRating = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SPRating = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FitchRating = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KrollRating = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ESGCertifiedType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ESGVerifier = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DealModelId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    IsPublishedMaturities = table.Column<bool>(type: "bit", nullable: false),
                    HistorySeriesID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GlobalSeriesID = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Series", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Series_Deals_DealModelId",
                        column: x => x.DealModelId,
                        principalTable: "Deals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FirmMember",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FirmId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsAdmin = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FirmMember", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FirmMember_Firm_FirmId",
                        column: x => x.FirmId,
                        principalTable: "Firm",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TopAccount",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AccountName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ParAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MaturityDateUTC = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PerformanceId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreatedDateUTC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HistoryTopAccountID = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TopAccount", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TopAccount_Performance_PerformanceId",
                        column: x => x.PerformanceId,
                        principalTable: "Performance",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Maturity",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsTermed = table.Column<bool>(type: "bit", nullable: false),
                    MaturityDateUTC = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Par = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Coupon = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Yield = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DollarYield = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    YieldDenom = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CUSIP9LastThree = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    SeriesId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreatedDateUTC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CallDateUTC = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CallPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CallType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HistoryMaturityID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GlobalMaturityID = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Maturity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Maturity_Series_SeriesId",
                        column: x => x.SeriesId,
                        principalTable: "Series",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FirmMember_FirmId",
                table: "FirmMember",
                column: "FirmId");

            migrationBuilder.CreateIndex(
                name: "IX_Maturity_SeriesId",
                table: "Maturity",
                column: "SeriesId");

            migrationBuilder.CreateIndex(
                name: "IX_Performance_DealModelId",
                table: "Performance",
                column: "DealModelId",
                unique: true,
                filter: "[DealModelId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Series_DealModelId",
                table: "Series",
                column: "DealModelId");

            migrationBuilder.CreateIndex(
                name: "IX_TopAccount_PerformanceId",
                table: "TopAccount",
                column: "PerformanceId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppointmentData");

            migrationBuilder.DropTable(
                name: "BoardMessages");

            migrationBuilder.DropTable(
                name: "ChatMessages");

            migrationBuilder.DropTable(
                name: "Conversations");

            migrationBuilder.DropTable(
                name: "DealExpenditure");

            migrationBuilder.DropTable(
                name: "DealParticipants");

            migrationBuilder.DropTable(
                name: "Documents");

            migrationBuilder.DropTable(
                name: "FirmMember");

            migrationBuilder.DropTable(
                name: "Maturity");

            migrationBuilder.DropTable(
                name: "NotificationPreferences");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "Reference");

            migrationBuilder.DropTable(
                name: "TopAccount");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Firm");

            migrationBuilder.DropTable(
                name: "Series");

            migrationBuilder.DropTable(
                name: "Performance");

            migrationBuilder.DropTable(
                name: "Deals");
        }
    }
}
