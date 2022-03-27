using System.Collections.Immutable;
using Kehlet.AutoInterface.SourceGenerators.Extensions;
using Kehlet.AutoInterface.SourceGenerators.Visitors;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Kehlet.AutoInterface.SourceGenerators;

public partial class InterfaceGenerator
{
    internal class Parser
    {
        private readonly Compilation _compilation;
        private readonly CancellationToken _cancellationToken;

        public Parser(Compilation compilation, CancellationToken cancellationToken)
        {
            _compilation = compilation;
            _cancellationToken = cancellationToken;
        }

        public ImmutableArray<TargetType> GetTargetTypes(ImmutableArray<TypeDeclarationSyntax> classes)
        {
            var results = ImmutableArray.CreateBuilder<TargetType>(classes.Length);

            foreach (var group in classes.GroupBy(x => x.SyntaxTree))
            {
                SemanticModel? sm = null;
                foreach (var typeDeclarationSyntax in group)
                {
                    _cancellationToken.ThrowIfCancellationRequested();

                    sm ??= _compilation.GetSemanticModel(typeDeclarationSyntax.SyntaxTree);

                    var symbol = sm.GetDeclaredSymbol(typeDeclarationSyntax);
                    if (symbol is null)
                    {
                        continue;
                    }

                    var attribute = symbol.GetAttributes(FullDefaultImplementationAttributeName).FirstOrDefault();
                    if (attribute is null)
                    {
                        continue;
                    }

                    var interfaceAccessibility = attribute.ConstructorArguments is { IsDefaultOrEmpty: false } args && args[0].Value is 1
                                                     ? Accessibility.Internal
                                                     : Accessibility.Public;


                    var visitor = new TypeVisitor(sm, interfaceAccessibility);
                    typeDeclarationSyntax.Accept(visitor);

                    results.Add(visitor.Builder.Build());
                }
            }

            return results.ToImmutableArray();
        }

        public static bool IsSyntaxTarget(SyntaxNode node, CancellationToken _)
        {
            return node is ClassDeclarationSyntax { AttributeLists.Count: > 0 } or StructDeclarationSyntax { AttributeLists.Count: > 0 };
        }

        public static TypeDeclarationSyntax? GetSemanticTarget(GeneratorSyntaxContext context, CancellationToken _)
        {
            var node = (TypeDeclarationSyntax) context.Node;

            foreach (var attributeSyntax in node.AttributeLists.SelectMany(x => x.Attributes))
            {
                var symbol = ModelExtensions.GetSymbolInfo(context.SemanticModel, attributeSyntax).Symbol;
                if (symbol is not IMethodSymbol attributeConstructor)
                {
                    continue;
                }

                var fullName = attributeConstructor.ContainingType.ToDisplayString();
                if (fullName == FullDefaultImplementationAttributeName)
                {
                    return node;
                }
            }

            return null;
        }
    }

    internal class TargetType
    {
        public string Namespace { get; init; } = null!;
        public Accessibility Accessibility { get; init; }
        public TypeKind Kind { get; init; }
        public bool IsPublic { get; init; }
        public string Name { get; init; } = null!;
        public ImmutableArray<TypeParameter> TypeParameters { get; init; }
        public ImmutableArray<Member> Members { get; init; }
        public string? Docs { get; init; }
    }

    internal class Member
    {
        public bool ReturnsByRef { get; init; }
        public bool ReturnsByRefReadonly { get; init; }
        public string ReturnType { get; init; } = null!;
        public string Name { get; init; } = null!;
        public ImmutableArray<TypeParameter> TypeParameters { get; init; }
        public ImmutableArray<Parameter> Parameters { get; init; }
        public ImmutableArray<string> Accessors { get; init; }
        public string? Docs { get; init; }
        public MemberType Type { get; init; }
    }

    internal enum MemberType
    {
        None,
        Indexer,
        Property,
        Method
    }

    internal class Parameter
    {
        public string Name { get; init; } = null!;
        public string Type { get; init; } = null!;
        public string? DefaultValue { get; init; }

        public override string ToString()
        {
            return DefaultValue is { }
                       ? $"{Type} {Name} {DefaultValue}"
                       : $"{Type} {Name}";
        }
    }

    internal class TypeParameter
    {
        public string Name { get; init; } = null!;
        public Variance Variance { get; init; }

        public override string ToString()
        {
            return Variance switch
            {
                Variance.Covariant => $"out {Name}",
                Variance.Contravariant => $"in {Name}",
                _ => Name
            };
        }
    }

    internal enum Variance
    {
        None,
        Covariant,
        Contravariant
    }
}
