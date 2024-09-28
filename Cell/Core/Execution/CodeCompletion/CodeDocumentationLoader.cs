using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;

namespace Cell.Core.Execution.CodeCompletion
{
    public static class CodeDocumentationLoader
    {
        private static Dictionary<string, string> _loadedXmlDocumentation = [];
        private static bool _isXmlFileLoaded = false;

        public static void LoadXmlDocumentation(string xmlDocumentation)
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

        public static string GetDirectoryPath(this Assembly assembly)
        {
            var codeBase = assembly.Location;
            var uri = new UriBuilder(codeBase);
            var path = Uri.UnescapeDataString(uri.Path);
            var directory = Path.GetDirectoryName(path);
            return directory ?? throw new NullReferenceException(nameof(directory));
        }

        internal static HashSet<Assembly> loadedAssemblies = new HashSet<Assembly>();

        public static void LoadXmlDocumentation(Assembly assembly)
        {
            if (loadedAssemblies.Contains(assembly))
            {
                return; // Already loaded
            }
            string directoryPath = assembly.GetDirectoryPath();
            string xmlFilePath = Path.Combine(directoryPath, assembly.GetName().Name + ".xml");
            if (File.Exists(xmlFilePath))
            {
                LoadXmlDocumentation(File.ReadAllText(xmlFilePath));
                loadedAssemblies.Add(assembly);
            }
        }

        private static string XmlDocumentationKeyHelper(string typeFullNameString, string memberNameString)
        {
            string key = Regex.Replace(typeFullNameString, @"\[.*\]", string.Empty).Replace('+', '.');
            if (memberNameString != null)
            {
                key += "." + memberNameString;
            }
            return key;
        }

        public static string GetDocumentation(this Type type)
        {
            LoadXmlDocumentation(type.Assembly);
            string key = "T:" + XmlDocumentationKeyHelper(type.FullName, null);
            if (_loadedXmlDocumentation.TryGetValue(key, out var documentation)) return documentation;
            return string.Empty;
        }

        public static string GetDocumentation(this PropertyInfo propertyInfo)
        {
            string key = "P:" + XmlDocumentationKeyHelper(propertyInfo.DeclaringType.FullName, propertyInfo.Name);
            if (_loadedXmlDocumentation.TryGetValue(key, out var documentation)) return documentation;
            return string.Empty;
        }

        public static string GetDocumentation(this FieldInfo fieldInfo)
        {
            string key = "F:" + XmlDocumentationKeyHelper(fieldInfo.DeclaringType.FullName, fieldInfo.Name);
            if (_loadedXmlDocumentation.TryGetValue(key, out var documentation)) return documentation;
            return string.Empty;
        }

        public static string GetDocumentation(this EventInfo eventInfo)
        {
            string key = "E:" + XmlDocumentationKeyHelper(eventInfo.DeclaringType.FullName, eventInfo.Name);
            if (_loadedXmlDocumentation.TryGetValue(key, out var documentation)) return documentation;
            return string.Empty;
        }

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
            return null;
        }

        public static string GetDocumentation(this ConstructorInfo constructorInfo)
        {
            return "Documentation not yet working for ConstructorInfo";
        }

        public static string GetDocumentation(this MethodInfo methodInfo)
        {
            return "Documentation not yet working for MethodInfo";
        }

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
    }
}
