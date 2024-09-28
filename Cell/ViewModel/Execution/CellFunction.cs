using Cell.Common;
using Cell.Core.Execution.References;
using Cell.Execution;
using Cell.Execution.References;
using Cell.Execution.SyntaxWalkers.CellReferences;
using Cell.Execution.SyntaxWalkers.UserCollections;
using Cell.Model;
using Cell.ViewModel.Application;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;

namespace Cell.ViewModel.Execution
{
    /// <summary>
    /// Encapsulates a <see cref="CellFunctionModel"/> and gives it the ability to compile and run.
    /// </summary>
    public partial class CellFunction : PropertyChangedBase
    {
        /// <summary>
        /// A null function that can be used as a placeholder.
        /// </summary>
        public static readonly CellFunction Null = new(CellFunctionModel.Null);
        private static readonly List<string> UsingNamespaces = [
            "System",
            "System.Linq",
            "System.Collections",
            "System.Collections.Generic",
            "Cell.Model",
            "Cell.Model.Plugin",
            "Cell.ViewModel",
            "Cell.Execution",
            "Cell.ViewModel.Cells.Types",
            ];

        /// <summary>
        /// The namespaces that are available to all functions preformatted for use in code.
        /// </summary>
        public static readonly string UsingNamespacesString = string.Join('\n', UsingNamespaces.Select(x => $"using {x};"));
        private MethodInfo? _compiledMethod;
        private ulong _fingerprintOfProcessedDependencies;
        private bool _wasCompileSuccessful;

        /// <summary>
        /// Creates a new instance of <see cref="CellFunction"/>
        /// </summary>
        /// <param name="model"></param>
        public CellFunction(CellFunctionModel model)
        {
            Model = model;
            Model.PropertyChanged += ModelPropertyChanged;
            AttemptToRecompileMethod();
        }

        public event Action<CellFunction>? DependenciesChanged;

        public List<CellModel> CellsThatUseFunction => ApplicationViewModel.Instance.CellPopulateManager.GetCellsThatUsePopulateFunction(this);

        public List<ICollectionReference> CollectionDependencies { get; set; } = [];

        public MethodInfo? CompiledMethod => _compiledMethod ?? Compile();

        public CompileResult CompileResult { get; private set; }

        public IEnumerable<IReferenceFromCell> Dependencies => ((IEnumerable<IReferenceFromCell>)LocationDependencies).Concat(CollectionDependencies);

        public List<LocationReference> LocationDependencies { get; set; } = [];

        public CellFunctionModel Model { get; set; }

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
            get => _wasCompileSuccessful; set
            {
                if (_wasCompileSuccessful == value) return;
                _wasCompileSuccessful = value;
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

        public string GetUserFriendlyCode(CellModel? cell, IReadOnlyDictionary<string, string> collectionNameToDataTypeMap)
        {
            var intermediateCode = new CollectionReferenceSyntaxTransformer(collectionNameToDataTypeMap).TransformTo(Model.Code);
            if (cell != null) intermediateCode = new CellReferenceSyntaxTransformer(cell).TransformTo(intermediateCode);
            return intermediateCode.Replace("_Range_", "..");
        }

        public CompileResult Run(Context pluginContext, CellModel cell)
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

        public void SetUserFriendlyCode(string userFriendlyCode, CellModel? cell, IReadOnlyDictionary<string, string> collectionNameToDataTypeMap)
        {
            var intermediateCode = userFriendlyCode.Replace("..", "_Range_").Replace("\t", "    ");
            if (cell != null) intermediateCode = new CellReferenceSyntaxTransformer(cell).TransformFrom(intermediateCode);
            Model.Code = new CollectionReferenceSyntaxTransformer(collectionNameToDataTypeMap).TransformFrom(intermediateCode);
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
            if (e.PropertyName == nameof(CellFunctionModel.Code))
            {
                AttemptToRecompileMethod();
            }
        }

        private CompileResult RunUnsafe(Context pluginContext, CellModel cell, MethodInfo? method)
        {
            var result = new CompileResult { WasSuccess = true, ExecutionResult = "Success" };
            // TODO: Do I actually need to check the return type here?
            if (Model.ReturnType != "void") result.ReturnedObject = method?.Invoke(null, [pluginContext, cell]);
            else method?.Invoke(null, [pluginContext, cell]);
            return result;
        }
    }
}
