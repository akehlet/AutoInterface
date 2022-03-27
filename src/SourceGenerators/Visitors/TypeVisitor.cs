using System.Collections.Immutable;
using Kehlet.AutoInterface.SourceGenerators.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Kehlet.AutoInterface.SourceGenerators.Visitors;

internal sealed class TypeVisitor : CSharpSyntaxVisitor
{
    private readonly SemanticModel _semanticModel;
    private readonly Accessibility _interfaceAccessibility;
    public TypeBuilder Builder { get; } = new();

    public TypeVisitor(SemanticModel semanticModel, Accessibility interfaceAccessibility)
    {
        _semanticModel = semanticModel;
        _interfaceAccessibility = interfaceAccessibility;
    }

    public override void VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        VisitTypeDeclaration(node);
    }

    public override void VisitStructDeclaration(StructDeclarationSyntax node)
    {
        VisitTypeDeclaration(node);
    }

    private void VisitTypeDeclaration(TypeDeclarationSyntax node)
    {
        var symbol = _semanticModel.GetDeclaredSymbol(node);
        if (symbol is null)
        {
            return;
        }

        Builder.InterfaceAccessibility = _interfaceAccessibility;
        Builder.Namespace = node.GetNamespace();
        Builder.TypeAccessibility = symbol.DeclaredAccessibility;
        Builder.Kind = symbol.TypeKind;
        Builder.Name = symbol.Name;
        Builder.Docs = symbol.GetDocumentationCommentXml();

        if (node.Arity > 0)
        {
            Visit(node.TypeParameterList);

            foreach (var constraintClause in node.ConstraintClauses)
            {
                Visit(constraintClause);
            }
        }

        foreach (var member in node.Members)
        {
            Visit(member);
        }
    }

    public override void VisitTypeParameterList(TypeParameterListSyntax node)
    {
        foreach (var parameter in node.Parameters)
        {
            Visit(parameter);
        }
    }

    public override void VisitTypeParameter(TypeParameterSyntax node)
    {
        Builder.TypeParameters.Add(new InterfaceGenerator.TypeParameter
        {
            Name = node.Identifier.ToString(),
            Variance = InterfaceGenerator.Variance.None
        });
    }

    public override void VisitTypeParameterConstraintClause(TypeParameterConstraintClauseSyntax node)
    {

    }

    public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        VisitMember(node);
    }

    public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
    {
        VisitMember(node);
    }

    public override void VisitIndexerDeclaration(IndexerDeclarationSyntax node)
    {
        VisitMember(node);
    }

    private void VisitMember(MemberDeclarationSyntax node)
    {
        var symbol = _semanticModel.GetDeclaredSymbol(node);
        if (symbol is null)
        {
            return;
        }

        if (symbol.DeclaredAccessibility != Accessibility.Public)
        {
            return;
        }

        var visitor = new MemberVisitor(_semanticModel);
        node.Accept(visitor);

        Builder.Members.Add(visitor.Builder.Build());
    }
}

internal sealed class TypeBuilder
{
    public Accessibility InterfaceAccessibility { get; set; }
    public string Namespace { get; set; } = "";
    public Accessibility TypeAccessibility { get; set; }
    public TypeKind Kind { get; set; }
    public string Name { get; set; } = "";
    public List<InterfaceGenerator.TypeParameter> TypeParameters { get; } = new();
    public string? Docs { get; set; }
    public List<InterfaceGenerator.Member> Members { get; } = new();

    public InterfaceGenerator.TargetType Build()
    {
        return new InterfaceGenerator.TargetType
        {
            Namespace = Namespace,
            Accessibility = TypeAccessibility,
            Kind = Kind,
            IsPublic = InterfaceAccessibility is Accessibility.Public,
            Name = Name,
            TypeParameters = TypeParameters.ToImmutableArray(),
            Members = Members.ToImmutableArray(),
            Docs = Docs,
        };
    }
}
