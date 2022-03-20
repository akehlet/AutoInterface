using System.Linq;
using Kehlet.AutoInterface.SourceGenerators;
using Xunit;

namespace Kehlet.AutoInterface.UnitTests.SourceGenerators;

public class Attributes
{
    [Fact]
    public void GenerateAttributesWhenEmptySource()
    {
        const string source = "";

        const string expected = @"#nullable enable

using System;

namespace Kehlet.AutoInterface.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
internal class DefaultImplementationAttribute : Attribute
{
    public DefaultImplementationAttribute(Accessibility accessibility = Accessibility.Public)
    {
        Accessibility = accessibility;
    }

    public Accessibility Accessibility { get; }
}

internal enum Accessibility
{
    Public,
    Internal
}

[AttributeUsage(AttributeTargets.GenericParameter)]
internal class InAttribute : Attribute { }

[AttributeUsage(AttributeTargets.GenericParameter)]
internal class OutAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
internal class ExcludeAttribute : Attribute { }
";

        var runnable = new IncrementalGeneratorRunner(source, new InterfaceGenerator());
        var result = runnable.Run(out var output, out var diagnostics);

        Assert.Empty(diagnostics);
        Assert.Empty(result.Diagnostics);
        Assert.Equal(expected: 2, output.SyntaxTrees.Count());
        Assert.Single(result.GeneratedTrees);
        Assert.Equal(expected, result.GeneratedTrees[0].ToString());
    }
}
