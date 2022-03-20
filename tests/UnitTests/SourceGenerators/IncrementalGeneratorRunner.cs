using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Kehlet.AutoInterface.UnitTests.SourceGenerators;

public class IncrementalGeneratorRunner
{
    public IncrementalGeneratorRunner(string source, params IIncrementalGenerator[] generators)
    {
        Input = CreateCompilation(source);
        Driver = CreateDriver(Input, generators);
    }

    public Compilation Input { get; }
    public GeneratorDriver Driver { get; }

    public GeneratorDriverRunResult Run(out Compilation output, out ImmutableArray<Diagnostic> diagnostics)
    {
        return Driver.RunGeneratorsAndUpdateCompilation(Input, out output, out diagnostics).GetRunResult();
    }

    private static Compilation CreateCompilation(string source) => CSharpCompilation.Create(
        assemblyName: "compilation",
        syntaxTrees: new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest)) },
        references: new[] { MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location) },
        options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
    );

    private static GeneratorDriver CreateDriver(Compilation compilation, params IIncrementalGenerator[] generators) =>
        CSharpGeneratorDriver.Create(generators).WithUpdatedParseOptions(compilation.SyntaxTrees.First().Options);
}
