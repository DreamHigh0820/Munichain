using System.ComponentModel.DataAnnotations;
using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;

namespace Shared.Validators
{
    public class DateTimeValidator : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            return IsValid(value != null ? value : null);
        }

        protected override ValidationResult IsValid(object value, ValidationContext context)
        {
            DateTime? temp = value as DateTime?;

            if (!temp.HasValue)
            {
                return ValidationResult.Success;
            }
            else
            {
                if (temp > new DateTime(1990, 1, 1) && temp < new DateTime(2100, 1, 1))
                {
                    return ValidationResult.Success;
                }
            }

            return new ValidationResult("Invalid", memberNames: new List<string> { context?.MemberName });
        }
    }
}
