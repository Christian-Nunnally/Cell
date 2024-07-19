using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.Text.Json.Serialization;
using Cell.Plugin;
using System.Reflection;
using Cell.Plugin.SyntaxRewriters;

namespace Cell.Model
{
    public partial class PluginFunction
    {
        private const string codeHeader = @"using System;
using System.Collections.Generic;
using Cell.Model;
using Cell.ViewModel;
using Cell.Model.Plugin;
using Cell.Plugin;                

namespace Plugin
{
    public class Program
    {
        public static ";

        private const string codeFooter = @"
        }
    }
}";

        private const string methodHeader = @" PluginMethod(PluginContext c, CellModel cell)
        {
            ";

        private string code = string.Empty;
        private readonly List<CellModel> _cellsToNotify = [];
        private bool _isSyntaxTreeValid;

        public bool IsSyntaxTreeValid => _isSyntaxTreeValid;

        public List<string> SheetDependencies { get; set; } = [];

        public List<int> RowDependencies { get; set; } = [];

        public List<int> ColumnDependencies { get; set; } = [];

        public List<string> CollectionDependencies { get; set; } = [];

        public string Name { get; set; } = string.Empty;

        public bool DoesFunctionReturnObject { get; set; } = false;

        [JsonIgnore]
        public MethodInfo? CompiledMethod => _compiledMethod ?? Compile();

        private MethodInfo? _compiledMethod;

        [JsonIgnore]
        public CompileResult CompileResult { get; private set; }

        public PluginFunction()
        {
        }

        public PluginFunction(string name, string code, bool isTrigger)
        {
            Name = name;
            DoesFunctionReturnObject = !isTrigger;
            Code = code;
        }

        public string Code
        {
            get => code; 
            set
            {
                _compiledMethod = null;
                code = value;
                FindAndRefreshDependencies();
                if (_isSyntaxTreeValid) Compile();
            }
        }

        [JsonIgnore]
        public SyntaxTree SyntaxTree { get; set; } = CSharpSyntaxTree.ParseText("");

        public void FindAndRefreshDependencies()
        {
            _isSyntaxTreeValid = false;
            ClearAllDependencies();
            var returnValue = DoesFunctionReturnObject ? "object" : "void";
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(codeHeader + returnValue + methodHeader + code + codeFooter);
            SyntaxNode? root = syntaxTree.GetRoot();

            var cellLocationSyntaxRewriter = new FindAndReplaceCellLocationsSyntaxRewriter();
            root = cellLocationSyntaxRewriter.Visit(root);
            if (root is null) throw new Exception("Syntax root should not be null after rewrite.");
            SyntaxTree = root.SyntaxTree;
            foreach (var (Sheet, Row, Column) in cellLocationSyntaxRewriter.Locations)
            {
                AddDependencyOnLocation(Sheet, Row, Column);
            }

            var collectionReferenceSyntaxRewriter = new FindAndReplaceCollectionReferencesSyntaxWalker();
            root = collectionReferenceSyntaxRewriter.Visit(root);
            if (!collectionReferenceSyntaxRewriter.Result.Success) return;

            if (root is null) throw new Exception("Syntax root should not be null after rewrite.");
            SyntaxTree = root.SyntaxTree;
            _isSyntaxTreeValid = true;
            CollectionDependencies.AddRange(collectionReferenceSyntaxRewriter.CollectionReferences);

            NotifyDependenciesHaveChanged();
        }

        private void ClearAllDependencies()
        {
            SheetDependencies.Clear();
            RowDependencies.Clear();
            ColumnDependencies.Clear();
            CollectionDependencies.Clear();
        }

        private void AddDependencyOnLocation(string sheetName, int row, int column)
        {
            SheetDependencies.Add(sheetName);
            RowDependencies.Add(row);
            ColumnDependencies.Add(column);
        }

        private void NotifyDependenciesHaveChanged()
        {
            _cellsToNotify.ForEach(cell => cell.UpdateDependencySubscriptions(this));
        }

        internal void StopListeningForDependencyChanges(CellModel cell)
        {
            _cellsToNotify.Remove(cell);
        }

        internal void StartListeningForDependencyChanges(CellModel cell)
        {
            _cellsToNotify.Add(cell);
        }

        public MethodInfo? Compile()
        {
            if (!_isSyntaxTreeValid) FindAndRefreshDependencies();
            try
            {
                var compiler = new RoslynCompiler("Plugin.Program", SyntaxTree, [typeof(Console)]);
                var compiled = compiler.Compile() ?? throw new Exception("Error during compile - compiled object is null");
                _compiledMethod = compiled.GetMethod("PluginMethod") ?? throw new Exception("Error during compile - compiled object is null");
                CompileResult = new CompileResult { Success = true, Result = "" };
            }
            catch (Exception e)
            {
                CompileResult = new CompileResult { Success = false, Result = e.Message };
            }
            return _compiledMethod;
        }
    }
}
