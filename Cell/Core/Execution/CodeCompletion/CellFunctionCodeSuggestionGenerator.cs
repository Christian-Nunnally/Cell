using Cell.Core.Common;
using Cell.Core.Data;
using Cell.Core.Execution.Functions;
using Cell.Core.Execution.SyntaxWalkers.CellReferences;
using Cell.Model;
using Cell.Model.Plugin;
using Cell.Plugin.SyntaxWalkers;
using FontAwesome.Sharp;
using ICSharpCode.AvalonEdit.CodeCompletion;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;

namespace Cell.Core.Execution.CodeCompletion
{
    /// <summary>
    /// Class to generate code completion suggestions for the code editor.
    /// </summary>
    public class CellFunctionCodeSuggestionGenerator
    {
        private readonly static Dictionary<Type, IList<ICompletionData>> _cachedCompletionData = [];
        private static readonly IList<ICompletionData> NoCompletionData = [new CodeCompletionData("", "No completion data found", "", IconChar.None)];
        private static List<ICompletionData>? _cachedGlobalCompletionData = null;
        private readonly CellModel? _cellContext;
        private readonly SemanticAnalyzer _semanticAnalyzer;
        private string _currentUserCodeText = string.Empty;
        private Dictionary<string, Type> _computedOuterContextVariables = [];

        /// <summary>
        /// Creates a new instance of <see cref="CellFunctionCodeSuggestionGenerator"/>.
        /// </summary>
        /// <param name="usings">A list of namepsaces to include for type context.</param>
        /// <param name="cellContext">The context cell to use when resolving location references.</param>
        public CellFunctionCodeSuggestionGenerator(IEnumerable<string> usings, CellModel? cellContext)
        {
            _semanticAnalyzer = new SemanticAnalyzer(usings);
            _cellContext = cellContext;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <param name="variableNameToTypeMapForOuterContext">A dictionary of variable names and thier type to be considered as valid even if they are not declared in the given code.</param>
        public void UpdateCode(string code, IReadOnlyDictionary<string, string> variableNameToTypeMapForOuterContext)
        {
            _currentUserCodeText = code;
            _computedOuterContextVariables = CreateOuterContextVariablesForFunction(code, variableNameToTypeMapForOuterContext);
            _semanticAnalyzer.UpdateCode(code, _computedOuterContextVariables);
        }

        /// <summary>
        /// Generates a list of completion suggestions given some code, the position of the carrot, the using statements, and any variables that are in a global scope.
        /// </summary>
        /// <param name="carrotPosition">The position of the carrot.</param>
        /// <returns></returns>
        public IList<ICompletionData> CreateCompletionData(int carrotPosition)
        {
            if (TryGetTypeAtTextPositionUsingSemanticAnalyzer(carrotPosition, out var type))
            {
                return CreateCompletionDataForType(type!);
            }

            return TryGetCompletionDataFromTheWordBeforeTheCursor(carrotPosition, out var completionData)
                ? completionData!
                : NoCompletionData;
        }

        /// <summary>
        /// Generates a list of variables and thier types that are in the outer context of a function, including the cell and context variables, plus any cell references.
        /// </summary>
        /// <param name="code">The code look for cell references in.</param>
        /// <param name="collectionNameToDataTypeMap">A map of collection names and the data type of the items in them.</param>
        /// <returns>A map from variable name to variable type.</returns>
        public Dictionary<string, Type> CreateOuterContextVariablesForFunction(string code, IReadOnlyDictionary<string, string> collectionNameToDataTypeMap)
        {
            var outerContextVariables = CreateStandardCellFunctionGlobalVariableTypeMap(collectionNameToDataTypeMap);
            if (_cellContext == null) return outerContextVariables;

            var cellReferenceToCodeSyntaxRewriter = new CellReferenceToCodeSyntaxRewriter(_cellContext.Location);
            var codeToCellReferenceSyntaxRewriter = new CodeToCellReferenceSyntaxRewriter(_cellContext.Location);
            var cellReferenceFinder = new CellReferenceSyntaxWalker();
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var root = syntaxTree.GetRoot();
            root = cellReferenceToCodeSyntaxRewriter.Visit(root);
            cellReferenceFinder.Visit(root);
            foreach (var cellReference in cellReferenceFinder.LocationReferences)
            {
                if (_cellContext is null) break;
                var type = cellReference.IsRange ? typeof(CellRange) : typeof(CellModel);
                var name = codeToCellReferenceSyntaxRewriter.GetUserFriendlyCellReferenceText(cellReference);
                if (outerContextVariables.ContainsKey(name)) continue;
                outerContextVariables.Add(name, type);
            }

            return outerContextVariables;
        }

        private Dictionary<string, Type> CreateStandardCellFunctionGlobalVariableTypeMap(IReadOnlyDictionary<string, string> collectionNameTypeMap)
        {
            var outerContextVariables = new Dictionary<string, Type> { { "c", typeof(Context) }, { "cell", typeof(CellModel) } };
            foreach (var (userCollectionName, typeName) in collectionNameTypeMap)
            {
                var type = PluginModel.GetTypeFromString(typeName);
                var enumerableType = typeof(UserList<>).MakeGenericType(type);
                outerContextVariables.Add(userCollectionName, enumerableType);
            }

            return outerContextVariables;
        }

        private List<ICompletionData> CreateCompletionDataForGlobalContext(Dictionary<string, Type> variableNameToTypeMapForOuterContext)
        {
            if (_cachedGlobalCompletionData != null) return _cachedGlobalCompletionData;

            _cachedGlobalCompletionData = [.. variableNameToTypeMapForOuterContext.Select(CreateCompletionDataForKeyValuePair)];
            return _cachedGlobalCompletionData;
        }

        private ICompletionData CreateCompletionDataForKeyValuePair(KeyValuePair<string, Type> nameTypePair)
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

        private IList<ICompletionData> CreateCompletionDataForType(Type type)
        {
            if (_cachedCompletionData.TryGetValue(type, out var completionData)) return completionData;
            CodeDocumentationLoader.LoadXmlDocumentation(type.Assembly);

            var members = type.GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public).Where(m => (m is not MethodInfo mi || !mi.IsSpecialName) && m is not ConstructorInfo);
            var newCompletionData = members.Select(CreateCompletionDataFromMemberInfo).OfType<ICompletionData>().ToList();
            _cachedCompletionData.Add(type, newCompletionData);
            return newCompletionData;
        }

