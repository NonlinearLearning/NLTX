using System.Collections.Immutable;
using System.Text;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler;

internal static partial class CompilerPipeline
{
    private static ImmutableArray<FieldFormatIr> BindFieldFormats(PacketGraphManifest manifest,
        List<CompilerDiagnostic> diagnostics, Dictionary<(WireFormat Format, int? Capacity), WireFormatIr> cache,
        HashSet<WireFormat> active, Dictionary<Type, bool> externalModels, int? bodyCapacity)
    {
        var result = ImmutableArray.CreateBuilder<FieldFormatIr>();
        foreach (var binding in manifest.FieldFormats)
        {
            if (!manifest.WireFields.Contains(binding.Field) || binding.Format.ValueType != binding.Field.ValueType)
            {
                Add(diagnostics, "CPK170", DiagnosticPhase.Bind, "A format must bind a matching field of this graph.",
                    manifest, binding.Field.SourceLocation);
                continue;
            }
            if ((binding.Format.Directions & manifest.CodecDirections) != manifest.CodecDirections)
                Add(diagnostics, "CPK171", DiagnosticPhase.Bind, "The field format does not support every requested codec direction.",
                    manifest, binding.Field.SourceLocation);
            if (manifest.CodecFieldOwners.Any(owner => ReferenceEquals(owner.Field, binding.Field))
                || manifest.Edges.Any(edge => edge.Kind == PacketGraphEdgeKind.CodecOutput
                    && ReferenceEquals(edge.Target, binding.Field)))
                Add(diagnostics, "CPK172", DiagnosticPhase.Bind, "A formatted field cannot also be carried by another codec.",
                    manifest, binding.Field.SourceLocation);
            var format = BindFormat(binding.Format, manifest, diagnostics, cache, active, externalModels, bodyCapacity);
            if (format is not null) result.Add(new(binding.Field, format));
        }
        return result.ToImmutable();
    }

    private static WireFormatIr? BindFormat(WireFormat format, PacketGraphManifest owner,
        List<CompilerDiagnostic> diagnostics, Dictionary<(WireFormat Format, int? Capacity), WireFormatIr> cache,
        HashSet<WireFormat> active, Dictionary<Type, bool> externalModels, int? bodyCapacity)
    {
        if (cache.TryGetValue((format, bodyCapacity), out var existing)) return existing;
        if (!active.Add(format))
        {
            Add(diagnostics, "CPK174", DiagnosticPhase.Validate,
                "Recursive format references require a named codec rather than an expanded static format graph.",
                owner, owner.SourceLocation);
            return null;
        }
        try
        {
            if (format.Definition is { } definition
                && (!definition.DependencyModel.Inputs.IsEmpty
                    || definition.ExternalDependencyModel is not null
                    || !definition.ExternalDependencies.IsEmpty))
            {
                Add(diagnostics, "CPK173", DiagnosticPhase.Bind,
                    "A nested format cannot declare external protocol inputs or an external dependency model. Declare its context-dependent codec on the owning packet.",
                    owner, owner.SourceLocation);
                return null;
            }
            if (format.LengthRange is { } range
                && (range.MinimumLength < 0 || range.MaximumLength < range.MinimumLength))
            {
                Add(diagnostics, "CPK175", DiagnosticPhase.Validate, "The format byte range is invalid.", owner, owner.SourceLocation);
                return null;
            }
            PacketIr? structure = null;
            if (format.Kind == WireFormatKind.Codec)
            {
                if (ReferenceEquals(format.WriteMethod, CompilerMethodCatalog.Unresolved)
                    || ReferenceEquals(format.ReadMethod, CompilerMethodCatalog.Unresolved))
                {
                    Add(diagnostics, "CPK184", DiagnosticPhase.Bind,
                        "Format codecs require generated declaration metadata.", owner, owner.SourceLocation);
                    return null;
                }
                var write = CodecRefNode.Create(format.WriteMethod, owner.SourceLocation, format.LengthRange);
                var read = CodecRefNode.Create(format.ReadMethod, owner.SourceLocation, format.LengthRange);
                ValidateCodecRefMethod(owner, write, diagnostics);
                ValidateCodecRefMethod(owner, read, diagnostics);
                var valueType = TypeRef.From(format.ValueType);
                if (write.Direction != CodecDirection.Encode || read.Direction != CodecDirection.Decode
                    || write.Method.Parameters.Length != 2 || read.Method.Parameters.Length != 1
                    || !write.Method.Parameters[1].Type.Matches(valueType)
                    || !read.Method.ReturnType.Matches(valueType))
                    Add(diagnostics, "CPK186", DiagnosticPhase.Bind,
                        "A named format requires a writer accepting its value and a reader returning that value.",
                        owner, owner.SourceLocation);
            }
            else if (format.Kind == WireFormatKind.Structure)
            {
                int? capacity = bodyCapacity;
                if (format.LengthRange is { } declaredRange)
                    capacity = Math.Min(capacity ?? declaredRange.MaximumLength, declaredRange.MaximumLength);
                structure = BindAndValidate(format.Definition!, diagnostics, cache, active, externalModels, capacity);
                if (structure is null) return null;
                if (capacity is { } bound)
                    structure = ApplyFrameBudget(structure, format.Definition!, bound, diagnostics);
            }
            var result = new WireFormatIr(format, structure);
            cache.Add((format, bodyCapacity), result);
            return result;
        }
        finally { active.Remove(format); }
    }

