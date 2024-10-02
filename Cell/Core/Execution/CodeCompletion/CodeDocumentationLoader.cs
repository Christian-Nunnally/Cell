using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;

namespace Cell.Core.Execution.CodeCompletion
{
    /// <summary>
    /// Loads code documentation from the xml documentation file generated during build and stores it in a dictionary for quick lookup.
    /// </summary>
    public static partial class CodeDocumentationLoader
    {
        private static readonly HashSet<Assembly> _loadedAssemblies = [];
        private static readonly Dictionary<string, string> _loadedXmlDocumentation = [];
        /// <summary>
        /// Gets the documentation for the specified type from the documentation xml file created during build.
        /// </summary>
        /// <param name="constructorInfo">The item to get documentation for.</param>
        /// <returns>The documentation for the item, in document comment format.</returns>
        public static string GetDocumentation(this ConstructorInfo constructorInfo)
        {
            return $"Documentation not yet working for ConstructorInfo {constructorInfo.Name}";
        }

        /// <summary>
        /// Gets the documentation for the specified type from the documentation xml file created during build.
        /// </summary>
        /// <param name="eventInfo">The item to get documentation for.</param>
        /// <returns>The documentation for the item, in document comment format.</returns>
        public static string GetDocumentation(this EventInfo eventInfo)
        {
            string key = "E:" + XmlDocumentationKeyHelper(eventInfo.DeclaringType?.FullName ?? string.Empty, eventInfo.Name);
            if (_loadedXmlDocumentation.TryGetValue(key, out var documentation)) return documentation;
            return string.Empty;
        }

        /// <summary>
        /// Gets the documentation for the specified type from the documentation xml file created during build.
        /// </summary>
        /// <param name="fieldInfo">The item to get documentation for.</param>
        /// <returns>The documentation for the item, in document comment format.</returns>
        public static string GetDocumentation(this FieldInfo fieldInfo)
        {
            string key = "F:" + XmlDocumentationKeyHelper(fieldInfo.DeclaringType?.FullName ?? string.Empty, fieldInfo.Name);
            if (_loadedXmlDocumentation.TryGetValue(key, out var documentation)) return documentation;
            return string.Empty;
        }

        /// <summary>
        /// Gets the documentation for the specified type from the documentation xml file created during build.
        /// </summary>
        /// <param name="memberInfo">The item to get documentation for.</param>
        /// <returns>The documentation for the item, in document comment format.</returns>
        public static string GetDocumentation(this MemberInfo memberInfo)
        {
            if (memberInfo.MemberType.HasFlag(MemberTypes.Field))
            {
                return ((FieldInfo)memberInfo).GetDocumentation();
            }
            else if (memberInfo.MemberType.HasFlag(MemberTypes.Property))
            {
                return ((PropertyInfo)memberInfo).GetDocumentation();
            }
            else if (memberInfo.MemberType.HasFlag(MemberTypes.Event))
            {
                return ((EventInfo)memberInfo).GetDocumentation();
            }
            else if (memberInfo.MemberType.HasFlag(MemberTypes.Constructor))
            {
                return ((ConstructorInfo)memberInfo).GetDocumentation();
            }
            else if (memberInfo.MemberType.HasFlag(MemberTypes.Method))
            {
                return ((MethodInfo)memberInfo).GetDocumentation();
            }
            else if (memberInfo.MemberType.HasFlag(MemberTypes.TypeInfo) || memberInfo.MemberType.HasFlag(MemberTypes.NestedType))
            {
                return ((TypeInfo)memberInfo).GetDocumentation();
            }
            return string.Empty;
        }

        /// <summary>
        /// Gets the documentation for the specified type from the documentation xml file created during build.
        /// </summary>
        /// <param name="methodInfo">The item to get documentation for.</param>
        /// <returns>The documentation for the item, in document comment format.</returns>
        public static string GetDocumentation(this MethodInfo methodInfo)
        {
            return $"Documentation not yet working for MethodInfo {methodInfo.Name}";
        }

