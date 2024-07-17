
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Cell.Persistence;
using System.Text.Json.Serialization;
using Cell.Plugin;
using System.Reflection;
using Cell.Plugin.SyntaxRewriters;

namespace Cell.Model
{
    public partial class PluginFunction
    {
        private const string classHeader = @"
            using System;
            using Cell.Model;
            using Cell.Model.Plugin;
            using Cell.Plugin;                

            namespace Plugin
            {
                public class Program
                {";

        private const string classFooter = @"
                }
            }";

        private const string populateHeader = @"
                    public static object PluginMethod(PluginContext c, CellModel cell)
                    {
                        ";
        private const string triggerHeader = @"
                    public static void PluginMethod(PluginContext c, CellModel cell)
                    {
                        ";
        private const string methodFooter = @"
                    }";

        private string code = string.Empty;
        private readonly List<CellModel> _cellsToNotify = [];

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

        public PluginFunction(string name, string code, bool isTrigger)
        {
            Name = name;
            DoesFunctionReturnObject = isTrigger;
            Code = code;
        }

        public string Code
        {
            get => code; set
            {
                code = value;
                FindAndRefreshDependencies();
                Compile();
            }
        }

        [JsonIgnore]
        public SyntaxTree SyntaxTree { get; set; } = CSharpSyntaxTree.ParseText("");

        public void FindAndRefreshDependencies()
        {
            ClearAllDependencies();
            var methodHeader = DoesFunctionReturnObject ? triggerHeader : populateHeader;
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(classHeader + methodHeader + code + methodFooter + classFooter);
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
            if (root is null) throw new Exception("Syntax root should not be null after rewrite.");
            SyntaxTree = root.SyntaxTree;
            CollectionDependencies.AddRange(collectionReferenceSyntaxRewriter.CollectionReferences);

            NotifyDependenciesHaveChanged();

            // TODO: move this to the plugin function loader unless you want testtesttest to be saved.
            PluginFunctionLoader.SavePluginFunction(DoesFunctionReturnObject ? PluginFunctionLoader.TriggerFunctionsDirectoryName : PluginFunctionLoader.PopulateFunctionsDirectoryName, this);
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

        private MethodInfo? Compile()
        {
            _compiledMethod = null;
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
