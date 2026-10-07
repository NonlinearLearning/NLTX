using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler;

internal static partial class CSharpBackend
{
    public static string GetHintName(PacketIr packet) =>
        $"Packet_{packet.MessageId}_{GeneratedBaseName(packet)}.g.cs";

    public static IEnumerable<string> GetGeneratedTypeNames(PacketIr packet)
    {
        foreach (var generated in GetGeneratedPackets(packet))
        {
            string name = GeneratedBaseName(generated);
            if (generated.CodecDirections.HasFlag(PacketCodecDirections.Decode)) yield return name + "PacketCodecReader";
            if (generated.CodecDirections.HasFlag(PacketCodecDirections.Encode)) yield return name + "PacketCodecWriter";
            if (!generated.FieldFormats.IsDefaultOrEmpty) yield return FormatHelperName(generated);
        }
    }

    public static IEnumerable<PacketIr> GetGeneratedPackets(PacketIr packet)
    {
        yield return packet;
        string name = GeneratedBaseName(packet);
        var formats = new FormatEmission(packet);
        for (int index = 0; index < formats.Formats.Count; index++)
        {
            if (formats.Formats[index].Structure is not { } structure) continue;
            foreach (var nested in GetGeneratedPackets(structure with { GeneratedName = name + "_Format" + index }))
                yield return nested;
        }
    }

