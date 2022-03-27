using System.Collections.Immutable;
using System.Text;
using Kehlet.AutoInterface.SourceGenerators;
using Xunit;
using Emitter = Kehlet.AutoInterface.SourceGenerators.InterfaceGenerator.Emitter;
using TypeParameter = Kehlet.AutoInterface.SourceGenerators.InterfaceGenerator.TypeParameter;

namespace Kehlet.AutoInterface.UnitTests.SourceGenerators;

public class EmitterTests
{
    [Fact]
    public void AppendInvariantTypeParameter()
    {
        const string expected = "<T>";

        var builder = new StringBuilder();
        var typeParameters = ImmutableArray.Create(new TypeParameter
        {
            Name = "T",
            Variance = InterfaceGenerator.Variance.None
        });

        Emitter.AppendTypeParameters(builder, typeParameters);

        var result = builder.ToString();

        Assert.Equal(expected, result);
    }

    [Fact]
    public void AppendCovariantTypeParameter()
    {
        const string expected = "<out T>";

        var builder = new StringBuilder();
        var typeParameters = ImmutableArray.Create(new TypeParameter
        {
            Name = "T",
            Variance = InterfaceGenerator.Variance.Covariant
        });

        Emitter.AppendTypeParameters(builder, typeParameters, withVariance: true);

        var result = builder.ToString();

        Assert.Equal(expected, result);
    }

    [Fact]
    public void AppendContravariantTypeParameter()
    {
        const string expected = "<in T>";

        var builder = new StringBuilder();
        var typeParameters = ImmutableArray.Create(new TypeParameter
        {
            Name = "T",
            Variance = InterfaceGenerator.Variance.Contravariant
        });

        Emitter.AppendTypeParameters(builder, typeParameters, withVariance: true);

        var result = builder.ToString();

        Assert.Equal(expected, result);
    }
}
