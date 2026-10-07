using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Generator;

[Generator]
public sealed class CodecDeclarationGenerator : IIncrementalGenerator
{
    private const string SharedFunctionAttributeMetadataName =
        "Terraria.NetWork.Prototype.PacketDesignCompiler.PacketSharedFunctionAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var methods = context.SyntaxProvider.CreateSyntaxProvider(
            static (node, _) => node is MemberAccessExpressionSyntax or IdentifierNameSyntax or GenericNameSyntax,
            static (syntax, cancellation) =>
            {
                var converted = syntax.SemanticModel.GetTypeInfo(syntax.Node, cancellation).ConvertedType;
                var argument = syntax.Node.Parent is ArgumentSyntax
                    ? syntax.SemanticModel.GetOperation(syntax.Node.Parent, cancellation) as IArgumentOperation : null;
                var value = argument?.Value ?? syntax.SemanticModel.GetOperation(syntax.Node, cancellation);
                while (value is IConversionOperation conversion) value = conversion.Operand;
                var symbol = syntax.SemanticModel.GetSymbolInfo(syntax.Node, cancellation);
                var method = value is IDelegateCreationOperation creation
                    && creation.Target is IMethodReferenceOperation reference ? reference.Method
                    : symbol.Symbol as IMethodSymbol
                        ?? (symbol.CandidateSymbols.Length == 1 ? symbol.CandidateSymbols[0] as IMethodSymbol : null);
                if (method is null || !method.IsStatic || method.DeclaredAccessibility != Accessibility.Public)
                    return null;
                if (converted is null || (converted.TypeKind != TypeKind.Delegate
                    && converted.ToDisplayString() != "System.Delegate"))
                {
                    // Another generator may supply Members referenced by earlier arguments.
                    // Collect the method declaration even while that invocation is unbound.
                    if (syntax.Node.Parent is not ArgumentSyntax { Parent: ArgumentListSyntax
                        { Parent: InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax call } } }
                        || call.Name.Identifier.ValueText is not ("FunctionName" or "FromCodec"))
                        return null;
                    converted = syntax.SemanticModel.Compilation.GetTypeByMetadataName("System.Delegate");
                }
                if (converted is null || ContainsTypeParameter(converted)) return null;
                return new Declaration(method, converted);
            }).Where(static declaration => declaration is not null).Collect();
        context.RegisterSourceOutput(methods, static (output, declarations) => Emit(output, declarations));
    }

    private static void Emit(SourceProductionContext output, ImmutableArray<Declaration?> declarations)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var source = new StringBuilder("#nullable enable\n");
        source.AppendLine("namespace Terraria.NetWork.Prototype.PacketDesignCompiler.GeneratedDeclarations;");
        source.AppendLine("internal static class CodecDeclarations");
        source.AppendLine("{");
        source.AppendLine("    [global::System.Runtime.CompilerServices.ModuleInitializer]");
        source.AppendLine("    internal static void Register()");
        source.AppendLine("    {");
        foreach (var declaration in declarations.OfType<Declaration>())
        {
            var method = declaration.Method;
            if (method.ReturnsByRef || method.ReturnsByRefReadonly
                || ContainsTypeParameter(method.ReturnType) || method.Parameters.Any(static parameter => ContainsTypeParameter(parameter.Type)))
                continue;
            string owner = TypeName(method.ContainingType);
            string name = SyntaxFacts.GetKeywordKind(method.Name) != SyntaxKind.None ? "@" + method.Name : method.Name;
            string reference = owner + "." + name;
            if (method.IsGenericMethod)
                reference += "<" + string.Join(", ", method.TypeArguments.Select(TypeName)) + ">";
            string delegateType = declaration.DelegateType.ToDisplayString(
                SymbolDisplayFormat.FullyQualifiedFormat.WithMiscellaneousOptions(
                    SymbolDisplayFormat.FullyQualifiedFormat.MiscellaneousOptions
                    | SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier));
            if (!seen.Add(delegateType + ":" + reference)) continue;

            source.Append("        global::Terraria.NetWork.Prototype.PacketDesignCompiler.CompilerMethodCatalog.Register((")
                .Append(delegateType).Append(")").Append(reference).AppendLine(",");
            source.AppendLine("            new global::Terraria.NetWork.Prototype.PacketDesignCompiler.MethodRef(");
            source.Append("                ").Append(TypeReference(method.ContainingType)).AppendLine(",");
            source.Append("                ").Append(SymbolDisplay.FormatLiteral(method.Name, true)).AppendLine(",");
            source.Append("                ").Append(TypeReference(method.ReturnType)).AppendLine(",");
            source.AppendLine("                global::System.Collections.Immutable.ImmutableArray.Create<global::Terraria.NetWork.Prototype.PacketDesignCompiler.CalculationParameterRef>(");
            for (int index = 0; index < method.Parameters.Length; index++)
            {
                var parameter = method.Parameters[index];
                source.Append("                    new global::Terraria.NetWork.Prototype.PacketDesignCompiler.CalculationParameterRef(")
                    .Append(SymbolDisplay.FormatLiteral(parameter.Name, true)).Append(", ")
                    .Append(TypeReference(parameter.Type)).Append(", ")
                    .Append(parameter.RefKind != RefKind.None ? "true" : "false").Append(", ")
                    .Append(parameter.RefKind == RefKind.Out ? "true" : "false").Append(", ")
                    .Append(parameter.RefKind is RefKind.In or RefKind.RefReadOnlyParameter ? "true" : "false").Append(", ")
                    .Append(parameter.IsOptional ? "true" : "false").Append(", ")
                    .Append(parameter.IsParams ? "true" : "false").Append(")")
                    .AppendLine(index + 1 < method.Parameters.Length ? "," : string.Empty);
            }
            source.AppendLine("                ),");
            source.Append("                true, true, ").Append(method.IsGenericMethod ? "true" : "false")
                .Append(", ").Append(IsPartialContainingType(method) ? "true" : "false")
                .Append(", ").Append(HasSharedFunctionAttribute(method.ContainingType) ? "true" : "false")
                .AppendLine("));");
        }
        source.AppendLine("    }");
        source.AppendLine("}");
        output.AddSource("CodecDeclarations.g.cs", source.ToString());
    }

    private static bool ContainsTypeParameter(ITypeSymbol type) => type is ITypeParameterSymbol
        || type is IArrayTypeSymbol array && ContainsTypeParameter(array.ElementType)
        || type is INamedTypeSymbol named && named.TypeArguments.Any(ContainsTypeParameter);

    private static bool IsPartialContainingType(IMethodSymbol method) =>
        method.ContainingType.DeclaringSyntaxReferences.Any(reference =>
        {
            var syntax = reference.GetSyntax();
            return syntax is TypeDeclarationSyntax declaration
                && declaration.Modifiers.Any(SyntaxKind.PartialKeyword);
        });

    private static bool HasSharedFunctionAttribute(INamedTypeSymbol type) =>
        type.GetAttributes().Any(attribute => string.Equals(
            attribute.AttributeClass?.ToDisplayString(),
            SharedFunctionAttributeMetadataName,
            StringComparison.Ordinal));

    private static string TypeName(ITypeSymbol type) => type.SpecialType == SpecialType.System_Void
        ? "void" : type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

    private static string TypeReference(ITypeSymbol type) =>
        "global::Terraria.NetWork.Prototype.PacketDesignCompiler.TypeRef.From(typeof(" + TypeName(type) + "))";

    private sealed class Declaration
    {
        public Declaration(IMethodSymbol method, ITypeSymbol delegateType)
        {
            Method = method;
            DelegateType = delegateType;
        }

        public IMethodSymbol Method { get; }
        public ITypeSymbol DelegateType { get; }
    }
}
