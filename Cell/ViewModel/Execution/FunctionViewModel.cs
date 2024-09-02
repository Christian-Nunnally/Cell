using Cell.Common;
using Cell.Execution;
using Cell.Execution.SyntaxWalkers;
using Cell.Model;
using Cell.Persistence;
using Cell.View.ToolWindow;
using Cell.ViewModel.Application;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;

namespace Cell.ViewModel.Execution
{
    public partial class FunctionViewModel : PropertyChangedBase
    {
        public readonly List<CellModel> CellsToNotify = [];
        private MethodInfo? _compiledMethod;
        private ulong _fingerprintOfProcessedDependencies;
        private bool wasCompileSuccessful;
        public FunctionViewModel(PluginFunctionModel model)
        {
            Model = model;
            Model.PropertyChanged += ModelPropertyChanged;
            AttemptToRecompileMethod();
        }

        public event Action<FunctionViewModel>? DependenciesChanged;

        public IEnumerable<CellModel> CellsThatUseFunction => CellsToNotify;

        public List<string> CollectionDependencies { get; set; } = [];

        public MethodInfo? CompiledMethod => _compiledMethod ?? Compile();

        public CompileResult CompileResult { get; private set; }

        public List<CellReference> LocationDependencies { get; set; } = [];

        public PluginFunctionModel Model { get; set; }

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

        public SyntaxTree SyntaxTree { get; set; } = CSharpSyntaxTree.ParseText("");

        public int UsageCount => CellsToNotify.Count + UserCollectionLoader.ObservableCollections.Count(x => x.Model.SortAndFilterFunctionName == Name);

        public bool WasCompileSuccessful
        {
            get => wasCompileSuccessful; set
            {
                if (wasCompileSuccessful == value) return;
                wasCompileSuccessful = value;
                NotifyPropertyChanged(nameof(WasCompileSuccessful));
            }
        }

        public MethodInfo? Compile()
        {
            try
            {
                var compiler = new RoslynCompiler(SyntaxTree);
                var compiled = compiler.Compile() ?? throw new Exception("Error during compile - compiled object is null");
                _compiledMethod = compiled.GetMethod("PluginMethod") ?? throw new Exception("Error during compile - compiled object is null");
                CompileResult = new CompileResult { Success = true, Result = "" };
                WasCompileSuccessful = true;
            }
            catch (Exception e)
            {
                CompileResult = new CompileResult { Success = false, Result = e.Message };
                WasCompileSuccessful = false;
            }
            return _compiledMethod;
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
            DependenciesChanged?.Invoke(this);
        }

        public SemanticModel GetSemanticModel()
        {
            var compilation = CSharpCompilation.Create("PluginAssembly")
                .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
                .AddSyntaxTrees(SyntaxTree);
            return compilation.GetSemanticModel(SyntaxTree);
        }

        public string GetUserFriendlyCode(CellModel? cell)
        {
            var intermediateCode = new CollectionReferenceSyntaxTransformer(ApplicationViewModel.Instance.UserCollectionLoader.GetDataTypeStringForCollection, UserCollectionLoader.CollectionNames.Contains).TransformTo(Model.Code);
            if (cell != null) intermediateCode = new CellReferenceSyntaxTransformer(cell).TransformTo(intermediateCode);
            return intermediateCode.Replace("_Range_", "..");
        }

        public void SetUserFriendlyCode(string userFriendlyCode, CellModel? cell)
        {
            var intermediateCode = userFriendlyCode.Replace("..", "_Range_");
            if (cell != null) intermediateCode = new CellReferenceSyntaxTransformer(cell).TransformFrom(intermediateCode);
            Model.Code = new CollectionReferenceSyntaxTransformer(ApplicationViewModel.Instance.UserCollectionLoader.GetDataTypeStringForCollection, UserCollectionLoader.CollectionNames.Contains).TransformFrom(intermediateCode);

            // Populate cells that use this function as populate:
            foreach (var cellModel in CellsToNotify.ToList())
            {
                cellModel.PopulateText();
            }
        }

        // TODO REMOVE
        internal void StartListeningForDependencyChanges(CellModel cell) => CellsToNotify.Add(cell);

        internal void StopListeningForDependencyChanges(CellModel cell) => CellsToNotify.Remove(cell);

        private static void RefactorCellsFunctionUseage(string oldName, string newName)
        {
            foreach (var cells in ApplicationViewModel.Instance.CellTracker.AllCells)
            {
                if (cells.PopulateFunctionName == oldName) cells.PopulateFunctionName = newName;
                if (cells.TriggerFunctionName == oldName) cells.TriggerFunctionName = newName;
            }
        }

        private void AttemptToRecompileMethod()
        {
            _compiledMethod = null;
            ExtractDependencies();
            if (_fingerprintOfProcessedDependencies == Model.Fingerprint) Compile();
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

        private void ModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PluginFunctionModel.Code))
            {
                AttemptToRecompileMethod();
            }
        }
    }
}
