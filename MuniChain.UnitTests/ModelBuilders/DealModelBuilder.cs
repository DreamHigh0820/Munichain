using Shared.Models.DealComponents;
using Shared.Models.Enums;
using System;

namespace Tests.ModelBuilders
{
    public class DealModelBuilder
    {
        private DealModel deal = new();

        public DealModelBuilder()
        {
        }

        public DealModelBuilder GetDefaultDeal()
        {
            Performance perf = new Performance()
            {
                Id = Guid.NewGuid().ToString(),
                RowVersion = GetByteArray(25)
            };

            deal = new DealModel()
            {
                Id = Guid.NewGuid().ToString(),
                Issuer = "Test Issuer",
                Size = 10000,
                State = States.LA,
                LastModifiedBy = "Garrett",
                LastModifiedByDisplayName = "Garrett",
                RowVersion = GetByteArray(25),
                CreatedDateUTC = DateTime.Now,
                Performance = perf
            };

            return this;
        }

        public DealModelBuilder Status(string status)
        {
            deal.Status = status;
            return this;
        }
        public DealModelBuilder HistoryDealId(string id)
        {
            deal.HistoryDealID = id;
            return this;
        }

        public DealModel Build()
        {
            return deal;
        }

        private byte[] GetByteArray(int sizeInKb)
        {
            Random rnd = new Random();
            byte[] b = new byte[sizeInKb * 1024]; // convert kb to byte
            rnd.NextBytes(b);
            return b;
        }
    }
}
