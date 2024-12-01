using Cell.Core.Common;
using Cell.Core.Execution.References;
using Cell.Core.Execution.SyntaxWalkers.CellReferences;
using Cell.Core.Execution.SyntaxWalkers.UserCollections;
using Cell.Model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;
using System.Text.Json;

namespace Cell.Core.Execution.Functions
{
    /// <summary>
    /// Encapsulates a <see cref="CellFunctionModel"/> and gives it the ability to compile and run.
    /// </summary>
    public partial class CellFunction : PropertyChangedBase
    {
        private const string codeFooter = "\n}}}";
        private const string codeHeader = "\n\nnamespace Plugin { public class Program { public static ";
        private const string CompiledMethodName = "M";
        private const string methodHeader = $" {CompiledMethodName}(IContext c, CellModel cell) {{\n";
        private readonly static RoslynCompiler _compiler = new();
        /// <summary>
        /// A null function that can be used as a placeholder.
        /// </summary>
        public static readonly CellFunction Null = new(CellFunctionModel.Null, Logger.Null);
        /// <summary>
        /// The namespaces that are available to all functions preformatted for use in code.
        /// </summary>
        public readonly static List<string> UsingNamespaces =
        [
            "System",
            "System.Linq",
            "System.Collections",
            "System.Collections.Generic",
            "Cell.Model",
            "Cell.Model.Plugin",
            "Cell.ViewModel",
            "Cell.Core.Execution.Functions",
            "Cell.ViewModel.Cells.Types",
        ];
        /// <summary>
        /// The namespaces that are available to all functions preformatted for use in code.
        /// </summary>
        public static readonly string UsingNamespacesString = string.Join('\n', UsingNamespaces.Select(x => $"using {x};"));
        private MethodInfo? _compiledMethod;
        private ulong _fingerprintOfProcessedDependencies;
        private ulong _fingerprintOfCompiledMethod;
        private bool _wasCompileSuccessful;
        /// <summary>
        /// Creates a new instance of <see cref="CellFunction"/>
        /// </summary>
        /// <param name="model">The function model.</param>
        /// <param name="logger">The logger to log messages to.</param>
        public CellFunction(CellFunctionModel model, Logger logger)
        {
            CompileResult = new CompileResult { ExecutionResult ="Not compiled"};
            _logger = logger;
            Model = model;
            Model.PropertyChanged += ModelPropertyChanged;
            ExtractDependencies();
        }

        /// <summary>
        /// Occurs when the dependencies of the function have changed and anyone using the function needs to recompute thier dependencies.
        /// </summary>
        public event Action<CellFunction>? DependenciesChanged;

        /// <summary>
        /// Gets a list of all of the collection this function depends on.
        /// </summary>
        public List<ICollectionReference> CollectionDependencies { get; set; } = [];

        /// <summary>
        /// Gets the result of the last compile.
        /// </summary>
        public CompileResult CompileResult { get; private set; }

        /// <summary>
        /// Gets a list of everything this function references.
        /// </summary>
        public IEnumerable<IReferenceFromCell> Dependencies => ((IEnumerable<IReferenceFromCell>)LocationDependencies).Concat(CollectionDependencies);

        /// <summary>
        /// Gets a list of all of the location references function has.
        /// </summary>
        public List<LocationReference> LocationDependencies { get; set; } = [];

        private readonly Logger _logger;

        /// <summary>
        /// The persisted model for this function.
        /// </summary>
        public CellFunctionModel Model { get; set; }

        /// <summary>
        /// The syntax tree of the code for this function.
        /// </summary>
        public SyntaxTree SyntaxTree { get; set; } = CSharpSyntaxTree.ParseText("");

        private string FullCode => UsingNamespacesString + codeHeader + Model.ReturnType + methodHeader + Model.Code + codeFooter;

        private bool WasCompileSuccessful
        {
            get => _wasCompileSuccessful; set
            {
                if (_wasCompileSuccessful == value) return;
                _wasCompileSuccessful = value;
                NotifyPropertyChanged(nameof(WasCompileSuccessful));
            }
        }

        /// <summary>
        /// Gets the user friendly code for this function, which uses collection names like "myCollection." for collections and symbols like "A1" for cells.
        /// </summary>
        /// <param name="cell">The cell to resolve references for to make the code user friendly.</param>
        /// <param name="collectionNameToDataTypeMap">The map of collections to thier types.</param>
        /// <returns></returns>
        public string GetUserFriendlyCode(CellModel? cell, IReadOnlyDictionary<string, string> collectionNameToDataTypeMap)
        {
            var intermediateCode = new CollectionReferenceSyntaxTransformer(collectionNameToDataTypeMap).TransformTo(Model.Code);
            if (cell != null) intermediateCode = new CellLocationReferenceSyntaxTransformer(cell.Location).TransformTo(intermediateCode);
            return intermediateCode.Replace("_Range_", "..");
        }

