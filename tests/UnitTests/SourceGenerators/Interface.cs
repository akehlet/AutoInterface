using Kehlet.AutoInterface.SourceGenerators;
using Xunit;

namespace Kehlet.AutoInterface.UnitTests.SourceGenerators;

public class Interface
{
    [Fact]
    public void GenerateNothingWhenRecordClass()
    {
        const string source = @"
using Kehlet.AutoInterface.Attributes;

[DefaultImplementation]
partial record C
{
}
";

        var runnable = new IncrementalGeneratorRunner(source, new InterfaceGenerator());
        var result = runnable.Run(out var output);

        Assert.Empty(output.GetDiagnostics());
        Assert.Empty(result.Diagnostics);
        Assert.Single(result.GeneratedTrees);
    }

    [Fact]
    public void GenerateNothingWhenRecordStruct()
    {
        const string source = @"
using Kehlet.AutoInterface.Attributes;

[DefaultImplementation]
partial record struct C
{
}
";

        var runnable = new IncrementalGeneratorRunner(source, new InterfaceGenerator());
        var result = runnable.Run(out var output);

        Assert.Empty(output.GetDiagnostics());
        Assert.Empty(result.Diagnostics);
        Assert.Single(result.GeneratedTrees);
    }

    [Fact]
    public void DontGenerateInterfaceWhenAccessibilityIsUnsupported()
    {
        const string source = @"
using Kehlet.AutoInterface.Attributes;

[DefaultImplementation]
private partial class C
{
}
";

        const string expected = "";

        var runnable = new IncrementalGeneratorRunner(source, new InterfaceGenerator());
        var result = runnable.Run(out _);

        Assert.Empty(result.Diagnostics);
        Assert.Equal(expected: 2, result.GeneratedTrees.Length);
        Assert.Equal(expected, result.GeneratedTrees[1].ToString());
    }

    [Fact]
    public void GenerateNothingWhenWrongAttribute()
    {
        const string source = @"
using System;

[CLSCompliant(false)]
partial class C
{
}
";

        var runnable = new IncrementalGeneratorRunner(source, new InterfaceGenerator());
        var result = runnable.Run(out var output);

        Assert.Single(output.GetDiagnostics());
        Assert.Empty(result.Diagnostics);
        Assert.Single(result.GeneratedTrees);
    }

