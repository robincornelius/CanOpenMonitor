using System;
using System.IO;
using System.Linq;
using System.Reflection;
#if NET8_0_OR_GREATER
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
#else
using Microsoft.CSharp;
using System.CodeDom.Compiler;
#endif

namespace CanMonitor
{
    internal static class SourcePluginCompiler
    {
        internal static Assembly Compile(string source)
        {
#if NET8_0_OR_GREATER
            // Use the running runtime's reference set, including WinForms. Explicitly add
            // the host contracts so source plugins share the host's type identities.
            string trusted = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES");
            string[] paths = trusted.Split(Path.PathSeparator).Concat(new[]
            {
                typeof(PDOInterface.IInterfaceService).Assembly.Location,
                typeof(libCanopenSimple.libCanopenSimple).Assembly.Location,
                typeof(WeifenLuo.WinFormsUI.Docking.DockContent).Assembly.Location
            }).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
            var compilation = CSharpCompilation.Create("CanMonitorPlugin_" + Guid.NewGuid().ToString("N"),
                new[] { CSharpSyntaxTree.ParseText(source) },
                paths.Select(path => MetadataReference.CreateFromFile(path)),
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            using (var output = new MemoryStream())
            {
                var result = compilation.Emit(output);
                if (!result.Success)
                {
                    throw new InvalidOperationException(string.Join(Environment.NewLine,
                        result.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)));
                }
                return Assembly.Load(output.ToArray());
            }
#else
            using (var provider = new CSharpCodeProvider())
            {
                var parameters = new CompilerParameters
                {
                    GenerateInMemory = true,
                    GenerateExecutable = false
                };
                parameters.ReferencedAssemblies.AddRange(new[]
                {
                    "System.dll", "System.Core.dll", "System.Data.dll", "System.Windows.Forms.dll",
                    "System.Drawing.dll", typeof(PDOInterface.IInterfaceService).Assembly.Location,
                    typeof(libCanopenSimple.libCanopenSimple).Assembly.Location,
                    typeof(WeifenLuo.WinFormsUI.Docking.DockContent).Assembly.Location
                });
                CompilerResults result = provider.CompileAssemblyFromSource(parameters, source);
                if (result.Errors.HasErrors)
                {
                    throw new InvalidOperationException(string.Join(Environment.NewLine,
                        result.Errors.Cast<CompilerError>().Select(error => error.ToString())));
                }
                return result.CompiledAssembly;
            }
#endif
        }
    }
}
