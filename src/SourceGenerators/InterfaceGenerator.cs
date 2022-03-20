using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

[assembly: InternalsVisibleTo("UnitTests")]

namespace Kehlet.AutoInterface.SourceGenerators;

[Generator]
public partial class InterfaceGenerator : IIncrementalGenerator
{
    private const string AttributeNamespace = "Kehlet.AutoInterface.Attributes";
    private const string DefaultImplementationAttributeName = "DefaultImplementationAttribute";
    private const string FullDefaultImplementationAttributeName = $"{AttributeNamespace}.{DefaultImplementationAttributeName}";
    private const string DefaultImplementationAttributeFileName = $"{DefaultImplementationAttributeName}.g.cs";
    private const string InAttributeName = "InAttribute";
    private const string FullInAttributeName = $"{AttributeNamespace}.{InAttributeName}";
    private const string OutAttributeName = "OutAttribute";
    private const string FullOutAttributeName = $"{AttributeNamespace}.{OutAttributeName}";
    private const string ExcludeAttributeName = "ExcludeAttribute";
    private const string FullExcludeAttributeName = $"{AttributeNamespace}.{ExcludeAttributeName}";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(i => { i.AddSource(DefaultImplementationAttributeFileName, SourceText.From(DefaultImplementationAttribute, Encoding.UTF8)); });

        var provider = context.SyntaxProvider
                              .CreateSyntaxProvider(Parser.IsSyntaxTarget, Parser.GetSemanticTarget)
                              .Where(syntax => syntax is { });

        var compilation = context.CompilationProvider.Combine(provider.Collect());

        context.RegisterSourceOutput(compilation, static (spc, tuple) => Execute(tuple.Left, tuple.Right!, spc));
    }

    private static void Execute(Compilation compilation, ImmutableArray<TypeDeclarationSyntax> classes, SourceProductionContext context)
    {
        // Stryker disable all : Early return for performance.
        if (classes.IsDefaultOrEmpty)
        {
            return;
        }
        // Stryker restore all

        var distinct = classes.Distinct().ToImmutableArray();

        var parser = new Parser(compilation, context.CancellationToken);
        var impl = parser.GetTargetTypes(distinct);
        var emitter = new Emitter();

        foreach (var @class in impl)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var result = emitter.Emit(@class);
            var source = SourceText.From(result, Encoding.UTF8);
            context.AddSource($"{@class.Name}.g.cs", source);
        }
    }
}
