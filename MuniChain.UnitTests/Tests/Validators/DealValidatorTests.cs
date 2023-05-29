using Shared.Models.DealComponents;
using Shared.Validators;
using Xunit;

namespace Tests.Tests.Validators
{
    public class DealValidatorTests
    {
        [Fact]
        public void ValidateDealReturnsMessages()
        {
            // Arrange
            DealModel deal = new DealModel();
            var dealValidator = deal.Validate();

            // Act
            var result = deal.Validate();

            // Assert
            Assert.Contains("Test", result);
        }
    }
}
