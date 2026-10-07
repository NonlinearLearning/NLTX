using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler;

internal static partial class CompilerPipeline
{
    public static CompilationResult Run(
        CompilationInputBundle input,
        CompilerOptions options,
        CancellationToken cancellationToken)
    {
        var diagnostics = new List<CompilerDiagnostic>();
        var packets = new List<PacketIr>();
        var formats = new Dictionary<(WireFormat Format, int? Capacity), WireFormatIr>();
        var activeFormats = new HashSet<WireFormat>(ReferenceEqualityComparer.Instance);
        var externalModels = new Dictionary<Type, bool>();

        if (input.Frame is { } frame && (frame.Capacity <= 0 || frame.HeaderBytes < 0
            || frame.HeaderBytes > frame.Capacity))
        {
            Add(diagnostics, "CPK187", DiagnosticPhase.Input,
                "A frame requires a positive capacity and a non-negative header no larger than its capacity.", null, SourceLocation.None);
            return new CompilationResult(ComputeInputFingerprint(input), [], diagnostics.ToImmutableArray(), []);
        }

        if (input.Manifests.Length == 0)
        {
            Add(
                diagnostics,
                "CPK001",
                DiagnosticPhase.Input,
                "At least one packet graph is required.",
                null,
                SourceLocation.None);
        }

        input = ResolveExternalDependencies(input, diagnostics);

        var messageIds = new Dictionary<(WireDirection Direction, byte MessageId), PacketGraphManifest>();
        var packetTypes = new Dictionary<Type, PacketGraphManifest>();
        foreach (var manifest in input.Manifests)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (manifest is null)
            {
                Add(
                    diagnostics,
                    "CPK002",
                    DiagnosticPhase.Input,
                    "A packet graph manifest cannot be null.",
                    null,
                    SourceLocation.None);
                continue;
            }

            foreach (var direction in new[] { WireDirection.ClientToServer, WireDirection.ServerToClient })
            {
                if (manifest.WireDirection.HasFlag(direction)
                    && !messageIds.TryAdd((direction, manifest.MessageId), manifest))
                {
                    Add(
                        diagnostics,
                        "CPK010",
                        DiagnosticPhase.Aggregate,
                        $"Message id {manifest.MessageId} is declared by more than one packet graph for {direction}.",
                        manifest,
                        manifest.SourceLocation);
                }
            }

            if (input.Protocol is null && !packetTypes.TryAdd(manifest.PacketType, manifest))
            {
                Add(
                    diagnostics,
                    "CPK011",
                    DiagnosticPhase.Aggregate,
                    $"Packet type '{manifest.PacketType}' is declared by more than one packet graph.",
                    manifest,
                    manifest.SourceLocation);
            }
        }

        foreach (var manifest in input.Manifests)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (manifest is null)
            {
                continue;
            }

