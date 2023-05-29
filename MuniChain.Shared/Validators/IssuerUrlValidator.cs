using System.ComponentModel.DataAnnotations;

namespace Shared.Validators
{
    public sealed class IssuerUrlValidator : DataTypeAttribute
    {
        public IssuerUrlValidator()
            : base(DataType.Url)
        {
        }

        public override bool IsValid(object? value)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
            {
                return true;
            }

            return value is string valueAsString &&
                (valueAsString.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                || valueAsString.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                || valueAsString.StartsWith("www.", StringComparison.OrdinalIgnoreCase)
                || valueAsString.StartsWith("ftp://", StringComparison.OrdinalIgnoreCase));
        }
    }
}