        private ICompletionData CreateCompletionDataFromMemberInfo(MemberInfo info)
        {
            var name = info.Name;
            var userVisibleString = $"{name} : {GetUnderlyingType(info)?.Name ?? "Unable to get type name"}";
            var documentation = info.GetDocumentation();
            return new CodeCompletionData(name, userVisibleString, documentation, IconChar.Line);
        }

        private Type? GetUnderlyingType(MemberInfo member)
        {
            ArgumentNullException.ThrowIfNull(member);
            return member.MemberType switch
            {
                MemberTypes.Event => ((EventInfo)member).EventHandlerType,
                MemberTypes.Field => ((FieldInfo)member).FieldType,
                MemberTypes.Method => ((MethodInfo)member).ReturnType,
                MemberTypes.Property => ((PropertyInfo)member).PropertyType,
                MemberTypes.Constructor => ((ConstructorInfo)member).DeclaringType,
                MemberTypes.NestedType => ((System.Reflection.TypeInfo)member).BaseType,
                _ => throw new ArgumentException("Input MemberInfo must be if type EventInfo, FieldInfo, MethodInfo, or PropertyInfo"),
            };
        }

        private string GetVariableNamePriorToCarot(int carrotPosition)
        {
            if (_currentUserCodeText.Length > carrotPosition || _currentUserCodeText.Length == 0) return "";
            var offset = carrotPosition - 1;
            if (_currentUserCodeText[offset] == '.')
            {
                while (offset > 0 && char.IsLetterOrDigit(_currentUserCodeText[offset - 1])) offset--;
                return _currentUserCodeText[offset..(carrotPosition - 1)];
            }
            return "";
        }

        private bool TryGetCompletionDataFromTheWordBeforeTheCursor(int carrotPosition, out IList<ICompletionData>? completionData)
        {
            completionData = null;
            var typeNameInFrontOfCarrot = GetVariableNamePriorToCarot(carrotPosition);
            if (typeNameInFrontOfCarrot == string.Empty)
            {
                completionData = CreateCompletionDataForGlobalContext(_computedOuterContextVariables);
            }
            else if (CellReferenceToCodeSyntaxRewriter.IsCellLocation(typeNameInFrontOfCarrot))
            {
                completionData = CreateCompletionDataForType(typeof(CellModel));
            }
            return completionData is not null;
        }

        /// <summary>
        /// Attempts to get the type at a given position in the code using semantic analysis.
        /// </summary>
        /// <param name="carrotPosition">The index in the code to attempt to get the type from.</param>
        /// <param name="type">The type, if it was able to be resolved.</param>
        /// <returns>True if the type was able to be resolved.</returns>
        public bool TryGetTypeAtTextPositionUsingSemanticAnalyzer(int carrotPosition, out Type? type)
        {
            type = null;
            var typeSymbol = _semanticAnalyzer.GetTypeAtPosition(carrotPosition);
            if (typeSymbol is null) return false;
            type = TypeSymbolConverter.GetTypeFromSymbol(typeSymbol);
            return type is not null;
        }
    }
}
