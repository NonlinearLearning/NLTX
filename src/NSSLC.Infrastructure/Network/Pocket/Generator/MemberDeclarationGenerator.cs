using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Generator;

[Generator]
public sealed class MemberDeclarationGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var members = context.SyntaxProvider.CreateSyntaxProvider(
            static (node, _) => node is MemberAccessExpressionSyntax access && access.Name.Identifier.ValueText == "Members",
            static (syntax, cancellation) =>
            {
                var access = (MemberAccessExpressionSyntax)syntax.Node;
                var owner = syntax.SemanticModel.GetSymbolInfo(access.Expression, cancellation).Symbol as INamedTypeSymbol;
                return owner is not null && SymbolEqualityComparer.Default.Equals(owner.ContainingAssembly,
                    syntax.SemanticModel.Compilation.Assembly) ? owner.OriginalDefinition : null;
            }).Where(static owner => owner is not null).Collect();
        context.RegisterSourceOutput(members, static (output, owners) => EmitMembers(output, owners));

        var models = context.SyntaxProvider.CreateSyntaxProvider(
            static (node, _) => node is TypeOfExpressionSyntax or TypeDeclarationSyntax
                || node is MemberAccessExpressionSyntax access && access.Name.Identifier.ValueText == "Members",
            static (syntax, cancellation) =>
            {
                INamedTypeSymbol? owner = syntax.Node switch
                {
                    TypeOfExpressionSyntax type => syntax.SemanticModel.GetTypeInfo(type.Type, cancellation).Type as INamedTypeSymbol,
                    TypeDeclarationSyntax type => syntax.SemanticModel.GetDeclaredSymbol(type, cancellation) as INamedTypeSymbol,
                    MemberAccessExpressionSyntax access => syntax.SemanticModel.GetSymbolInfo(access.Expression, cancellation).Symbol as INamedTypeSymbol,
                    _ => null
                };
                if (owner is null || !SymbolEqualityComparer.Default.Equals(owner.ContainingAssembly,
                        syntax.SemanticModel.Compilation.Assembly)) return null;
                return owner.GetMembers("Instance").OfType<IPropertySymbol>().Any()
                    ? owner : null;
            }).Where(static owner => owner is not null).Collect();
        context.RegisterSourceOutput(models, static (output, owners) => EmitModels(output, owners));
    }

    private static void EmitMembers(SourceProductionContext output, ImmutableArray<INamedTypeSymbol?> owners)
    {
        var seen = new HashSet<ISymbol>(SymbolEqualityComparer.Default);
        foreach (var owner in owners.OfType<INamedTypeSymbol>())
        {
            if (!seen.Add(owner) || owner.GetTypeMembers("Members").Length != 0) continue;
            var source = new StringBuilder("#nullable enable\n");
            if (!owner.ContainingNamespace.IsGlobalNamespace)
                source.Append("namespace ").Append(owner.ContainingNamespace.ToDisplayString()).AppendLine(";");
            var containers = new Stack<INamedTypeSymbol>();
            for (var type = owner; type is not null; type = type.ContainingType) containers.Push(type);
            foreach (var type in containers)
            {
                string kind = type.IsRecord ? type.TypeKind == TypeKind.Struct ? "record struct" : "record class"
                    : type.TypeKind == TypeKind.Struct ? "struct" : "class";
                source.Append("partial ").Append(kind).Append(' ').Append(Identifier(type.Name));
                if (type.TypeParameters.Length != 0)
                    source.Append('<').Append(string.Join(", ", type.TypeParameters.Select(parameter => Identifier(parameter.Name)))).Append('>');
                source.AppendLine("\n{");
            }
            source.AppendLine("    public static class Members\n    {");
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (var member in owner.GetMembers().Where(static member => !member.IsImplicitlyDeclared))
            {
                ITypeSymbol valueType;
                bool readable, writable, indexed;
                string kind;
                if (member is IFieldSymbol field)
                {
                    valueType = field.Type;
                    readable = field.DeclaredAccessibility == Accessibility.Public;
                    writable = readable && !field.IsReadOnly && !field.IsConst;
                    indexed = false;
                    kind = "Field";
                }
                else if (member is IPropertySymbol property && property.ExplicitInterfaceImplementations.Length == 0)
                {
                    valueType = property.Type;
                    readable = property.GetMethod?.DeclaredAccessibility == Accessibility.Public;
                    writable = property.SetMethod?.DeclaredAccessibility == Accessibility.Public;
                    indexed = property.IsIndexer;
                    kind = "Property";
                }
                else continue;
                string name = member.MetadataName;
                if (!SyntaxFacts.IsValidIdentifier(name) && SyntaxFacts.GetKeywordKind(name) == SyntaxKind.None) continue;
                if (!names.Add(name)) continue;
                source.Append("        public static global::Terraria.NetWork.Prototype.PacketDesignCompiler.MemberDeclaration ")
                    .Append(Identifier(name)).AppendLine(" { get; } = new(");
                source.Append("            typeof(").Append(TypeName(owner)).Append("), typeof(").Append(TypeName(valueType)).AppendLine("),");
                source.Append("            new global::Terraria.NetWork.Prototype.PacketDesignCompiler.MemberRef(")
                    .Append(TypeReference(owner)).Append(", ").Append(SymbolDisplay.FormatLiteral(name, true))
                    .Append(", global::Terraria.NetWork.Prototype.PacketDesignCompiler.MemberKind.").Append(kind)
                    .Append(", ").Append(TypeReference(valueType)).Append(", ").Append(Flag(readable))
                    .Append(", ").Append(Flag(writable)).Append(", ").Append(Flag(member.IsStatic))
                    .Append(", ").Append(Flag(indexed)).AppendLine("));");
            }
            source.AppendLine("    }");
            foreach (var _ in containers) source.AppendLine("}");
            string hint = owner.ToDisplayString().Replace('.', '_').Replace('<', '_').Replace('>', '_').Replace(',', '_').Replace(' ', '_');
            output.AddSource(hint + ".Members.g.cs", source.ToString());
        }
    }

    private static void EmitModels(SourceProductionContext output, ImmutableArray<INamedTypeSymbol?> owners)
    {
        var seen = new HashSet<ISymbol>(SymbolEqualityComparer.Default);
        var source = new StringBuilder("#nullable enable\n");
        source.AppendLine("namespace Terraria.NetWork.Prototype.PacketDesignCompiler.GeneratedDeclarations;");
        source.AppendLine("internal static class ProtocolModelDeclarations\n{");
        source.AppendLine("    [global::System.Runtime.CompilerServices.ModuleInitializer]\n    internal static void Register()\n    {");
        foreach (var owner in owners.OfType<INamedTypeSymbol>())
        {
            if (!seen.Add(owner)) continue;
            bool visible = true;
            for (var container = owner; container is not null; container = container.ContainingType)
                visible &= container.DeclaredAccessibility == Accessibility.Public;
            var instances = owner.GetMembers("Instance").OfType<IPropertySymbol>().ToArray();
            bool hasInstance = instances.Length == 1 && instances[0] is { IsStatic: true, IsIndexer: false }
                && instances[0].GetMethod?.DeclaredAccessibility == Accessibility.Public
                && SymbolEqualityComparer.Default.Equals(instances[0].Type, owner);
            string read = visible && hasInstance ? "static () => " + TypeName(owner) + ".Instance" : "null";
            source.Append("        global::Terraria.NetWork.Prototype.PacketDesignCompiler.CompilerMemberCatalog.RegisterModel(")
                .Append(SymbolDisplay.FormatLiteral(owner.ContainingAssembly.Name, true)).Append(", typeof(")
                .Append(TypeName(owner)).AppendLine("),");
            source.Append("            new global::Terraria.NetWork.Prototype.PacketDesignCompiler.ProtocolModelRef(")
                .Append(Flag(owner.TypeKind == TypeKind.Class)).Append(", ").Append(Flag(visible)).Append(", ")
                .Append(Flag(owner.IsAbstract)).Append(", ").Append(Flag(owner.IsSealed)).Append(", ").Append(read).AppendLine("));");
        }
        source.AppendLine("    }\n}");
        output.AddSource("ProtocolModelDeclarations.g.cs", source.ToString());
    }

    private static string Identifier(string name) => SyntaxFacts.GetKeywordKind(name) != SyntaxKind.None ? "@" + name : name;
    private static string Flag(bool value) => value ? "true" : "false";
    private static string TypeName(ITypeSymbol type) => type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    private static string TypeReference(ITypeSymbol type) =>
        "global::Terraria.NetWork.Prototype.PacketDesignCompiler.TypeRef.From(typeof(" + TypeName(type) + "))";
}
