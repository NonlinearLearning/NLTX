#nullable enable
using System;
using System.IO;
using System.Buffers.Binary;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Generated;

public sealed class TerrariaV4_CombatTextStringPacketPacketCodecReader
{
    private readonly ReadOnlyMemory<byte> _frame;
    private int _cursor;

    public TerrariaV4_CombatTextStringPacketPacketCodecReader(ReadOnlyMemory<byte> frame)
    {
        _frame = frame;
        _ = global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.ProtocolInputs.Instance;
    }

    // Reads one frame out of a larger receive buffer. The declared length is the
    // frame boundary: neither a scalar field nor a codec block can reach past it,
    // so a first frame can never consume bytes belonging to the second.
    public TerrariaV4_CombatTextStringPacketPacketCodecReader(ReadOnlyMemory<byte> buffer, int frameLength)
    {
        if (frameLength < 0 || frameLength > buffer.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(frameLength));
        }

        _frame = buffer.Slice(0, frameLength);
        _ = global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.ProtocolInputs.Instance;
    }

    public int Consumed { get; private set; }
    public PacketReadError? LastError { get; private set; }

    public global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.CombatTextStringPacket? TryRead()
        => TryReadCore(requireFrameEnd: false);

    public global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.CombatTextStringPacket? TryReadFrame()
        => TryReadCore(requireFrameEnd: true);

    public PacketReadResult<global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.CombatTextStringPacket> ReadDetailed() => ReadResult(false);
    public PacketReadResult<global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.CombatTextStringPacket> ReadFrameDetailed() => ReadResult(true);

    private PacketReadResult<global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.CombatTextStringPacket> ReadResult(bool requireFrameEnd)
    {
        var packet = TryReadCore(requireFrameEnd);
        if (LastError is { } error) return PacketReadResult<global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.CombatTextStringPacket>.Failed(error);
        return PacketReadResult<global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.CombatTextStringPacket>.Succeeded(packet!, Consumed);
    }

    private global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.CombatTextStringPacket? Fail(PacketReadStatus status, PacketReadErrorCode code, int offset, string? member, string message)
    {
        LastError = new PacketReadError(status, code, _cursor + offset, member, message);
        return null;
    }

    private global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.CombatTextStringPacket? TryReadCore(bool requireFrameEnd)
    {
        Consumed = 0;
        LastError = null;
        var read = 0;
        if (requireFrameEnd && _frame.Length - _cursor > 65532) return Fail(PacketReadStatus.InvalidData, PacketReadErrorCode.LengthOutOfRange, read, null, "The packet body exceeds its protocol capacity.");
        var source = _frame.Span.Slice(_cursor, Math.Min(_frame.Length - _cursor, 65532));
        if (source.Length - read < 4) return Fail(PacketReadStatus.Truncated, PacketReadErrorCode.Truncated, read, "X", "The scalar field is truncated.");
        var value0 = BinaryPrimitives.ReadSingleLittleEndian(source.Slice(read, 4));
        read += 4;
        if (source.Length - read < 4) return Fail(PacketReadStatus.Truncated, PacketReadErrorCode.Truncated, read, "Y", "The scalar field is truncated.");
        var value1 = BinaryPrimitives.ReadSingleLittleEndian(source.Slice(read, 4));
        read += 4;
        var formatReader2 = new PacketWireReader(_frame, _cursor + read, source.Length - read);
        global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.PacketRgb value2;
        try
        {
            value2 = TerrariaV4_CombatTextStringPacketWireFormats.Read0(formatReader2);
        }
        catch (PacketWireTruncationException error)
        {
            return Fail(PacketReadStatus.Truncated, PacketReadErrorCode.Truncated, read + formatReader2.Position, "Color", error.Message);
        }
        catch (PacketWireFormatException error)
        {
            return Fail(PacketReadStatus.InvalidData, error.Code, read + formatReader2.Position, "Color", error.Message);
        }
        catch (InvalidDataException error)
        {
            return Fail(PacketReadStatus.InvalidData, PacketReadErrorCode.InvalidCodecData, read + formatReader2.Position, "Color", error.Message);
        }
        read += formatReader2.Position;
        var formatReader3 = new PacketWireReader(_frame, _cursor + read, source.Length - read);
        global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.NetworkText value3;
        try
        {
            value3 = TerrariaV4_CombatTextStringPacketWireFormats.Read1(formatReader3);
        }
        catch (PacketWireTruncationException error)
        {
            return Fail(PacketReadStatus.Truncated, PacketReadErrorCode.Truncated, read + formatReader3.Position, "Text", error.Message);
        }
        catch (PacketWireFormatException error)
        {
            return Fail(PacketReadStatus.InvalidData, error.Code, read + formatReader3.Position, "Text", error.Message);
        }
        catch (InvalidDataException error)
        {
            return Fail(PacketReadStatus.InvalidData, PacketReadErrorCode.InvalidCodecData, read + formatReader3.Position, "Text", error.Message);
        }
        read += formatReader3.Position;
        if (requireFrameEnd && _cursor + read != _frame.Length) return Fail(PacketReadStatus.InvalidData, PacketReadErrorCode.TrailingBytes, read, null, "The packet body contains trailing bytes.");
        var packet = new global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.CombatTextStringPacket
        {
            X = value0,
            Y = value1,
            Color = value2,
            Text = value3,
        };
        _cursor += read;
        Consumed = read;
        return packet;
    }
}

