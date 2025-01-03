﻿using Cell.Core.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Reflection;

namespace Cell.Core.Execution.CodeCompletion
{
    /// <summary>
    /// Runs semantic analysis on a code snippet to provide type information.
    /// </summary>
    public class SemanticAnalyzer
    {
        private string _prefixText = string.Empty;
        private SyntaxNode _root;
        private readonly IEnumerable<PortableExecutableReference> _references;
        private SemanticModel _semanticModel;
        private readonly string _usingsCodes;

        /// <summary>
        /// Creates a new instance of <see cref="SemanticAnalyzer"/>.
        /// </summary>
        /// <param name="usings">Additional namespaces to provide information about types not declared in 'code'.</param>
        public SemanticAnalyzer(IEnumerable<string> usings)
        {
            _usingsCodes = string.Join("\n", usings.Select(x => $"using {x};\n"));

            var currentDomain = AppDomain.CurrentDomain;
            var currentLoadedAssemblies = currentDomain.GetAssemblies();
            static bool IsValidAssembly(Assembly a) => !a.IsDynamic && !string.IsNullOrEmpty(a.Location);
            _references = currentLoadedAssemblies
                .Where(IsValidAssembly)
                .Select(x => x.Location)
                .Select(x => MetadataReference.CreateFromFile(x));
            var syntaxTree = CSharpSyntaxTree.ParseText("");
            _root = syntaxTree.GetRoot();
            var compilation = CSharpCompilation.Create("Analysis", [syntaxTree], _references);
            _semanticModel = compilation.GetSemanticModel(syntaxTree);
        }

        /// <summary>
        /// Updates the semantic model based on the new code string.
        /// </summary>
        /// <param name="code">The new code to analyze.</param>
        /// <param name="variableNameToTypeMapForOuterContext">Map of variable names to thier types to resolve references to names not resolved normally, like global variables.</param>
        public void UpdateCode(string code, Dictionary<string, Type> variableNameToTypeMapForOuterContext)
        {
            var outerContextDeclarationsCode = string.Join("\n", variableNameToTypeMapForOuterContext.Select(x => $"{x.Value.GetPrettyFullGenericTypeName()} {x.Key};\n"));
            _prefixText = $"{_usingsCodes}\n{outerContextDeclarationsCode}\n";
            var syntaxTree = CSharpSyntaxTree.ParseText(_prefixText + code);
            _root = syntaxTree.GetRoot();
            var compilation = CSharpCompilation.Create("Analysis", [syntaxTree], _references);
            _semanticModel = compilation.GetSemanticModel(syntaxTree);
            AnalyzeVarDeclarations();
        }

        /// <summary>
        /// Map of variable names to thier types for all declarations found in the code snippet.
        /// </summary>
        public Dictionary<string, ITypeSymbol> NameToTypeMap { get; } = [];

        /// <summary>
        /// Queries the semantic model for the type of the expression at the specified text carrot position.
        /// </summary>
        /// <param name="position">The position in terms of characters of text.</param>
        /// <returns>The type, if a type was able to be resolved at that location.</returns>
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
                    if (typeInfo.Type is null) continue;
                    NameToTypeMap.Add(variable.Identifier.ToString(), typeInfo.Type);
                }
            }
        }
    }
}