        /// <summary>
        /// Gets the documentation for the specified type from the documentation xml file created during build.
        /// </summary>
        /// <param name="parameterInfo">The item to get documentation for.</param>
        /// <returns>The documentation for the item, in document comment format.</returns>
        public static string GetDocumentation(this ParameterInfo parameterInfo)
        {
            string memberDocumentation = parameterInfo.Member.GetDocumentation();
            if (memberDocumentation != null)
            {
                string regexPattern =
                  Regex.Escape(@"<param name=" + "\"" + parameterInfo.Name + "\"" + @">") +
                  ".*?" +
                  Regex.Escape(@"</param>");
                Match match = Regex.Match(memberDocumentation, regexPattern);
                if (match.Success)
                {
                    return match.Value;
                }
            }
            return $"No documentation found for {parameterInfo.Name}";
        }

        /// <summary>
        /// Gets the documentation for the specified type from the documentation xml file created during build.
        /// </summary>
        /// <param name="propertyInfo">The item to get documentation for.</param>
        /// <returns>The documentation for the item, in document comment format.</returns>
        public static string GetDocumentation(this PropertyInfo propertyInfo)
        {
            string key = "P:" + XmlDocumentationKeyHelper(propertyInfo.DeclaringType?.FullName ?? string.Empty, propertyInfo.Name);
            if (_loadedXmlDocumentation.TryGetValue(key, out var documentation)) return documentation;
            return string.Empty;
        }

        /// <summary>
        /// Gets the documentation for the specified type from the documentation xml file created during build.
        /// </summary>
        /// <param name="type">The item to get documentation for.</param>
        /// <returns>The documentation for the item, in document comment format.</returns>
        public static string GetDocumentation(this Type type)
        {
            LoadXmlDocumentation(type.Assembly);
            string key = "T:" + XmlDocumentationKeyHelper(type.FullName ?? string.Empty, null);
            if (_loadedXmlDocumentation.TryGetValue(key, out var documentation)) return documentation;
            return string.Empty;
        }

        /// <summary>
        /// Loads the xml documentation generated by the build.
        /// </summary>
        /// <param name="assembly">The assembly to determine the documentation file name from.</param>
        public static void LoadXmlDocumentation(Assembly assembly)
        {
            if (_loadedAssemblies.Contains(assembly)) return;
            string directoryPath = assembly.GetDirectoryPath();
            string xmlFilePath = Path.Combine(directoryPath, assembly.GetName().Name + ".xml");
            if (File.Exists(xmlFilePath))
            {
                LoadXmlDocumentation(File.ReadAllText(xmlFilePath));
                _loadedAssemblies.Add(assembly);
            }
        }

        private static string GetDirectoryPath(this Assembly assembly)
        {
            var codeBase = assembly.Location;
            var uri = new UriBuilder(codeBase);
            var path = Uri.UnescapeDataString(uri.Path);
            var directory = Path.GetDirectoryName(path);
            return directory ?? throw new NullReferenceException(nameof(directory));
        }

        [GeneratedRegex(@"\[.*\]")]
        private static partial Regex GetTextBetweenSquareBrackets();

        private static void LoadXmlDocumentation(string xmlDocumentation)
        {
            using XmlReader xmlReader = XmlReader.Create(new StringReader(xmlDocumentation));
            while (xmlReader.Read())
            {
                if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "member")
                {
                    var rawName = xmlReader["name"];
                    if (rawName == null) continue;
                    _loadedXmlDocumentation[rawName] = xmlReader.ReadInnerXml();
                }
            }
        }

        private static string XmlDocumentationKeyHelper(string typeFullNameString, string? memberNameString)
        {
            string key = GetTextBetweenSquareBrackets().Replace(typeFullNameString, string.Empty).Replace('+', '.');
            if (memberNameString != null)
            {
                key += "." + memberNameString;
            }
            return key;
        }
    }
}