public sealed class TerrariaV4_CombatTextStringPacketPacketCodecWriter
{

    public TerrariaV4_CombatTextStringPacketPacketCodecWriter()
    {
        _ = global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.ProtocolInputs.Instance;
    }

    public MemoryStream? TryWrite(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.CombatTextStringPacket packet)
    {
        if (packet is null) return null;
        var stream = new MemoryStream();
        Span<byte> buffer = stackalloc byte[8];
        BinaryPrimitives.WriteSingleLittleEndian(buffer, packet.X);
        stream.Write(buffer[..4]);
        if (stream.Length > 65532) return null;
        BinaryPrimitives.WriteSingleLittleEndian(buffer, packet.Y);
        stream.Write(buffer[..4]);
        if (stream.Length > 65532) return null;
        try
        {
            TerrariaV4_CombatTextStringPacketWireFormats.Write0(new PacketWireWriter(stream, (int)(65532L - stream.Length)), packet.Color);
        }
        catch (PacketWireFormatException)
        {
            stream.Dispose();
            return null;
        }
        catch (PacketWireLimitException)
        {
            stream.Dispose();
            return null;
        }
        if (stream.Length > 65532) return null;
        try
        {
            TerrariaV4_CombatTextStringPacketWireFormats.Write1(new PacketWireWriter(stream, (int)(65532L - stream.Length)), packet.Text);
        }
        catch (PacketWireFormatException)
        {
            stream.Dispose();
            return null;
        }
        catch (PacketWireLimitException)
        {
            stream.Dispose();
            return null;
        }
        if (stream.Length > 65532) return null;
        stream.Position = 0;
        return stream;
    }
}
public sealed class TerrariaV4_CombatTextStringPacket_Format0PacketCodecReader
{
    private readonly ReadOnlyMemory<byte> _frame;
    private int _cursor;

    public TerrariaV4_CombatTextStringPacket_Format0PacketCodecReader(ReadOnlyMemory<byte> frame)
    {
        _frame = frame;
    }

    // Reads one frame out of a larger receive buffer. The declared length is the
    // frame boundary: neither a scalar field nor a codec block can reach past it,
    // so a first frame can never consume bytes belonging to the second.
    public TerrariaV4_CombatTextStringPacket_Format0PacketCodecReader(ReadOnlyMemory<byte> buffer, int frameLength)
    {
        if (frameLength < 0 || frameLength > buffer.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(frameLength));
        }