        /// <summary>
        /// Runs the function with the given context and cell.
        /// </summary>
        /// <param name="pluginContext">The context to give to this function.</param>
        /// <returns>The result of the function.</returns>
        public CompileResult Run(IContext pluginContext)
        {
            Compile();
            if (!CompileResult.WasSuccess) return CompileResult;
            try
            {
                return RunUnsafe(pluginContext, _compiledMethod);
            }
            catch (Exception e)
            {
                return new CompileResult { WasSuccess = false, ExecutionResult = "Function errored during execution: " + e.Message };
            }
        }

        /// <summary>
        /// Sets the code of this function from a user friendly code string, which uses collection names like "myCollection." for collections and symbols like "A1" for cells.
        /// </summary>
        /// <param name="userFriendlyCode">The user friendly code.</param>
        /// <param name="cell">The cell the user is editing this code from.</param>
        /// <param name="collectionNameToDataTypeMap">The list of all valid collections and thier items data types.</param>
        public void SetUserFriendlyCode(string userFriendlyCode, CellModel? cell, IReadOnlyDictionary<string, string> collectionNameToDataTypeMap)
        {
            var intermediateCode = userFriendlyCode.Replace("..", "_Range_").Replace("\t", "    ");
            if (cell != null) intermediateCode = new CellLocationReferenceSyntaxTransformer(cell.Location).TransformFrom(intermediateCode);
            Model.Code = new CollectionReferenceSyntaxTransformer(collectionNameToDataTypeMap).TransformFrom(intermediateCode);
        }

        private void AttemptToRecompileMethod()
        {
            _compiledMethod = null;
            ExtractDependencies();
            if (_fingerprintOfProcessedDependencies == Model.Fingerprint) Compile();
        }

        /// <summary>
        /// Compiles the function if needed.
        /// </summary>
        public void Compile()
        {
            if (_fingerprintOfCompiledMethod != Model.Fingerprint)
            {
                try
                {
                    var compiled = _compiler.Compile(SyntaxTree) ?? throw new Exception("Error during compile - compiled object is null");
                    _compiledMethod = compiled.GetMethod(CompiledMethodName) ?? throw new Exception("Error during compile - compiled object is null");
                    CompileResult = new CompileResult { WasSuccess = true, ExecutionResult = "" };
                    WasCompileSuccessful = true;
                    _fingerprintOfCompiledMethod = Model.Fingerprint;
                }
                catch (Exception e)
                {
                    CompileResult = new CompileResult { WasSuccess = false, ExecutionResult = e.Message };
                    WasCompileSuccessful = false;
                }
                NotifyPropertyChanged(nameof(CompileResult));
            }
        }

        private void ExtractCellLocationReferences(SyntaxNode? root)
        {
            LocationDependencies.Clear();
            var walker = new CellReferenceSyntaxWalker();
            walker.Visit(root);
            var foundDependencies = walker.LocationReferences;
            LocationDependencies.AddRange(foundDependencies);
        }

        private void ExtractCollectionReferences(SyntaxNode? root)
        {
            CollectionDependencies.Clear();
            var walker = new CollectionReferenceSyntaxWalker();
            walker.Visit(root);
            var foundCollectionReferences = walker.CollectionReferences;
            CollectionDependencies.AddRange(foundCollectionReferences.Distinct());
        }

        private void ExtractDependencies()
        {
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(FullCode);
            SyntaxNode? root = syntaxTree.GetRoot();
            try
            {
                ExtractCellLocationReferences(root);
                ExtractCollectionReferences(root);
            }
            catch (CellError e)
            {
                _logger.Log($"Error in {nameof(ExtractDependencies)}: {e.Message}");
            }
            catch (InvalidCastException e)
            {
                _logger.Log($"Error in {nameof(ExtractDependencies)}: {e.Message}");
            }
            SyntaxTree = root.SyntaxTree;
            _fingerprintOfProcessedDependencies = Model.Fingerprint;
            DependenciesChanged?.Invoke(this);
        }

        private void ModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CellFunctionModel.Code))
            {
                AttemptToRecompileMethod();
            }
        }

        private CompileResult RunUnsafe(IContext pluginContext, MethodInfo? method) => new()
        {
            WasSuccess = true,
            ExecutionResult = "Success",
            ReturnedObject = method?.Invoke(null, [pluginContext, pluginContext.ContextCell])
        };

        internal void Log(string messege) => _logger.Log(messege);
    }
}
