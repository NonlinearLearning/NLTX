#nullable enable
using System;
using System.IO;
using System.Buffers.Binary;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Generated;

public sealed class TerrariaV4_PlayerBuffsPacketCodecReader
{
    private readonly ReadOnlyMemory<byte> _frame;
    private int _cursor;

    public TerrariaV4_PlayerBuffsPacketCodecReader(ReadOnlyMemory<byte> frame)
    {
        _frame = frame;
        _ = global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.ProtocolInputs.Instance;
    }

    // Reads one frame out of a larger receive buffer. The declared length is the
    // frame boundary: neither a scalar field nor a codec block can reach past it,
    // so a first frame can never consume bytes belonging to the second.
    public TerrariaV4_PlayerBuffsPacketCodecReader(ReadOnlyMemory<byte> buffer, int frameLength)
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

    public global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.PlayerBuffsPacket? TryRead()
        => TryReadCore(requireFrameEnd: false);

    public global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.PlayerBuffsPacket? TryReadFrame()
        => TryReadCore(requireFrameEnd: true);

    public PacketReadResult<global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.PlayerBuffsPacket> ReadDetailed() => ReadResult(false);
    public PacketReadResult<global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.PlayerBuffsPacket> ReadFrameDetailed() => ReadResult(true);

    private PacketReadResult<global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.PlayerBuffsPacket> ReadResult(bool requireFrameEnd)
    {
        var packet = TryReadCore(requireFrameEnd);
        if (LastError is { } error) return PacketReadResult<global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.PlayerBuffsPacket>.Failed(error);
        return PacketReadResult<global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.PlayerBuffsPacket>.Succeeded(packet!, Consumed);
    }

    private global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.PlayerBuffsPacket? Fail(PacketReadStatus status, PacketReadErrorCode code, int offset, string? member, string message)
    {
        LastError = new PacketReadError(status, code, _cursor + offset, member, message);
        return null;
    }

    private global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.PlayerBuffsPacket? TryReadCore(bool requireFrameEnd)
    {
        Consumed = 0;
        LastError = null;
        var read = 0;
        if (requireFrameEnd && _frame.Length - _cursor > 65532) return Fail(PacketReadStatus.InvalidData, PacketReadErrorCode.LengthOutOfRange, read, null, "The packet body exceeds its protocol capacity.");
        var source = _frame.Span.Slice(_cursor, Math.Min(_frame.Length - _cursor, 65532));
        if (source.Length - read < 1) return Fail(PacketReadStatus.Truncated, PacketReadErrorCode.Truncated, read, "Player", "The scalar field is truncated.");
        var value0 = source[read];
        read += 1;
        int codecAvailable1 = source.Length - read;
        if (codecAvailable1 < 0) return Fail(PacketReadStatus.Truncated, PacketReadErrorCode.Truncated, read, "ReadBuffTypes", "The codec block and following fields are shorter than their minimum length.");
        int codecBound1 = global::System.Math.Min(codecAvailable1 - 0, 65531);
        var codec1 = new global::Terraria.NetWork.Prototype.PacketDesignCompiler.Wire.PacketWireReader(_frame, _cursor + read, codecBound1);
        int codecStart1 = codec1.Position;
        global::System.Collections.Generic.IReadOnlyList<global::System.UInt16> codecValue1_1;
        try
        {
            codecValue1_1 = global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.PlayerBuffsPacket.ReadBuffTypes(codec1)!;
        }
        catch (global::Terraria.NetWork.Prototype.PacketDesignCompiler.Wire.PacketWireTruncationException exception)
        {
            if (codecAvailable1 > codecBound1 && ReferenceEquals(exception.Reader, codec1)) return Fail(PacketReadStatus.InvalidData, PacketReadErrorCode.LengthOutOfRange, read + codec1.Position, "ReadBuffTypes", exception.Message);
            return Fail(PacketReadStatus.Truncated, PacketReadErrorCode.Truncated, read + codec1.Position, "ReadBuffTypes", exception.Message);
        }
        catch (global::Terraria.NetWork.Prototype.PacketDesignCompiler.Wire.PacketWireFormatException exception)
        {
            return Fail(PacketReadStatus.InvalidData, exception.Code, read + codec1.Position, "ReadBuffTypes", exception.Message);
        }
        catch (InvalidDataException exception)
        {
            return Fail(PacketReadStatus.InvalidData, PacketReadErrorCode.InvalidCodecData, read + codec1.Position, "ReadBuffTypes", exception.Message);
        }
        int codecRead1 = codec1.Position - codecStart1;
        if (codecRead1 < 0 || codecRead1 > 65531) return Fail(PacketReadStatus.InvalidData, PacketReadErrorCode.LengthOutOfRange, read, "ReadBuffTypes", "The codec consumption is outside its declared length range.");
        read += codecRead1;
        if (requireFrameEnd && _cursor + read != _frame.Length) return Fail(PacketReadStatus.InvalidData, PacketReadErrorCode.TrailingBytes, read, null, "The packet body contains trailing bytes.");
        var packet = new global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.PlayerBuffsPacket
        {
            Player = value0,
            BuffTypes = codecValue1_1,
        };
        _cursor += read;
        Consumed = read;
        return packet;
    }
}

public sealed class TerrariaV4_PlayerBuffsPacketCodecWriter
{

    public TerrariaV4_PlayerBuffsPacketCodecWriter()
    {
        _ = global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.ProtocolInputs.Instance;
    }

    public MemoryStream? TryWrite(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.PlayerBuffsPacket packet)
    {
        if (packet is null) return null;
        var stream = new MemoryStream();
        Span<byte> buffer = stackalloc byte[8];
        stream.WriteByte((byte)(packet.Player));
        if (stream.Length > 65532) return null;
        long codecAvailable1 = 65532L - stream.Length - 0;
        if (codecAvailable1 < 0) return null;
        int codecBound1 = (int)Math.Min(codecAvailable1, 65531);
        var codec1 = new global::Terraria.NetWork.Prototype.PacketDesignCompiler.Wire.PacketWireWriter(stream, codecBound1);
        int codecStart1 = codec1.BytesWritten;
        try
        {
            global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.PlayerBuffsPacket.WriteBuffTypes(codec1, packet.BuffTypes);
        }
        catch (global::Terraria.NetWork.Prototype.PacketDesignCompiler.Wire.PacketWireLimitException)
        {
            return null;
        }
        catch (global::Terraria.NetWork.Prototype.PacketDesignCompiler.Wire.PacketWireFormatException)
        {
            return null;
        }
        int codecWritten1 = codec1.BytesWritten - codecStart1;
        if (codecWritten1 < 0 || codecWritten1 > 65531) return null;
        if (stream.Length > 65532) return null;
        stream.Position = 0;
        return stream;
    }
}
