using Moq;
using Shared.Validators;
using Xunit;

namespace Tests.Tests.Validators
{
    public class IssuerValidatorTests
    {
        private MockRepository mockRepository;



        public IssuerValidatorTests()
        {
            mockRepository = new MockRepository(MockBehavior.Strict);


        }

        private IssuerUrlValidator CreateIssuerValidator()
        {
            return new IssuerUrlValidator();
        }

        [Fact]
        public void IsValid_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var issuerValidator = CreateIssuerValidator();
            object? value = null;

            // Act
            var result = issuerValidator.IsValid(
                value);

            // Assert
            Assert.True(false);
            mockRepository.VerifyAll();
        }
    }
}
