using Shared.Models.DealComponents;
using Shared.Models.Users;
using Shared.Validators;
using System.Collections.Generic;
using Xunit;

namespace Tests.Tests.Validators
{
    public class DocumentPermissionValidatorTests
    {

        [Fact]
        public void Validate_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            Document doc = null;
            List<DealParticipant> dealParticipants = null;
            User user = null;

            // Act
            var result = DocumentPermissionValidator.Validate(
                doc,
                dealParticipants,
                user,
                false,
                false);

            // Assert
            Assert.True(result);
        }
    }
}