    private static void ValidateConstruction(PacketGraphManifest manifest, List<CompilerDiagnostic> diagnostics)
    {
        if (!manifest.CodecDirections.HasFlag(PacketCodecDirections.Decode)) return;
        if (manifest.PacketType.IsAbstract)
        {
            Add(diagnostics, "CPK177", DiagnosticPhase.Bind,
                "Decoding requires a concrete packet value type.", manifest, manifest.SourceLocation);
            return;
        }
        if (manifest.Construction is { } construction)
        {
            var arguments = construction.Arguments;
            if (construction.Constructor.DeclaringType != manifest.PacketType
                || arguments.Length != manifest.WireFields.Length
                || arguments.Distinct(ReferenceEqualityComparer.Instance).Count() != arguments.Length
                || manifest.WireFields.Any(field => !arguments.Contains(field)))
                Add(diagnostics, "CPK176", DiagnosticPhase.Bind,
                    "Constructor arguments must cover every packet field exactly once.", manifest, manifest.SourceLocation);
        }
        else if (!manifest.PacketType.IsValueType
            && manifest.PacketType.GetConstructor(Type.EmptyTypes) is null)
            Add(diagnostics, "CPK177", DiagnosticPhase.Bind,
                "Decoding requires a public parameterless constructor or an explicit BindConstructor declaration.",
                manifest, manifest.SourceLocation);
    }

    private static void AppendCompositionFingerprint(StringBuilder builder, PacketGraphManifest manifest,
        Dictionary<WireFormat, int> formatIds)
    {
        builder.Append("layout:");
        foreach (var field in manifest.WireFields) builder.Append(field.Member.Name).Append(',');
        if (manifest.Construction is { } construction)
        {
            builder.Append("constructor:").Append(construction.Constructor.DeclaringType?.AssemblyQualifiedName);
            foreach (var field in construction.Arguments) builder.Append(':').Append(field.Member.Name);
        }
        foreach (var binding in manifest.FieldFormats.OrderBy(binding => manifest.WireFields.IndexOf(binding.Field)))
        {
            builder.Append("format-field:").Append(binding.Field.Member.Name).Append(':');
            AppendFormat(binding.Format);
        }

        void AppendFormat(WireFormat format)
        {
            if (formatIds.TryGetValue(format, out int id)) { builder.Append("ref:").Append(id); return; }
            formatIds.Add(format, formatIds.Count);
            builder.Append(format.Name).Append(':').Append(format.Kind).Append(':')
                .Append(format.ValueType.AssemblyQualifiedName).Append(':').Append(format.Directions).Append(':')
                .Append(format.LengthRange?.MinimumLength).Append(':').Append(format.LengthRange?.MaximumLength);
            switch (format.Kind)
            {
                case WireFormatKind.Scalar: builder.Append(format.Scalar); break;
                case WireFormatKind.Codec:
                    AppendMethodFingerprint(builder, format.WriteMethod!);
                    AppendMethodFingerprint(builder, format.ReadMethod!);
                    break;
                case WireFormatKind.Structure:
                    AppendManifestFingerprint(builder, format.Definition!, formatIds);
                    break;
            }
            builder.Append(';');
        }
    }

    private static string TypeFingerprint(TypeRef type) => type.AssemblyName + ":" + type.CSharpName;

    private static void AppendMethodFingerprint(StringBuilder builder, MethodRef method)
    {
        builder.Append(TypeFingerprint(method.DeclaringType)).Append(':').Append(method.Name).Append(':')
            .Append(TypeFingerprint(method.ReturnType)).Append(':')
            .Append(method.IsStatic).Append(':').Append(method.IsPublic).Append(':').Append(method.IsGeneric)
            .Append(':').Append(method.IsPartialContainingType).Append(':').Append(method.IsPacketSharedFunction);
        foreach (var parameter in method.Parameters)
            builder.Append(':').Append(parameter.Name).Append(':').Append(TypeFingerprint(parameter.Type)).Append(':')
                .Append(parameter.IsByRef).Append(':').Append(parameter.IsOut).Append(':').Append(parameter.IsIn)
                .Append(':').Append(parameter.IsOptional).Append(':').Append(parameter.IsParamArray);
        builder.Append(';');
    }
}
