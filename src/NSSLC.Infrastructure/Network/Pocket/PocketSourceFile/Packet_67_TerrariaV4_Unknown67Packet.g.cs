#nullable enable
using System;
using System.IO;
using System.Buffers.Binary;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Generated;

public sealed class TerrariaV4_Unknown67PacketPacketCodecReader
{
    private readonly ReadOnlyMemory<byte> _frame;
    private int _cursor;

    public TerrariaV4_Unknown67PacketPacketCodecReader(ReadOnlyMemory<byte> frame)
    {
        _frame = frame;
        _ = global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.ProtocolInputs.Instance;
    }

    // Reads one frame out of a larger receive buffer. The declared length is the
    // frame boundary: neither a scalar field nor a codec block can reach past it,
    // so a first frame can never consume bytes belonging to the second.
    public TerrariaV4_Unknown67PacketPacketCodecReader(ReadOnlyMemory<byte> buffer, int frameLength)
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

    public global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.Unknown67Packet? TryRead()
        => TryReadCore(requireFrameEnd: false);

    public global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.Unknown67Packet? TryReadFrame()
        => TryReadCore(requireFrameEnd: true);

    public PacketReadResult<global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.Unknown67Packet> ReadDetailed() => ReadResult(false);
    public PacketReadResult<global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.Unknown67Packet> ReadFrameDetailed() => ReadResult(true);

    private PacketReadResult<global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.Unknown67Packet> ReadResult(bool requireFrameEnd)
    {
        var packet = TryReadCore(requireFrameEnd);
        if (LastError is { } error) return PacketReadResult<global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.Unknown67Packet>.Failed(error);
        return PacketReadResult<global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.Unknown67Packet>.Succeeded(packet!, Consumed);
    }

    private global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.Unknown67Packet? Fail(PacketReadStatus status, PacketReadErrorCode code, int offset, string? member, string message)
    {
        LastError = new PacketReadError(status, code, _cursor + offset, member, message);
        return null;
    }

    private global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.Unknown67Packet? TryReadCore(bool requireFrameEnd)
    {
        Consumed = 0;
        LastError = null;
        var read = 0;
        if (requireFrameEnd && _frame.Length - _cursor > 65532) return Fail(PacketReadStatus.InvalidData, PacketReadErrorCode.LengthOutOfRange, read, null, "The packet body exceeds its protocol capacity.");
        var source = _frame.Span.Slice(_cursor, Math.Min(_frame.Length - _cursor, 65532));
        if (requireFrameEnd && _cursor + read != _frame.Length) return Fail(PacketReadStatus.InvalidData, PacketReadErrorCode.TrailingBytes, read, null, "The packet body contains trailing bytes.");
        var packet = new global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.Unknown67Packet
        {
        };
        _cursor += read;
        Consumed = read;
        return packet;
    }
}

public sealed class TerrariaV4_Unknown67PacketPacketCodecWriter
{

    public TerrariaV4_Unknown67PacketPacketCodecWriter()
    {
        _ = global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.ProtocolInputs.Instance;
    }

    public MemoryStream? TryWrite(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.Unknown67Packet packet)
    {
        if (packet is null) return null;
        long length = 0L;
        if (length > 65532) return null;
        var stream = new MemoryStream((int)length);
        Span<byte> buffer = stackalloc byte[8];
        stream.Position = 0;
        return stream;
    }
}
