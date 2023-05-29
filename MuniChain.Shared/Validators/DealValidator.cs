using Shared.Models.DealComponents;

namespace Shared.Validators
{
    public static class DealValidator
    {
        public static List<string> Validate(this DealModel deal)
        {
            List<string> errors = new List<string>();

            // All series size must match deal size
            if (!SeriesSizeMatch(deal.Series, deal.Size))
                errors.Add("Series size must match the total deal size");
            // All maturities in series must match the series size
            if (!MaturitySizeMatch(deal.Series))
                errors.Add("Maturities par size must match the total size of the respective Series");

            return errors;
        }

        private static bool SeriesSizeMatch(List<Series> series, decimal? dealSize)
        {
            // If no series, dont check size
            if (series == null || !series.Any())
                return true;

            decimal? seriesSum = series.Sum(x => x.Size);
            return seriesSum == dealSize;
        }

        private static bool MaturitySizeMatch(List<Series> series)
        {
            // If no series, dont check maturities
            if (series == null || !series.Any())
                return true;

            foreach (var seriesItem in series)
            {
                // Has to have maturities to perform check
                if (seriesItem.Maturities != null && seriesItem.Maturities.Any())
                {
                    decimal? maturitySum = seriesItem.Maturities.Sum(x => x.Par);
                    if (maturitySum != seriesItem.Size)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}