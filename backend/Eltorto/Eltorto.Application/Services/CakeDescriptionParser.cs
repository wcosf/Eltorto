using System.Globalization;
using System.Text.RegularExpressions;

namespace Eltorto.Application.Services;

public static class CakeDescriptionParser
{
    private static readonly Regex MinWeightFractionRegex = new(
        @"(?:от\s*)?(\d{1,2})\s*[,.]\s*(\d)\s*кг",
        RegexOptions.IgnoreCase);

    private static readonly Regex MinWeightIntegerRegex = new(
        @"(?:от\s*)?(\d{1,2})\s*кг",
        RegexOptions.IgnoreCase);

    private static readonly Regex GramsRangeRegex = new(
        @"\d+\s*[-–]\s*\d+\s*(?:г|гр|г\.?)",
        RegexOptions.IgnoreCase);

    private static readonly Regex GramsRegex = new(
        @"(\d{1,3})\s*гр",
        RegexOptions.IgnoreCase);

    private static readonly Regex PriceRegex = new(
        @"(\d{2,7})\s*руб",
        RegexOptions.IgnoreCase);

    public static decimal? ParseMinWeightKg(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return null;

        var fractionMatch = MinWeightFractionRegex.Match(description);
        if (fractionMatch.Success)
        {
            var value = $"{fractionMatch.Groups[1].Value}.{fractionMatch.Groups[2].Value}";
            if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var fractionKg))
                return fractionKg;
        }

        var integerMatch = MinWeightIntegerRegex.Match(description);
        if (integerMatch.Success
            && decimal.TryParse(integerMatch.Groups[1].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var integerKg))
        {
            return integerKg;
        }

        if (GramsRangeRegex.IsMatch(description))
            return null;

        var gramsMatch = GramsRegex.Match(description);
        if (gramsMatch.Success
            && decimal.TryParse(gramsMatch.Groups[1].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var grams))
        {
            return decimal.Round(grams / 1000m, 2);
        }

        return null;
    }

    public static decimal? ParsePrice(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return null;

        var match = PriceRegex.Match(description);
        if (match.Success
            && decimal.TryParse(match.Groups[1].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var price))
        {
            return price;
        }

        return null;
    }
}