            var packet = BindAndValidate(manifest, diagnostics, formats, activeFormats, externalModels,
                input.Frame is { } declaredFrame ? declaredFrame.Capacity - declaredFrame.HeaderBytes : null);
            if (packet is not null)
            {
                if (input.Frame is { } layout)
                    packet = ApplyFrameBudget(packet, manifest, layout.Capacity - layout.HeaderBytes, diagnostics);
                if (input.Protocol is { } protocol)
                {
                    var registration = protocol.Packets.Single(entry => ReferenceEquals(entry.Packet, manifest));
                    packet = packet with { GeneratedName = protocol.Name + "_" + registration.Name };
                }
                packets.Add(packet);
            }
        }

        var orderedDiagnostics = diagnostics
            .OrderBy(static diagnostic => diagnostic.SourceLocation.Path, StringComparer.Ordinal)
            .ThenBy(static diagnostic => diagnostic.SourceLocation.Line)
            .ThenBy(static diagnostic => diagnostic.SourceLocation.Column)
            .ThenBy(static diagnostic => diagnostic.Code, StringComparer.Ordinal)
            .ThenBy(static diagnostic => diagnostic.Message, StringComparer.Ordinal)
            .ToImmutableArray();

        if (orderedDiagnostics.Any(static diagnostic => diagnostic.IsError))
        {
            return new CompilationResult(
                ComputeInputFingerprint(input),
                [],
                orderedDiagnostics,
                []);
        }

        var orderedPackets = packets
            .OrderBy(static packet => packet.MessageId)
            .ThenBy(static packet => packet.PacketType.FullName, StringComparer.Ordinal)
            .ToImmutableArray();

        ValidateOutputNames(orderedPackets, input.Protocol, diagnostics);
        orderedDiagnostics = diagnostics
            .OrderBy(static diagnostic => diagnostic.SourceLocation.Path, StringComparer.Ordinal)
            .ThenBy(static diagnostic => diagnostic.SourceLocation.Line)
            .ThenBy(static diagnostic => diagnostic.SourceLocation.Column)
            .ThenBy(static diagnostic => diagnostic.Code, StringComparer.Ordinal)
            .ThenBy(static diagnostic => diagnostic.Message, StringComparer.Ordinal)
            .ToImmutableArray();

        if (orderedDiagnostics.Any(static diagnostic => diagnostic.IsError))
        {
            return new CompilationResult(
                ComputeInputFingerprint(input),
                [],
                orderedDiagnostics,
                []);
        }

        ImmutableArray<GeneratedArtifact> artifacts = [];
        if (options.EmitSources)
        {
            artifacts = CSharpBackend.Emit(orderedPackets);
            if (input.Protocol is { } protocol)
                artifacts = artifacts.Add(CSharpBackend.EmitProtocol(protocol, orderedPackets));
        }

        return new CompilationResult(
            ComputeInputFingerprint(input),
            orderedPackets,
            orderedDiagnostics,
            artifacts);
    }

    private static PacketIr? BindAndValidate(
        PacketGraphManifest manifest,
        List<CompilerDiagnostic> diagnostics,
        Dictionary<(WireFormat Format, int? Capacity), WireFormatIr> formatCache,
        HashSet<WireFormat> activeFormats,
        Dictionary<Type, bool> externalModels,
        int? bodyCapacity = null)
    {
        var localErrorCount = diagnostics.Count(static diagnostic => diagnostic.IsError);
        if (manifest.ExternalDependencyModel is null && !manifest.ExternalDependencies.IsEmpty)
            manifest = ResolveExternalDependencies(new CompilationInputBundle([manifest]), diagnostics).Manifests[0];
        manifest = ResolveMemberDeclarations(manifest, diagnostics);
        if (diagnostics.Count(static diagnostic => diagnostic.IsError) != localErrorCount) return null;
        if (!ValidateExternalDependencyModel(manifest, diagnostics, externalModels)) return null;
        foreach (var node in manifest.Nodes)
        {
            var method = node switch
            {
                CodecRefNode codec => codec.Method,
                PacketCalculationNode calculation => calculation.Method,
                _ => null
            };
            if (ReferenceEquals(method, CompilerMethodCatalog.Unresolved))
                Add(diagnostics, "CPK184", DiagnosticPhase.Bind,
                    "A named public static method requires generated declaration metadata. Reference the declaration generator when building its call site.",
                    manifest, node.SourceLocation);
        }
        if (diagnostics.Count(static diagnostic => diagnostic.IsError) != localErrorCount) return null;
        var declared = new HashSet<PacketGraphNode>(manifest.Nodes, ReferenceEqualityComparer.Instance);
        var fieldNodes = new List<PacketFieldNode>();
        var calculationNodes = new List<PacketCalculationNode>();
        var codecNodes = new List<CodecRefNode>();
        var fieldFormats = BindFieldFormats(manifest, diagnostics, formatCache, activeFormats, externalModels, bodyCapacity);
        var inferredRanges = new Dictionary<PacketFieldNode, ByteRange>(ReferenceEqualityComparer.Instance);
        var formattedFields = manifest.FieldFormats.Select(static binding => binding.Field)
            .ToHashSet(ReferenceEqualityComparer.Instance);
        ValidateConstruction(manifest, diagnostics);

        // Fields carried by a whole-block codec have no scalar wire image of their own.
        var decodeBlockCarried = new HashSet<PacketFieldNode>(ReferenceEqualityComparer.Instance);
        var encodeBlockCarried = new HashSet<PacketFieldNode>(ReferenceEqualityComparer.Instance);
        foreach (var edge in manifest.Edges)
        {
            if (edge is null)
            {
                continue;
            }

            if (edge.Kind == PacketGraphEdgeKind.CodecOutput
                && edge.Source is CodecRefNode
                && edge.Target is PacketFieldNode output)
            {
                decodeBlockCarried.Add(output);
            }
        }

        var ownedFields = new HashSet<PacketFieldNode>(ReferenceEqualityComparer.Instance);
        foreach (var owner in manifest.CodecFieldOwners)
        {
            if (!declared.Contains(owner.Codec) || !declared.Contains(owner.Field)
                || owner.Codec.Direction != CodecDirection.Encode || !ownedFields.Add(owner.Field))
                Add(diagnostics, "CPK185", DiagnosticPhase.Validate,
                    "Each owned field must belong to this graph and have exactly one declared encode codec owner.",
                    manifest, owner.Field.SourceLocation);
            encodeBlockCarried.Add(owner.Field);
        }

        foreach (var node in manifest.Nodes)
        {
            if (node is null)
            {
                Add(diagnostics, "CPK020", DiagnosticPhase.Bind, "A graph node cannot be null.", manifest, manifest.SourceLocation);
                continue;
            }

            switch (node)
            {
                case PacketFieldNode field:
                    if (field.Member.IsStatic || field.Member.IsIndexed)
                        Add(diagnostics, "CPK190", DiagnosticPhase.Bind,
                            "A packet field must be an instance field or a non-indexed instance property.",
                            manifest, field.SourceLocation);
                    if (!field.Member.ValueType.Matches(TypeRef.From(field.ValueType)))
                        Add(diagnostics, "CPK191", DiagnosticPhase.Bind,
                            "The generated member type must match the packet field value type.",
                            manifest, field.SourceLocation);
                    if (!field.Member.DeclaringType.Matches(TypeRef.From(manifest.PacketType)))
                    {
                        Add(
                            diagnostics,
                            "CPK022",
                            DiagnosticPhase.Bind,
                            "A packet field node must reference a member declared by the graph packet type.",
                            manifest,
                            field.SourceLocation);
                    }

                    if (manifest.CodecDirections.HasFlag(PacketCodecDirections.Encode)
                        && !field.Member.IsReadable)
                    {
                        Add(
                            diagnostics,
                            "CPK023",
                            DiagnosticPhase.Bind,
                            $"Packet member '{field.Member.Name}' must be publicly readable.",
                            manifest,
                            field.SourceLocation);
                    }

                    if (manifest.CodecDirections.HasFlag(PacketCodecDirections.Decode)
                        && manifest.Construction is null && !field.Member.IsWritable)
                    {
                        Add(
                            diagnostics,
                            "CPK024",
                            DiagnosticPhase.Bind,
                            $"Packet member '{field.Member.Name}' must be publicly writable for decoding.",
                            manifest,
                            field.SourceLocation);
                    }

                    if (field.IsVariableLength)
                    {
                        ByteRange range;
                        if (field.InfersLengthRange)
                        {
                            if (bodyCapacity is null)
                                Add(diagnostics, "CPK201", DiagnosticPhase.Bind,
                                    $"Variable field '{field.Member.Name}' requires a frame capacity to infer its byte budget.",
                                    manifest, field.SourceLocation);
                            range = InferPayloadRange(field.ValueType, bodyCapacity ?? 0);
                            inferredRanges.Add(field, range);
                        }
                        else range = field.LengthRange!;
                        if (range.MinimumLength < 0
                            || range.MaximumLength < 0
                            || range.MinimumLength > range.MaximumLength)
                        {
                            Add(
                                diagnostics,
                                "CPK075",
                                DiagnosticPhase.Validate,
                                $"Variable packet field '{field.Member.Name}' must have a non-negative minimum length not greater than its maximum length.",
                                manifest,
                                field.SourceLocation);
                        }

                        bool isBytes = field.ValueType == typeof(ReadOnlyMemory<byte>);
                        bool isString = field.ValueType == typeof(string);
                        if (isBytes && range.MaximumLength > ushort.MaxValue)
                        {
                            Add(
                                diagnostics,
                                "CPK076",
                                DiagnosticPhase.Validate,
                                $"Variable byte field '{field.Member.Name}' exceeds the current backend's UInt16 length capacity.",
                                manifest,
                                field.SourceLocation);
                        }

                        if (!isBytes && !isString)
                        {
                            Add(
                                diagnostics,
                                "CPK077",
                                DiagnosticPhase.Bind,
                                $"Variable packet field '{field.Member.Name}' must be ReadOnlyMemory<byte> or string.",
                                manifest,
                                field.SourceLocation);
                        }
                    }
                    else if (!TryGetScalar(field.ValueType, out _)
                        && !formattedFields.Contains(field)
                        && !CanUseBlockField(field, manifest.CodecDirections, decodeBlockCarried, encodeBlockCarried))
                    {
                        Add(
                            diagnostics,
                            "CPK025",
                            DiagnosticPhase.Bind,
                            $"Packet member '{field.Member.Name}' has no scalar or codec wire image for every declared direction.",
                            manifest,
                            field.SourceLocation);
                    }

                    fieldNodes.Add(field);
                    break;

                case PacketCalculationNode calculation:
                    calculationNodes.Add(calculation);
                    ValidateCalculationMethod(manifest, calculation, diagnostics);
                    break;

                case GameSystemInputNode input:
                    if (manifest.ExternalDependencyModel is { } modelType
                        && (input.ModelMember is not { } member
                            || !member.DeclaringType.Matches(TypeRef.From(modelType))
                            || !member.ValueType.Matches(TypeRef.From(input.ValueType))
                            || member.Kind != MemberKind.Property || !member.IsReadable
                            || member.IsStatic || member.IsIndexed))
                    {
                        Add(diagnostics, "CPK182", DiagnosticPhase.Bind,
                            "Every external input must select a matching member of this packet's external dependency model.",
                            manifest, input.SourceLocation);
                    }
                    break;

                case CodecRefNode codec:
                    codecNodes.Add(codec);
                    ValidateCodecRefMethod(manifest, codec, diagnostics);
                    if (codec.LengthRange is { } codecRange
                        && (codecRange.MinimumLength < 0
                            || codecRange.MaximumLength < 0
                            || codecRange.MinimumLength > codecRange.MaximumLength))
                    {
                        Add(
                            diagnostics,
                            "CPK146",
                            DiagnosticPhase.Validate,
                            $"Codec '{codec.Method.Name}' must have a non-negative minimum byte length not greater than its maximum byte length.",
                            manifest,
                            codec.SourceLocation);
                    }

                    PacketCodecDirections requiredDirection = codec.Direction == CodecDirection.Decode
                        ? PacketCodecDirections.Decode
                        : PacketCodecDirections.Encode;
                    if (!manifest.CodecDirections.HasFlag(requiredDirection))
                    {
                        Add(
                            diagnostics,
                            "CPK145",
                            DiagnosticPhase.Validate,
                            $"The packet does not declare {codec.Direction} support, but codec '{codec.Method.Name}' is present.",
                            manifest,
                            codec.SourceLocation);
                    }
                    break;

                default:
                    if (node.Kind == PacketGraphNodeKind.GameSystemInput)
                    {
                        break;
                    }

                    Add(
                        diagnostics,
                        "CPK026",
                        DiagnosticPhase.Bind,
                        $"Unsupported graph node type '{node.GetType()}'.",
                        manifest,
                        node.SourceLocation);
                    break;
            }
        }

        var fieldsByMember = new HashSet<(string Name, MemberKind Kind)>();
        foreach (var field in fieldNodes)
        {
            if (!fieldsByMember.Add((field.Member.Name, field.Member.Kind)))
            {
                Add(
                    diagnostics,
                    "CPK027",
                    DiagnosticPhase.Bind,
                    $"The packet member '{field.Member.Name}' is represented by more than one field node.",
                    manifest,
                    field.SourceLocation);
            }
        }

        var fields = manifest.WireFields
            .Select((node, index) =>
            {
                var valueType = node.ValueType;

                // A block-carried field legitimately has no scalar image (that is the whole
                // point: the codec owns its bytes). FieldIr still needs a ScalarKind, and
                // Byte is the inert placeholder -- both the reader and writer loops skip
                // block-carried fields entirely, so this value is never emitted. Any other
                // use of such a field IS type-checked, and must not rely on this fallback.
                var scalar = TryGetScalar(valueType, out var resolvedScalar)
                    ? resolvedScalar
                    : ScalarKind.Byte;
                return new FieldIr(
                    node,
                    node.Member,
                    scalar,
                    Nullable.GetUnderlyingType(valueType) is not null,
                    index,
                    null,
                    inferredRanges.GetValueOrDefault(node) ?? node.LengthRange,
                    node.SourceLocation);
            })
            .ToArray();

        var fieldByNode = new Dictionary<PacketFieldNode, FieldIr>(ReferenceEqualityComparer.Instance);
        var fieldOrdinal = new Dictionary<PacketFieldNode, int>(ReferenceEqualityComparer.Instance);
        foreach (var field in fields)
        {
            fieldByNode.Add(field.Node, field);
            fieldOrdinal.Add(field.Node, field.WireOrdinal);
        }
        var presenceByTarget = new Dictionary<PacketFieldNode, PresenceBindingIr>(ReferenceEqualityComparer.Instance);
        var dependencies = new List<DependencyIr>();

        foreach (var edge in manifest.Edges)
        {
            if (edge is null || !declared.Contains(edge.Source) || !declared.Contains(edge.Target))
            {
                Add(
                    diagnostics,
                    "CPK030",
                    DiagnosticPhase.Bind,
                    "Every edge endpoint must be one of the exact node objects declared by the graph.",
                    manifest,
                    edge?.SourceLocation ?? manifest.SourceLocation);
                continue;
            }

            switch (edge.Kind)
            {
                case PacketGraphEdgeKind.Presence:
                    ValidatePresenceEdge(
                        manifest,
                        edge,
                        fieldByNode,
                        fieldOrdinal,
                        presenceByTarget,
                        diagnostics);
                    dependencies.Add(new DependencyIr(
                        edge.Source,
                        edge.Target,
                        edge.Kind,
                        edge.PresenceBit,
                        edge.Ordinal,
                        edge.SourceLocation));
                    break;

                case PacketGraphEdgeKind.CalculationInput:
                    ValidateCalculationInputEdge(
                        manifest,
                        edge,
                        calculationNodes,
                        fieldByNode,
                        diagnostics);
                    dependencies.Add(new DependencyIr(
                        edge.Source,
                        edge.Target,
                        edge.Kind,
                        edge.PresenceBit,
                        edge.Ordinal,
                        edge.SourceLocation));
                    break;

                case PacketGraphEdgeKind.CodecInput:
                    ValidateCodecInputEdge(
                        manifest,
                        edge,
                        codecNodes,
                        calculationNodes,
                        fieldByNode,
                        diagnostics);
                    dependencies.Add(new DependencyIr(
                        edge.Source,
                        edge.Target,
                        edge.Kind,
                        edge.PresenceBit,
                        edge.Ordinal,
                        edge.SourceLocation));
                    break;

                case PacketGraphEdgeKind.CodecOutput:
                    ValidateCodecOutputEdge(
                        manifest,
                        edge,
                        codecNodes,
                        fieldByNode,
                        diagnostics);
                    dependencies.Add(new DependencyIr(
                        edge.Source,
                        edge.Target,
                        edge.Kind,
                        edge.PresenceBit,
                        edge.Ordinal,
                        edge.SourceLocation));
                    break;

                default:
                    Add(
                        diagnostics,
                        "CPK031",
                        DiagnosticPhase.Validate,
                        $"Edge kind '{edge.Kind}' is not part of the current packet graph profile.",
                        manifest,
                        edge.SourceLocation);
                    break;
            }
        }

        foreach (var field in fields.Where(static field => field.IsNullable))
        {
            bool encodedByCodec = !manifest.CodecDirections.HasFlag(PacketCodecDirections.Encode)
                || ownedFields.Contains(field.Node);
            bool decodedByCodec = !manifest.CodecDirections.HasFlag(PacketCodecDirections.Decode)
                || manifest.Edges.Any(edge => edge.Kind == PacketGraphEdgeKind.CodecOutput
                    && ReferenceEquals(edge.Target, field.Node));
            if (!presenceByTarget.ContainsKey(field.Node) && !(encodedByCodec && decodedByCodec))
            {
                Add(
                    diagnostics,
                    "CPK057",
                    DiagnosticPhase.Validate,
                    $"Nullable packet field '{field.Member.Name}' requires a Presence edge.",
                    manifest,
                    field.SourceLocation);
            }
        }

        ValidateDag(manifest, diagnostics);
        ValidateWireScopedCalculations(
            manifest,
            calculationNodes,
            diagnostics);

        var calculations = new List<PacketCalculationIr>();
        foreach (var calculation in calculationNodes)
        {
            var parameterEdges = manifest.Edges
                .Where(edge => edge.Kind == PacketGraphEdgeKind.CalculationInput
                    && ReferenceEquals(edge.Target, calculation))
                .ToArray();
            var parameters = calculation.Method.Parameters;
            var inputsByOrdinal = new Dictionary<int, PacketCalculationInputIr>();

            foreach (var edge in parameterEdges)
            {
                if (edge.Ordinal is not int ordinal || ordinal < 0)
                {
                    continue;
                }

                if (!inputsByOrdinal.TryAdd(
                        ordinal,
                        new PacketCalculationInputIr(
                            edge.Source,
                            ordinal,
                            ordinal < parameters.Length
                                ? parameters[ordinal].Type
                                : TypeRef.From(typeof(void)),
                            edge.SourceLocation)))
                {
                    Add(
                        diagnostics,
                        "CPK040",
                        DiagnosticPhase.Validate,
                        $"Calculation parameter ordinal {ordinal} is bound more than once.",
                        manifest,
                        edge.SourceLocation);
                }
            }

            if (inputsByOrdinal.Count != parameters.Length
                || inputsByOrdinal.Keys.Any(ordinal => ordinal >= parameters.Length)
                || Enumerable.Range(0, parameters.Length).Any(ordinal => !inputsByOrdinal.ContainsKey(ordinal)))
            {
                Add(
                    diagnostics,
                    "CPK041",
                    DiagnosticPhase.Validate,
                    $"Calculation '{calculation.Method.Name}' must have exactly one input edge for each parameter ordinal.",
                    manifest,
                    calculation.SourceLocation);
            }

            foreach (var pair in inputsByOrdinal)
            {
                if (pair.Key >= parameters.Length)
                {
                    continue;
                }

                var parameter = parameters[pair.Key];
                var sourceType = GetNodeValueTypeRef(pair.Value.Source);
                if (!sourceType.Matches(parameter.Type))
                {
                    Add(
                        diagnostics,
                        "CPK042",
                        DiagnosticPhase.Validate,
                        $"Calculation parameter {pair.Key} expects '{parameter.Type.Identity}', but the graph input provides '{sourceType.Identity}'.",
                        manifest,
                        pair.Value.SourceLocation);
                }
            }

            calculations.Add(new PacketCalculationIr(
                calculation,
                calculation.Method,
                inputsByOrdinal
                    .OrderBy(static pair => pair.Key)
                    .Select(static pair => pair.Value)
                    .ToImmutableArray(),
                calculation.SourceLocation));
        }

        if (diagnostics.Count(static diagnostic => diagnostic.IsError) != localErrorCount)
        {
            return null;
        }

        var boundFields = fields
            .Select(field => presenceByTarget.TryGetValue(field.Node, out var presence)
                ? field with { Presence = presence }
                : field)
            .ToImmutableArray();

        var codecs = BindCodecs(manifest, codecNodes, fields, diagnostics);
        if (diagnostics.Count(static diagnostic => diagnostic.IsError) != localErrorCount)
        {
            return null;
        }

        var decodeOps = boundFields
            .Select(static field => new DecodeFieldOperation(field.Node, field.WireOrdinal))
            .ToList();
        var encodeOps = boundFields
            .Select(static field => new EncodeFieldOperation(field.Node, field.WireOrdinal))
            .ToList();
        var decodeCodecs = codecs
            .Where(static codec => codec.Node.Direction == CodecDirection.Decode)
            .Select(static codec => new DecodeCodecOperation(codec.Node, codec.Sequence))
            .ToImmutableArray();
        var encodeCodecs = codecs
            .Where(static codec => codec.Node.Direction == CodecDirection.Encode)
            .Select(static codec => new EncodeCodecOperation(codec.Node, codec.Sequence))
            .ToImmutableArray();

        return new PacketIr(
            manifest.PacketId,
            manifest.MessageId,
            manifest.PacketType,
            manifest.CodecDirections,
            boundFields,
            dependencies.ToImmutableArray(),
            calculations.ToImmutableArray(),
            codecs,
            new LoweredPacket(
                new DecodeProjection(decodeOps.ToImmutableArray(), decodeCodecs),
                new EncodeProjection(encodeOps.ToImmutableArray(), encodeCodecs)),
            manifest.DependencyModel,
            manifest.WireDirection,
            fieldFormats,
            manifest.Construction,
            ExternalDependencyModel: manifest.ExternalDependencyModel);
    }

    private static bool ValidateExternalDependencyModel(PacketGraphManifest manifest,
        List<CompilerDiagnostic> diagnostics, Dictionary<Type, bool> models)
    {
        if (manifest.ExternalDependencyModel is not { } type) return true;
        if (models.TryGetValue(type, out bool valid)) return valid;

        var declaration = CompilerMemberCatalog.FindModel(type);
        if (declaration is null)
        {
            Add(diagnostics, "CPK192", DiagnosticPhase.Bind,
                $"External dependency model '{type}' requires generated model metadata. Reference the declaration generator in the definition project.",
                manifest, manifest.SourceLocation);
            models.Add(type, false);
            return false;
        }
        if (!declaration.IsClass || !declaration.IsPublic || declaration.IsAbstract
            || !declaration.IsSealed || declaration.ReadInstance is null)
        {
            Add(diagnostics, "CPK180", DiagnosticPhase.Bind,
                $"External dependency model '{type}' must be a public sealed class with a public static Instance getter of its own type.",
                manifest, manifest.SourceLocation);
            models.Add(type, false);
            return false;
        }

        string reason = "Instance returned null.";
        try
        {
            if (declaration.ReadInstance() is not null)
            {
                models.Add(type, true);
                return true;
            }
        }
        catch (InvalidOperationException error)
        {
            reason = error.Message;
        }

        Add(diagnostics, "CPK181", DiagnosticPhase.Bind,
            $"External dependency model '{type}' must be constructed before PacketDesignCompiler.Compile(). {reason}",
            manifest, manifest.SourceLocation);
        models.Add(type, false);
        return false;
    }

    // Binds whole-code-block codecs and assigns the unified wire sequence.
    //
    // Fields and codecs share one ordered sequence so the backend can interleave
    // reads/writes in exact wire order. A codec starts at the earliest carried
    // field's wire position. 'Sequence' on CodecRefIr is that
    // shared slot index, independent of declaration or parameter-binding order.
    private static ImmutableArray<CodecRefIr> BindCodecs(
        PacketGraphManifest manifest,
        IReadOnlyList<CodecRefNode> codecNodes,
        IReadOnlyList<FieldIr> fields,
        List<CompilerDiagnostic> diagnostics)
    {
        if (codecNodes.Count == 0)
        {
            return ImmutableArray<CodecRefIr>.Empty;
        }

        var fieldByNode = new Dictionary<PacketFieldNode, FieldIr>(ReferenceEqualityComparer.Instance);
        foreach (var field in fields)
        {
            fieldByNode[field.Node] = field;
        }

        var declaredCodecs = new HashSet<CodecRefNode>(codecNodes, ReferenceEqualityComparer.Instance);
        var bound = new List<CodecRefIr>(codecNodes.Count);

        foreach (var codec in codecNodes)
        {
            var outputEdges = manifest.Edges
                .Where(edge => edge.Kind == PacketGraphEdgeKind.CodecOutput
                    && ReferenceEquals(edge.Source, codec))
                .ToArray();
            var ownedFields = manifest.CodecFieldOwners
                .Where(owner => ReferenceEquals(owner.Codec, codec))
                .Select(static owner => owner.Field)
                .ToImmutableArray();
            var carriedFields = codec.Direction == CodecDirection.Encode
                ? ownedFields.ToArray()
                : outputEdges.Select(static edge => edge.Target).OfType<PacketFieldNode>().ToArray();
            var startFields = carriedFields.Where(fieldByNode.ContainsKey)
                .Select(field => fieldByNode[field]).ToArray();
            if (carriedFields.Length == 0)
                Add(diagnostics, "CPK193", DiagnosticPhase.Validate,
                    $"Codec '{codec.Method.Name}' requires an owned encode field or a bound decode output to infer its wire position.",
                    manifest, codec.SourceLocation);
            if (startFields.Length != carriedFields.Length)
                Add(diagnostics, "CPK123", DiagnosticPhase.Validate,
                    "Codec position inference requires carried fields declared in this graph.",
                    manifest, codec.SourceLocation);
            int startOrdinal = startFields.Length == 0 ? 0 : startFields.Min(static field => field.WireOrdinal);
            var parameterEdges = manifest.Edges
                .Where(edge => edge.Kind == PacketGraphEdgeKind.CodecInput
                    && ReferenceEquals(edge.Target, codec))
                .ToArray();
            var parameters = codec.Method.Parameters;

            // Ordinal 0 is the compiler-injected wire transport, so caller-supplied
            // inputs start at ordinal 1 and must cover 1..parameters.Length-1 exactly.
            var expectedInputCount = Math.Max(0, parameters.Length - 1);
            var inputsByOrdinal = new Dictionary<int, PacketCalculationInputIr>();
            foreach (var edge in parameterEdges)
            {
                if (edge.Ordinal is not int ordinal || ordinal < 1)
                {
                    continue;
                }

                if (!inputsByOrdinal.TryAdd(
                        ordinal,
                        new PacketCalculationInputIr(
                            edge.Source,
                            ordinal,
                            ordinal < parameters.Length
                                ? parameters[ordinal].Type
                                : TypeRef.From(typeof(void)),
                            edge.SourceLocation)))
                {
                    Add(
                        diagnostics,
                        "CPK120",
                        DiagnosticPhase.Validate,
                        $"Codec parameter ordinal {ordinal} is bound more than once.",
                        manifest,
                        edge.SourceLocation);
                }
            }

            if (inputsByOrdinal.Count != expectedInputCount
                || inputsByOrdinal.Keys.Any(ordinal => ordinal >= parameters.Length)
                || Enumerable.Range(1, expectedInputCount).Any(ordinal => !inputsByOrdinal.ContainsKey(ordinal)))
            {
                Add(
                    diagnostics,
                    "CPK121",
                    DiagnosticPhase.Validate,
                    $"Codec '{codec.Method.Name}' must have exactly one CodecInput edge for each parameter after the wire transport.",
                    manifest,
                    codec.SourceLocation);
            }

            foreach (var pair in inputsByOrdinal)
            {
                if (pair.Key >= parameters.Length)
                {
                    continue;
                }

                var parameter = parameters[pair.Key];
                var sourceType = GetNodeValueTypeRef(pair.Value.Source);
                if (!sourceType.Matches(parameter.Type))
                {
                    Add(
                        diagnostics,
                        "CPK122",
                        DiagnosticPhase.Validate,
                        $"Codec parameter {pair.Key} expects '{parameter.Type.Identity}', but the graph input provides '{sourceType.Identity}'.",
                        manifest,
                        pair.Value.SourceLocation);
                }
            }

            // A decode codec can only be handed values the reader has already produced.
            // Referencing a field that is read later would emit a use-before-read.
            if (codec.Direction == CodecDirection.Decode && startFields.Length != 0)
            {
                foreach (var edge in parameterEdges)
                {
                    IEnumerable<PacketGraphNode> fieldInputs;
                    if (edge.Source is PacketCalculationNode calculationInput)
                    {
                        fieldInputs = manifest.Edges
                            .Where(inputEdge => inputEdge.Kind == PacketGraphEdgeKind.CalculationInput
                                && ReferenceEquals(inputEdge.Target, calculationInput))
                            .Select(static inputEdge => inputEdge.Source)
                            .ToArray();
                    }
                    else
                    {
                        fieldInputs = [edge.Source];
                    }

                    foreach (var source in fieldInputs.OfType<PacketFieldNode>())
                    {
                        if (fieldByNode.TryGetValue(source, out var inputIr)
                            && inputIr.WireOrdinal >= startOrdinal)
                        {
                            Add(
                                diagnostics,
                                "CPK132",
                                DiagnosticPhase.Validate,
                                $"Decode codec '{codec.Method.Name}' depends on field '{source.Member.Name}' through an input that is decoded at or after the codec starts.",
                                manifest,
                                edge.SourceLocation);
                        }
                    }
                }
            }
            // Decoded values bind fields and replace their standalone wire bytes.
            var hasOutputs = codec.TryGetDecodeOutputs(out var outputTypes);
            var outputs = ImmutableArray<CodecOutputIr>.Empty;

            if (outputEdges.Length > 0 && !hasOutputs)
            {
                Add(
                    diagnostics,
                    "CPK133",
                    DiagnosticPhase.Validate,
                    $"Codec '{codec.Method.Name}' binds output fields but has no decoded return value.",
                    manifest,
                    codec.SourceLocation);
            }
            else if (hasOutputs)
            {
                var boundOutputs = new Dictionary<int, CodecOutputIr>();
                foreach (var edge in outputEdges)
                {
                    if (edge.Ordinal is not int ordinal || ordinal < 1)
                    {
                        continue;
                    }

                    if (ordinal > outputTypes.Length)
                    {
                        Add(
                            diagnostics,
                            "CPK134",
                            DiagnosticPhase.Validate,
                            $"Codec '{codec.Method.Name}' returns {outputTypes.Length} value(s), so output ordinal {ordinal} does not exist.",
                            manifest,
                            edge.SourceLocation);
                        continue;
                    }

                    if (edge.Target is not PacketFieldNode target)
                    {
                        continue;
                    }

                    if (!boundOutputs.TryAdd(
                            ordinal,
                            new CodecOutputIr(target, ordinal, outputTypes[ordinal - 1], edge.SourceLocation)))
                    {
                        Add(
                            diagnostics,
                            "CPK135",
                            DiagnosticPhase.Validate,
                            $"Codec output ordinal {ordinal} is bound more than once.",
                            manifest,
                            edge.SourceLocation);
                        continue;
                    }

                    var targetType = TypeRef.From(target.ValueType);
                    if (!targetType.Matches(outputTypes[ordinal - 1]))
                    {
                        Add(
                            diagnostics,
                            "CPK136",
                            DiagnosticPhase.Validate,
                            $"Codec output {ordinal} carries '{outputTypes[ordinal - 1].Identity}', but field '{target.Member.Name}' is '{targetType.Identity}'.",
                            manifest,
                            edge.SourceLocation);
                    }
                }

                outputs = boundOutputs
                    .OrderBy(static pair => pair.Key)
                    .Select(static pair => pair.Value)
                    .ToImmutableArray();

                for (var ordinal = 1; ordinal <= outputTypes.Length; ordinal++)
                {
                    if (!boundOutputs.ContainsKey(ordinal))
                    {
                        Add(
                            diagnostics,
                            "CPK137",
                            DiagnosticPhase.Validate,
                            $"Codec '{codec.Method.Name}' returns value {ordinal} but no CodecOutput edge binds it to a field.",
                            manifest,
                            codec.SourceLocation);
                    }
                }
            }

            bound.Add(new CodecRefIr(
                codec,
                codec.Method,
                inputsByOrdinal
                    .OrderBy(static pair => pair.Key)
                    .Select(static pair => pair.Value)
                    .ToImmutableArray(),
                outputs,
                startOrdinal,
                startOrdinal,
                codec.SourceLocation,
                ownedFields));
        }

        // Every field must be written by exactly one thing on the encode side. A field
        // owned by a decode codec but consumed by no encode codec would vanish from the
        // wire silently, which is the same class of bug as writing it twice.
        foreach (var field in fields)
        {
            var producingDecode = bound.Any(codec =>
                codec.Node.Direction == CodecDirection.Decode && codec.Owns(field.Node));
            if (!producingDecode || !manifest.CodecDirections.HasFlag(PacketCodecDirections.Encode))
            {
                continue;
            }

            var consumedByEncode = bound.Any(codec =>
                codec.Node.Direction == CodecDirection.Encode
                && codec.Owns(field.Node));
            if (!consumedByEncode)
            {
                Add(
                    diagnostics,
                    "CPK143",
                    DiagnosticPhase.Validate,
                    $"Field '{field.Member.Name}' is produced by a decode codec but has no explicit encode codec owner.",
                    manifest,
                    field.SourceLocation);
            }
        }

        // Two codecs cannot claim the same slot on the same direction: their byte
        // ranges would be ambiguous. Report CPK124 rather than silently ordering them.
        foreach (var directionGroup in bound.GroupBy(static codec => codec.Node.Direction))
        {
            foreach (var slotGroup in directionGroup.GroupBy(static codec => codec.Sequence))
            {
                if (slotGroup.Count() > 1)
                {
                    foreach (var duplicate in slotGroup.Skip(1))
                    {
                        Add(
                            diagnostics,
                            "CPK124",
                            DiagnosticPhase.Validate,
                            $"More than one {directionGroup.Key} codec occupies wire slot {slotGroup.Key}.",
                            manifest,
                            duplicate.SourceLocation);
                    }
                }
            }
        }

        return bound.ToImmutableArray();
    }

    private static void ValidateCodecRefMethod(
        PacketGraphManifest manifest,
        CodecRefNode codec,
        List<CompilerDiagnostic> diagnostics)
    {
        var method = codec.Method;
        var transportType = TypeRef.From(codec.Direction == CodecDirection.Decode
            ? typeof(Wire.PacketWireReader)
            : typeof(Wire.PacketWireWriter));

        if (!method.IsStatic || !method.IsPublic || method.IsGeneric)
        {
            Add(
                diagnostics,
                "CPK125",
                DiagnosticPhase.Bind,
                $"Codec '{method.Name}' must be a public static non-generic method.",
                manifest,
                codec.SourceLocation);
        }

        if (method.Parameters.IsDefaultOrEmpty || method.Parameters[0].Type != transportType)
        {
            Add(
                diagnostics,
                "CPK126",
                DiagnosticPhase.Bind,
                $"Codec '{method.Name}' must take '{transportType.Identity}' as its first parameter.",
                manifest,
                codec.SourceLocation);
        }

        // Transport positions measure byte lengths. Writers return void;
        // readers return decoded data.
        if (codec.Direction == CodecDirection.Encode
            && method.ReturnType != TypeRef.From(typeof(void)))
        {
            Add(
                diagnostics,
                "CPK127",
                DiagnosticPhase.Bind,
                $"Encode codec '{method.Name}' must return void; byte counts are measured by the wire transport.",
                manifest,
                codec.SourceLocation);
        }

        foreach (var parameter in method.Parameters)
        {
            if (parameter.IsByRef
                || parameter.IsOut
                || parameter.IsIn
                || parameter.IsOptional
                || parameter.IsParamArray)
            {
                Add(
                    diagnostics,
                    "CPK128",
                    DiagnosticPhase.Validate,
                    $"Codec parameter '{parameter.Name}' must be a required by-value parameter.",
                    manifest,
                    codec.SourceLocation);
            }
        }
    }

    private static bool CanUseBlockField(
        PacketFieldNode field,
        PacketCodecDirections directions,
        IReadOnlySet<PacketFieldNode> decodeBlockCarried,
        IReadOnlySet<PacketFieldNode> encodeBlockCarried)
    {
        bool decodeCovered = !directions.HasFlag(PacketCodecDirections.Decode)
            || decodeBlockCarried.Contains(field);
        bool encodeCovered = !directions.HasFlag(PacketCodecDirections.Encode)
            || encodeBlockCarried.Contains(field);
        return decodeCovered && encodeCovered;
    }

    private static void ValidateCodecInputEdge(
        PacketGraphManifest manifest,
        PacketGraphEdge edge,
        IReadOnlyList<CodecRefNode> codecs,
        IReadOnlyList<PacketCalculationNode> calculations,
        IReadOnlyDictionary<PacketFieldNode, FieldIr> fields,
        List<CompilerDiagnostic> diagnostics)
    {
        if (edge.Target is not CodecRefNode codec || !codecs.Contains(codec, ReferenceEqualityComparer.Instance))
        {
            Add(diagnostics, "CPK129", DiagnosticPhase.Validate, "CodecInput edges must target a declared codec node.", manifest, edge.SourceLocation);
        }

        if (edge.Ordinal is null or < 1)
        {
            Add(diagnostics, "CPK130", DiagnosticPhase.Validate, "CodecInput edges require a parameter ordinal of at least 1 (ordinal 0 is the wire transport).", manifest, edge.SourceLocation);
        }

        // Packet fields are read from the value under construction; system inputs become
        // explicit reader/writer parameters; calculations are evaluated at this codec call.
        if (edge.Source is not PacketFieldNode and not GameSystemInputNode and not PacketCalculationNode)
        {
            Add(diagnostics, "CPK131", DiagnosticPhase.Validate, "CodecInput edges must source a packet field, a game-system input, or a packet calculation.", manifest, edge.SourceLocation);
        }

        if (edge.Source is PacketFieldNode inputField && !fields.ContainsKey(inputField))
        {
            Add(diagnostics, "CPK143", DiagnosticPhase.Validate, "The codec input field is not bound to this packet graph.", manifest, edge.SourceLocation);
        }

        if (edge.Source is PacketCalculationNode calculation && !calculations.Contains(calculation, ReferenceEqualityComparer.Instance))
        {
            Add(diagnostics, "CPK144", DiagnosticPhase.Validate, "The codec input calculation is not declared in this packet graph.", manifest, edge.SourceLocation);
        }
    }

    private static void ValidateWireScopedCalculations(
        PacketGraphManifest manifest,
        IReadOnlyList<PacketCalculationNode> calculations,
        List<CompilerDiagnostic> diagnostics)
    {
        foreach (var calculation in calculations)
        {
            var codecConsumers = manifest.Edges.Count(edge =>
                edge.Kind == PacketGraphEdgeKind.CodecInput
                && ReferenceEquals(edge.Source, calculation));
            if (codecConsumers != 1)
            {
                Add(
                    diagnostics,
                    "CPK045",
                    DiagnosticPhase.Validate,
                    $"Calculation '{calculation.Method.Name}' must feed exactly one CodecRef input; detached or shared calculations are outside the protocol model.",
                    manifest,
                    calculation.SourceLocation);
            }
        }
    }

    private static void ValidateCodecOutputEdge(
        PacketGraphManifest manifest,
        PacketGraphEdge edge,
        IReadOnlyList<CodecRefNode> codecs,
        IReadOnlyDictionary<PacketFieldNode, FieldIr> fields,
        List<CompilerDiagnostic> diagnostics)
    {
        // Unlike an input, an output must come from a decode codec: an encode codec only
        // consumes values, so a field it "produces" would have no writer.
        if (edge.Source is not CodecRefNode codec
            || !codecs.Contains(codec, ReferenceEqualityComparer.Instance))
        {
            Add(diagnostics, "CPK139", DiagnosticPhase.Validate, "CodecOutput edges must source a declared codec node.", manifest, edge.SourceLocation);
        }
        else if (codec.Direction != CodecDirection.Decode)
        {
            Add(diagnostics, "CPK140", DiagnosticPhase.Validate, "CodecOutput edges are only valid on a decode codec; an encode codec cannot produce a field.", manifest, edge.SourceLocation);
        }

        if (edge.Ordinal is null or < 1)
        {
            Add(diagnostics, "CPK141", DiagnosticPhase.Validate, "CodecOutput edges require an output ordinal of at least 1.", manifest, edge.SourceLocation);
        }

        if (edge.Target is not PacketFieldNode target || !fields.ContainsKey(target))
        {
            Add(diagnostics, "CPK142", DiagnosticPhase.Validate, "CodecOutput edges must target a packet field node bound to this graph.", manifest, edge.SourceLocation);
        }
    }

    private static void ValidateCalculationMethod(
        PacketGraphManifest manifest,
        PacketCalculationNode calculation,
        List<CompilerDiagnostic> diagnostics)
    {
        var method = calculation.Method;
        if (!method.IsStatic
            || !method.IsPublic
            || method.IsGeneric
            || method.ReturnType == TypeRef.From(typeof(void)))
        {
            Add(
                diagnostics,
                "CPK043",
                DiagnosticPhase.Bind,
                $"Calculation '{method.Name}' must be a public static non-generic method with a return value.",
                manifest,
                calculation.SourceLocation);
        }

        foreach (var parameter in method.Parameters)
        {
            if (parameter.IsByRef
                || parameter.IsOut
                || parameter.IsIn
                || parameter.IsOptional
                || parameter.IsParamArray)
            {
                Add(
                    diagnostics,
                    "CPK044",
                    DiagnosticPhase.Validate,
                    $"Calculation parameter '{parameter.Name}' must be a required by-value parameter.",
                    manifest,
                    calculation.SourceLocation);
            }
        }
    }

    private static void ValidatePresenceEdge(
        PacketGraphManifest manifest,
        PacketGraphEdge edge,
        IReadOnlyDictionary<PacketFieldNode, FieldIr> fieldByNode,
        IReadOnlyDictionary<PacketFieldNode, int> fieldOrdinal,
        IDictionary<PacketFieldNode, PresenceBindingIr> presenceByTarget,
        List<CompilerDiagnostic> diagnostics)
    {
        if (edge.Source is not PacketFieldNode source
            || edge.Target is not PacketFieldNode target
            || !fieldByNode.ContainsKey(source)
            || !fieldByNode.ContainsKey(target))
        {
            Add(diagnostics, "CPK050", DiagnosticPhase.Validate, "Presence edges must connect packet field nodes.", manifest, edge.SourceLocation);
            return;
        }

        if (!presenceByTarget.TryAdd(target, new PresenceBindingIr(source, target, edge.PresenceBit, edge.SourceLocation)))
        {
            Add(diagnostics, "CPK051", DiagnosticPhase.Validate, "A nullable packet field can have only one presence source.", manifest, edge.SourceLocation);
        }

        if (Nullable.GetUnderlyingType(target.ValueType) is null)
        {
            Add(diagnostics, "CPK052", DiagnosticPhase.Validate, "A presence edge target must be nullable.", manifest, edge.SourceLocation);
        }

        if (fieldOrdinal[source] >= fieldOrdinal[target])
        {
            Add(diagnostics, "CPK053", DiagnosticPhase.Validate, "A presence source must occur before its nullable field on the wire.", manifest, edge.SourceLocation);
        }

        // An absent nullable mask cannot claim that its dependent field is present.
        var underlyingSource = Nullable.GetUnderlyingType(source.ValueType) ?? source.ValueType;

        // A non-scalar source has no bit image at all, so it can never act as a presence
        // gate. The scalar lookup must be checked for SUCCESS rather than folded to a
        // default: 'TryGetScalar(...) ? scalar : ScalarKind.Byte' would classify a
        // NetworkText as a 1-byte integer and let the graph through, producing a
        // generated reader that casts it to ulong and does not compile.
        if (!ScalarKinds.TryFromType(underlyingSource, out var sourceScalar)
            || !ScalarKinds.IsInteger(sourceScalar))
        {
            Add(
                diagnostics,
                "CPK054",
                DiagnosticPhase.Validate,
                $"An MSK source must be an integer scalar, but '{source.Member.Name}' is '{source.ValueType}'.",
                manifest,
                edge.SourceLocation);
            return;
        }

        if (edge.PresenceBit is null or < 0 || edge.PresenceBit >= ScalarKinds.Width(sourceScalar) * 8)
        {
            Add(diagnostics, "CPK056", DiagnosticPhase.Validate, "Integer presence sources require a bit index within the source width.", manifest, edge.SourceLocation);
        }
    }

    private static void ValidateCalculationInputEdge(
        PacketGraphManifest manifest,
        PacketGraphEdge edge,
        IReadOnlyList<PacketCalculationNode> calculations,
        IReadOnlyDictionary<PacketFieldNode, FieldIr> fields,
        List<CompilerDiagnostic> diagnostics)
    {
        if (edge.Target is not PacketCalculationNode calculation || !calculations.Contains(calculation, ReferenceEqualityComparer.Instance))
        {
            Add(diagnostics, "CPK060", DiagnosticPhase.Validate, "CalculationInput edges must target a declared calculation node.", manifest, edge.SourceLocation);
        }

        if (edge.Source is not PacketFieldNode && edge.Source.Kind != PacketGraphNodeKind.GameSystemInput)
        {
            Add(diagnostics, "CPK061", DiagnosticPhase.Validate, "CalculationInput edges must source a packet field or a game-system input node.", manifest, edge.SourceLocation);
        }

        if (edge.Source is PacketFieldNode field && !fields.ContainsKey(field))
        {
            Add(diagnostics, "CPK062", DiagnosticPhase.Validate, "The calculation input field is not bound to this packet graph.", manifest, edge.SourceLocation);
        }

        if (edge.Ordinal is null or < 0)
        {
            Add(diagnostics, "CPK063", DiagnosticPhase.Validate, "CalculationInput edges require a non-negative parameter ordinal.", manifest, edge.SourceLocation);
        }
    }

    private static void ValidateDag(PacketGraphManifest manifest, List<CompilerDiagnostic> diagnostics)
    {
        var adjacency = new Dictionary<PacketGraphNode, List<PacketGraphNode>>(ReferenceEqualityComparer.Instance);
        foreach (var node in manifest.Nodes)
        {
            adjacency[node] = [];
        }

        foreach (var edge in manifest.Edges)
        {
            if (adjacency.TryGetValue(edge.Source, out var targets))
            {
                targets.Add(edge.Target);
            }
        }

        var visiting = new HashSet<PacketGraphNode>(ReferenceEqualityComparer.Instance);
        var visited = new HashSet<PacketGraphNode>(ReferenceEqualityComparer.Instance);
        foreach (var node in manifest.Nodes)
        {
            if (Visit(node))
            {
                Add(diagnostics, "CPK070", DiagnosticPhase.Validate, "The packet graph contains a cycle.", manifest, node.SourceLocation);
                return;
            }
        }

        bool Visit(PacketGraphNode node)
        {
            if (visited.Contains(node))
            {
                return false;
            }

            if (!visiting.Add(node))
            {
                return true;
            }

            foreach (var target in adjacency[node])
            {
                if (Visit(target))
                {
                    return true;
                }
            }

            visiting.Remove(node);
            visited.Add(node);
            return false;
        }
    }

    private static TypeRef GetNodeValueTypeRef(PacketGraphNode node) => node switch
    {
        PacketFieldNode field => TypeRef.From(field.ValueType),
        GameSystemInputNode input => TypeRef.From(input.ValueType),
        PacketCalculationNode calculation => calculation.Method.ReturnType,
        _ => TypeRef.From(typeof(void))
    };

    private static bool TryGetScalar(Type type, out ScalarKind scalar)
    {
        try
        {
            scalar = ScalarKinds.FromType(type);
            return true;
        }
        catch (ArgumentException)
        {
            scalar = default;
            return false;
        }
    }

    private static void ValidateOutputNames(
        ImmutableArray<PacketIr> packets,
        ProtocolManifest? protocol,
        List<CompilerDiagnostic> diagnostics)
    {
        var hints = new HashSet<string>(StringComparer.Ordinal);
        var types = new HashSet<string>(StringComparer.Ordinal);
        if (protocol is not null)
        {
            hints.Add(protocol.Name + "Protocol.g.cs");
            types.Add(protocol.Name + "Protocol");
        }
        foreach (var packet in packets)
        {
            var hint = CSharpBackend.GetHintName(packet);
            if (!hints.Add(hint))
            {
                Add(diagnostics, "CPK110", DiagnosticPhase.Emit, $"Generated hint name '{hint}' is duplicated.", null, SourceLocation.None);
            }
            foreach (string type in CSharpBackend.GetGeneratedTypeNames(packet))
            {
                if (!types.Add(type.TrimStart('@')))
                    Add(diagnostics, "CPK111", DiagnosticPhase.Emit, $"Generated type name '{type}' is duplicated.", null, SourceLocation.None);
            }

        }
    }

    private static string ComputeInputFingerprint(CompilationInputBundle input)
    {
        var builder = new StringBuilder();
        var formatIds = new Dictionary<WireFormat, int>(ReferenceEqualityComparer.Instance);
        if (input.Frame is { } frame)
            builder.Append("frame:").Append(frame.Capacity).Append(':').Append(frame.HeaderBytes).Append(';');
        if (input.Protocol is { } protocol)
        {
            builder.Append("protocol:").Append(protocol.Name).Append(':').Append(protocol.Version).Append(';');
            foreach (var entry in protocol.Packets)
                builder.Append(entry.Name).Append(':').Append(entry.Packet.PacketId).Append(';');
        }
        foreach (var manifest in input.Manifests.OrderBy(static manifest => manifest.MessageId))
        {
            AppendManifestFingerprint(builder, manifest, formatIds);
        }

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(builder.ToString())));
    }

    private static void AppendManifestFingerprint(StringBuilder builder, PacketGraphManifest manifest,
        Dictionary<WireFormat, int> formatIds)
    {
        AppendCompositionFingerprint(builder, manifest, formatIds);
        AppendFunctionDeclarationFingerprint(builder, manifest);
        builder.Append(manifest.MessageId).Append('|')
            .Append(manifest.WireDirection).Append('|')
            .Append(manifest.CodecDirections).Append('|')
            .Append(manifest.PacketType.AssemblyQualifiedName).Append('|');
        builder.Append("external-model:").Append(manifest.ExternalDependencyModel?.AssemblyQualifiedName).Append(';');
        foreach (var dependency in manifest.DependencyModel.Inputs)
            builder.Append("dependency:").Append(manifest.Nodes.IndexOf(dependency.Node))
                .Append(':').Append(dependency.Ordinal).Append(';');
        foreach (var node in manifest.Nodes)
        {
            builder.Append(node.Kind).Append('|').Append(node.GetType().AssemblyQualifiedName).Append('|');
            if (node is PacketFieldNode field)
            {
                builder.Append(TypeFingerprint(field.Member.DeclaringType)).Append(':')
                    .Append(field.Member.Name).Append(':')
                    .Append(field.Member.Kind).Append(':')
                    .Append(TypeFingerprint(field.Member.ValueType)).Append(':')
                    .Append(field.Member.IsReadable).Append(':').Append(field.Member.IsWritable).Append(':')
                    .Append(field.Member.IsStatic).Append(':').Append(field.Member.IsIndexed).Append(':')
                    .Append(TypeFingerprint(TypeRef.From(field.ValueType))).Append(':')
                    .Append(field.LengthRange?.MinimumLength).Append(':')
                    .Append(field.LengthRange?.MaximumLength);
                builder.Append(":infer-range:").Append(field.InfersLengthRange);
            }
            else if (node is PacketCalculationNode calculation)
            {
                AppendMethodFingerprint(builder, calculation.Method);
            }
            else if (node is CodecRefNode codec)
            {
                builder.Append(codec.Direction).Append(':');
                AppendMethodFingerprint(builder, codec.Method);
                builder.Append(':').Append(codec.LengthRange?.MinimumLength).Append(':')
                    .Append(codec.LengthRange?.MaximumLength);
                // The value outlet is part of the signature, so it belongs in the
                // fingerprint: two codecs with the same inputs but different output
                // shapes generate different readers.
                if (codec.TryGetDecodeOutputs(out var outputs))
                {
                    foreach (var output in outputs)
                    {
                        builder.Append("->").Append(TypeFingerprint(output));
                    }
                }
            }
            else if (node is GameSystemInputNode input)
            {
                builder.Append(TypeFingerprint(TypeRef.From(input.ValueType)));
                if (input.ModelMember is { } member)
                    builder.Append(":member:").Append(TypeFingerprint(member.DeclaringType))
                        .Append(':').Append(member.Name).Append(':').Append(member.Kind)
                        .Append(':').Append(TypeFingerprint(member.ValueType))
                        .Append(':').Append(member.IsReadable).Append(':').Append(member.IsWritable)
                        .Append(':').Append(member.IsStatic).Append(':').Append(member.IsIndexed);
            }

            builder.Append(';');
        }

        foreach (var edge in manifest.Edges)
        {
            builder.Append(edge.Kind).Append(':')
                .Append(manifest.Nodes.IndexOf(edge.Source)).Append(':')
                .Append(manifest.Nodes.IndexOf(edge.Target)).Append(':')
                .Append(edge.PresenceBit).Append(':')
                .Append(edge.Ordinal).Append(';');
        }

        foreach (var owner in manifest.CodecFieldOwners)
        {
            builder.Append("Owns:")
                .Append(manifest.Nodes.IndexOf(owner.Codec)).Append(':')
                .Append(manifest.Nodes.IndexOf(owner.Field)).Append(';');
        }
    }

    private static void Add(
        List<CompilerDiagnostic> diagnostics,
        string code,
        DiagnosticPhase phase,
        string message,
        PacketGraphManifest? manifest,
        SourceLocation location)
    {
        diagnostics.Add(new CompilerDiagnostic(code, DiagnosticSeverity.Error, phase, message, manifest?.PacketId, location));
    }
}
