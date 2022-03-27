using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Kehlet.AutoInterface.SourceGenerators.Visitors;

internal sealed class MemberVisitor : CSharpSyntaxVisitor
{
    private readonly SemanticModel _semanticModel;
    public MemberBuilder Builder { get; } = new();

    public MemberVisitor(SemanticModel semanticModel)
    {
        _semanticModel = semanticModel;
    }

    public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        var symbol = _semanticModel.GetDeclaredSymbol(node)!;

        Builder.Type = InterfaceGenerator.MemberType.Method;
        Builder.ReturnsByRef = symbol.ReturnsByRef;
        Builder.ReturnsByRefReadonly = symbol.ReturnsByRefReadonly;
        Builder.ReturnType = symbol.ReturnType.ToDisplayString();
        Builder.Name = symbol.Name;
        Builder.Docs = symbol.GetDocumentationCommentXml();

        Visit(node.TypeParameterList);
        Visit(node.ParameterList);
    }

    public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
    {
        var symbol = _semanticModel.GetDeclaredSymbol(node)!;

        Builder.Type = InterfaceGenerator.MemberType.Property;
        Builder.ReturnsByRef = symbol.ReturnsByRef;
        Builder.ReturnsByRefReadonly = symbol.ReturnsByRefReadonly;
        Builder.ReturnType = symbol.Type.ToDisplayString();
        Builder.Name = symbol.Name;
        Builder.Docs = symbol.GetDocumentationCommentXml();

        if (symbol.GetMethod is not null)
        {
            Builder.Accessors.Add("get");
        }

        if (symbol.SetMethod is not null)
        {
            Builder.Accessors.Add(symbol.SetMethod.IsInitOnly ? "init" : "set");
        }
    }

    public override void VisitIndexerDeclaration(IndexerDeclarationSyntax node)
    {
        var symbol = _semanticModel.GetDeclaredSymbol(node)!;

        Builder.Type = InterfaceGenerator.MemberType.Indexer;
        Builder.ReturnsByRef = symbol.ReturnsByRef;
        Builder.ReturnsByRefReadonly = symbol.ReturnsByRefReadonly;
        Builder.ReturnType = symbol.Type.ToDisplayString();
        Builder.Name = symbol.Name;
        Builder.Docs = symbol.GetDocumentationCommentXml();

        Visit(node.ParameterList);

        if (symbol.GetMethod is not null)
        {
            Builder.Accessors.Add("get");
        }

        if (symbol.SetMethod is not null)
        {
            Builder.Accessors.Add("set");
        }
    }

    public override void VisitTypeParameterList(TypeParameterListSyntax node)
    {
        foreach (var parameter in node.Parameters)
        {
            VisitTypeParameter(parameter);
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

    public override void VisitParameterList(ParameterListSyntax node)
    {
        foreach (var parameter in node.Parameters)
        {
            VisitParameter(parameter);
        }
    }

    public override void VisitBracketedParameterList(BracketedParameterListSyntax node)
    {
        foreach (var parameter in node.Parameters)
        {
            VisitParameter(parameter);
        }
    }

    public override void VisitParameter(ParameterSyntax node)
    {
        var symbol = _semanticModel.GetDeclaredSymbol(node);
        if (symbol is null)
        {
            return;
        }

        Builder.Parameters.Add(new InterfaceGenerator.Parameter
        {
            Name = node.Identifier.ToString(),
            Type = symbol.Type.ToDisplayString(),
            DefaultValue = node.Default?.ToString()
        });
    }
}

internal sealed class MemberBuilder
{
    public InterfaceGenerator.MemberType Type { get; set; }
    public bool ReturnsByRef { get; set; }
    public bool ReturnsByRefReadonly { get; set; }
    public string ReturnType { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Docs { get; set; }
    public List<InterfaceGenerator.TypeParameter> TypeParameters { get; } = new();
    public List<InterfaceGenerator.Parameter> Parameters { get; } = new();
    public List<string> Accessors { get; } = new();

    public InterfaceGenerator.Member Build()
    {
        return new InterfaceGenerator.Member
        {
            ReturnsByRef = ReturnsByRef,
            ReturnsByRefReadonly = ReturnsByRefReadonly,
            ReturnType = ReturnType,
            Name = Name,
            TypeParameters = TypeParameters.ToImmutableArray(),
            Parameters = Parameters.ToImmutableArray(),
            Accessors = Accessors.ToImmutableArray(),
            Docs = Docs,
            Type = Type,
        };
    }
}
