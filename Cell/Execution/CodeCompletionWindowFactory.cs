using Cell.Data;
using Cell.Model;
using Cell.Model.Plugin;
using Cell.Plugin.SyntaxWalkers;
using Cell.ViewModel.Application;
using Cell.ViewModel.Execution;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Editing;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Reflection;
using System.Text;

namespace Cell.Execution
{
    internal static class CodeCompletionWindowFactory
    {
        private readonly static Dictionary<string, List<string>> _cachedTypes = [];
        private readonly static Dictionary<string, string> _typeNameToFullyQualifiedTypeNameMap = [];
        public static CompletionWindow? Create(TextArea textArea, string type, bool doesFunctionReturnValue)
        {
            var model = new PluginFunctionModel("testtesttest", textArea.Document.Text, doesFunctionReturnValue ? "object" : "void");
            var function = new FunctionViewModel(model);
            var syntaxTree = function.SyntaxTree;
            var sematicModel = function.GetSemanticModel();
            var variableNode = syntaxTree.GetRoot().DescendantNodes().OfType<VariableDeclarationSyntax>().FirstOrDefault(x => x.Variables.First().Identifier.Text == type);
            if (variableNode != null)
            {
                var typeInfo = sematicModel.GetTypeInfo(variableNode.Type);

                if (typeInfo.Type is not null)
                {
                    var completionWindow = new CompletionWindow(textArea);
                    var data = completionWindow.CompletionList.CompletionData;
                    // TODO: which one?
                    var name = typeInfo.Type.GetFullMetadataName();
                    var name2 = typeInfo.Type.ToString();
                    GetMembersOfType(name).ForEach(x => data.Add(new PluginContextCompletionData(x)));
                    return completionWindow;
                }
            }
            if (type == "c")
            {
                var completionWindow = new CompletionWindow(textArea);
                IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;
                data.Add(new PluginContextCompletionData("GoToSheet"));
                data.Add(new PluginContextCompletionData("GoToCell"));
                data.Add(new PluginContextCompletionData("SheetNames"));
                return completionWindow;
            }
            else if (type == "cell" || CellReferenceToCodeSyntaxRewriter.IsCellLocation(type))
            {
                var completionWindow = new CompletionWindow(textArea);
                IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;
                foreach (var property in typeof(CellModel).GetProperties())
                {
                    data.Add(new PluginContextCompletionData(property.Name));
                }
                return completionWindow;
            }
            else if (ApplicationViewModel.Instance.UserCollectionLoader.CollectionNames.Contains(type))
            {
                var completionWindow = new CompletionWindow(textArea);
                IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;
                foreach (var property in typeof(UserList<PluginModel>).GetMethods())
                {
                    data.Add(new PluginContextCompletionData(property.Name));
                }
                return completionWindow;
            }
            return null;
        }

        public static string GetFullMetadataName(this ISymbol s)
        {
            if (s == null || IsRootNamespace(s))
            {
                return string.Empty;
            }

            var sb = new StringBuilder(s.MetadataName);
            var last = s;

            s = s.ContainingSymbol;

            while (!IsRootNamespace(s))
            {
                if (s is ITypeSymbol && last is ITypeSymbol)
                {
                    sb.Insert(0, '+');
                }
                else
                {
                    sb.Insert(0, '.');
                }

                // TODO: which one?
                //sb.Insert(0, s.OriginalDefinition.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));
                sb.Insert(0, s.MetadataName);
                s = s.ContainingSymbol;
            }

            return sb.ToString();
        }

        public static List<string> GetMembersOfType(string typeName)
        {
            if (_cachedTypes.TryGetValue(typeName, out var members)) return members;
            if (_typeNameToFullyQualifiedTypeNameMap.TryGetValue(typeName, out var fullQualifiedName))
            {
                var type = Type.GetType(fullQualifiedName);
                if (type is not null)
                {
                    _cachedTypes[typeName] = type.GetMembers().Select(x => x.Name).ToList();
                    return _cachedTypes[typeName];
                }
            }
            return ["No members found"];
        }

        public static void RegisterTypesInAssembly(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsPublic)
                {
                    if (_typeNameToFullyQualifiedTypeNameMap.ContainsKey(type.Name))
                    {
                        throw new InvalidOperationException($"Type name {type.Name} is already registered. Semmantic analysis doesn't currently know the fqn of the type, so type names must be globally unique.");
                    }
                    if (type.FullName is not null) _typeNameToFullyQualifiedTypeNameMap[type.Name] = type.FullName;
                }
            }
        }

        private static bool IsRootNamespace(ISymbol symbol)
        {
            return symbol is INamespaceSymbol s && s.IsGlobalNamespace;
        }
    }
}
