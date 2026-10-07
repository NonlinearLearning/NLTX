#nullable enable
using System;
using System.IO;
using System.Buffers.Binary;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Generated;

public sealed class TerrariaV4_AreaTileChangePacketPacketCodecReader
{
    private readonly ReadOnlyMemory<byte> _frame;
    private readonly global::System.Boolean[] _input0;
    private int _cursor;

    public TerrariaV4_AreaTileChangePacketPacketCodecReader(ReadOnlyMemory<byte> frame)
    {
        _frame = frame;
        var external = global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.ProtocolInputs.Instance;
        _input0 = external.FrameImportant;
    }

    // Reads one frame out of a larger receive buffer. The declared length is the
    // frame boundary: neither a scalar field nor a codec block can reach past it,
    // so a first frame can never consume bytes belonging to the second.
    public TerrariaV4_AreaTileChangePacketPacketCodecReader(ReadOnlyMemory<byte> buffer, int frameLength)
    {
        if (frameLength < 0 || frameLength > buffer.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(frameLength));
        }

        _frame = buffer.Slice(0, frameLength);
        var external = global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.ProtocolInputs.Instance;
        _input0 = external.FrameImportant;
    }

    public int Consumed { get; private set; }
    public PacketReadError? LastError { get; private set; }

    public global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.AreaTileChangePacket? TryRead()
        => TryReadCore(requireFrameEnd: false);

    public global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.AreaTileChangePacket? TryReadFrame()
        => TryReadCore(requireFrameEnd: true);

    public PacketReadResult<global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.AreaTileChangePacket> ReadDetailed() => ReadResult(false);
    public PacketReadResult<global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.AreaTileChangePacket> ReadFrameDetailed() => ReadResult(true);

    private PacketReadResult<global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.AreaTileChangePacket> ReadResult(bool requireFrameEnd)
    {
        var packet = TryReadCore(requireFrameEnd);
        if (LastError is { } error) return PacketReadResult<global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.AreaTileChangePacket>.Failed(error);
        return PacketReadResult<global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.AreaTileChangePacket>.Succeeded(packet!, Consumed);
    }

    private global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.AreaTileChangePacket? Fail(PacketReadStatus status, PacketReadErrorCode code, int offset, string? member, string message)
    {
        LastError = new PacketReadError(status, code, _cursor + offset, member, message);
        return null;
    }

    private global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.AreaTileChangePacket? TryReadCore(bool requireFrameEnd)
    {
        Consumed = 0;
        LastError = null;
        var read = 0;
        if (requireFrameEnd && _frame.Length - _cursor > 65532) return Fail(PacketReadStatus.InvalidData, PacketReadErrorCode.LengthOutOfRange, read, null, "The packet body exceeds its protocol capacity.");
        var source = _frame.Span.Slice(_cursor, Math.Min(_frame.Length - _cursor, 65532));
        if (source.Length - read < 2) return Fail(PacketReadStatus.Truncated, PacketReadErrorCode.Truncated, read, "StartX", "The scalar field is truncated.");
        var value0 = BinaryPrimitives.ReadInt16LittleEndian(source.Slice(read, 2));
        read += 2;
        if (source.Length - read < 2) return Fail(PacketReadStatus.Truncated, PacketReadErrorCode.Truncated, read, "StartY", "The scalar field is truncated.");
        var value1 = BinaryPrimitives.ReadInt16LittleEndian(source.Slice(read, 2));
        read += 2;
        if (source.Length - read < 1) return Fail(PacketReadStatus.Truncated, PacketReadErrorCode.Truncated, read, "Width", "The scalar field is truncated.");
        var value2 = source[read];
        read += 1;
        if (source.Length - read < 1) return Fail(PacketReadStatus.Truncated, PacketReadErrorCode.Truncated, read, "Height", "The scalar field is truncated.");
        var value3 = source[read];
        read += 1;
        if (source.Length - read < 1) return Fail(PacketReadStatus.Truncated, PacketReadErrorCode.Truncated, read, "ChangeType", "The scalar field is truncated.");
        var value4 = source[read];
        read += 1;
        int codecAvailable5 = source.Length - read;
        if (codecAvailable5 < 0) return Fail(PacketReadStatus.Truncated, PacketReadErrorCode.Truncated, read, "ReadTileBytes", "The codec block and following fields are shorter than their minimum length.");
        int codecBound5 = global::System.Math.Min(codecAvailable5 - 0, 65525);
        var codec5 = new global::Terraria.NetWork.Prototype.PacketDesignCompiler.Wire.PacketWireReader(_frame, _cursor + read, codecBound5);
        int codecStart5 = codec5.Position;
        global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.Packet20Tile[] codecValue5_1;
        try
        {
            codecValue5_1 = global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.AreaTileChangePacket.ReadTileBytes(codec5, value2, value3, _input0)!;
        }
        catch (global::Terraria.NetWork.Prototype.PacketDesignCompiler.Wire.PacketWireTruncationException exception)
        {
            if (codecAvailable5 > codecBound5 && ReferenceEquals(exception.Reader, codec5)) return Fail(PacketReadStatus.InvalidData, PacketReadErrorCode.LengthOutOfRange, read + codec5.Position, "ReadTileBytes", exception.Message);
            return Fail(PacketReadStatus.Truncated, PacketReadErrorCode.Truncated, read + codec5.Position, "ReadTileBytes", exception.Message);
        }
        catch (global::Terraria.NetWork.Prototype.PacketDesignCompiler.Wire.PacketWireFormatException exception)
        {
            return Fail(PacketReadStatus.InvalidData, exception.Code, read + codec5.Position, "ReadTileBytes", exception.Message);
        }
        catch (InvalidDataException exception)
        {
            return Fail(PacketReadStatus.InvalidData, PacketReadErrorCode.InvalidCodecData, read + codec5.Position, "ReadTileBytes", exception.Message);
        }
        int codecRead5 = codec5.Position - codecStart5;
        if (codecRead5 < 0 || codecRead5 > 65525) return Fail(PacketReadStatus.InvalidData, PacketReadErrorCode.LengthOutOfRange, read, "ReadTileBytes", "The codec consumption is outside its declared length range.");
        read += codecRead5;
        if (requireFrameEnd && _cursor + read != _frame.Length) return Fail(PacketReadStatus.InvalidData, PacketReadErrorCode.TrailingBytes, read, null, "The packet body contains trailing bytes.");
        var packet = new global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.AreaTileChangePacket
        {
            StartX = value0,
            StartY = value1,
            Width = value2,
            Height = value3,
            ChangeType = value4,
            Tiles = codecValue5_1,
        };
        _cursor += read;
        Consumed = read;
        return packet;
    }
}

