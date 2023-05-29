using Domain.Services.Database;
using Domain.UIProvider;
using Moq;
using Shared.Models.DealComponents;
using Shared.Models.Enums;
using System.Linq;
using Xunit;

namespace Tests.Tests.Services.UIProvider
{
    public class DealViewUIProviderTests
    {
        public Mock<IDealService> dealMock;
        public Mock<IFirmsService> firmMock;
        public Mock<IDealParticipantService> dealParticipantMock;

        public DealViewUIProviderTests()
        {
            dealMock = new Mock<IDealService>();
            firmMock = new Mock<IFirmsService>();
            dealParticipantMock = new Mock<IDealParticipantService>();
        }

        [Theory]
        [InlineData(true, true, new string[] { "Deal.Admin" }, "Information", true)]
        [InlineData(true, false, new string[] { "Deal.None" }, "Information", false)]
        [InlineData(true, true, new string[] { "Deal.None" }, "Information", true)]
        [InlineData(true, true, new string[] { "Deal.Read" }, "Information", true)]
        [InlineData(true, true, new string[] { "Deal.Edit" }, "Information", true)]
        public void ShouldShowElement(bool IsPublicView, bool HasBeenPublished, string[] currentUserPermissions, string TabName, bool expected)
        {
            DealViewResponse handler = new DealViewResponse()
            {
                IsPublicView = IsPublicView,
                DealGranted = new DealModel()
                {
                    HasBeenPublished = HasBeenPublished
                },
                CurrentUserPermissions = currentUserPermissions.ToList()
            };

            var sut = GetSut();
            var actual = sut.ShowElement(handler, TabName);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        // Audit Deal tests
        [InlineData(true, false, true, true, "Deal.Admin", true, DealViewType.ByID)]
        [InlineData(false, false, false, true, "Deal.Admin", true, DealViewType.ByID)]
        [InlineData(false, false, false, true, "Deal.Read", true, DealViewType.NotFound)]
        //Master copy deal tests
        // -- Requesting public view
        [InlineData(false, true, true, true, "Deal.Admin", true, DealViewType.ByID)]
        [InlineData(false, true, true, true, "Deal.None", true, DealViewType.DealReadFalse)]
        [InlineData(true, true, true, true, "Deal.None", true, DealViewType.LatestPublished)]
        [InlineData(true, true, true, true, "Deal.Read", true, DealViewType.LatestPublished)]
        // -- Asking for private view
        [InlineData(false, true, true, false, "Deal.Admin", false, DealViewType.ByID)]
        [InlineData(false, true, true, false, "Deal.None", false, DealViewType.DealReadFalse)]
        public void GetDealViewModelForParticipant(
            bool HasBeenPublished, bool IsMasterDeal, bool IsLatestPublished, bool RequestingPublicCopy, string CurrentUserPermissions,
            bool ExpectedPublicView, DealViewType ExpectedViewType)
        {
            var sut = GetSut();
            var actual = sut.GetDealViewModelForParticipant(HasBeenPublished, IsMasterDeal, IsLatestPublished, RequestingPublicCopy, (new string[] { CurrentUserPermissions }).ToList());

            // Assert
            Assert.Equal(ExpectedPublicView, actual.Item1);
            Assert.Equal(ExpectedViewType, actual.Item2);
        }

        public DealViewUIProvider GetSut()
        {
            return new DealViewUIProvider(dealMock.Object, dealParticipantMock.Object, firmMock.Object);
        }

    }
}