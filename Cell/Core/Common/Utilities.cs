using Cell.Model;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;

namespace Cell.Common
{
    public static partial class Utilities
    {
        public static void CopyPublicProperties(this object source, object target, string[] blacklist)
        {
            var targetType = target?.GetType() ?? throw new CellError("source objects is null");
            var sourceType = source?.GetType() ?? throw new CellError("source objects is null");
            var sourceProperties = sourceType.GetProperties();

            foreach (PropertyInfo sourceProperty in sourceProperties)
            {
                CopyProperty(source, target, blacklist, targetType, sourceProperty);
            }
        }

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

        public static (string SheetName, int Row, int Column) GetLocationFromUnqiueLocationString(string unqiueLocationString)
        {
            var splitString = unqiueLocationString.Split('_');
            return (splitString[0], int.Parse(splitString[1]), int.Parse(splitString[2]));
        }

        public static Type[] GetTypesInNamespace(Assembly assembly, string nameSpace)
        {
            return
              assembly.GetTypes()
                      .Where(t => string.Equals(t.Namespace, nameSpace, StringComparison.Ordinal))
                      .ToArray();
        }

        public static string GetUnqiueLocationString(string sheet, int row, int column) => $"{sheet}_{row}_{column}";

        public static string GetUnqiueLocationString(this CellModel model) => GetUnqiueLocationString(model.SheetName, model.Row, model.Column);

        [GeneratedRegex(@"[#][0-9A-Fa-f]{6}\b")]
        public static partial Regex IsHexidecimalColorCode();

        public static Thickness ParseStringIntoThickness(string stringThickness)
        {
            var split = stringThickness.Split(',');
            if (split.Length == 1)
            {
                var size = double.Parse(split[0]);
                return new Thickness(size, size, size, size);
            }
            else if (split.Length == 2)
            {
                var horizontial = double.Parse(split[0]);
                var vertical = double.Parse(split[1]);
                return new Thickness(vertical, horizontial, vertical, horizontial);
            }
            var left = double.Parse(split[0]);
            var top = double.Parse(split[1]);
            var right = double.Parse(split[2]);
            var bottom = double.Parse(split[3]);
            return new Thickness(left, top, right, bottom);
        }

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

        private static void CopyProperty(object source, object target, string[] blacklist, Type targetType, PropertyInfo sourceProperty)
        {
            // Can read source property
            if (!sourceProperty.CanRead) return;

            // Target property exists
            var targetProperty = targetType.GetProperty(sourceProperty.Name);
            if (targetProperty == null) return;

            // Property is not blacklisted
            if (blacklist.Contains(sourceProperty.Name)) return;

            // Can write target property
            if (!targetProperty.CanWrite) return;
            var nonPrivateSetMethod = targetProperty.GetSetMethod(true);
            if (nonPrivateSetMethod != null && nonPrivateSetMethod.IsPrivate) return;
            var setMethod = targetProperty.GetSetMethod();
            if (setMethod == null) return;
            if ((setMethod.Attributes & MethodAttributes.Static) != 0) return;

            // Target property type is assignable from source property type
            if (!targetProperty.PropertyType.IsAssignableFrom(sourceProperty.PropertyType)) return;

            targetProperty.SetValue(target, sourceProperty.GetValue(source, null), null);
        }
    }
}
