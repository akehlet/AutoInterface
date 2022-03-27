using System.Linq;
using Kehlet.AutoInterface.SourceGenerators;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Kehlet.AutoInterface.UnitTests.SourceGenerators;

public class Method
{
    [Fact]
    public void GenerateMethodForClass()
    {
        const string source = @"
using Kehlet.AutoInterface.Attributes;

[DefaultImplementation]
partial class C
{
    public void M() { }
}
";

        const string expected = @"#nullable enable

internal partial class C : IC { }

public partial interface IC
{
    void M();
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
    public void GenerateMethodWithDocs()
    {
        const string source = @"
using Kehlet.AutoInterface.Attributes;

[DefaultImplementation]
partial class C
{
    /// <summary>
    /// Test
    /// </summary>
    public void M() { }
}
";

        const string expected = @"#nullable enable

internal partial class C : IC { }

public partial interface IC
{
    /// <member name=""M:C.M"">
    ///     <summary>
    ///     Test
    ///     </summary>
    /// </member>
    void M();
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
    public void GenerateMethodWithRefReturn()
    {
        const string source = @"
using Kehlet.AutoInterface.Attributes;

[DefaultImplementation]
partial class C
{
    private int a = 0;

    public ref int M() { return ref a; }
}
";

        const string expected = @"#nullable enable

internal partial class C : IC { }

public partial interface IC
{
    ref int M();
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
    public void GenerateMethodWithRefReadoblyReturn()
    {
        const string source = @"
using Kehlet.AutoInterface.Attributes;

[DefaultImplementation]
partial class C
{
    private readonly int a = 0;

    public ref readonly int M() { return ref a; }
}
";

        const string expected = @"#nullable enable

internal partial class C : IC { }

public partial interface IC
{
    ref readonly int M();
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
    public void GenerateMethodWithParameter()
    {
        const string source = @"
using Kehlet.AutoInterface.Attributes;

[DefaultImplementation]
partial class C
{
    public void M(int i) { }
}
";

        const string expected = @"#nullable enable

internal partial class C : IC { }

public partial interface IC
{
    void M(int i);
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
    public void GenerateMethodWithTwoParameters()
    {
        const string source = @"
using Kehlet.AutoInterface.Attributes;

[DefaultImplementation]
partial class C
{
    public void M(int i, string s) { }
}
";

        const string expected = @"#nullable enable

internal partial class C : IC { }

public partial interface IC
{
    void M(int i, string s);
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
    public void GenerateMethodWithParameterWithDefaultValue()
    {
        const string source = @"
using Kehlet.AutoInterface.Attributes;

[DefaultImplementation]
partial class C
{
    public void M(int i = int.MaxValue) { }
}
";

        const string expected = @"#nullable enable

internal partial class C : IC { }

public partial interface IC
{
    void M(int i = int.MaxValue);
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
    public void GenerateMethodWithTypeParameter()
    {
        const string source = @"
using Kehlet.AutoInterface.Attributes;

[DefaultImplementation]
partial class C
{
    public void M<T>() { }
}
";

        const string expected = @"#nullable enable

internal partial class C : IC { }

public partial interface IC
{
    void M<T>();
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
    public void GenerateMethodWithTwoTypeParameters()
    {
        const string source = @"
using Kehlet.AutoInterface.Attributes;

[DefaultImplementation]
partial class C
{
    public void M<T,Y>() { }
}
";

        const string expected = @"#nullable enable

internal partial class C : IC { }

public partial interface IC
{
    void M<T, Y>();
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
    public void GenerateMethodWithInvariantTypeParameter()
    {
        const string source = @"
using Kehlet.AutoInterface.Attributes;

[DefaultImplementation]
partial class C
{
    public void M<[In] T>() { }
}
";

        const string expected = @"#nullable enable

internal partial class C : IC { }

public partial interface IC
{
    void M<T>();
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
    public void GenerateMethodWithParameterFromAnotherNamespace()
    {
        const string source = @"
using Kehlet.AutoInterface.Attributes;

namespace A
{
    public class G<T> { }
}

namespace Q
{
    public class H { }
}

namespace B
{
    using A;
    using Q;

    [DefaultImplementation]
    partial class C
    {
        public void M(G<H> i) { } 
    }
}
";

        const string expected = @"#nullable enable

namespace B;

internal partial class C : IC { }

public partial interface IC
{
    void M(A.G<Q.H> i);
}
";

        var runnable = new IncrementalGeneratorRunner(source, new InterfaceGenerator());
        var result = runnable.Run(out var output);

        Assert.Empty(result.Diagnostics);
        Assert.Empty(output.GetDiagnostics());
        Assert.Equal(expected: 2, result.GeneratedTrees.Length);
        Assert.Equal(expected, result.GeneratedTrees[1].ToString());
    }

    [Fact]
    public void GenerateMethodWithReturnFromAnotherNamespace()
    {
        const string source = @"
using Kehlet.AutoInterface.Attributes;

namespace A
{
    public class G<T> { }
}

namespace Q
{
    public class H { }
}

namespace B
{
    using A;
    using Q;

    [DefaultImplementation]
    partial class C
    {
        public G<H> M() { return default!; }
    }
}
";

        const string expected = @"#nullable enable

namespace B;

internal partial class C : IC { }

public partial interface IC
{
    A.G<Q.H> M();
}
";

        var runnable = new IncrementalGeneratorRunner(source, new InterfaceGenerator());
        var result = runnable.Run(out var output);

        Assert.Empty(result.Diagnostics);
        Assert.Empty(output.GetDiagnostics().Where(x => x.Severity == DiagnosticSeverity.Error));
        Assert.Equal(expected: 2, result.GeneratedTrees.Length);
        Assert.Equal(expected, result.GeneratedTrees[1].ToString());
    }

    [Fact]
    public void GenerateMethodWithUnboundReturnType()
    {
        const string source = @"
using Kehlet.AutoInterface.Attributes;
using System.Threading.Tasks;

[DefaultImplementation]
partial class C
{
    public Task<> M() { return default!; }
}
";

        const string expected = @"#nullable enable

internal partial class C : IC { }

public partial interface IC
{
    System.Threading.Tasks.Task<TResult> M();
}
";

        var runnable = new IncrementalGeneratorRunner(source, new InterfaceGenerator());
        var result = runnable.Run(out var output);

        Assert.Empty(result.Diagnostics);
        Assert.Equal(expected: 2, result.GeneratedTrees.Length);
        Assert.Equal(expected, result.GeneratedTrees[1].ToString());
    }
}
