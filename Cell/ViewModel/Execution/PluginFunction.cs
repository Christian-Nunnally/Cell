using Cell.Common;
using Cell.Execution;
using Cell.Execution.SyntaxWalkers.CellReferences;
using Cell.Execution.SyntaxWalkers.UserCollections;
using Cell.Model;
using Cell.ViewModel.Application;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;

namespace Cell.ViewModel.Execution
{
    public partial class PluginFunction : PropertyChangedBase
    {
        public List<CellModel> CellsThatUseFunction => ApplicationViewModel.Instance.CellPopulateManager.GetCellsThatUsePopulateFunction(this);
        private MethodInfo? _compiledMethod;
        private ulong _fingerprintOfProcessedDependencies;
        private bool wasCompileSuccessful;
        public PluginFunction(PluginFunctionModel model)
        {
            Model = model;
            Model.PropertyChanged += ModelPropertyChanged;
            AttemptToRecompileMethod();
        }

        public event Action<PluginFunction>? DependenciesChanged;

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
                DialogFactory.ShowYesNoConfirmationDialog("Refactor?", $"Do you want to update cells that used '{oldName}' to use '{Model.Name}' instead?", () => RefactorCellsFunctionUseage(oldName, Model.Name));
            }
        }

        public SyntaxTree SyntaxTree { get; set; } = CSharpSyntaxTree.ParseText("");

        public int UsageCount => CellsThatUseFunction.Count;

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
                CompileResult = new CompileResult { WasSuccess = true, ExecutionResult = "" };
                WasCompileSuccessful = true;
            }
            catch (Exception e)
            {
                CompileResult = new CompileResult { WasSuccess = false, ExecutionResult = e.Message };
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

        public string GetUserFriendlyCode(CellModel? cell, Func<string, string> getDataTypeFromCollectionNameFunction, IEnumerable<string> collectionNames)
        {
            var intermediateCode = new CollectionReferenceSyntaxTransformer(getDataTypeFromCollectionNameFunction, collectionNames.Contains).TransformTo(Model.Code);
            if (cell != null) intermediateCode = new CellReferenceSyntaxTransformer(cell).TransformTo(intermediateCode);
            return intermediateCode.Replace("_Range_", "..");
        }

        public void SetUserFriendlyCode(string userFriendlyCode, CellModel? cell, Func<string, string> getDataTypeFromCollectionNameFunction, IEnumerable<string> collectionNames)
        {
            var intermediateCode = userFriendlyCode.Replace("..", "_Range_").Replace("\t", "    ");
            if (cell != null) intermediateCode = new CellReferenceSyntaxTransformer(cell).TransformFrom(intermediateCode);
            Model.Code = new CollectionReferenceSyntaxTransformer(getDataTypeFromCollectionNameFunction, collectionNames.Contains).TransformFrom(intermediateCode);
        }

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

        public CompileResult Run(PluginContext pluginContext, CellModel cell)
        {
            var method = CompiledMethod;
            if (!CompileResult.WasSuccess) return CompileResult;
            try
            {
                return RunUnsafe(pluginContext, cell, method);
            }
            catch (Exception e)
            {
                return new CompileResult { WasSuccess = false, ExecutionResult = "Function errored during execution: " + e.Message };
            }
        }

        private CompileResult RunUnsafe(PluginContext pluginContext, CellModel cell, MethodInfo? method)
        {
            var result = new CompileResult { WasSuccess = true, ExecutionResult = "Success"};
            // TODO: Do I actually need to check the return type here?
            if (Model.ReturnType != "void") result.ReturnedObject = method?.Invoke(null, [pluginContext, cell]);
            else method?.Invoke(null, [pluginContext, cell]);
            return result;
        }
    }
}
