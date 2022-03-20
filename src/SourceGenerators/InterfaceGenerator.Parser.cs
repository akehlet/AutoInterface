using System.Collections.Immutable;
using AutoInterface.Extensions;
using Microsoft.CodeAnalysis;
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

                    var symbol = sm.GetDeclaredSymbol(typeDeclarationSyntax) as INamedTypeSymbol;
                    if (symbol is null)
                    {
                        continue;
                    }

                    var attribute = symbol.GetAttributes(FullDefaultImplementationAttributeName).FirstOrDefault();
                    if (attribute is null)
                    {
                        continue;
                    }

                    var isPublic = !(attribute.ConstructorArguments is { IsDefaultOrEmpty: false } args && args[0].Value is 1);

                    var @class = new TargetType
                    {
                        Namespace = typeDeclarationSyntax.GetNamespace(),
                        Accessibility = symbol.DeclaredAccessibility,
                        Kind = symbol.TypeKind,
                        IsPublic = isPublic,
                        Name = symbol.Name,
                        TypeParameters = GetTypeParameters(symbol.TypeParameters, allowVariance: true),
                        Members = GetPublicMembers(sm, typeDeclarationSyntax),
                        Docs = symbol.GetDocumentationCommentXml()
                    };

                    results.Add(@class);
                }
            }

            return results.ToImmutableArray();
        }

        private ImmutableArray<Member> GetPublicMembers(SemanticModel sm, TypeDeclarationSyntax typeDeclarationSyntax)
        {
            // Stryker disable all : Early return for performance.
            if (typeDeclarationSyntax.Members.Count == 0)
            {
                return ImmutableArray<Member>.Empty;
            }
            // Stryker restore all

            var members = ImmutableArray.CreateBuilder<Member>(typeDeclarationSyntax.Members.Count);

            foreach (var member in typeDeclarationSyntax.Members)
            {
                _cancellationToken.ThrowIfCancellationRequested();

                // Can't get symbol for events with an identifier ??
                var memberSymbol = sm.GetDeclaredSymbol(member);
                if (memberSymbol?.DeclaredAccessibility is not Accessibility.Public)
                {
                    continue;
                }

                var attribute = memberSymbol.GetAttributes(FullExcludeAttributeName).FirstOrDefault();
                if (attribute is not null)
                {
                    continue;
                }

                switch (memberSymbol)
                {
                    case IPropertySymbol property:
                        members.Add(new Member
                        {
                            Type = property.IsIndexer ? MemberType.Indexer : MemberType.Property,
                            ReturnsByRef = property.ReturnsByRef,
                            ReturnsByRefReadonly = property.ReturnsByRefReadonly,
                            ReturnType = property.Type.ToDisplayString(),
                            Name = property.Name,
                            Parameters = GetParameters(member),
                            Accessors = GetAccessors(property),
                            Docs = property.GetDocumentationCommentXml()
                        });
                        break;
                    case IMethodSymbol { MethodKind: MethodKind.Ordinary } method:
                        members.Add(new Member
                        {
                            Type = MemberType.Method,
                            ReturnsByRef = method.ReturnsByRef,
                            ReturnsByRefReadonly = method.ReturnsByRefReadonly,
                            ReturnType = method.ReturnType.ToDisplayString(),
                            Name = method.Name,
                            TypeParameters = GetTypeParameters(method.TypeParameters),
                            Parameters = GetParameters(member),
                            Docs = method.GetDocumentationCommentXml()
                        });
                        break;
                    case IEventSymbol eventSymbol:
                        members.Add(new Member
                        {
                            Type = MemberType.Event,
                            ReturnType = eventSymbol.Type.ToDisplayString(),
                            Name = eventSymbol.Name,
                            Docs = eventSymbol.GetDocumentationCommentXml()
                        });
                        break;
                }
            }

            return members.ToImmutableArray();
        }

        private static ImmutableArray<TypeParameter> GetTypeParameters(ImmutableArray<ITypeParameterSymbol> typeParameters, bool allowVariance = false)
        {
            // Stryker disable all : Early return for performance.
            if (typeParameters.IsDefaultOrEmpty)
            {
                return ImmutableArray<TypeParameter>.Empty;
            }
            // Stryker restore all

            var result = ImmutableArray.CreateBuilder<TypeParameter>(typeParameters.Length);
            foreach (var typeParameter in typeParameters)
            {
                var variance = !allowVariance ? Variance.None
                               : typeParameter.GetAttributes(FullOutAttributeName).FirstOrDefault() is not null ? Variance.Covariant
                               : typeParameter.GetAttributes(FullInAttributeName).FirstOrDefault() is not null ? Variance.Contravariant
                               : Variance.None;

                result.Add(new TypeParameter
                {
                    Name = typeParameter.Name,
                    Variance = variance
                });
            }

            return result.ToImmutable();
        }

        private static ImmutableArray<Parameter> GetParameters(MemberDeclarationSyntax syntax)
        {
            var parameters = syntax switch
            {
                MethodDeclarationSyntax method => method.ParameterList.Parameters,
                IndexerDeclarationSyntax indexer => indexer.ParameterList.Parameters,
                _ => default
            };

            // Stryker disable all : Early return for performance.
            if (parameters.Count is 0)
            {
                return ImmutableArray<Parameter>.Empty;
            }
            // Stryker restore all

            var p = ImmutableArray.CreateBuilder<Parameter>(parameters.Count);
            foreach (var parameter in parameters)
            {
                if (parameter.Type is null)
                {
                    continue;
                }

                p.Add(new Parameter
                {
                    Name = parameter.Identifier.ToString(),
                    Type = parameter.Type.ToString(),
                    DefaultValue = parameter.Default?.ToString()
                });
            }

            return p.ToImmutable();
        }

        private static ImmutableArray<string> GetAccessors(IPropertySymbol property)
        {
            var accessors = ImmutableArray.CreateBuilder<string>(2);
            if (property.GetMethod is { DeclaredAccessibility: Accessibility.Public })
            {
                accessors.Add("get;");
            }

            if (property.SetMethod is { DeclaredAccessibility: Accessibility.Public } setMethod)
            {
                accessors.Add(setMethod.IsInitOnly ? "init;" : "set;");
            }

            return accessors.ToImmutableArray();
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
                var symbol = context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol;
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
        Method,
        Event
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
