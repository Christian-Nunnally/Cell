using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace Cell.Plugin
{
    public class RoslynCompiler
    {
        readonly CSharpCompilation _compilation;
        Assembly _generatedAssembly;
        Type? _proxyType;
        string _assemblyName;
        string _typeName;

        public RoslynCompiler(string typeName, string code, Type[] typesToReference)
        {
            _typeName = typeName;
            var refs = typesToReference.Select(h => MetadataReference.CreateFromFile(h.Assembly.Location) as MetadataReference).ToList();

            //some default refeerences
            refs.Add(MetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Runtime.dll")));
            refs.Add(MetadataReference.CreateFromFile(typeof(Object).Assembly.Location));
            refs.Add(MetadataReference.CreateFromFile(typeof(PluginContext).Assembly.Location));
            refs.Add(MetadataReference.CreateFromFile(typeof(CellTextProvider).Assembly.Location));

            //generate syntax tree from code and config compilation options
            var syntax = CSharpSyntaxTree.ParseText(code);
            var options = new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                allowUnsafe: true,
                optimizationLevel: OptimizationLevel.Release);

            _compilation = CSharpCompilation.Create(_assemblyName = Guid.NewGuid().ToString(), new List<SyntaxTree> { syntax }, refs, options);
        }

        public RoslynCompiler(string typeName, SyntaxTree syntax, Type[] typesToReference)
        {
            _typeName = typeName;
            var refs = typesToReference.Select(h => MetadataReference.CreateFromFile(h.Assembly.Location) as MetadataReference).ToList();

            //some default refeerences
            refs.Add(MetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Runtime.dll")));
            refs.Add(MetadataReference.CreateFromFile(typeof(Object).Assembly.Location));
            refs.Add(MetadataReference.CreateFromFile(typeof(PluginContext).Assembly.Location));
            refs.Add(MetadataReference.CreateFromFile(typeof(CellTextProvider).Assembly.Location));

            //generate syntax tree from code and config compilation options
            var options = new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                allowUnsafe: true,
                optimizationLevel: OptimizationLevel.Release);

            _compilation = CSharpCompilation.Create(_assemblyName = Guid.NewGuid().ToString(), new List<SyntaxTree> { syntax }, refs, options);
        }

        public Type Compile()
        {

            if (_proxyType != null) return _proxyType;

            using (var ms = new MemoryStream())
            {
                var result = _compilation.Emit(ms);
                if (!result.Success)
                {
                    var compilationErrors = result.Diagnostics.Where(diagnostic =>
                            diagnostic.IsWarningAsError ||
                            diagnostic.Severity == DiagnosticSeverity.Error)
                        .ToList();
                    if (compilationErrors.Any())
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

                _generatedAssembly = AssemblyLoadContext.Default.LoadFromStream(ms);

                _proxyType = _generatedAssembly.GetType(_typeName);
                return _proxyType;
            }
        }
    }
}