public sealed class TerrariaV4_AreaTileChangePacketPacketCodecWriter
{
    private readonly global::System.Boolean[] _input0;
    private readonly global::System.Boolean _input1;

    public TerrariaV4_AreaTileChangePacketPacketCodecWriter()
    {
        var external = global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.ProtocolInputs.Instance;
        _input0 = external.FrameImportant;
        _input1 = external.IsServer;
    }

    public MemoryStream? TryWrite(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.AreaTileChangePacket packet)
    {
        if (packet is null) return null;
        var stream = new MemoryStream();
        Span<byte> buffer = stackalloc byte[8];
        BinaryPrimitives.WriteInt16LittleEndian(buffer, packet.StartX);
        stream.Write(buffer[..2]);
        if (stream.Length > 65532) return null;
        BinaryPrimitives.WriteInt16LittleEndian(buffer, packet.StartY);
        stream.Write(buffer[..2]);
        if (stream.Length > 65532) return null;
        stream.WriteByte((byte)(packet.Width));
        if (stream.Length > 65532) return null;
        stream.WriteByte((byte)(packet.Height));
        if (stream.Length > 65532) return null;
        stream.WriteByte((byte)(packet.ChangeType));
        if (stream.Length > 65532) return null;
        long codecAvailable5 = 65532L - stream.Length - 0;
        if (codecAvailable5 < 0) return null;
        int codecBound5 = (int)Math.Min(codecAvailable5, 65525);
        var codec5 = new global::Terraria.NetWork.Prototype.PacketDesignCompiler.Wire.PacketWireWriter(stream, codecBound5);
        int codecStart5 = codec5.BytesWritten;
        try
        {
            global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.AreaTileChangePacket.WriteTileBytes(codec5, packet.Width, packet.Height, _input0, _input1, packet.Tiles);
        }
        catch (global::Terraria.NetWork.Prototype.PacketDesignCompiler.Wire.PacketWireLimitException)
        {
            return null;
        }
        catch (global::Terraria.NetWork.Prototype.PacketDesignCompiler.Wire.PacketWireFormatException)
        {
            return null;
        }
        int codecWritten5 = codec5.BytesWritten - codecStart5;
        if (codecWritten5 < 0 || codecWritten5 > 65525) return null;
        if (stream.Length > 65532) return null;
        stream.Position = 0;
        return stream;
    }
}