    [Fact]
    public void GenerateInterfaceForClass()
    {
        const string source = @"
using Kehlet.AutoInterface.Attributes;

[DefaultImplementation]
partial class C
{
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

        Assert.Empty(output.GetDiagnostics());
        Assert.Empty(result.Diagnostics);
        Assert.Equal(expected: 2, result.GeneratedTrees.Length);
        Assert.Equal(expected, result.GeneratedTrees[1].ToString());
    }

    [Fact]
    public void GenerateInterfaceForStruct()
    {
        const string source = @"
using Kehlet.AutoInterface.Attributes;

[DefaultImplementation]
partial struct C
{
}
";

        const string expected = @"#nullable enable

internal partial struct C : IC { }

public partial interface IC
{
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
    public void GenerateInterfaceWithDocs()
    {
        const string source = @"
using Kehlet.AutoInterface.Attributes;

/// <summary>
/// Test
/// </summary>
[DefaultImplementation]
partial class C
{
}
";

        const string expected = @"#nullable enable

internal partial class C : IC { }

/// <member name=""T:C"">
///     <summary>
///     Test
///     </summary>
/// </member>
public partial interface IC
{
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
    public void GenerateInterfaceWithNamespace()
    {
        const string source = @"
using Kehlet.AutoInterface.Attributes;

namespace N;

[DefaultImplementation]
partial struct C
{
}
";

        const string expected = @"#nullable enable

namespace N;

internal partial struct C : IC { }

public partial interface IC
{
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
    public void GenerateInterfaceWithNestedNamespace()
    {
        const string source = @"
using Kehlet.AutoInterface.Attributes;

namespace N1
{
    namespace N2
    {
        [DefaultImplementation]
        partial struct C
        {
        }
    }
}
";

        const string expected = @"#nullable enable

namespace N1.N2;

internal partial struct C : IC { }

public partial interface IC
{
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
    public void GenerateInternalInterfaceForClass()
    {
        const string source = @"
using Kehlet.AutoInterface.Attributes;

[DefaultImplementation(Accessibility.Internal)]
partial class C
{
}
";

        const string expected = @"#nullable enable

internal partial class C : IC { }

internal partial interface IC
{
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
    public void GenerateInterfaceForPublicClass()
    {
        const string source = @"
using Kehlet.AutoInterface.Attributes;

[DefaultImplementation]
public partial class C
{
}
";

        const string expected = @"#nullable enable

public partial class C : IC { }

public partial interface IC
{
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
    public void GenerateInterfaceForClassWithTypeParameter()
    {
        const string source = @"
using Kehlet.AutoInterface.Attributes;

[DefaultImplementation]
partial class C<T>
{
}
";

        const string expected = @"#nullable enable

internal partial class C<T> : IC<T> { }

public partial interface IC<T>
{
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
    public void GenerateInterfaceForClassWithCovariantTypeParameter()
    {
        const string source = @"
using Kehlet.AutoInterface.Attributes;

[DefaultImplementation]
partial class C<[Out]T>
{
}
";

        const string expected = @"#nullable enable

internal partial class C<T> : IC<T> { }

public partial interface IC<out T>
{
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
    public void GenerateInterfaceForClassWithContravariantTypeParameter()
    {
        const string source = @"
using Kehlet.AutoInterface.Attributes;

[DefaultImplementation]
partial class C<[In]T>
{
}
";

        const string expected = @"#nullable enable

internal partial class C<T> : IC<T> { }

public partial interface IC<in T>
{
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
    public void GenerateInterfaceForClassWithTwoContravariantTypeParameters()
    {
        const string source = @"
using Kehlet.AutoInterface.Attributes;

[DefaultImplementation]
partial class C<[In]T, [In]Y>
{
}
";

        const string expected = @"#nullable enable

internal partial class C<T, Y> : IC<T, Y> { }

public partial interface IC<in T, in Y>
{
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
    public void GenerateInterfaceForClassWithPrivateMember()
    {
        const string source = @"
using Kehlet.AutoInterface.Attributes;

[DefaultImplementation]
partial class C
{
    private void M() { }
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

        Assert.Empty(output.GetDiagnostics());
        Assert.Empty(result.Diagnostics);
        Assert.Equal(expected: 2, result.GeneratedTrees.Length);
        Assert.Equal(expected, result.GeneratedTrees[1].ToString());
    }

    [Fact]
    public void GenerateInterfaceForClassWithExcludedMember()
    {
        const string source = @"
using Kehlet.AutoInterface.Attributes;

[DefaultImplementation]
partial class C
{
    [Exclude]
    public void M() { }
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

        Assert.Empty(output.GetDiagnostics());
        Assert.Empty(result.Diagnostics);
        Assert.Equal(expected: 2, result.GeneratedTrees.Length);
        Assert.Equal(expected, result.GeneratedTrees[1].ToString());
    }

    [Fact]
    public void GenerateInterfaceForClassWithTypeParameterWithClassConstraint()
    {
        const string source = @"
using Kehlet.AutoInterface.Attributes;

[DefaultImplementation]
partial class C<T> where T : class
{
}
";

        const string expected = @"#nullable enable

internal partial class C<T> : IC<T> { }

public partial interface IC<T>
    where T : class
{
}
";

        var runnable = new IncrementalGeneratorRunner(source, new InterfaceGenerator());
        var result = runnable.Run(out var output);

        Assert.Empty(output.GetDiagnostics());
        Assert.Empty(result.Diagnostics);
        Assert.Equal(expected: 2, result.GeneratedTrees.Length);
        Assert.Equal(expected, result.GeneratedTrees[1].ToString());
    }
}
