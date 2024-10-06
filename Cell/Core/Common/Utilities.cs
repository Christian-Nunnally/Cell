using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;

namespace Cell.Common
{
    /// <summary>
    /// Random utilities that are used throughout the project.
    /// </summary>
    public static partial class Utilities
    {
        /// <summary>
        /// Copies all public properties from the source object to the target object. Excludes properties in the blacklist.
        /// </summary>
        /// <param name="source">The object to copy from.</param>
        /// <param name="target">The object to copy to.</param>
        /// <param name="blacklist">The list of property names that should not be updated.</param>
        /// <exception cref="CellError"></exception>
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

        /// <summary>
        /// Generates a random string of the given length.
        /// </summary>
        /// <param name="length">The length of string to generate.</param>
        /// <returns>The random string of the given length.</returns>
        public static string GenerateUnqiueId(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[length];
            var random = new Random();
            for (int i = 0; i < length; i++)
            {
                var randomNumberInRange = random.Next(chars.Length);
                stringChars[i] = chars[randomNumberInRange];
            }
            return new string(stringChars);
        }

        /// <summary>
        /// Returns the smallest possible rectangle that includes all points and is parellel to the xy plane.
        /// </summary>
        /// <param name="points">The list of points to inlcude.</param>
        /// <returns>A rectangle that includes all of the points.</returns>
        public static Rect GetBoundingRectangle(List<Point> points)
        {
            var minX = points.Min(p => p.X);
            var minY = points.Min(p => p.Y);
            var maxX = points.Max(p => p.X);
            var maxY = points.Max(p => p.Y);
            var topLeft = new Point(minX, minY);
            var bottomRight = new Point(maxX, maxY);
            return new Rect(topLeft, bottomRight);
        }

        /// <summary>
        /// Generates a hash from the given string.
        /// </summary>
        /// <param name="text">The string to get the hash from.</param>
        /// <returns></returns>
        public static ulong GetHashFromString(this string text)
        {
            var hashedValue = 3074457345618258791ul;
            for (int i = 0; i < text.Length; i++)
            {
                hashedValue += text[i];
                hashedValue *= 3074457345618258799ul;
            }
            return hashedValue;
        }

        /// <summary>
        /// Gets the location parts from a unqiue location string.
        /// </summary>
        /// <param name="unqiueLocationString">The location string.</param>
        /// <returns>The parts of the location broken apart.</returns>
        public static (string SheetName, int Row, int Column) GetLocationFromUnqiueLocationString(string unqiueLocationString)
        {
            var splitString = unqiueLocationString.Split('_');
            return (splitString[0], int.Parse(splitString[1]), int.Parse(splitString[2]));
        }

        /// <summary>
        /// Gets the pretty full name of the type, including the generic arguments and the fulle name references.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The string name of the type.</returns>
        public static string GetPrettyFullGenericTypeName(this Type type)
        {
            if (!type.IsGenericType) return type?.FullName ?? "";

            var genericTypeDefinition = type.GetGenericTypeDefinition();
            var genericArguments = type.GetGenericArguments();

            var typeName = genericTypeDefinition.FullName?[..genericTypeDefinition.FullName.IndexOf('`')] ?? string.Empty;
            var args = string.Join(", ", Array.ConvertAll(genericArguments, arg => arg.FullName));

            return $"{typeName}<{args}>";
        }

        /// <summary>
        /// Gets the pretty name of the type, including the generic arguments.
        /// </summary>
        /// <param name="type"></param>
        /// <returns>The string name of the type.</returns>
        public static string GetPrettyGenericTypeName(this Type type)
        {
            if (!type.IsGenericType) return type?.FullName ?? "";

            var genericTypeDefinition = type.GetGenericTypeDefinition();
            var genericArguments = type.GetGenericArguments();

            var typeName = genericTypeDefinition.Name[..genericTypeDefinition.Name.IndexOf('`')];
            var args = string.Join(", ", Array.ConvertAll(genericArguments, arg => arg.Name));

            return $"{typeName}<{args}>";
        }

        /// <summary>
        /// Gets all the types in the given namespace from the given assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="nameSpace">The fully qualified name of the namespace.</param>
        /// <returns></returns>
        public static Type[] GetTypesInNamespace(Assembly assembly, string nameSpace)
        {
            return
              assembly.GetTypes()
                      .Where(t => string.Equals(t.Namespace, nameSpace, StringComparison.Ordinal))
                      .ToArray();
        }

        //public static string GetUnqiueLocationString(string sheet, int row, int column) => $"{sheet}_{row}_{column}";

        //public static string GetUnqiueLocationString(this CellModel model) => GetUnqiueLocationString(model.Location.SheetName, model.Location.Row, model.Location.Column);

        [GeneratedRegex(@"[#][0-9A-Fa-f]{6}\b")]
        public static partial Regex IsHexidecimalColorCode();

        /// <summary>
        /// Takes a string like "2,3,4,5" or "3,1" or "1" and returns a thickness object.
        /// </summary>
        /// <param name="stringThickness">The string thickness.</param>
        /// <returns>A thickness object.</returns>
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

        /// <summary>
        /// Tries to parse a string like "2,3,4,5" or "3,1" or "1" into a thickness object.
        /// </summary>
        /// <param name="stringThickness">The string thickness.</param>
        /// <param name="thickness">The resulting thickness object.</param>
        /// <returns>True if the string was a valid thickness object.</returns>
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
