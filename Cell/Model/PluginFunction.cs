using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.Text.Json.Serialization;
using Cell.Plugin;
using System.Reflection;
using Cell.Plugin.SyntaxRewriters;
using Cell.Exceptions;

namespace Cell.Model
{
    public partial class PluginFunction : PropertyChangedBase
    {
        private const string codeHeader = "using System; using System.Linq; using System.Collections.Generic; using Cell.Model; using Cell.ViewModel; using Cell.Model.Plugin; using Cell.Plugin;\n\nnamespace Plugin { public class Program { public static ";
        private const string codeFooter = "\n}}}";
        private const string methodHeader = " PluginMethod(PluginContext c, CellModel cell) {\n";

        private string code = string.Empty;
        private readonly List<CellModel> _cellsToNotify = [];
        private bool _isSyntaxTreeValid;
        private MethodInfo? _compiledMethod;

        [JsonIgnore]
        public bool IsSyntaxTreeValid => _isSyntaxTreeValid;

        public List<CellLocation> LocationDependencies { get; set; } = [];

        public List<string> CollectionDependencies { get; set; } = [];

        public string Name { get; set; } = string.Empty;

        public string ReturnType { get; set; } = "void";

        [JsonIgnore]
        public MethodInfo? CompiledMethod => _compiledMethod ?? Compile();

        [JsonIgnore]
        public CompileResult CompileResult { get; private set; }


        [JsonIgnore]
        public SyntaxTree SyntaxTree { get; set; } = CSharpSyntaxTree.ParseText("");

        private string FullCode => codeHeader + ReturnType + methodHeader + code + codeFooter;
        
        public PluginFunction() { }

        public PluginFunction(string name, string code, string returnType)
        {
            Name = name;
            ReturnType = returnType;
            Code = code;
        }

        public string Code
        {
            get => code; 
            set
            {
                if (code == value) return;
                code = value;
                NotifyPropertyChanged(nameof(Code));
                _compiledMethod = null;
                ExtractAndTransformDependencies();
                if (_isSyntaxTreeValid) Compile();
            }
        }

        public void ExtractAndTransformDependencies()
        {
            _isSyntaxTreeValid = false;
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(FullCode);
            
            SyntaxNode? root = syntaxTree.GetRoot();
            try
            {
                root = ExtractAndTransformCellLocationReferences(root);
                root = ExtractAndTransformCollectionReferences(root);
            }
            catch (PluginFunctionSyntaxTransformException)
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
            if (!resultStatus.Success) throw new PluginFunctionSyntaxTransformException(resultStatus.Result);
            var foundDependencies = collectionReferenceSyntaxRewriter.CollectionReferences;
            CollectionDependencies.AddRange(foundDependencies);
            return root;
        }

        private SyntaxNode ExtractAndTransformCellLocationReferences(SyntaxNode? root)
        {
            LocationDependencies.Clear();
            var cellLocationSyntaxRewriter = new FindAndReplaceCellLocationsSyntaxRewriter();
            root = cellLocationSyntaxRewriter.Visit(root) ?? throw new Exception("Syntax root should not be null after rewrite.");
            SyntaxTree = root.SyntaxTree;
            var foundDependencies = cellLocationSyntaxRewriter.LocationReferences;
            LocationDependencies.AddRange(foundDependencies);
            return root;
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
