using Domain.Services.Database;
using Moq;
using Tests.ModelBuilders;
using System.Threading.Tasks;
using Tests.TestStartup;
using Xunit;

namespace Tests.Tests.Services.DataService
{
    public class DealServiceTests : BaseTestStartup
    {
        public Mock<IDealParticipantService> dealParticipantMock;

        public DealServiceTests()
        {
            dealParticipantMock = new Mock<IDealParticipantService>();
        }

        [Theory]
        [InlineData("Draft", "Published", true)]
        [InlineData("Draft", "Draft", false)]
        [InlineData("Published", "Published", true)]
        [InlineData("Published", "Draft", true)]
        public async Task HasBeenPublished(string status1, string status2, bool expected)
        {
            var deal1 = new DealModelBuilder().GetDefaultDeal().Status("Draft").Build();
            var deal2 = new DealModelBuilder().GetDefaultDeal().Status(status1).HistoryDealId(deal1.Id).Build();
            var deal3 = new DealModelBuilder().GetDefaultDeal().Status(status2).HistoryDealId(deal1.Id).Build();

            using (var db = factory.CreateDbContext())
            {
                db.Deals.AddRange(deal1, deal2, deal3);
                db.SaveChanges();
            }

            var sut = GetSut();

            using (var db = factory.CreateDbContext())
            {
                var actual = await sut.HasBeenPublished(deal1.Id, deal1.HistoryDealID);
                Assert.Equal(expected, actual);
            }
        }

        public DealService GetSut()
        {
            return new DealService(factory, dealParticipantMock.Object);
        }
    }
}