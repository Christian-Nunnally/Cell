using Cell.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace Cell.Execution
{
    public class RoslynCompiler
    {
        private static readonly CSharpCompilationOptions _compilationOptions = new(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true, optimizationLevel: OptimizationLevel.Release);
        private readonly CSharpCompilation _compilation;
        private readonly string _typeName = "Plugin.Program";
        private static List<PortableExecutableReference>? _portableExecutableReferences;
        public RoslynCompiler(SyntaxTree syntax)
        {
            _compilation = CSharpCompilation.Create(Guid.NewGuid().ToString(), [syntax], PortableExecutableReferences, _compilationOptions);
        }

        private static List<PortableExecutableReference> PortableExecutableReferences => _portableExecutableReferences ??=
        [
            GetRuntimeMetadataReference(),
            GetMetadataReferenceForType(typeof(object)),
            GetMetadataReferenceForType(typeof(Context)),
            GetMetadataReferenceForType(typeof(Console)),
            GetMetadataReferenceForType(typeof(Enumerable)),
            GetMetadataReferenceForType(typeof(List<>)),
            GetMetadataReferenceForType(typeof(System.Collections.SortedList)),
            GetCollectionsReference()
        ];

        public Type Compile()
        {
            using var ms = new MemoryStream();
            var result = _compilation.Emit(ms);
            if (!result.Success)
            {
                var compilationErrors = result.Diagnostics.Where(diagnostic =>
                        diagnostic.IsWarningAsError ||
                        diagnostic.Severity == DiagnosticSeverity.Error)
                    .ToList();
                if (compilationErrors.Count != 0)
                {
                    var firstError = compilationErrors.First();
                    var errorNumber = firstError.Id;
                    var errorDescription = firstError.GetMessage();
                    var firstErrorMessage = $"{errorNumber}: {errorDescription};";
                    var exception = new Exception($"Compilation failed, first error is: {firstErrorMessage}");
                    compilationErrors.ForEach(e => { if (!exception.Data.Contains(e.Id)) exception.Data.Add(e.Id, e.GetMessage()); });
                    throw exception;
                }
            }
            ms.Seek(0, SeekOrigin.Begin);

            var generatedAssembly = AssemblyLoadContext.Default.LoadFromStream(ms) ?? throw new Exception("Could not load generated assembly");
            return generatedAssembly.GetType(_typeName) ?? throw new Exception($"Unable to get type '{_typeName}' from assembly");
        }

        private static PortableExecutableReference GetCollectionsReference()
        {
            var assemblyLocation = typeof(Dictionary<,>).Assembly.Location;
            var directoryName = Path.GetDirectoryName(assemblyLocation) ?? throw new CellError($"Could not get directory name from {assemblyLocation}");
            var collectionsDllPath = Path.Combine(directoryName, "System.Collections.dll");
            return MetadataReference.CreateFromFile(collectionsDllPath);
        }

        private static PortableExecutableReference GetMetadataReferenceForType(Type type)
        {
            var assembly = type.Assembly;
            var location = assembly.Location;
            return MetadataReference.CreateFromFile(location);
        }

        private static PortableExecutableReference GetRuntimeMetadataReference()
        {
            var garbageCollectorType = typeof(System.Runtime.GCSettings);
            var garbageCollectorTypeInfo = garbageCollectorType.GetTypeInfo();
            var garbageCollectorAssembly = garbageCollectorTypeInfo.Assembly;
            var assemblyLocation = garbageCollectorAssembly.Location;
            var directory = Path.GetDirectoryName(assemblyLocation) ?? throw new Exception($"Could not get directory name from {assemblyLocation}");
            var systemRuntimeDllPath = Path.Combine(directory, "System.Runtime.dll");
            return MetadataReference.CreateFromFile(systemRuntimeDllPath);
        }
    }
}
