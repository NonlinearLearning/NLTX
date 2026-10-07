using System.Text;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler;

internal static partial class CSharpBackend
{
    private sealed class FormatEmission
    {
        public readonly List<WireFormatIr> Formats = [];
        private readonly Dictionary<WireFormat, int> _indexes = new(ReferenceEqualityComparer.Instance);

        public FormatEmission(PacketIr packet)
        {
            if (!packet.FieldFormats.IsDefaultOrEmpty)
                foreach (var binding in packet.FieldFormats) Add(binding.Format);
        }

        public int Index(WireFormatIr format) => _indexes[format.Format];

        private void Add(WireFormatIr format)
        {
            if (_indexes.ContainsKey(format.Format)) return;
            _indexes.Add(format.Format, Formats.Count);
            Formats.Add(format);
        }
    }

    private static string FormatHelperName(PacketIr packet) => GeneratedBaseName(packet) + "WireFormats";

    private static void EmitFormattedReader(StringBuilder builder, PacketIr packet, FieldIr field,
        FieldFormatIr binding, string local)
    {
        string reader = "formatReader" + field.WireOrdinal;
        int index = new FormatEmission(packet).Index(binding.Format);
        AppendLine(builder, $"        var {reader} = new PacketWireReader(_frame, _cursor + read, source.Length - read);");
        AppendLine(builder, $"        {field.Member.ValueType.CSharpName} {local};");
        AppendLine(builder, "        try");
        AppendLine(builder, "        {");
        AppendLine(builder, $"            {local} = {FormatHelperName(packet)}.Read{index}({reader});");
        AppendLine(builder, "        }");
        AppendLine(builder, "        catch (PacketWireTruncationException error)");
        AppendLine(builder, "        {");
        AppendLine(builder, $"            return Fail(PacketReadStatus.Truncated, PacketReadErrorCode.Truncated, read + {reader}.Position, {Literal(field.Member.Name)}, error.Message);");
        AppendLine(builder, "        }");
        AppendLine(builder, "        catch (PacketWireFormatException error)");
        AppendLine(builder, "        {");
        AppendLine(builder, $"            return Fail(PacketReadStatus.InvalidData, error.Code, read + {reader}.Position, {Literal(field.Member.Name)}, error.Message);");
        AppendLine(builder, "        }");
        AppendLine(builder, "        catch (InvalidDataException error)");
        AppendLine(builder, "        {");
        AppendLine(builder, $"            return Fail(PacketReadStatus.InvalidData, PacketReadErrorCode.InvalidCodecData, read + {reader}.Position, {Literal(field.Member.Name)}, error.Message);");
        AppendLine(builder, "        }");
        AppendLine(builder, $"        read += {reader}.Position;");
    }

    private static void EmitFormattedWriter(StringBuilder builder, PacketIr packet, FieldIr field,
        FieldFormatIr binding)
    {
        int index = new FormatEmission(packet).Index(binding.Format);
        AppendLine(builder, "        try");
        AppendLine(builder, "        {");
        string limit = packet.BodyCapacity is { } capacity ? $", (int)({capacity}L - stream.Length)" : string.Empty;
        AppendLine(builder, $"            {FormatHelperName(packet)}.Write{index}(new PacketWireWriter(stream{limit}), packet.{Identifier(field.Member.Name)});");
        AppendLine(builder, "        }");
        AppendLine(builder, "        catch (PacketWireFormatException)");
        AppendLine(builder, "        {");
        AppendLine(builder, "            stream.Dispose();");
        AppendLine(builder, "            return null;");
        AppendLine(builder, "        }");
        AppendLine(builder, "        catch (PacketWireLimitException)");
        AppendLine(builder, "        {");
        AppendLine(builder, "            stream.Dispose();");
        AppendLine(builder, "            return null;");
        AppendLine(builder, "        }");
    }

