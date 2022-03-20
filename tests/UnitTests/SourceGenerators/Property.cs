using System.Linq;
using Kehlet.AutoInterface.SourceGenerators;
using Xunit;

namespace Kehlet.AutoInterface.UnitTests.SourceGenerators;

public class Property
{
    [Fact]
    public void DontGeneratePrivateProperty()
    {
        const string source = @"
using Kehlet.AutoInterface.Attributes;

[DefaultImplementation]
partial class C
{
    private int P { get; }
}
";

        const string expected = @"#nullable enable

internal partial class C : IC { }

public partial interface IC
{
}
";

        var runnable = new IncrementalGeneratorRunner(source, new InterfaceGenerator());
        var result = runnable.Run(out var output, out var diagnostics);

        Assert.Empty(diagnostics);
        Assert.Empty(result.Diagnostics);
        Assert.Equal(expected: 3, output.SyntaxTrees.Count());
        Assert.Equal(expected: 2, result.GeneratedTrees.Length);
        Assert.Equal(expected, result.GeneratedTrees[1].ToString());
    }

    [Fact]
    public void DontGeneratePropertyWithoutAccessors()
    {
        const string source = @"
using Kehlet.AutoInterface.Attributes;

[DefaultImplementation]
partial class C
{
    private int P { }
}
";

        const string expected = @"#nullable enable

internal partial class C : IC { }

public partial interface IC
{
}
";

        var runnable = new IncrementalGeneratorRunner(source, new InterfaceGenerator());
        var result = runnable.Run(out var output, out var diagnostics);

        Assert.Empty(diagnostics);
        Assert.Empty(result.Diagnostics);
        Assert.Equal(expected: 3, output.SyntaxTrees.Count());
        Assert.Equal(expected: 2, result.GeneratedTrees.Length);
        Assert.Equal(expected, result.GeneratedTrees[1].ToString());
    }

    [Fact]
    public void GeneratePropertyForClass()
    {
        const string source = @"
using Kehlet.AutoInterface.Attributes;

[DefaultImplementation]
partial class C
{
    public int P { get; }
}
";

        const string expected = @"#nullable enable

internal partial class C : IC { }

public partial interface IC
{
    int P { get; }
}
";

        var runnable = new IncrementalGeneratorRunner(source, new InterfaceGenerator());
        var result = runnable.Run(out var output, out var diagnostics);

        Assert.Empty(diagnostics);
        Assert.Empty(result.Diagnostics);
        Assert.Equal(expected: 3, output.SyntaxTrees.Count());
        Assert.Equal(expected: 2, result.GeneratedTrees.Length);
        Assert.Equal(expected, result.GeneratedTrees[1].ToString());
    }

    [Fact]
    public void GeneratePropertyForClassWithSetter()
    {
        const string source = @"
using Kehlet.AutoInterface.Attributes;

[DefaultImplementation]
partial class C
{
    public int P { get; set; }
}
";

        const string expected = @"#nullable enable

internal partial class C : IC { }

public partial interface IC
{
    int P { get; set; }
}
";

        var runnable = new IncrementalGeneratorRunner(source, new InterfaceGenerator());
        var result = runnable.Run(out var output, out var diagnostics);

        Assert.Empty(diagnostics);
        Assert.Empty(result.Diagnostics);
        Assert.Equal(expected: 3, output.SyntaxTrees.Count());
        Assert.Equal(expected: 2, result.GeneratedTrees.Length);
        Assert.Equal(expected, result.GeneratedTrees[1].ToString());
    }

    [Fact]
    public void GeneratePropertyForClassWithInitOnlySetter()
    {
        const string source = @"
using Kehlet.AutoInterface.Attributes;

[DefaultImplementation]
partial class C
{
    public int P { get; init; }
}
";

        const string expected = @"#nullable enable

internal partial class C : IC { }

public partial interface IC
{
    int P { get; init; }
}
";

        var runnable = new IncrementalGeneratorRunner(source, new InterfaceGenerator());
        var result = runnable.Run(out var output, out var diagnostics);

        Assert.Empty(diagnostics);
        Assert.Empty(result.Diagnostics);
        Assert.Equal(expected: 3, output.SyntaxTrees.Count());
        Assert.Equal(expected: 2, result.GeneratedTrees.Length);
        Assert.Equal(expected, result.GeneratedTrees[1].ToString());
    }

    [Fact]
    public void GeneratePropertyForClassWithOnlySetter()
    {
        const string source = @"
using Kehlet.AutoInterface.Attributes;

[DefaultImplementation]
partial class C
{
    public int P { set; }
}
";

        const string expected = @"#nullable enable

internal partial class C : IC { }

public partial interface IC
{
    int P { set; }
}
";

        var runnable = new IncrementalGeneratorRunner(source, new InterfaceGenerator());
        var result = runnable.Run(out var output, out var diagnostics);

        Assert.Empty(diagnostics);
        Assert.Empty(result.Diagnostics);
        Assert.Equal(expected: 3, output.SyntaxTrees.Count());
        Assert.Equal(expected: 2, result.GeneratedTrees.Length);
        Assert.Equal(expected, result.GeneratedTrees[1].ToString());
    }
}
