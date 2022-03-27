using Kehlet.AutoInterface.SourceGenerators;
using Xunit;

namespace Kehlet.AutoInterface.UnitTests.SourceGenerators;

public class Indexer
{
    [Fact]
    public void GenerateIndexerForClass()
    {
        const string source = @"
using Kehlet.AutoInterface.Attributes;

[DefaultImplementation]
partial class C
{
    public int this[int i] => 0;
}
";

        const string expected = @"#nullable enable

internal partial class C : IC { }

public partial interface IC
{
    int this[int i] { get; }
}
";

        var runnable = new IncrementalGeneratorRunner(source, new InterfaceGenerator());
        var result = runnable.Run(out var output);

        Assert.Empty(output.GetDiagnostics());
        Assert.Empty(result.Diagnostics);
        Assert.Equal(expected: 2, result.GeneratedTrees.Length);
        Assert.Equal(expected, result.GeneratedTrees[1].ToString());
    }

    [Fact]
    public void GenerateIndexerWithTwoParameters()
    {
        const string source = @"
using Kehlet.AutoInterface.Attributes;

[DefaultImplementation]
partial class C
{
    public int this[int i, int y] => 0;
}
";

        const string expected = @"#nullable enable

internal partial class C : IC { }

public partial interface IC
{
    int this[int i, int y] { get; }
}
";

        var runnable = new IncrementalGeneratorRunner(source, new InterfaceGenerator());
        var result = runnable.Run(out var output);

        Assert.Empty(output.GetDiagnostics());
        Assert.Empty(result.Diagnostics);
        Assert.Equal(expected: 2, result.GeneratedTrees.Length);
        Assert.Equal(expected, result.GeneratedTrees[1].ToString());
    }

    [Fact]
    public void GenerateIndexerWithSetter()
    {
        const string source = @"
using Kehlet.AutoInterface.Attributes;

[DefaultImplementation]
partial class C
{
    public int this[int i] { get => 0; set { } }
}
";

        const string expected = @"#nullable enable

internal partial class C : IC { }

public partial interface IC
{
    int this[int i] { get; set; }
}
";

        var runnable = new IncrementalGeneratorRunner(source, new InterfaceGenerator());
        var result = runnable.Run(out var output);

        Assert.Empty(output.GetDiagnostics());
        Assert.Empty(result.Diagnostics);
        Assert.Equal(expected: 2, result.GeneratedTrees.Length);
        Assert.Equal(expected, result.GeneratedTrees[1].ToString());
    }

    [Fact]
    public void DontGenerateIndexerWithoutAccessor()
    {
        const string source = @"
using Kehlet.AutoInterface.Attributes;

[DefaultImplementation]
partial class C
{
    public int this[int i] {  }
}
";

        const string expected = @"#nullable enable

internal partial class C : IC { }

public partial interface IC
{
}
";

        var runnable = new IncrementalGeneratorRunner(source, new InterfaceGenerator());
        var result = runnable.Run(out var output);

        Assert.Single(output.GetDiagnostics());
        Assert.Empty(result.Diagnostics);
        Assert.Equal(expected: 2, result.GeneratedTrees.Length);
        Assert.Equal(expected, result.GeneratedTrees[1].ToString());
    }

    [Fact]
    public void DontGenerateIndexerWithoutParameter()
    {
        const string source = @"
using Kehlet.AutoInterface.Attributes;

[DefaultImplementation]
partial class C
{
    public int this[] { set { } }
}
";

        const string expected = @"#nullable enable

internal partial class C : IC { }

public partial interface IC
{
}
";

        var runnable = new IncrementalGeneratorRunner(source, new InterfaceGenerator());
        var result = runnable.Run(out var output);

        Assert.Empty(result.Diagnostics);
        Assert.Equal(expected: 2, result.GeneratedTrees.Length);
        Assert.Equal(expected, result.GeneratedTrees[1].ToString());
    }
}
