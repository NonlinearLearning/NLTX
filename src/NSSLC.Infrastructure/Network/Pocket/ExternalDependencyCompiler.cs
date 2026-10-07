using System.Collections.Immutable;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler;

internal static partial class CompilerPipeline
{
    private static CompilationInputBundle ResolveExternalDependencies(
        CompilationInputBundle input, List<CompilerDiagnostic> diagnostics)
    {
        var models = input.Manifests.Where(static manifest => manifest is not null)
            .Select(static manifest => manifest.ExternalDependencyModel).OfType<Type>().Distinct().ToArray();
        if (input.Protocol is not null && models.Length > 1)
            Add(diagnostics, "CPK198", DiagnosticPhase.Aggregate,
                "A protocol must use one global external dependency model.", null, SourceLocation.None);

        var manifests = input.Manifests.Select(manifest =>
        {
            if (manifest is null || manifest.ExternalDependencies.IsEmpty) return manifest!;
            if (manifest.ExternalDependencies.Length != 1)
                Add(diagnostics, "CPK198", DiagnosticPhase.Validate,
                    "ExternalDependencies may be declared only once per packet.", manifest, manifest.SourceLocation);
            var members = manifest.ExternalDependencies.SelectMany(static list => list).ToArray();
            if (members.Select(static member => member.Member).Distinct().Count() != members.Length)
                Add(diagnostics, "CPK198", DiagnosticPhase.Validate,
                    "ExternalDependencies must contain distinct model members.", manifest, manifest.SourceLocation);
            if (members.Select(static member => member.DeclaringType).Distinct().Count() > 1)
                Add(diagnostics, "CPK182", DiagnosticPhase.Bind,
                    "External dependencies must belong to one model.", manifest, manifest.SourceLocation);

            Type? model = manifest.ExternalDependencyModel;
            if (model is null)
            {
                var candidates = models.Length == 1 ? models
                    : CompilerMemberCatalog.FindModels(TypeRef.From(manifest.PacketType).AssemblyName);
                if (candidates.Length == 1) model = candidates[0];
                else
                    Add(diagnostics, "CPK198", DiagnosticPhase.Bind,
                        "An empty ExternalDependencies declaration requires one model in the protocol or generated assembly declarations.",
                        manifest, manifest.SourceLocation);
            }
            return CopyManifest(manifest, model);
        }).ToImmutableArray();

        if (input.Protocol is not { } protocol) return new CompilationInputBundle(manifests, input.Frame);
        var definitions = protocol.Packets.Select((entry, index) => new ProtocolPacketDefinition(entry.Name, manifests[index]))
            .ToImmutableArray();
        return new CompilationInputBundle(new ProtocolManifest(protocol.Name, protocol.Version, definitions, protocol.Frame));
    }

    private static PacketGraphManifest CopyManifest(PacketGraphManifest manifest, Type? model) => new(
        manifest.MessageId, manifest.PacketType, manifest.CodecDirections, manifest.Nodes, manifest.Edges,
        manifest.SourceLocation, manifest.WireDirection, manifest.CodecFieldOwners, manifest.DependencyModel,
        manifest.WireFields, manifest.FieldFormats, manifest.Construction, model, manifest.Functions,
        manifest.MemberPresence, manifest.ExternalDependencies);
}