        _frame = buffer.Slice(0, frameLength);
    }

    public int Consumed { get; private set; }
    public PacketReadError? LastError { get; private set; }

    public global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.PacketRgb? TryRead()
        => TryReadCore(requireFrameEnd: false);

    public global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.PacketRgb? TryReadFrame()
        => TryReadCore(requireFrameEnd: true);

    public PacketReadResult<global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.PacketRgb> ReadDetailed() => ReadResult(false);
    public PacketReadResult<global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.PacketRgb> ReadFrameDetailed() => ReadResult(true);

    private PacketReadResult<global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.PacketRgb> ReadResult(bool requireFrameEnd)
    {
        var packet = TryReadCore(requireFrameEnd);
        if (LastError is { } error) return PacketReadResult<global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.PacketRgb>.Failed(error);
        return PacketReadResult<global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.PacketRgb>.Succeeded(packet!.Value, Consumed);
    }

    private global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.PacketRgb? Fail(PacketReadStatus status, PacketReadErrorCode code, int offset, string? member, string message)
    {
        LastError = new PacketReadError(status, code, _cursor + offset, member, message);
        return null;
    }

    private global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.PacketRgb? TryReadCore(bool requireFrameEnd)
    {
        Consumed = 0;
        LastError = null;
        var read = 0;
        if (requireFrameEnd && _frame.Length - _cursor > 65532) return Fail(PacketReadStatus.InvalidData, PacketReadErrorCode.LengthOutOfRange, read, null, "The packet body exceeds its protocol capacity.");
        var source = _frame.Span.Slice(_cursor, Math.Min(_frame.Length - _cursor, 65532));
        if (source.Length - read < 1) return Fail(PacketReadStatus.Truncated, PacketReadErrorCode.Truncated, read, "Red", "The scalar field is truncated.");
        var value0 = source[read];
        read += 1;
        if (source.Length - read < 1) return Fail(PacketReadStatus.Truncated, PacketReadErrorCode.Truncated, read, "Green", "The scalar field is truncated.");
        var value1 = source[read];
        read += 1;
        if (source.Length - read < 1) return Fail(PacketReadStatus.Truncated, PacketReadErrorCode.Truncated, read, "Blue", "The scalar field is truncated.");
        var value2 = source[read];
        read += 1;
        if (requireFrameEnd && _cursor + read != _frame.Length) return Fail(PacketReadStatus.InvalidData, PacketReadErrorCode.TrailingBytes, read, null, "The packet body contains trailing bytes.");
        var packet = new global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.PacketRgb(value0, value1, value2);
        _cursor += read;
        Consumed = read;
        return packet;
    }
}

public sealed class TerrariaV4_CombatTextStringPacket_Format0PacketCodecWriter
{
    public MemoryStream? TryWrite(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.PacketRgb packet)
    {
        long length = 0L + (long)(1) + (long)(1) + (long)(1);
        if (length > 65532) return null;
        var stream = new MemoryStream((int)length);
        Span<byte> buffer = stackalloc byte[8];
        stream.WriteByte((byte)(packet.Red));
        if (stream.Length > 65532) return null;
        stream.WriteByte((byte)(packet.Green));
        if (stream.Length > 65532) return null;
        stream.WriteByte((byte)(packet.Blue));
        if (stream.Length > 65532) return null;
        stream.Position = 0;
        return stream;
    }
}
internal static class TerrariaV4_CombatTextStringPacketWireFormats
{
    internal static global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.PacketRgb Read0(PacketWireReader reader)
    {
        var nested = new TerrariaV4_CombatTextStringPacket_Format0PacketCodecReader(reader.Frame.Slice(reader.Position));
        var result = nested.ReadDetailed();
        if (!result.Success)
        {
            var error = result.Error!;
            reader.Skip(error.Offset);
            if (error.Status == PacketReadStatus.Truncated) throw new PacketWireTruncationException(error.Message, reader);
            throw new PacketWireFormatException(error.Message, error.Code);
        }
        reader.Skip(result.Consumed);
        return result.Packet!;
    }
    internal static void Write0(PacketWireWriter writer, global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.PacketRgb value)
    {
        var nested = new TerrariaV4_CombatTextStringPacket_Format0PacketCodecWriter();
        using var stream = nested.TryWrite(value);
        if (stream is null) throw new PacketWireFormatException("The nested format value is invalid.");
        writer.WriteBytes(stream.GetBuffer().AsSpan(0, checked((int)stream.Length)));
    }
    internal static global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.NetworkText Read1(PacketWireReader reader)
    {
        return global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.NetworkTextCodec.Read(reader);
    }
    internal static void Write1(PacketWireWriter writer, global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.NetworkText value)
    {
        global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.NetworkTextCodec.Write(writer, value);
    }
}
