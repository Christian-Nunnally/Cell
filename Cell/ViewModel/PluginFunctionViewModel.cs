using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Cell.Plugin;
using System.Reflection;
using Cell.Plugin.SyntaxWalkers;
using Cell.Common;
using Cell.Persistence;
using Cell.View.ToolWindow;
using Cell.Data;
using Cell.Model;

namespace Cell.ViewModel
{
    public partial class PluginFunctionViewModel : PropertyChangedBase
    {
        private readonly List<CellModel> _cellsToNotify = [];

        private ulong _fingerprintOfProcessedDependencies;
        private MethodInfo? _compiledMethod;

        public int UsageCount => _cellsToNotify.Count + UserCollectionLoader.ObservableCollections.Count(x => x.Model.SortAndFilterFunctionName == Name);

        public List<CellReference> LocationDependencies { get; set; } = [];

        public List<string> CollectionDependencies { get; set; } = [];

        public PluginFunctionModel Model { get; set; }

        public MethodInfo? CompiledMethod => _compiledMethod ?? Compile();

        public CompileResult CompileResult { get; private set; }

        public SyntaxTree SyntaxTree { get; set; } = CSharpSyntaxTree.ParseText("");

        public IEnumerable<CellModel> CellsThatUseFunction => _cellsToNotify;

        public string Name
        {
            get { return Model.Name; }
            set
            {
                if (Model.Name == value) return;
                var oldName = Model.Name;
                Model.Name = value;
                NotifyPropertyChanged(nameof(Name));
                DialogWindow.ShowYesNoConfirmationDialog("Refactor?", $"Do you want to update cells that used '{oldName}' to use '{Model.Name}' instead?", () => RefactorCellsFunctionUseage(oldName, Model.Name));
            }
        }

        private static void RefactorCellsFunctionUseage(string oldName, string newName)
        {
            foreach (var cells in Cells.Instance.AllCells)
            {
                if (cells.PopulateFunctionName == oldName) cells.PopulateFunctionName = newName;
                if (cells.TriggerFunctionName == oldName) cells.TriggerFunctionName = newName;
            }
        }

        public PluginFunctionViewModel(PluginFunctionModel model)
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
            ExtractDependencies();
            if (_fingerprintOfProcessedDependencies == Model.Fingerprint) Compile();
        }

        public void SetUserFriendlyCode(string userFriendlyCode, CellModel? cell)
        {
            var intermediateCode = userFriendlyCode.Replace("..", "_Range_");
            if (cell != null) intermediateCode = new CellReferenceSyntaxTransformer(cell).TransformFrom(intermediateCode);
            Model.Code = new CollectionReferenceSyntaxTransformer(UserCollectionLoader.GetDataTypeStringForCollection, UserCollectionLoader.CollectionNames.Contains).TransformFrom(intermediateCode);

            // Populate cells that use this function as populate:
            foreach (var cellModel in _cellsToNotify)
            {
                cellModel.PopulateText();
            }
        }

        public string GetUserFriendlyCode(CellModel? cell)
        {
            var intermediateCode = new CollectionReferenceSyntaxTransformer(UserCollectionLoader.GetDataTypeStringForCollection, UserCollectionLoader.CollectionNames.Contains).TransformTo(Model.Code);
            if (cell != null) intermediateCode = new CellReferenceSyntaxTransformer(cell).TransformTo(intermediateCode);
            return intermediateCode.Replace("_Range_", "..");
        }

        public void ExtractDependencies()
        {
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(Model.FullCode);
            SyntaxNode? root = syntaxTree.GetRoot();
            try
            {
                ExtractCellLocationReferences(root);
                ExtractAndTransformCollectionReferences(root);
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
            _fingerprintOfProcessedDependencies = Model.Fingerprint;
            NotifyDependenciesHaveChanged();
        }

        public SemanticModel GetSemanticModel()
        {
            var compilation = CSharpCompilation.Create("PluginAssembly")
                .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
                .AddSyntaxTrees(SyntaxTree);
            return compilation.GetSemanticModel(SyntaxTree);
        }

        private void ExtractAndTransformCollectionReferences(SyntaxNode? root)
        {
            CollectionDependencies.Clear();
            var walker = new CollectionReferenceSyntaxWalker();
            walker.Visit(root);
            var foundCollectionReferences = walker.CollectionReferences;
            CollectionDependencies.AddRange(foundCollectionReferences.Distinct());
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
            try
            {
                var typesToAddAssemblyReferencesFor = new Type[] { typeof(Console), typeof(Enumerable) };
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