    private static void EmitFormats(StringBuilder builder, PacketIr packet)
    {
        var emission = new FormatEmission(packet);
        if (emission.Formats.Count == 0) return;
        for (int index = 0; index < emission.Formats.Count; index++)
        {
            if (emission.Formats[index].Structure is { } structure)
                builder.Append(EmitPacket(structure with
                {
                    GeneratedName = GeneratedBaseName(packet) + "_Format" + index
                }, includeHeader: false));
        }
        AppendLine(builder, $"internal static class {FormatHelperName(packet)}");
        AppendLine(builder, "{");
        foreach (var format in emission.Formats)
        {
            int index = emission.Index(format);
            if (format.Format.Directions.HasFlag(PacketCodecDirections.Decode))
                EmitFormatRead(builder, packet, format, index);
            if (format.Format.Directions.HasFlag(PacketCodecDirections.Encode))
                EmitFormatWrite(builder, packet, format, index);
        }
        AppendLine(builder, "}");
    }

    private static void EmitFormatRead(StringBuilder builder, PacketIr packet,
        WireFormatIr plan, int index, bool enforceRange = true)
    {
        WireFormat format = plan.Format;
        string suffix = enforceRange ? string.Empty : "Core";
        AppendLine(builder, $"    internal static {TypeName(format.ValueType)} Read{index}{suffix}(PacketWireReader reader)");
        AppendLine(builder, "    {");
        if (enforceRange && format.Kind != WireFormatKind.Codec && format.LengthRange is not null)
        {
            EmitBoundedFormatRead(builder, format, $"Read{index}Core(block)");
            AppendLine(builder, "    }");
            EmitFormatRead(builder, packet, plan, index, enforceRange: false);
            return;
        }
        switch (format.Kind)
        {
            case WireFormatKind.Scalar:
                if (format.Scalar == ScalarKind.Bool)
                {
                    AppendLine(builder, "        byte value = reader.ReadByte();");
                    AppendLine(builder, "        if (value > 1) throw new PacketWireFormatException(\"Boolean fields require 0 or 1.\");");
                    AppendLine(builder, "        return value != 0;");
                }
                else AppendLine(builder, $"        return reader.{ScalarRead(format.Scalar)}();");
                break;
            case WireFormatKind.Structure:
                var nested = plan.Structure! with { GeneratedName = GeneratedBaseName(packet) + "_Format" + index };
                AppendLine(builder, $"        var nested = new {GeneratedBaseName(nested)}PacketCodecReader(reader.Frame.Slice(reader.Position));");
                AppendLine(builder, "        var result = nested.ReadDetailed();");
                AppendLine(builder, "        if (!result.Success)");
                AppendLine(builder, "        {");
                AppendLine(builder, "            var error = result.Error!;");
                AppendLine(builder, "            reader.Skip(error.Offset);");
                AppendLine(builder, "            if (error.Status == PacketReadStatus.Truncated) throw new PacketWireTruncationException(error.Message, reader);");
                AppendLine(builder, "            throw new PacketWireFormatException(error.Message, error.Code);");
                AppendLine(builder, "        }");
                AppendLine(builder, "        reader.Skip(result.Consumed);");
                AppendLine(builder, "        return result.Packet!;");
                break;
            case WireFormatKind.Codec:
                EmitFormatCodecRead(builder, format);
                break;
        }
        AppendLine(builder, "    }");
    }

    private static void EmitFormatCodecRead(StringBuilder builder, WireFormat format)
    {
        string method = format.ReadMethod!.DeclaringType.CSharpName + "." + Identifier(format.ReadMethod.Name);
        if (format.LengthRange is null)
        {
            AppendLine(builder, $"        return {method}(reader);");
            return;
        }
        EmitBoundedFormatRead(builder, format, method + "(block)");
    }