    public static ImmutableArray<GeneratedArtifact> Emit(ImmutableArray<PacketIr> packets)
    {
        return packets
            .OrderBy(static packet => packet.MessageId)
            .Select(packet =>
            {
                var source = EmitPacket(packet);
                return new GeneratedArtifact(
                    GetHintName(packet),
                    packet.PacketId,
                    source,
                    Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(source))));
            })
            .ToImmutableArray();
    }

    private static string EmitPacket(PacketIr packet, bool includeHeader = true)
    {
        var builder = new StringBuilder();
        if (includeHeader)
        {
            AppendLine(builder, "#nullable enable");
            AppendLine(builder, "using System;");
            AppendLine(builder, "using System.IO;");
            AppendLine(builder, "using System.Buffers.Binary;");
            AppendLine(builder, "using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;");
            AppendLine(builder, string.Empty);
            AppendLine(builder, "namespace Terraria.NetWork.Generated;");
            AppendLine(builder, string.Empty);
        }
        var packetName = TypeName(packet.PacketType);
        var generatedName = GeneratedBaseName(packet) + "PacketCodec";
        if (packet.CodecDirections.HasFlag(PacketCodecDirections.Decode))
        {
            EmitReader(builder, packet, packetName, generatedName + "Reader");
        }

        if (packet.CodecDirections.HasFlag(PacketCodecDirections.Encode))
        {
            if (packet.CodecDirections.HasFlag(PacketCodecDirections.Decode))
            {
                AppendLine(builder, string.Empty);
            }

            EmitWriter(builder, packet, packetName, generatedName + "Writer");
        }

        EmitFormats(builder, packet);
        return builder.ToString();
    }

    private static string GeneratedBaseName(PacketIr packet) => Identifier(packet.GeneratedName ?? packet.PacketType.Name);

    private static string InputName(PacketProtocolDependency input) => "input" + input.Ordinal;

    private static ImmutableArray<PacketProtocolDependency> RequiredProtocolDependencies(
        PacketIr packet, CodecDirection direction)
    {
        var required = new HashSet<PacketGraphNode>(ReferenceEqualityComparer.Instance);
        var calculations = CalculationMap(packet);
        foreach (var codec in packet.Codecs.Where(codec => codec.Node.Direction == direction))
        {
            foreach (var input in codec.Inputs)
            {
                if (input.Source is GameSystemInputNode) required.Add(input.Source);
                else if (input.Source is PacketCalculationNode calculation)
                    foreach (var argument in calculations[calculation].Inputs)
                        if (argument.Source is GameSystemInputNode) required.Add(argument.Source);
            }
        }
        return packet.DependencyModel.Inputs.Where(input => required.Contains(input.Node)).ToImmutableArray();
    }

    private static ImmutableArray<PacketProtocolDependency> ProtocolConstructorInputs(
        PacketIr packet, CodecDirection direction) => RequiredProtocolDependencies(packet, direction)
            .Where(static input => input.Node.ModelMember is null).ToImmutableArray();

    private static Dictionary<PacketGraphNode, string> CodecExternalParameterNames(
        ImmutableArray<PacketProtocolDependency> parameters) => parameters.ToDictionary(
            static input => (PacketGraphNode)input.Node, static input => "_" + InputName(input));

    private static string ProtocolParameterList(ImmutableArray<PacketProtocolDependency> parameters) =>
        string.Join(", ", parameters.Select(static input => input.ValueType.CSharpName + " " + InputName(input)));

    private static string ProtocolArgumentList(ImmutableArray<PacketProtocolDependency> parameters) =>
        string.Join(", ", parameters.Select(InputName));

    private static void EmitProtocolInputFields(StringBuilder builder,
        ImmutableArray<PacketProtocolDependency> parameters)
    {
        foreach (var input in parameters)
            AppendLine(builder, $"    private readonly {input.ValueType.CSharpName} _{InputName(input)};");
    }

    private static void EmitProtocolInputAssignments(StringBuilder builder,
        PacketIr packet, ImmutableArray<PacketProtocolDependency> parameters)
    {
        if (packet.ExternalDependencyModel is { } modelType)
        {
            string target = parameters.Any(static input => input.Node.ModelMember is not null) ? "var external" : "_";
            AppendLine(builder, $"        {target} = {TypeName(modelType)}.Instance;");
        }
        foreach (var input in parameters)
        {
            string name = InputName(input);
            string value = input.Node.ModelMember is { } member ? "external." + Identifier(member.Name) : name;
            if (input.Node.ModelMember is null && !input.ValueType.IsValueType)
                AppendLine(builder, $"        ArgumentNullException.ThrowIfNull({name});");
            AppendLine(builder, $"        _{name} = {value};");
        }
    }

    private static Dictionary<PacketGraphNode, PacketCalculationIr> CalculationMap(PacketIr packet)
    {
        var calculations = new Dictionary<PacketGraphNode, PacketCalculationIr>(ReferenceEqualityComparer.Instance);
        foreach (var calculation in packet.Calculations)
        {
            calculations.Add(calculation.Node, calculation);
        }

        return calculations;
    }

    private static string CodecInputExpression(
        PacketCalculationInputIr input,
        IReadOnlyDictionary<PacketGraphNode, string> externalInputNames,
        IReadOnlyDictionary<PacketGraphNode, PacketCalculationIr> calculationsByNode,
        Func<PacketFieldNode, string> packetFieldExpression)
    {
        return input.Source switch
        {
            PacketFieldNode field => packetFieldExpression(field),
            GameSystemInputNode => externalInputNames[input.Source],
            PacketCalculationNode calculationNode => CalculationExpression(
                calculationsByNode[calculationNode],
                externalInputNames,
                packetFieldExpression),
            _ => throw new InvalidOperationException(
                "Codec inputs must be packet fields, game-system inputs, or wire-scoped calculations.")
        };
    }

    private static string CalculationExpression(
        PacketCalculationIr calculation,
        IReadOnlyDictionary<PacketGraphNode, string> externalInputNames,
        Func<PacketFieldNode, string> packetFieldExpression)
    {
        var arguments = calculation.Inputs
            .OrderBy(static input => input.Ordinal)
            .Select(input => input.Source switch
            {
                PacketFieldNode field => packetFieldExpression(field),
                GameSystemInputNode => externalInputNames[input.Source],
                _ => throw new InvalidOperationException(
                    "Calculation inputs must be packet fields or explicit system inputs.")
            });

        return $"{calculation.Method.DeclaringType.CSharpName}.{Identifier(calculation.Method.Name)}({string.Join(", ", arguments)})";
    }

    private static void EmitReader(
        StringBuilder builder,
        PacketIr packet,
        string packetName,
        string readerName)
    {
        var fields = packet.Fields.OrderBy(static field => field.WireOrdinal).ToImmutableArray();
        var codecs = packet.Codecs
            .Where(static codec => codec.Node.Direction == CodecDirection.Decode)
            .OrderBy(static codec => codec.Sequence)
            .ToImmutableArray();
        var externalInputs = RequiredProtocolDependencies(packet, CodecDirection.Decode);
        var externalInputNames = CodecExternalParameterNames(externalInputs);
        var calculationsByNode = CalculationMap(packet);

        AppendLine(builder, $"public sealed class {readerName}");
        AppendLine(builder, "{");
        AppendLine(builder, "    private readonly ReadOnlyMemory<byte> _frame;");
        EmitProtocolInputFields(builder, externalInputs);
        AppendLine(builder, "    private int _cursor;");
        AppendLine(builder, string.Empty);
        string externalParameters = ProtocolParameterList(ProtocolConstructorInputs(packet, CodecDirection.Decode));
        string constructorParameters = externalParameters.Length == 0 ? string.Empty : ", " + externalParameters;
        AppendLine(builder, $"    public {readerName}(ReadOnlyMemory<byte> frame{constructorParameters})");
        AppendLine(builder, "    {");
        AppendLine(builder, "        _frame = frame;");
        EmitProtocolInputAssignments(builder, packet, externalInputs);
        AppendLine(builder, "    }");
        AppendLine(builder, string.Empty);
        AppendLine(builder, "    // Reads one frame out of a larger receive buffer. The declared length is the");
        AppendLine(builder, "    // frame boundary: neither a scalar field nor a codec block can reach past it,");
        AppendLine(builder, "    // so a first frame can never consume bytes belonging to the second.");
        AppendLine(builder, $"    public {readerName}(ReadOnlyMemory<byte> buffer, int frameLength{constructorParameters})");
        AppendLine(builder, "    {");
        AppendLine(builder, "        if (frameLength < 0 || frameLength > buffer.Length)");
        AppendLine(builder, "        {");
        AppendLine(builder, "            throw new ArgumentOutOfRangeException(nameof(frameLength));");
        AppendLine(builder, "        }");
        AppendLine(builder, string.Empty);
        AppendLine(builder, "        _frame = buffer.Slice(0, frameLength);");
        EmitProtocolInputAssignments(builder, packet, externalInputs);
        AppendLine(builder, "    }");
        AppendLine(builder, string.Empty);
        AppendLine(builder, "    public int Consumed { get; private set; }");
        AppendLine(builder, "    public PacketReadError? LastError { get; private set; }");
        AppendLine(builder, string.Empty);
        AppendLine(builder, $"    public {packetName}? TryRead()");
        AppendLine(builder, "        => TryReadCore(requireFrameEnd: false);");
        AppendLine(builder, string.Empty);
        AppendLine(builder, $"    public {packetName}? TryReadFrame()");
        AppendLine(builder, "        => TryReadCore(requireFrameEnd: true);");
        AppendLine(builder, string.Empty);
        AppendLine(builder, $"    public PacketReadResult<{packetName}> ReadDetailed() => ReadResult(false);");
        AppendLine(builder, $"    public PacketReadResult<{packetName}> ReadFrameDetailed() => ReadResult(true);");
        AppendLine(builder, string.Empty);
        AppendLine(builder, $"    private PacketReadResult<{packetName}> ReadResult(bool requireFrameEnd)");
        AppendLine(builder, "    {");
        AppendLine(builder, "        var packet = TryReadCore(requireFrameEnd);");
        AppendLine(builder, $"        if (LastError is {{ }} error) return PacketReadResult<{packetName}>.Failed(error);");
        string successfulPacket = packet.PacketType.IsValueType ? "packet!.Value" : "packet!";
        AppendLine(builder, $"        return PacketReadResult<{packetName}>.Succeeded({successfulPacket}, Consumed);");
        AppendLine(builder, "    }");
        AppendLine(builder, string.Empty);
        AppendLine(builder, $"    private {packetName}? Fail(PacketReadStatus status, PacketReadErrorCode code, int offset, string? member, string message)");
        AppendLine(builder, "    {");
        AppendLine(builder, "        LastError = new PacketReadError(status, code, _cursor + offset, member, message);");
        AppendLine(builder, "        return null;");
        AppendLine(builder, "    }");
        AppendLine(builder, string.Empty);
        AppendLine(builder, $"    private {packetName}? TryReadCore(bool requireFrameEnd)");
        AppendLine(builder, "    {");
        AppendLine(builder, "        Consumed = 0;");
        AppendLine(builder, "        LastError = null;");
        AppendLine(builder, "        var read = 0;");
        if (packet.BodyCapacity is { } readCapacity)
        {
            AppendLine(builder, $"        if (requireFrameEnd && _frame.Length - _cursor > {readCapacity}) return {ReadFailure("InvalidData", "LengthOutOfRange", null, "The packet body exceeds its protocol capacity.")};");
            AppendLine(builder, $"        var source = _frame.Span.Slice(_cursor, Math.Min(_frame.Length - _cursor, {readCapacity}));");
        }
        else AppendLine(builder, "        var source = _frame.Span.Slice(_cursor);");

        var valueLocals = new Dictionary<PacketGraphNode, string>(ReferenceEqualityComparer.Instance);
        var fieldsByOrdinal = fields.ToDictionary(static field => field.WireOrdinal);
        var codecsBySlot = codecs
            .GroupBy(static codec => codec.Sequence)
            .ToDictionary(static group => group.Key, static group => group.ToArray());
        var ownersByField = FieldOwners(codecs, CodecDirection.Decode, fields);

        // One unified walk: a head-anchored codec (slot 0) runs before every field,
        // then each field is followed by any codec anchored to it. This keeps the
        // emitted read order identical to the declared wire order.
        EmitCodecsAtSlot(
            builder,
            packet,
            codecsBySlot,
            slot: 0,
            valueLocals,
            ownersByField,
            externalInputNames,
            calculationsByNode);

        foreach (var field in fields)
        {
            if (field.WireOrdinal != 0)
                EmitCodecsAtSlot(
                    builder,
                    packet,
                    codecsBySlot,
                    slot: field.WireOrdinal,
                    valueLocals,
                    ownersByField,
                    externalInputNames,
                    calculationsByNode);
            // A field carried by a codec has no standalone bytes: the block already
            // decoded it and bound its local when the block ran.
            if (ownersByField.ContainsKey(field.Node))
            {
                continue;
            }

            var local = "value" + field.WireOrdinal;
            valueLocals[field.Node] = local;

            var formatted = packet.FieldFormats.IsDefaultOrEmpty ? null
                : packet.FieldFormats.SingleOrDefault(binding => ReferenceEquals(binding.Field, field.Node));
            if (formatted is not null)
            {
                EmitFormattedReader(builder, packet, field, formatted, local);
            }
            else if (field.IsVariableLength)
            {
                EmitVariableReader(builder, field, local);
            }
            else
            {
                var width = ScalarKinds.Width(field.ScalarKind);
                if (field.IsNullable)
                {
                    var presence = field.Presence
                        ?? throw new InvalidOperationException($"Nullable field '{field.Member.Name}' has no presence binding.");
                    var presenceExpression = PresenceExpression(presence, valueLocals);
                    AppendLine(builder, $"        var present{field.WireOrdinal} = {presenceExpression};");
                    AppendLine(builder, $"        {field.Member.ValueType.CSharpName} {local};");
                    AppendLine(builder, $"        if (present{field.WireOrdinal})");
                    AppendLine(builder, "        {");
                    AppendLine(builder, $"            if (source.Length - read < {width}) return {ReadFailure("Truncated", "Truncated", field.Member.Name, "The scalar field is truncated.")};");
                    AppendLine(builder, $"            {local} = ({field.Member.ValueType.CSharpName})({ReadExpression(field.ScalarKind, "source", "read")});");
                    AppendLine(builder, $"            read += {width};");
                    AppendLine(builder, "        }");
                    AppendLine(builder, "        else");
                    AppendLine(builder, "        {");
                    AppendLine(builder, $"            {local} = null;");
                    AppendLine(builder, "        }");
                }
                else
                {
                    AppendLine(builder, $"        if (source.Length - read < {width}) return {ReadFailure("Truncated", "Truncated", field.Member.Name, "The scalar field is truncated.")};");
                    AppendLine(builder, $"        var {local} = {ReadExpression(field.ScalarKind, "source", "read")};");
                    AppendLine(builder, $"        read += {width};");
                }
            }

        }

        AppendLine(builder, $"        if (requireFrameEnd && _cursor + read != _frame.Length) return {ReadFailure("InvalidData", "TrailingBytes", null, "The packet body contains trailing bytes.")};");
        if (packet.Construction is { } construction)
        {
            string arguments = string.Join(", ", construction.Arguments.Select(field => valueLocals[field]));
            AppendLine(builder, $"        var packet = new {packetName}({arguments});");
        }
        else
        {
            AppendLine(builder, $"        var packet = new {packetName}");
            AppendLine(builder, "        {");
            foreach (var field in fields)
                AppendLine(builder, $"            {Identifier(field.Member.Name)} = {valueLocals[field.Node]},");
            AppendLine(builder, "        };");
        }
        AppendLine(builder, "        _cursor += read;");
        AppendLine(builder, "        Consumed = read;");
        AppendLine(builder, "        return packet;");
        AppendLine(builder, "    }");
        AppendLine(builder, "}");
    }

    // Maps each field carried by a codec block to the codec that owns its bytes.
    private static Dictionary<PacketGraphNode, CodecRefIr> FieldOwners(
        IEnumerable<CodecRefIr> codecs,
        CodecDirection direction,
        IEnumerable<FieldIr> fields)
    {
        var owners = new Dictionary<PacketGraphNode, CodecRefIr>(ReferenceEqualityComparer.Instance);
        foreach (var codec in codecs)
        {
            if (codec.Node.Direction != direction)
            {
                continue;
            }

            if (direction == CodecDirection.Decode)
            {
                foreach (var output in codec.Outputs)
                {
                    owners[output.Target] = codec;
                }
            }
            else
            {
                foreach (var field in codec.OwnedFields)
                {
                    owners[field] = codec;
                }
            }
        }

        return owners;
    }

    private static void EmitCodecsAtSlot(
        StringBuilder builder,
        PacketIr packet,
        IReadOnlyDictionary<int, CodecRefIr[]> codecsBySlot,
        int slot,
        Dictionary<PacketGraphNode, string> valueLocals,
        IReadOnlyDictionary<PacketGraphNode, CodecRefIr> ownersByField,
        IReadOnlyDictionary<PacketGraphNode, string> externalInputNames,
        IReadOnlyDictionary<PacketGraphNode, PacketCalculationIr> calculationsByNode)
    {
        if (!codecsBySlot.TryGetValue(slot, out var codecs))
        {
            return;
        }

        foreach (var codec in codecs)
        {
            var transportLocal = "codec" + slot;
            var consumedLocal = "codecRead" + slot;
            var range = codec.EffectiveRange;
            string availableLocal = "codecAvailable" + slot;
            string boundedLocal = "codecBound" + slot;

            if (range is null)
            {
                AppendLine(builder, $"        var {transportLocal} = new global::Terraria.NetWork.Prototype.PacketDesignCompiler.Wire.PacketWireReader(_frame, _cursor + read, _frame.Length - _cursor - read);");
            }
            else
            {
                AppendLine(builder, $"        int {availableLocal} = source.Length - read;");
                AppendLine(builder, $"        if ({availableLocal} < {range.MinimumLength + codec.MinimumTrailingBytes}) return {ReadFailure("Truncated", "Truncated", codec.Method.Name, "The codec block and following fields are shorter than their minimum length.")};");
                AppendLine(builder, $"        int {boundedLocal} = global::System.Math.Min({availableLocal} - {codec.MinimumTrailingBytes}, {range.MaximumLength});");
                AppendLine(builder, $"        var {transportLocal} = new global::Terraria.NetWork.Prototype.PacketDesignCompiler.Wire.PacketWireReader(_frame, _cursor + read, {boundedLocal});");
            }

            // Reader-side inputs must be the locals already decoded at this point; the
            // packet object does not exist until every field has been read.
            var arguments = new List<string> { transportLocal };
            arguments.AddRange(codec.Inputs
                .OrderBy(static input => input.Ordinal)
                .Select(input => CodecInputExpression(
                    input,
                    externalInputNames,
                    calculationsByNode,
                    field => valueLocals[field])));

            var call = $"{codec.Method.DeclaringType.CSharpName}.{Identifier(codec.Method.Name)}({string.Join(", ", arguments)})";

            // Return values bind fields; only transport positions determine byte counts.
            var outputLocals = new List<(string Local, TypeRef Type)>();
            if (!codec.Outputs.IsDefaultOrEmpty)
            {
                foreach (var output in codec.Outputs.OrderBy(static output => output.Ordinal))
                {
                    if (output.Target is PacketFieldNode target)
                    {
                        var outputLocal = "codecValue" + slot + "_" + output.Ordinal;
                        valueLocals[target] = outputLocal;
                        outputLocals.Add((outputLocal, output.ValueType));
                    }
                }
            }

            AppendLine(builder, $"        int codecStart{slot} = {transportLocal}.Position;");
            foreach (var (local, type) in outputLocals)
            {
                AppendLine(builder, $"        {type.CSharpName} {local};");
            }
            string assignment = outputLocals.Count switch
            {
                0 => string.Empty,
                1 => outputLocals[0].Local + " = ",
                _ => "(" + string.Join(", ", outputLocals.Select(static output => output.Local)) + ") = "
            };
            string result = outputLocals.Count == 1 && CodecTypes.IsValueTuple(codec.Method.ReturnType, out _)
                ? call + ".Item1"
                : call;
            AppendLine(builder, "        try");
            AppendLine(builder, "        {");
            if (outputLocals.Count > 1)
            {
                AppendLine(builder, $"            var codecResult{slot} = {call};");
                for (int index = 0; index < outputLocals.Count; index++)
                {
                    string element = $"codecResult{slot}";
                    int ordinal = index + 1;
                    while (ordinal > 7)
                    {
                        element += ".Rest";
                        ordinal -= 7;
                    }
                    element += ".Item" + ordinal;
                    // TypeRef omits reference nullability; the null value itself is preserved.
                    if (!outputLocals[index].Type.IsValueType) element += "!";
                    AppendLine(builder, $"            {outputLocals[index].Local} = {element};");
                }
            }
            else
            {
                if (outputLocals.Count == 1 && !outputLocals[0].Type.IsValueType) result += "!";
                AppendLine(builder, $"            {assignment}{result};");
            }
            AppendLine(builder, "        }");

            AppendLine(builder, "        catch (global::Terraria.NetWork.Prototype.PacketDesignCompiler.Wire.PacketWireTruncationException exception)");
            AppendLine(builder, "        {");
            if (range is not null)
                AppendLine(builder, $"            if ({availableLocal} > {boundedLocal} && ReferenceEquals(exception.Reader, {transportLocal})) return Fail(PacketReadStatus.InvalidData, PacketReadErrorCode.LengthOutOfRange, read + {transportLocal}.Position, {Literal(codec.Method.Name)}, exception.Message);");
            AppendLine(builder, $"            return Fail(PacketReadStatus.Truncated, PacketReadErrorCode.Truncated, read + {transportLocal}.Position, {Literal(codec.Method.Name)}, exception.Message);");
            AppendLine(builder, "        }");
            AppendLine(builder, "        catch (global::Terraria.NetWork.Prototype.PacketDesignCompiler.Wire.PacketWireFormatException exception)");
            AppendLine(builder, "        {");
            AppendLine(builder, $"            return Fail(PacketReadStatus.InvalidData, exception.Code, read + {transportLocal}.Position, {Literal(codec.Method.Name)}, exception.Message);");
            AppendLine(builder, "        }");
            AppendLine(builder, "        catch (InvalidDataException exception)");
            AppendLine(builder, "        {");
            AppendLine(builder, $"            return Fail(PacketReadStatus.InvalidData, PacketReadErrorCode.InvalidCodecData, read + {transportLocal}.Position, {Literal(codec.Method.Name)}, exception.Message);");
            AppendLine(builder, "        }");
            AppendLine(builder, $"        int {consumedLocal} = {transportLocal}.Position - codecStart{slot};");
            if (range is not null)
            {
                AppendLine(
                    builder,
                    $"        if ({consumedLocal} < {range.MinimumLength} || {consumedLocal} > {range.MaximumLength}) return {ReadFailure("InvalidData", "LengthOutOfRange", codec.Method.Name, "The codec consumption is outside its declared length range.")};");
            }

            AppendLine(builder, $"        read += {consumedLocal};");
        }
    }

    private static void EmitVariableReader(
        StringBuilder builder,
        FieldIr field,
        string local)
    {
        ByteRange range = field.RangeFor(CodecDirection.Decode)
            ?? throw new InvalidOperationException($"Variable field '{field.Member.Name}' has no length range.");
        string lengthLocal = "length" + field.WireOrdinal;
        if (field.Node.ValueType == typeof(string))
        {
            string suffix = field.WireOrdinal.ToString();
            AppendLine(builder, $"        uint encodedLength{suffix} = 0;");
            AppendLine(builder, $"        int shift{suffix} = 0;");
            AppendLine(builder, "        while (true)");
            AppendLine(builder, "        {");
            AppendLine(builder, $"            if (read == source.Length) return {ReadFailure("Truncated", "Truncated", field.Member.Name, "The string length prefix is truncated.")};");
            AppendLine(builder, $"            byte part{suffix} = source[read++];");
            AppendLine(builder, $"            if (shift{suffix} == 28 && part{suffix} > 7) return {ReadFailure("InvalidData", "InvalidLengthPrefix", field.Member.Name, "The string length prefix overflows Int32.", "read - 1")};");
            AppendLine(builder, $"            encodedLength{suffix} |= (uint)(part{suffix} & 127) << shift{suffix};");
            AppendLine(builder, $"            if ((part{suffix} & 128) == 0) break;");
            AppendLine(builder, $"            shift{suffix} += 7;");
            AppendLine(builder, "        }");
            AppendLine(builder, $"        int {lengthLocal} = (int)encodedLength{suffix};");
        }
        else
        {
            const int prefixWidth = sizeof(ushort);
            AppendLine(builder, $"        if (source.Length - read < {prefixWidth}) return {ReadFailure("Truncated", "Truncated", field.Member.Name, "The byte length prefix is truncated.")};");
            AppendLine(builder, $"        var {lengthLocal} = BinaryPrimitives.ReadUInt16LittleEndian(source.Slice(read, {prefixWidth}));");
            AppendLine(builder, $"        read += {prefixWidth};");
        }
        AppendLine(
            builder,
            $"        if ({lengthLocal} < {range.MinimumLength} || {lengthLocal} > {range.MaximumLength}) return {ReadFailure("InvalidData", "LengthOutOfRange", field.Member.Name, "The payload length is outside its declared range.")};");
        AppendLine(builder, $"        if ({lengthLocal} > source.Length - read) return {ReadFailure("Truncated", "Truncated", field.Member.Name, "The variable field payload is truncated.")};");
        if (field.Node.InfersLengthRange && field.DecodeTrailingBytes > 0)
            AppendLine(builder, $"        if ((long){lengthLocal} + {field.DecodeTrailingBytes} > source.Length - read) return {ReadFailure("Truncated", "Truncated", field.Member.Name, "The variable field leaves insufficient bytes for following fields.")};");
        string payload = $"source.Slice(read, {lengthLocal})";
        string value = field.Node.ValueType == typeof(string)
            ? $"global::System.Text.Encoding.UTF8.GetString({payload})"
            : $"{payload}.ToArray()";
        AppendLine(builder, $"        var {local} = {value};");
        AppendLine(builder, $"        read += {lengthLocal};");
    }

    private static void EmitWriter(
        StringBuilder builder,
        PacketIr packet,
        string packetName,
        string writerName)
    {
        var fields = packet.Fields.OrderBy(static field => field.WireOrdinal).ToImmutableArray();
        var encodeCodecs = packet.Codecs
            .Where(static codec => codec.Node.Direction == CodecDirection.Encode)
            .OrderBy(static codec => codec.Sequence)
            .ToArray();
        var codecsBySlot = encodeCodecs
            .GroupBy(static codec => codec.Sequence)
            .ToDictionary(static group => group.Key, static group => group.ToArray());
        var externalInputs = RequiredProtocolDependencies(packet, CodecDirection.Encode);
        var externalInputNames = CodecExternalParameterNames(externalInputs);
        var calculationsByNode = CalculationMap(packet);

        // A field whose bytes are owned by a block is written by that block alone; the
        // scalar plan must not also emit it, or the field would appear twice on the wire.
        var ownedFields = FieldOwners(packet.Codecs, CodecDirection.Encode, fields);
        var plainFields = fields.Where(field => !ownedFields.ContainsKey(field.Node)).ToArray();

        AppendLine(builder, $"public sealed class {writerName}");
        AppendLine(builder, "{");
        if (!externalInputs.IsEmpty || packet.ExternalDependencyModel is not null)
        {
            EmitProtocolInputFields(builder, externalInputs);
            AppendLine(builder, string.Empty);
            AppendLine(builder, $"    public {writerName}({ProtocolParameterList(ProtocolConstructorInputs(packet, CodecDirection.Encode))})");
            AppendLine(builder, "    {");
            EmitProtocolInputAssignments(builder, packet, externalInputs);
            AppendLine(builder, "    }");
            AppendLine(builder, string.Empty);
        }
        AppendLine(builder, $"    public MemoryStream? TryWrite({packetName} packet)");
        AppendLine(builder, "    {");
        if (!packet.PacketType.IsValueType)
        {
            AppendLine(builder, "        if (packet is null) return null;");
        }

        foreach (var field in plainFields.Where(static field => field.IsNullable))
        {
            var presence = field.Presence
                ?? throw new InvalidOperationException($"Nullable field '{field.Member.Name}' has no presence binding.");
            AppendLine(builder, $"        if (packet.{Identifier(field.Member.Name)}.HasValue != {PresenceExpressionForPacket(presence)}) return null;");
        }

        foreach (var field in plainFields.Where(static field => field.IsVariableLength))
        {
            ByteRange range = field.RangeFor(CodecDirection.Encode)
                ?? throw new InvalidOperationException($"Variable field '{field.Member.Name}' has no length range.");
            string value = $"packet.{Identifier(field.Member.Name)}";
            string length = "length" + field.WireOrdinal;
            if (field.Node.ValueType == typeof(string))
            {
                AppendLine(builder, $"        if ({value} is null) return null;");
                AppendLine(builder, $"        int {length} = global::System.Text.Encoding.UTF8.GetByteCount({value});");
            }
            else
            {
                AppendLine(builder, $"        int {length} = {value}.Length;");
            }
            AppendLine(
                builder,
                $"        if ({length} < {range.MinimumLength} || {length} > {range.MaximumLength}) return null;");
            if (packet.BodyCapacity is { } variableCapacity)
                AppendLine(builder, $"        if ((long){length} + {VariablePrefixSizeExpression(field, length)} > {variableCapacity}) return null;");
        }

        if (encodeCodecs.Length == 0 && packet.FieldFormats.IsDefaultOrEmpty)
        {
            var lengthTerms = plainFields.Select(field => field.IsNullable
                ? $"({PresenceExpressionForPacket(field.Presence!)} ? {ScalarKinds.Width(field.ScalarKind)} : 0)"
                : field.IsVariableLength
                    ? $"{VariablePrefixSizeExpression(field, "length" + field.WireOrdinal)} + length{field.WireOrdinal}"
                : ScalarKinds.Width(field.ScalarKind).ToString()).ToArray();
            if (packet.BodyCapacity is { } plainCapacity)
            {
                AppendLine(builder, $"        long length = 0L{string.Concat(lengthTerms.Select(term => " + (long)(" + term + ")"))};");
                AppendLine(builder, $"        if (length > {plainCapacity}) return null;");
                AppendLine(builder, "        var stream = new MemoryStream((int)length);");
            }
            else
            {
                AppendLine(builder, $"        var length = {(lengthTerms.Length == 0 ? "0" : string.Join(" + ", lengthTerms))};");
                AppendLine(builder, "        var stream = new MemoryStream(length);");
            }
        }
        else
        {
            // Variable codec and format lengths are known only after writing.
            AppendLine(builder, "        var stream = new MemoryStream();");
        }

        AppendLine(builder, "        Span<byte> buffer = stackalloc byte[8];");

        EmitWriterCodecsAtSlot(
            builder,
            packet,
            codecsBySlot,
            slot: 0,
            externalInputNames,
            calculationsByNode);

        foreach (var field in fields)
        {
            // Skip block-carried fields: the block emits their bytes at its own slot.
            if (!ownedFields.ContainsKey(field.Node))
            {
                var formatted = packet.FieldFormats.IsDefaultOrEmpty ? null
                    : packet.FieldFormats.SingleOrDefault(binding => ReferenceEquals(binding.Field, field.Node));
                if (formatted is not null)
                {
                    EmitFormattedWriter(builder, packet, field, formatted);
                }
                else if (field.IsVariableLength)
                {
                    if (field.Node.InfersLengthRange && packet.BodyCapacity is { } variableBudget)
                    {
                        string length = "length" + field.WireOrdinal;
                        AppendLine(builder, $"        if ((long)stream.Length + {VariablePrefixSizeExpression(field, length)} + {length} + {field.EncodeTrailingBytes} > {variableBudget}) return null;");
                    }
                    foreach (var line in VariableWriteStatement(field))
                    {
                        AppendLine(builder, "        " + line);
                    }
                }
                else
                {
                    var value = $"packet.{Identifier(field.Member.Name)}";
                    var valueExpression = field.IsNullable ? value + ".GetValueOrDefault()" : value;
                    var writeLines = WriteStatement(field.ScalarKind, valueExpression);
                    if (field.IsNullable)
                    {
                        AppendLine(builder, $"        if ({PresenceExpressionForPacket(field.Presence!)})");
                        AppendLine(builder, "        {");
                        foreach (var line in writeLines)
                        {
                            AppendLine(builder, "            " + line);
                        }

                        AppendLine(builder, "        }");
                    }
                    else
                    {
                        foreach (var line in writeLines)
                        {
                            AppendLine(builder, "        " + line);
                        }
                    }
                }
            }

            if (packet.BodyCapacity is { } fieldCapacity)
                AppendLine(builder, $"        if (stream.Length > {fieldCapacity}) return null;");
            EmitWriterCodecsAtSlot(
                builder,
                packet,
                codecsBySlot,
                slot: field.WireOrdinal + 1,
                externalInputNames,
                calculationsByNode);
        }

        AppendLine(builder, "        stream.Position = 0;");
        AppendLine(builder, "        return stream;");
        AppendLine(builder, "    }");
        AppendLine(builder, "}");
    }

    private static void EmitWriterCodecsAtSlot(
        StringBuilder builder,
        PacketIr packet,
        IReadOnlyDictionary<int, CodecRefIr[]> codecsBySlot,
        int slot,
        IReadOnlyDictionary<PacketGraphNode, string> externalInputNames,
        IReadOnlyDictionary<PacketGraphNode, PacketCalculationIr> calculationsByNode)
    {
        if (!codecsBySlot.TryGetValue(slot, out var codecs))
        {
            return;
        }

        foreach (var codec in codecs)
        {
            var range = codec.EffectiveRange;
            var arguments = new List<string> { "codec" + slot };
            arguments.AddRange(codec.Inputs
                .OrderBy(static input => input.Ordinal)
                .Select(input => CodecInputExpression(
                    input,
                    externalInputNames,
                    calculationsByNode,
                    field => $"packet.{Identifier(field.Member.Name)}")));

            string limit = range is null ? string.Empty : ", " + range.MaximumLength;
            if (packet.BodyCapacity is { } capacity)
            {
                AppendLine(builder, $"        long codecAvailable{slot} = {capacity}L - stream.Length - {codec.MinimumTrailingBytes};");
                AppendLine(builder, $"        if (codecAvailable{slot} < {range!.MinimumLength}) return null;");
                AppendLine(builder, $"        int codecBound{slot} = (int)Math.Min(codecAvailable{slot}, {range.MaximumLength});");
                limit = ", codecBound" + slot;
            }
            AppendLine(builder,
                $"        var codec{slot} = new global::Terraria.NetWork.Prototype.PacketDesignCompiler.Wire.PacketWireWriter(stream{limit});");
            string call = $"{codec.Method.DeclaringType.CSharpName}.{Identifier(codec.Method.Name)}({string.Join(", ", arguments)})";
            AppendLine(builder, $"        int codecStart{slot} = codec{slot}.BytesWritten;");
            if (range is null)
            {
                AppendLine(builder, "        try");
                AppendLine(builder, "        {");
                AppendLine(builder, $"            {call};");
                AppendLine(builder, "        }");
                AppendLine(builder, "        catch (global::Terraria.NetWork.Prototype.PacketDesignCompiler.Wire.PacketWireFormatException)");
                AppendLine(builder, "        {");
                AppendLine(builder, "            return null;");
                AppendLine(builder, "        }");
            }
            else
            {
                string resultLocal = "codecWritten" + slot;
                AppendLine(builder, "        try");
                AppendLine(builder, "        {");
                AppendLine(builder, $"            {call};");
                AppendLine(builder, "        }");
                AppendLine(builder, "        catch (global::Terraria.NetWork.Prototype.PacketDesignCompiler.Wire.PacketWireLimitException)");
                AppendLine(builder, "        {");
                AppendLine(builder, "            return null;");
                AppendLine(builder, "        }");
                AppendLine(builder, "        catch (global::Terraria.NetWork.Prototype.PacketDesignCompiler.Wire.PacketWireFormatException)");
                AppendLine(builder, "        {");
                AppendLine(builder, "            return null;");
                AppendLine(builder, "        }");
                AppendLine(builder, $"        int {resultLocal} = codec{slot}.BytesWritten - codecStart{slot};");
                AppendLine(
                    builder,
                    $"        if ({resultLocal} < {range.MinimumLength} || {resultLocal} > {range.MaximumLength}) return null;");
            }
        }
    }

    private static string[] VariableWriteStatement(FieldIr field)
    {
        string value = $"packet.{Identifier(field.Member.Name)}";
        string length = "length" + field.WireOrdinal;
        if (field.Node.ValueType == typeof(string))
        {
            string remaining = "remainingLength" + field.WireOrdinal;
            return
            [
                $"uint {remaining} = (uint){length};",
                $"while ({remaining} >= 128)",
                "{",
                $"    stream.WriteByte((byte)(({remaining} & 127) | 128));",
                $"    {remaining} >>= 7;",
                "}",
                $"stream.WriteByte((byte){remaining});",
                $"stream.Write(global::System.Text.Encoding.UTF8.GetBytes({value}));"
            ];
        }

        const int prefixWidth = sizeof(ushort);

        return
        [
            $"BinaryPrimitives.WriteUInt16LittleEndian(buffer, checked((ushort){length}));",
            $"stream.Write(buffer[..{prefixWidth}]);",
            $"stream.Write({value}.Span);"
        ];
    }

    private static string VariablePrefixSizeExpression(FieldIr field, string length) =>
        field.Node.ValueType == typeof(string)
            ? $"({length} < 128 ? 1 : {length} < 16384 ? 2 : {length} < 2097152 ? 3 : {length} < 268435456 ? 4 : 5)"
            : "2";

    private static string PresenceExpression(
        PresenceBindingIr presence,
        IReadOnlyDictionary<PacketGraphNode, string> valueLocals)
    {
        var sourceValue = valueLocals[presence.Source];
        return PresenceExpressionCore(presence, sourceValue);
    }

    private static string PresenceExpressionForPacket(PresenceBindingIr presence) =>
        PresenceExpressionCore(presence, $"packet.{Identifier(presence.Source.Member.Name)}");

    // A presence source is a bool, or a bit inside an integer scalar. When that source
    // is itself nullable it may legitimately be absent, and an absent source cannot
    // truthfully claim the dependent field is present: the original protocol reads such
    // a bit only after the owning flag said the container exists. Unwrapping it blindly
    // (the previous behaviour) threw InvalidOperationException on a legal frame.
    private static string PresenceExpressionCore(PresenceBindingIr presence, string sourceValue)
    {
        var nullableSource = Nullable.GetUnderlyingType(presence.Source.ValueType) is not null;
        if (nullableSource)
        {
            // Treat "the source was not present" as "the dependent field is not present"
            // rather than faulting. The expression is a value, not a statement, so the
            // null case is folded into the comparison.
            var unwrapped = sourceValue + ".GetValueOrDefault()";
            return $"({sourceValue}.HasValue && ((unchecked((ulong){unwrapped}) & (1UL << {presence.Bit!.Value})) != 0))";
        }

        return $"((unchecked((ulong){sourceValue}) & (1UL << {presence.Bit!.Value})) != 0)";
    }

    private static string ReadExpression(ScalarKind kind, string source, string position) => kind switch
    {
        ScalarKind.Byte => $"{source}[{position}]",
        ScalarKind.SByte => $"unchecked((sbyte){source}[{position}])",
        ScalarKind.Bool => $"{source}[{position}] != 0",
        ScalarKind.Int16 => $"BinaryPrimitives.ReadInt16LittleEndian({source}.Slice({position}, 2))",
        ScalarKind.UInt16 => $"BinaryPrimitives.ReadUInt16LittleEndian({source}.Slice({position}, 2))",
        ScalarKind.Int32 => $"BinaryPrimitives.ReadInt32LittleEndian({source}.Slice({position}, 4))",
        ScalarKind.UInt32 => $"BinaryPrimitives.ReadUInt32LittleEndian({source}.Slice({position}, 4))",
        ScalarKind.Float32 => $"BinaryPrimitives.ReadSingleLittleEndian({source}.Slice({position}, 4))",
        ScalarKind.Int64 => $"BinaryPrimitives.ReadInt64LittleEndian({source}.Slice({position}, 8))",
        ScalarKind.UInt64 => $"BinaryPrimitives.ReadUInt64LittleEndian({source}.Slice({position}, 8))",
        _ => throw new ArgumentOutOfRangeException(nameof(kind))
    };

    private static string[] WriteStatement(ScalarKind kind, string value) => kind switch
    {
        ScalarKind.Byte or ScalarKind.SByte => [$"stream.WriteByte((byte)({value}));"],
        ScalarKind.Bool => [$"stream.WriteByte((byte)({value} ? 1 : 0));"],
        ScalarKind.Int16 => [$"BinaryPrimitives.WriteInt16LittleEndian(buffer, {value});", "stream.Write(buffer[..2]);"],
        ScalarKind.UInt16 => [$"BinaryPrimitives.WriteUInt16LittleEndian(buffer, {value});", "stream.Write(buffer[..2]);"],
        ScalarKind.Int32 => [$"BinaryPrimitives.WriteInt32LittleEndian(buffer, {value});", "stream.Write(buffer[..4]);"],
        ScalarKind.UInt32 => [$"BinaryPrimitives.WriteUInt32LittleEndian(buffer, {value});", "stream.Write(buffer[..4]);"],
        ScalarKind.Float32 => [$"BinaryPrimitives.WriteSingleLittleEndian(buffer, {value});", "stream.Write(buffer[..4]);"],
        ScalarKind.Int64 => [$"BinaryPrimitives.WriteInt64LittleEndian(buffer, {value});", "stream.Write(buffer[..8]);"],
        ScalarKind.UInt64 => [$"BinaryPrimitives.WriteUInt64LittleEndian(buffer, {value});", "stream.Write(buffer[..8]);"],
        _ => throw new ArgumentOutOfRangeException(nameof(kind))
    };

    private static string Literal(string? value) => System.Text.Json.JsonSerializer.Serialize(value);

    private static string ReadFailure(string status, string code, string? member, string message, string offset = "read") =>
        $"Fail(PacketReadStatus.{status}, PacketReadErrorCode.{code}, {offset}, {Literal(member)}, {Literal(message)})";

    private static string TypeName(Type type) => TypeRef.From(type).CSharpName;

    private static string Identifier(string value)
    {
        var result = string.IsNullOrWhiteSpace(value)
            ? "GeneratedName"
            : new string(value.Select(character => char.IsLetterOrDigit(character) || character == '_' ? character : '_').ToArray());
        return SyntaxFacts.GetKeywordKind(result) != SyntaxKind.None ? "@" + result : result;
    }

    private static void AppendLine(StringBuilder builder, string line) => builder.Append(line).Append('\n');
}
