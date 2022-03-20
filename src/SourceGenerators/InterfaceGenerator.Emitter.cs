using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Kehlet.AutoInterface.SourceGenerators;

public partial class InterfaceGenerator
{
    internal class Emitter
    {
        private const string Indent = "    ";

        private static string? AccessModifierMap(Accessibility accessibility) => accessibility switch
        {
            Accessibility.Internal => "internal",
            Accessibility.Public => "public",
            _ => null
        };

        private static string? TypeKindMap(TypeKind kind) => kind switch
        {
            TypeKind.Class => "class",
            TypeKind.Struct => "struct",
            _ => null
        };

        public string Emit(TargetType @class)
        {
            var builder = new StringBuilder();

            builder.AppendLine("#nullable enable");
            if (@class.Namespace.Length > 0)
            {
                builder.AppendLine($"{Environment.NewLine}namespace {@class.Namespace};");
            }

            if (AccessModifierMap(@class.Accessibility) is not { } classAccessibility)
            {
                return "";
            }

            if (TypeKindMap(@class.Kind) is not { } typeKind)
            {
                return "";
            }

            var typeParameters = EmitTypeParameters(@class.TypeParameters, withVariance: false);

            builder.AppendLine($@"
{classAccessibility} partial {typeKind} {@class.Name}{typeParameters} : I{@class.Name}{typeParameters} {{ }}
");

            AppendDocs(builder, @class.Docs);

            var interfaceAccessibility = @class.IsPublic ? "public" : "internal";

            builder.Append($"{interfaceAccessibility} partial interface I{@class.Name}");
            AppendTypeParameters(builder, @class.TypeParameters, withVariance: true);

            builder.AppendLine($"{Environment.NewLine}{{");
            AppendMembers(builder, @class.Members);
            builder.AppendLine("}");

            return builder.ToString();
        }

        public static void AppendMembers(StringBuilder builder, ImmutableArray<Member> members)
        {
            // Stryker disable all : Early return for performance.
            if (members.IsDefaultOrEmpty)
            {
                return;
            }
            // Stryker restore all

            foreach (var member in members)
            {
                switch (member.Type)
                {
                    case MemberType.Property when member.Accessors.IsDefaultOrEmpty:
                    case MemberType.Indexer when (member.Accessors.IsDefaultOrEmpty || member.Parameters.IsDefaultOrEmpty):
                        continue;
                }

                AppendDocs(builder, member.Docs, Indent);

                builder.Append(Indent);

                if (member.ReturnsByRefReadonly)
                {
                    builder.Append("ref readonly ");
                }
                else if (member.ReturnsByRef)
                {
                    builder.Append("ref ");
                }

                builder.Append($"{member.ReturnType} ");

                switch (member.Type)
                {
                    case MemberType.Indexer:
                        AppendIndexerSignature(builder, member);
                        break;
                    case MemberType.Property:
                        AppendPropertySignature(builder, member);
                        break;
                    case MemberType.Method:
                        AppendMethodSignature(builder, member);
                        break;
                }
            }
        }

        public static void AppendIndexerSignature(StringBuilder builder, Member member)
        {
            builder.Append("this[");
            builder.Append(string.Join(", ", member.Parameters));
            builder.Append("] { ");
            builder.Append(string.Join(" ", member.Accessors));
            builder.AppendLine(" }");
        }

        public static void AppendPropertySignature(StringBuilder builder, Member member)
        {
            builder.Append($"{member.Name} {{ ");
            builder.Append(string.Join(" ", member.Accessors));
            builder.AppendLine(" }");
        }

        public static void AppendMethodSignature(StringBuilder builder, Member member)
        {
            builder.Append(member.Name);
            AppendTypeParameters(builder, member.TypeParameters);
            builder.Append("(");
            builder.Append(string.Join(", ", member.Parameters));
            builder.AppendLine(");");
        }

        public static void AppendTypeParameters(StringBuilder builder, ImmutableArray<TypeParameter> typeParameters, bool withVariance = false)
        {
            if (typeParameters.IsDefaultOrEmpty)
            {
                return;
            }

            builder.Append("<");
            if (withVariance)
            {
                builder.Append(string.Join(", ", typeParameters));
            }
            else
            {
                builder.Append(string.Join(", ", typeParameters.Select(x => x.Name)));
            }

            builder.Append(">");
        }

        public static string EmitTypeParameters(ImmutableArray<TypeParameter> parameters, bool withVariance)
        {
            // Stryker disable all : Early return for performance.
            if (parameters.IsDefaultOrEmpty)
            {
                return "";
            }
            // Stryker restore all

            var builder = new StringBuilder();

            AppendTypeParameters(builder, parameters, withVariance);

            return builder.ToString();
        }

        public static void AppendDocs(StringBuilder builder, string? docs, string indent = "")
        {
            // Stryker disable all : Early return for performance.
            if (docs is null)
            {
                return;
            }
            // Stryker restore all

            var lines = docs.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                builder.AppendLine($"{indent}/// {line}");
            }
        }
    }

    private const string DefaultImplementationAttribute = @$"#nullable enable

using System;

namespace {AttributeNamespace};

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
internal class {DefaultImplementationAttributeName} : Attribute
{{
    public {DefaultImplementationAttributeName}(Accessibility accessibility = Accessibility.Public)
    {{
        Accessibility = accessibility;
    }}

    public Accessibility Accessibility {{ get; }}
}}

internal enum Accessibility
{{
    Public,
    Internal
}}

[AttributeUsage(AttributeTargets.GenericParameter)]
internal class {InAttributeName} : Attribute {{ }}

[AttributeUsage(AttributeTargets.GenericParameter)]
internal class {OutAttributeName} : Attribute {{ }}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
internal class {ExcludeAttributeName} : Attribute {{ }}
";
}
