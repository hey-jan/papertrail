using System.Globalization;

namespace PaperTrail.Extensions
{
    public static class CurrencyExtensions
    {
        private static readonly CultureInfo PhilippineCulture = new("en-PH");

        public static string ToPeso(this decimal amount)
        {
            return string.Format(PhilippineCulture, "{0:C}", amount);
        }

        public static string ToPeso(this decimal? amount)
        {
            return (amount ?? 0m).ToPeso();
        }
    }
}
