using System.Collections.Immutable;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler;

internal static partial class CompilerPipeline
{
    private static PacketIr ApplyFrameBudget(PacketIr packet, PacketGraphManifest manifest,
        int capacity, List<CompilerDiagnostic> diagnostics)
    {
        var budgets = new Dictionary<CodecRefNode, CodecRefIr>(ReferenceEqualityComparer.Instance);
        var fieldBudgets = new Dictionary<(PacketFieldNode, CodecDirection), (ByteRange Range, int Trailing)>();
        foreach (var direction in new[] { CodecDirection.Decode, CodecDirection.Encode })
        {
            if (!packet.CodecDirections.HasFlag(direction == CodecDirection.Decode
                    ? PacketCodecDirections.Decode : PacketCodecDirections.Encode)) continue;
            var codecs = packet.Codecs.Where(codec => codec.Node.Direction == direction).ToArray();
            var fields = packet.Fields.Where(field => !codecs.Any(codec => codec.Owns(field.Node))).ToArray();
            long minimum = MinimumPacketBytes(packet, direction);
            if (minimum > capacity)
            {
                Add(diagnostics, "CPK188", DiagnosticPhase.Validate,
                    $"The {direction} layout requires at least {minimum} bytes, but the packet body capacity is {capacity}.",
                    manifest, manifest.SourceLocation);
                continue;
            }
            foreach (var codec in codecs)
            {
                int codecMinimum = codec.Node.LengthRange?.MinimumLength ?? 0;
                int maximum = capacity - (int)(minimum - codecMinimum);
                if (codec.Node.LengthRange is { } declared && declared.MaximumLength > maximum)
                    Add(diagnostics, "CPK189", DiagnosticPhase.Validate,
                        $"Codec '{codec.Method.Name}' declares {declared.MaximumLength} bytes, but its protocol budget is {maximum}.",
                        manifest, codec.SourceLocation);
                int trailing = (int)(fields.Where(field => field.WireOrdinal + 1 > codec.Sequence)
                    .Sum(field => MinimumFieldBytes(packet, field, direction))
                    + codecs.Where(next => next.Sequence > codec.Sequence)
                        .Sum(next => (long)(next.Node.LengthRange?.MinimumLength ?? 0)));
                budgets.Add(codec.Node, codec with
                {
                    LengthRange = new ByteRange(codecMinimum,
                        Math.Min(maximum, codec.Node.LengthRange?.MaximumLength ?? maximum)),
                    MinimumTrailingBytes = trailing
                });
            }
            foreach (var field in fields.Where(static field => field.Node.InfersLengthRange))
            {
                int available = capacity - (int)(minimum - MinimumFieldBytes(packet, field, direction));
                int trailing = (int)(fields.Where(next => next.WireOrdinal > field.WireOrdinal)
                    .Sum(next => MinimumFieldBytes(packet, next, direction))
                    + codecs.Where(next => next.Sequence > field.WireOrdinal)
                        .Sum(next => (long)(next.Node.LengthRange?.MinimumLength ?? 0)));
                fieldBudgets.Add((field.Node, direction), (InferPayloadRange(field.Node.ValueType, available), trailing));
            }
        }
        return packet with
        {
            BodyCapacity = capacity,
            Codecs = packet.Codecs.Select(codec => budgets.GetValueOrDefault(codec.Node, codec)).ToImmutableArray(),
            Fields = packet.Fields.Select(field =>
            {
                bool decode = fieldBudgets.TryGetValue((field.Node, CodecDirection.Decode), out var read);
                bool encode = fieldBudgets.TryGetValue((field.Node, CodecDirection.Encode), out var write);
                if (!decode && !encode) return field;
                return field with
                {
                    LengthRange = decode && encode ? new ByteRange(0, Math.Min(read.Range.MaximumLength, write.Range.MaximumLength))
                        : decode ? read.Range : write.Range,
                    DecodeLengthRange = decode ? read.Range : null,
                    EncodeLengthRange = encode ? write.Range : null,
                    DecodeTrailingBytes = decode ? read.Trailing : 0,
                    EncodeTrailingBytes = encode ? write.Trailing : 0
                };
            }).ToImmutableArray()
        };
    }

    private static ByteRange InferPayloadRange(Type type, int wireBytes)
    {
        int lower = 0;
        int upper = Math.Min(Math.Max(0, wireBytes), type == typeof(ReadOnlyMemory<byte>) ? ushort.MaxValue : int.MaxValue);
        while (lower < upper)
        {
            int candidate = lower + (int)(((long)upper - lower + 1) / 2);
            int prefix = type == typeof(string) ? StringPrefixBytes(candidate) : sizeof(ushort);
            if ((long)candidate + prefix <= wireBytes) lower = candidate;
            else upper = candidate - 1;
        }
        return new(0, lower);
    }

    private static int StringPrefixBytes(int length)
    {
        int bytes = 1;
        for (; length >= 128; length >>= 7) bytes++;
        return bytes;
    }

    private static long MinimumPacketBytes(PacketIr packet, CodecDirection direction)
    {
        var codecs = packet.Codecs.Where(codec => codec.Node.Direction == direction).ToArray();
        long minimum = 0;
        foreach (var codec in codecs)
            minimum = AddMinimum(minimum, codec.Node.LengthRange?.MinimumLength ?? 0);
        foreach (var field in packet.Fields.Where(field => !codecs.Any(codec => codec.Owns(field.Node))))
            minimum = AddMinimum(minimum, MinimumFieldBytes(packet, field, direction));
        return minimum;
    }

    private static long MinimumFieldBytes(PacketIr packet, FieldIr field, CodecDirection direction)
    {
        if (field.IsNullable) return 0;
        var format = packet.FieldFormats.IsDefaultOrEmpty ? null
            : packet.FieldFormats.SingleOrDefault(binding => ReferenceEquals(binding.Field, field.Node));
        if (format is not null) return MinimumFormatBytes(format.Format, direction);
        if (field.LengthRange is not { } range) return ScalarKinds.Width(field.ScalarKind);
        int prefix = field.Node.ValueType == typeof(string)
            ? range.MinimumLength < 128 ? 1 : range.MinimumLength < 16384 ? 2
                : range.MinimumLength < 2097152 ? 3 : range.MinimumLength < 268435456 ? 4 : 5
            : sizeof(ushort);
        return (long)range.MinimumLength + prefix;
    }

    private static long MinimumFormatBytes(WireFormatIr format, CodecDirection direction)
    {
        long minimum = format.Format.Kind switch
        {
            WireFormatKind.Scalar => ScalarKinds.Width(format.Format.Scalar),
            WireFormatKind.Structure => MinimumPacketBytes(format.Structure!, direction),
            _ => 0
        };
        return Math.Max(minimum, format.Format.LengthRange?.MinimumLength ?? 0);
    }

    private static long AddMinimum(long left, long right) => Math.Min(int.MaxValue + 1L, left + right);
}
