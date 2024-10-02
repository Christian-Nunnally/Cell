using Cell.Common;
using Cell.Data;
using Cell.Model;
using Cell.Plugin.SyntaxWalkers;
using FontAwesome.Sharp;
using ICSharpCode.AvalonEdit.CodeCompletion;
using Microsoft.CodeAnalysis;
using System.Reflection;

namespace Cell.Core.Execution.CodeCompletion
{
    /// <summary>
    /// Factory for generating code completion suggestions.
    /// </summary>
    public static class CodeCompletionFactory
    {
        private readonly static Dictionary<Type, IList<ICompletionData>> _cachedCompletionData = [];
        private static readonly IList<ICompletionData> NoCompletionData = [new CodeCompletionData("", "No completion data found", "", IconChar.None)];
        private static List<ICompletionData>? _cachedGlobalCompletionData = null;
        /// <summary>
        /// Generates a list of completion suggestions given some code, the position of the carrot, the using statements, and any variables that are in a global scope.
        /// </summary>
        /// <param name="text">The code to generate suggestions from using semantic analysis.</param>
        /// <param name="carrotPosition">The position of the carrot.</param>
        /// <param name="usings">A list of namepsaces to include for type context.</param>
        /// <param name="variableNameToTypeMapForOuterContext">A dictionary of variable names and thier type to be considered as valid even if they are not declared in the given code.</param>
        /// <returns></returns>
        public static IList<ICompletionData> CreateCompletionData(string text, int carrotPosition, IEnumerable<string> usings, Dictionary<string, Type> variableNameToTypeMapForOuterContext)
        {
            if (TryGetTypeUsingSemanticAnalyzer(text, carrotPosition, usings, variableNameToTypeMapForOuterContext, out var type))
            {
                return CreateCompletionDataForType(type!);
            }

            return TryGetCompletionDataFromTheWordBeforeTheCursor(text, carrotPosition, variableNameToTypeMapForOuterContext, out var completionData)
                ? completionData!
                : NoCompletionData;
        }

        private static List<ICompletionData> CreateCompletionDataForGlobalContext(Dictionary<string, Type> variableNameToTypeMapForOuterContext)
        {
            if (_cachedGlobalCompletionData != null) return _cachedGlobalCompletionData;

            _cachedGlobalCompletionData = [.. variableNameToTypeMapForOuterContext.Select(CreateCompletionDataForKeyValuePair)];
            return _cachedGlobalCompletionData;
        }

        private static ICompletionData CreateCompletionDataForKeyValuePair(KeyValuePair<string, Type> nameTypePair)
        {
            var userVisibleString = nameTypePair.Key;
            var description = nameTypePair.Value.GetDocumentation();
            var type = nameTypePair.Value;
            var prettyTypeName = type.GetPrettyGenericTypeName();

            var icon = IconChar.Globe;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(UserList<>)) icon = IconChar.List;
            else if (type.IsEnum) icon = IconChar.ListAlt;
            else if (type.IsValueType) icon = IconChar.Cube;
            else if (type.IsInterface) icon = IconChar.ObjectGroup;
            else if (type.IsClass) icon = IconChar.ObjectGroup;
            else if (type.IsArray) icon = IconChar.ListAlt;

            var data = new CodeCompletionData(nameTypePair.Key, userVisibleString, prettyTypeName + description, icon);
            return data;
        }

        private static IList<ICompletionData> CreateCompletionDataForType(Type type)
        {
            if (_cachedCompletionData.TryGetValue(type, out var completionData)) return completionData;
            CodeDocumentationLoader.LoadXmlDocumentation(type.Assembly);

            var members = type.GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public).Where(m => (m is not MethodInfo mi || !mi.IsSpecialName) && m is not ConstructorInfo);
            var newCompletionData = members.Select(CreateCompletionDataFromMemberInfo).OfType<ICompletionData>().ToList();
            _cachedCompletionData.Add(type, newCompletionData);
            return newCompletionData;
        }

        private static ICompletionData CreateCompletionDataFromMemberInfo(MemberInfo info)
        {
            var name = info.Name;
            var userVisibleString = $"{name} : {info.GetUnderlyingType()?.Name ?? "Unable to get type name"}";
            var documentation = info.GetDocumentation();
            return new CodeCompletionData(name, userVisibleString, documentation, IconChar.Line);
        }

        private static Type? GetTypeFromSymbol(ITypeSymbol typeSymbol)
        {
            var symbolDisplayFormat = new SymbolDisplayFormat(typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);
            var fullQualifiedName = typeSymbol.ToDisplayString(symbolDisplayFormat);
            fullQualifiedName = fullQualifiedName + "," + typeSymbol.ContainingAssembly;

            return Type.GetType(fullQualifiedName);
        }

        private static Type? GetUnderlyingType(this MemberInfo member)
        {
            ArgumentNullException.ThrowIfNull(member);
            return member.MemberType switch
            {
                MemberTypes.Event => ((EventInfo)member).EventHandlerType,
                MemberTypes.Field => ((FieldInfo)member).FieldType,
                MemberTypes.Method => ((MethodInfo)member).ReturnType,
                MemberTypes.Property => ((PropertyInfo)member).PropertyType,
                MemberTypes.Constructor => ((ConstructorInfo)member).DeclaringType,
                _ => throw new ArgumentException("Input MemberInfo must be if type EventInfo, FieldInfo, MethodInfo, or PropertyInfo"),
            };
        }

        private static string GetVariableNamePriorToCarot(string text, int carrotPosition)
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

        private static bool TryGetCompletionDataFromTheWordBeforeTheCursor(string text, int carrotPosition, Dictionary<string, Type> variableNameToTypeMapForOuterContext, out IList<ICompletionData>? completionData)
        {
            completionData = null;
            var typeNameInFrontOfCarrot = GetVariableNamePriorToCarot(text, carrotPosition);
            if (typeNameInFrontOfCarrot == string.Empty)
            {
                completionData = CreateCompletionDataForGlobalContext(variableNameToTypeMapForOuterContext);
            }
            else if (CellReferenceToCodeSyntaxRewriter.IsCellLocation(typeNameInFrontOfCarrot))
            {
                completionData = CreateCompletionDataForType(typeof(CellModel));
            }
            return completionData is not null;
        }

        private static bool TryGetTypeUsingSemanticAnalyzer(string text, int carrotPosition, IEnumerable<string> usings, Dictionary<string, Type> variableNameToTypeMapForOuterContext, out Type? type)
        {
            type = null;
            SemanticAnalyzer semanticAnalyzer = new(text, usings, variableNameToTypeMapForOuterContext);
            var typeSymbol = semanticAnalyzer.GetTypeAtPosition(carrotPosition);
            if (typeSymbol is null) return false;
            type = TypeSymbolConverter.GetTypeFromSymbol(typeSymbol);
            return type is not null;
        }
    }
}
