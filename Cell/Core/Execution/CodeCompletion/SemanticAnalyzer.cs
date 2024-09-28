using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Reflection;

namespace Cell.Core.Execution.CodeCompletion
{
    public class SemanticAnalyzer
    {
        private readonly SyntaxNode _root;
        private readonly SemanticModel _semanticModel;
        private readonly string _prefixText;

        public Dictionary<string, ITypeSymbol> NameToTypeMap { get; } = [];

        /// <summary>
        /// Creates a new instance of <see cref="SemanticAnalyzer"/>.
        /// </summary>
        /// <param name="code">The code to analyze.</param>
        /// <param name="usings">Additional namespaces to provide information about types not declared in 'code'.</param>
        /// <param name="variableNameToTypeMapForOuterContext">Map of variable names to thier types to resolve references to names not resolved normally, like global variables.</param>
        public SemanticAnalyzer(string code, IEnumerable<string> usings, Dictionary<string, Type> variableNameToTypeMapForOuterContext)
        {
            var usingsCodes = string.Join("\n", usings.Select(x => $"using {x};\n"));
            var outerContextDeclarationsCode = string.Join("\n", variableNameToTypeMapForOuterContext.Select(x => $"{x.Value.Name} {x.Key};\n"));
            _prefixText = $"{usingsCodes}\n{outerContextDeclarationsCode}\n";

            var syntaxTree = CSharpSyntaxTree.ParseText(_prefixText + code);
            _root = syntaxTree.GetRoot();

            var currentDomain = AppDomain.CurrentDomain;
            var currentLoadedAssemblies = currentDomain.GetAssemblies();
            static bool IsValidAssembly(Assembly a) => !a.IsDynamic && !string.IsNullOrEmpty(a.Location);
            var references = currentLoadedAssemblies
                .Where(IsValidAssembly)
                .Select(x => x.Location)
                .Select(x => MetadataReference.CreateFromFile(x));

            var compilation = CSharpCompilation.Create("Analysis", [syntaxTree], references);
            foreach (var d in compilation.GetDiagnostics())
            {
                Console.WriteLine(CSharpDiagnosticFormatter.Instance.Format(d));
            }

            // Get semantic model
            _semanticModel = compilation.GetSemanticModel(syntaxTree);

            AnalyzeVarDeclarations();
        }

        public ITypeSymbol? GetTypeAtPosition(int position)
        {
            position += _prefixText.Length;
            var node = _root.FindNode(new TextSpan(position, 0));

            // Get the type information if the node is an expression
            if (node is CompilationUnitSyntax compilationUnit)
            {
                node = compilationUnit.ChildNodes().LastOrDefault();
                if (node is null) return null;
            }
            if (node is GlobalStatementSyntax globalStatement)
            {
                node = globalStatement.Statement;
            }
            if (node is ExpressionStatementSyntax expressionStatementSyntax)
            {
                node = expressionStatementSyntax.Expression;
            }
            if (node is MemberAccessExpressionSyntax memberAccessExpressionSyntax)
            {
                node = memberAccessExpressionSyntax.Expression;
            }
            if (node is ExpressionSyntax expression)
            {
                var typeInfo = _semanticModel.GetTypeInfo(expression);
                return typeInfo.Type;
            }
            return null;
        }

        private void AnalyzeVarDeclarations()
        {
            var localDeclarations = _root.DescendantNodes().OfType<LocalDeclarationStatementSyntax>();
            foreach (var localDeclaration in localDeclarations)
            {
                var variableDeclaration = localDeclaration.Declaration;
                var typeInfo = _semanticModel.GetTypeInfo(variableDeclaration.Type);
                foreach (var variable in variableDeclaration.Variables)
                {
                    if (NameToTypeMap.ContainsKey(variable.Identifier.ToString())) continue;
                    NameToTypeMap.Add(variable.Identifier.ToString(), typeInfo.Type);
                }
            }
        }
    }
}
