using Cell.Model.Plugin;
using ICSharpCode.AvalonEdit.CodeCompletion;
using System.Reflection;

namespace Cell.Core.Execution.CodeCompletion
{
    public static class CodeCompletionFactory
    {
        private readonly static Dictionary<string, string> _typeToFullyQualifiedTypeNameMap = [];
        private readonly static Dictionary<Type, IList<ICompletionData>> _cachedCompletionData = [];
        private static List<ICompletionData>? _cachedGlobalCompletionData = null;
        private static bool _haveAssembliesBeenRegistered = false;

        private static void RegisterTypesInAssembly()
        {
            if (_haveAssembliesBeenRegistered) return;
            RegisterTypesInAssembly(typeof(TodoItem).Assembly);
            _haveAssembliesBeenRegistered = true;
        }

        public static IList<ICompletionData> CreateCompletionData(string text, int carrotPosition, Dictionary<string, Type> variableNameToTypeMapForOuterContext)
        {
            RegisterTypesInAssembly();
            var typeNameInFrontOfCarrot = GetVariableTypePriorToCarot(text, carrotPosition);
            if (variableNameToTypeMapForOuterContext.TryGetValue(typeNameInFrontOfCarrot, out var type))
            {
                return CreateCompletionDataForType(type);
            }
            if (typeNameInFrontOfCarrot == string.Empty)
            {
                return CreateCompletionDataForGlobalContext(variableNameToTypeMapForOuterContext);
            }
            return [];
        }

        private static IList<ICompletionData> CreateCompletionDataForGlobalContext(Dictionary<string, Type> variableNameToTypeMapForOuterContext)
        {
            if (_cachedGlobalCompletionData != null) return _cachedGlobalCompletionData;

            _cachedGlobalCompletionData = [.. variableNameToTypeMapForOuterContext.Select(CreateCompletionDataForKeyValuePair)];
            return _cachedGlobalCompletionData;
        }

        private static ICompletionData CreateCompletionDataForKeyValuePair(KeyValuePair<string, Type> nameTypePair)
        {
            var userVisibleString = $"{nameTypePair.Key} : {nameTypePair.Value.Name}";
            var description = nameTypePair.Value.GetDocumentation();
            var data = new CodeCompletionData(nameTypePair.Key, userVisibleString, description);
            return data;
        }

        private static IList<ICompletionData> CreateCompletionDataForType(Type type)
        {
            if (_cachedCompletionData.TryGetValue(type, out var completionData)) return completionData;
            CodeDocumentationLoader.LoadXmlDocumentation(type.Assembly);

            var members = type.GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public).Where(m => m is not MethodInfo mi || !mi.IsSpecialName && m is not ConstructorInfo);
            var newCompletionData = members.Select(CreateCompletionDataFromMemberInfo).OfType<ICompletionData>().ToList();
            _cachedCompletionData.Add(type, newCompletionData);
            return newCompletionData;
        }

        private static ICompletionData CreateCompletionDataFromMemberInfo(MemberInfo info)
        {
            var userVisibleString = $"{info.Name} : {info.GetUnderlyingType().Name}";
            return new CodeCompletionData(info.Name, userVisibleString, info.GetDocumentation());
        }

        private static Type GetUnderlyingType(this MemberInfo member)
        {
            ArgumentNullException.ThrowIfNull(member);
            switch (member.MemberType)
            {
                case MemberTypes.Event:
                    return ((EventInfo)member).EventHandlerType;
                case MemberTypes.Field:
                    return ((FieldInfo)member).FieldType;
                case MemberTypes.Method:
                    return ((MethodInfo)member).ReturnType;
                case MemberTypes.Property:
                    return ((PropertyInfo)member).PropertyType;
                case MemberTypes.Constructor:
                    return ((ConstructorInfo)member).DeclaringType;
                default:
                    throw new ArgumentException
                    (
                     "Input MemberInfo must be if type EventInfo, FieldInfo, MethodInfo, or PropertyInfo"
                    );
            }
        }

        private static void RegisterTypesInAssembly(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsPublic)
                {
                    if (_typeToFullyQualifiedTypeNameMap.ContainsKey(type.Name))
                    {
                        throw new InvalidOperationException($"Type name {type.Name} is already registered. Semmantic analysis doesn't currently know the fqn of the type, so type names must be globally unique.");
                    }
                    if (type.FullName is not null) _typeToFullyQualifiedTypeNameMap[type.Name] = type.FullName;
                }
            }
        }

        private static string GetVariableTypePriorToCarot(string text, int carrotPosition)
        {
            if (text.Length > carrotPosition || text.Length == 0) return "";
            var offset = carrotPosition - 1;
            if (text[offset] == '.')
            {
                while (offset > 0 && char.IsLetterOrDigit(text[offset - 1])) offset--;
                return text[offset..(carrotPosition - 1)];
            }
            return "";
        }
    }
}
