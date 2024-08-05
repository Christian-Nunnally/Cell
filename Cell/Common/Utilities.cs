using Cell.Model;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using System.Windows;

namespace Cell.Common
{
    public static partial class Utilities
    {
        [GeneratedRegex(@"[#][0-9A-Fa-f]{6}\b")]
        public static partial Regex IsHexidecimalColorCode();

        public static string GenerateUnqiueId(int length)
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[length];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                var randomNumberInRange = random.Next(chars.Length);
                stringChars[i] = chars[randomNumberInRange];
            }

            return new string(stringChars);
        }

        public static string GetUnqiueLocationString(string sheet, int row, int column) => $"{sheet}_{row}_{column}";

        public static string GetUnqiueLocationString(this CellModel model) => GetUnqiueLocationString(model.SheetName, model.Row, model.Column);

        public static bool TryParseStringIntoThickness(string stringThickness, [MaybeNullWhen(false)] out Thickness thickness)
        {
            var split = stringThickness.Split(',');
            if (split.Length == 1)
            {
                if (!double.TryParse(split[0], out var size)) return false;
                thickness = new Thickness(size, size, size, size);
                return true;
            }
            else if (split.Length == 2)
            {
                if (!double.TryParse(split[0], out var vertical)) return false;
                if (!double.TryParse(split[1], out var horizontial)) return false;
                thickness = new Thickness(vertical, horizontial, vertical, horizontial);
                return true;
            }
            else if (split.Length == 4)
            {
                if (!double.TryParse(split[0], out var left)) return false;
                if (!double.TryParse(split[1], out var top)) return false;
                if (!double.TryParse(split[2], out var right)) return false;
                if (!double.TryParse(split[3], out var bottom)) return false;
                thickness = new Thickness(left, top, right, bottom);
                return true;
            }
            return false;
        }

        public static ulong GetHashFromString(this string read)
        {
            var hashedValue = 3074457345618258791ul;
            for (int i = 0; i < read.Length; i++)
            {
                hashedValue += read[i];
                hashedValue *= 3074457345618258799ul;
            }
            return hashedValue;
        }
    }
}
