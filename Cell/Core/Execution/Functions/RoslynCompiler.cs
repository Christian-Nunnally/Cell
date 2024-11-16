using Cell.Core.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace Cell.Core.Execution.Functions
{
    /// <summary>
    /// A compiler that uses Roslyn to compile C# code into executable types.
    /// </summary>
    public class RoslynCompiler
    {
        private static readonly CSharpCompilationOptions _compilationOptions = new(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true, optimizationLevel: OptimizationLevel.Release);
        private readonly string _typeName = "Plugin.Program";
        private static List<PortableExecutableReference>? _portableExecutableReferences;
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

        /// <summary>
        /// Compiles the syntax tree into an assembly and returns the type with the name "Plugin.Program".
        /// </summary>
        /// <param name="syntax">The syntax tree to compile.</param>
        /// <returns>The compiled type.</returns>
        /// <exception cref="Exception">If the compilation fails.</exception>
        public Type Compile(SyntaxTree syntax)
        {
            using var memoryStream = new MemoryStream();
            var compilation = CSharpCompilation.Create(Guid.NewGuid().ToString(), [syntax], PortableExecutableReferences, _compilationOptions);
            var result = compilation.Emit(memoryStream);
            CheckForErrors(result);
            memoryStream.Seek(0, SeekOrigin.Begin);

            var generatedAssembly = AssemblyLoadContext.Default.LoadFromStream(memoryStream) ?? throw new Exception("Could not load generated assembly");
            return generatedAssembly.GetType(_typeName) ?? throw new Exception($"Unable to get type '{_typeName}' from assembly");
        }

        private static void CheckForErrors(EmitResult result)
        {
            static bool IsConsideredError(Diagnostic d) => d.IsWarningAsError || d.Severity == DiagnosticSeverity.Error;

            if (result.Success) return;
            var compilationErrors = result.Diagnostics.Where(IsConsideredError).ToList();
            if (compilationErrors.Count == 0) return;
            var firstError = compilationErrors.First();
            var errorNumber = firstError.Id;
            var errorDescription = firstError.GetMessage();
            var firstErrorMessage = $"{errorNumber}: {errorDescription};";
            var exception = new Exception($"Compilation failed, first error is: {firstErrorMessage}");
            compilationErrors.ForEach(e => { if (!exception.Data.Contains(e.Id)) exception.Data.Add(e.Id, e.GetMessage()); });
            throw exception;
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
