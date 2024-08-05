using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Cell.Plugin;
using System.Reflection;
using Cell.Plugin.SyntaxWalkers;
using Cell.Common;

namespace Cell.Model
{
    public partial class PluginFunction
    {
        private readonly List<CellModel> _cellsToNotify = [];
        private bool _isSyntaxTreeValid;
        private MethodInfo? _compiledMethod;

        public bool IsSyntaxTreeValid => _isSyntaxTreeValid;

        public List<CellReference> LocationDependencies { get; set; } = [];

        public List<string> CollectionDependencies { get; set; } = [];

        public PluginFunctionModel Model { get; set; }

        public MethodInfo? CompiledMethod => _compiledMethod ?? Compile();

        public CompileResult CompileResult { get; private set; }

        public SyntaxTree SyntaxTree { get; set; } = CSharpSyntaxTree.ParseText("");

        public PluginFunction(PluginFunctionModel model)
        {
            Model = model;
            Model.PropertyChanged += ModelPropertyChanged;
            AttemptToRecompileMethod();
        }

        private void ModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PluginFunctionModel.Code))
            {
                AttemptToRecompileMethod();
            }
        }

        private void AttemptToRecompileMethod()
        {
            _compiledMethod = null;
            ExtractAndTransformDependencies();
            if (_isSyntaxTreeValid) Compile();
        }

        public void SetUserFriendlyCode(string userFriendlyCode, CellModel cell)
        {
            userFriendlyCode = userFriendlyCode.Replace("..", "_Range_");
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(userFriendlyCode);
            Model.Code = new CellReferenceToCodeSyntaxRewriter(cell).Visit(syntaxTree.GetRoot())?.ToFullString() ?? string.Empty;

            // Populate cells that use this function as populate:
            foreach (var cellModel in _cellsToNotify)
            {
                cellModel.PopulateText();
            }
        }

        public string GetUserFriendlyCode(CellModel cell)
        {
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(Model.Code);
            var rawCode = new CodeToCellReferenceSyntaxRewriter(cell).Visit(syntaxTree.GetRoot())?.ToFullString() ?? string.Empty;
            return rawCode.Replace("_Range_", "..");
        }

        public void ExtractAndTransformDependencies()
        {
            _isSyntaxTreeValid = false;
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(Model.FullCode);
            
            SyntaxNode? root = syntaxTree.GetRoot();
            try
            {
                ExtractCellLocationReferences(root);
                root = ExtractAndTransformCollectionReferences(root);
            }
            catch (CellError)
            {
                return;
            }
            catch (InvalidCastException)
            {
                return;
            }
            SyntaxTree = root.SyntaxTree;
            _isSyntaxTreeValid = true;
            NotifyDependenciesHaveChanged();
        }

        public SemanticModel GetSemanticModel()
        {
            var compilation = CSharpCompilation.Create("PluginAssembly")
                .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
                .AddSyntaxTrees(SyntaxTree);
            return compilation.GetSemanticModel(SyntaxTree);
        }

        private SyntaxNode ExtractAndTransformCollectionReferences(SyntaxNode? root)
        {
            CollectionDependencies.Clear();
            var collectionReferenceSyntaxRewriter = new FindAndReplaceCollectionReferencesSyntaxWalker();
            root = collectionReferenceSyntaxRewriter.Visit(root) ?? throw new Exception("Syntax root should not be null after rewrite.");
            var resultStatus = collectionReferenceSyntaxRewriter.Result;
            if (!resultStatus.Success) throw new CellError(resultStatus.Result);
            var foundDependencies = collectionReferenceSyntaxRewriter.CollectionReferences;
            CollectionDependencies.AddRange(foundDependencies);
            return root;
        }

        private void ExtractCellLocationReferences(SyntaxNode? root)
        {
            LocationDependencies.Clear();
            var walker = new CellReferenceSyntaxWalker();
            walker.Visit(root);
            var foundDependencies = walker.LocationReferences;
            LocationDependencies.AddRange(foundDependencies);
        }

        public MethodInfo? Compile()
        {
            if (!_isSyntaxTreeValid) ExtractAndTransformDependencies();
            try
            {
                var typesToAddAssemblyReferencesFor = new Type[] { typeof(Console), typeof(System.Linq.Enumerable) };
                var compiler = new RoslynCompiler(SyntaxTree, typesToAddAssemblyReferencesFor);
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

        private void NotifyDependenciesHaveChanged() => _cellsToNotify.ForEach(cell => cell.UpdateDependencySubscriptions(this));

        internal void StopListeningForDependencyChanges(CellModel cell) => _cellsToNotify.Remove(cell);

        internal void StartListeningForDependencyChanges(CellModel cell) => _cellsToNotify.Add(cell);
    }
}