    private static void EmitBoundedFormatRead(StringBuilder builder, WireFormat format, string call)
    {
        var range = format.LengthRange!;
        AppendLine(builder, $"        if (reader.Remaining < {range.MinimumLength}) throw new PacketWireTruncationException(\"The format is shorter than its minimum byte length.\", reader);");
        AppendLine(builder, $"        var block = new PacketWireReader(reader.Frame, reader.Position, Math.Min(reader.Remaining, {range.MaximumLength}));");
        AppendLine(builder, "        try");
        AppendLine(builder, "        {");
        AppendLine(builder, $"            var result = {call};");
        AppendLine(builder, $"            if (block.Position < {range.MinimumLength}) throw new PacketWireFormatException(\"The format is shorter than its minimum byte length.\", PacketReadErrorCode.LengthOutOfRange);");
        AppendLine(builder, "            return result;");
        AppendLine(builder, "        }");
        AppendLine(builder, "        catch (PacketWireTruncationException error) when (reader.Remaining > block.Frame.Length && ReferenceEquals(error.Reader, block))");
        AppendLine(builder, "        {");
        AppendLine(builder, "            throw new PacketWireFormatException(\"The format exceeds its byte range.\", PacketReadErrorCode.LengthOutOfRange);");
        AppendLine(builder, "        }");
        AppendLine(builder, "        finally { reader.Skip(block.Position); }");
    }

    private static void EmitFormatWrite(StringBuilder builder, PacketIr packet,
        WireFormatIr plan, int index, bool enforceRange = true)
    {
        WireFormat format = plan.Format;
        string suffix = enforceRange ? string.Empty : "Core";
        AppendLine(builder, $"    internal static void Write{index}{suffix}(PacketWireWriter writer, {TypeName(format.ValueType)} value)");
        AppendLine(builder, "    {");
        if (enforceRange && format.Kind != WireFormatKind.Codec && format.LengthRange is not null)
        {
            EmitBoundedFormatWrite(builder, format, $"Write{index}Core(block, value)");
            AppendLine(builder, "    }");
            EmitFormatWrite(builder, packet, plan, index, enforceRange: false);
            return;
        }
        switch (format.Kind)
        {
            case WireFormatKind.Scalar:
                AppendLine(builder, $"        writer.{ScalarWrite(format.Scalar)}(value);");
                break;
            case WireFormatKind.Structure:
                var nested = plan.Structure! with { GeneratedName = GeneratedBaseName(packet) + "_Format" + index };
                AppendLine(builder, $"        var nested = new {GeneratedBaseName(nested)}PacketCodecWriter();");
                AppendLine(builder, "        using var stream = nested.TryWrite(value);");
                AppendLine(builder, "        if (stream is null) throw new PacketWireFormatException(\"The nested format value is invalid.\");");
                AppendLine(builder, "        writer.WriteBytes(stream.GetBuffer().AsSpan(0, checked((int)stream.Length)));");
                break;
            case WireFormatKind.Codec:
                string method = format.WriteMethod!.DeclaringType.CSharpName + "." + Identifier(format.WriteMethod.Name);
                if (format.LengthRange is not null)
                    EmitBoundedFormatWrite(builder, format, method + "(block, value)");
                else AppendLine(builder, $"        {method}(writer, value);");
                break;
        }
        AppendLine(builder, "    }");
    }

    private static void EmitBoundedFormatWrite(StringBuilder builder, WireFormat format, string call)
    {
        var range = format.LengthRange!;
        AppendLine(builder, "        using var stream = new MemoryStream();");
        AppendLine(builder, $"        var block = new PacketWireWriter(stream, {range.MaximumLength});");
        AppendLine(builder, $"        {call};");
        AppendLine(builder, $"        if (block.BytesWritten < {range.MinimumLength}) throw new PacketWireFormatException(\"The format is shorter than its byte range.\", PacketReadErrorCode.LengthOutOfRange);");
        AppendLine(builder, "        writer.WriteBytes(stream.GetBuffer().AsSpan(0, checked((int)stream.Length)));");
    }

    private static string ScalarRead(ScalarKind scalar) => scalar switch
    {
        ScalarKind.Bool => "ReadBoolean", ScalarKind.Float32 => "ReadSingle", _ => "Read" + scalar
    };

    private static string ScalarWrite(ScalarKind scalar) => scalar switch
    {
        ScalarKind.Bool => "WriteBoolean", ScalarKind.Float32 => "WriteSingle", _ => "Write" + scalar
    };
}
