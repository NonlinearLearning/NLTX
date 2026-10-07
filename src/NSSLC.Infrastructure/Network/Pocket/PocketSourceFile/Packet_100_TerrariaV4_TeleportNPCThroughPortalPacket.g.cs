#nullable enable
using System;
using System.IO;
using System.Buffers.Binary;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Generated;

public sealed class TerrariaV4_TeleportNPCThroughPortalPacketPacketCodecReader
{
    private readonly ReadOnlyMemory<byte> _frame;
    private int _cursor;

    public TerrariaV4_TeleportNPCThroughPortalPacketPacketCodecReader(ReadOnlyMemory<byte> frame)
    {
        _frame = frame;
        _ = global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.ProtocolInputs.Instance;
    }

    // Reads one frame out of a larger receive buffer. The declared length is the
    // frame boundary: neither a scalar field nor a codec block can reach past it,
    // so a first frame can never consume bytes belonging to the second.
    public TerrariaV4_TeleportNPCThroughPortalPacketPacketCodecReader(ReadOnlyMemory<byte> buffer, int frameLength)
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

    public global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.TeleportNPCThroughPortalPacket? TryRead()
        => TryReadCore(requireFrameEnd: false);

    public global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.TeleportNPCThroughPortalPacket? TryReadFrame()
        => TryReadCore(requireFrameEnd: true);

    public PacketReadResult<global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.TeleportNPCThroughPortalPacket> ReadDetailed() => ReadResult(false);
    public PacketReadResult<global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.TeleportNPCThroughPortalPacket> ReadFrameDetailed() => ReadResult(true);

    private PacketReadResult<global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.TeleportNPCThroughPortalPacket> ReadResult(bool requireFrameEnd)
    {
        var packet = TryReadCore(requireFrameEnd);
        if (LastError is { } error) return PacketReadResult<global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.TeleportNPCThroughPortalPacket>.Failed(error);
        return PacketReadResult<global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.TeleportNPCThroughPortalPacket>.Succeeded(packet!, Consumed);
    }

    private global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.TeleportNPCThroughPortalPacket? Fail(PacketReadStatus status, PacketReadErrorCode code, int offset, string? member, string message)
    {
        LastError = new PacketReadError(status, code, _cursor + offset, member, message);
        return null;
    }

    private global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.TeleportNPCThroughPortalPacket? TryReadCore(bool requireFrameEnd)
    {
        Consumed = 0;
        LastError = null;
        var read = 0;
        if (requireFrameEnd && _frame.Length - _cursor > 65532) return Fail(PacketReadStatus.InvalidData, PacketReadErrorCode.LengthOutOfRange, read, null, "The packet body exceeds its protocol capacity.");
        var source = _frame.Span.Slice(_cursor, Math.Min(_frame.Length - _cursor, 65532));
        if (source.Length - read < 2) return Fail(PacketReadStatus.Truncated, PacketReadErrorCode.Truncated, read, "NpcIndex", "The scalar field is truncated.");
        var value0 = BinaryPrimitives.ReadUInt16LittleEndian(source.Slice(read, 2));
        read += 2;
        if (source.Length - read < 2) return Fail(PacketReadStatus.Truncated, PacketReadErrorCode.Truncated, read, "PortalColorIndex", "The scalar field is truncated.");
        var value1 = BinaryPrimitives.ReadInt16LittleEndian(source.Slice(read, 2));
        read += 2;
        if (source.Length - read < 4) return Fail(PacketReadStatus.Truncated, PacketReadErrorCode.Truncated, read, "PositionX", "The scalar field is truncated.");
        var value2 = BinaryPrimitives.ReadSingleLittleEndian(source.Slice(read, 4));
        read += 4;
        if (source.Length - read < 4) return Fail(PacketReadStatus.Truncated, PacketReadErrorCode.Truncated, read, "PositionY", "The scalar field is truncated.");
        var value3 = BinaryPrimitives.ReadSingleLittleEndian(source.Slice(read, 4));
        read += 4;
        if (source.Length - read < 4) return Fail(PacketReadStatus.Truncated, PacketReadErrorCode.Truncated, read, "VelocityX", "The scalar field is truncated.");
        var value4 = BinaryPrimitives.ReadSingleLittleEndian(source.Slice(read, 4));
        read += 4;
        if (source.Length - read < 4) return Fail(PacketReadStatus.Truncated, PacketReadErrorCode.Truncated, read, "VelocityY", "The scalar field is truncated.");
        var value5 = BinaryPrimitives.ReadSingleLittleEndian(source.Slice(read, 4));
        read += 4;
        if (requireFrameEnd && _cursor + read != _frame.Length) return Fail(PacketReadStatus.InvalidData, PacketReadErrorCode.TrailingBytes, read, null, "The packet body contains trailing bytes.");
        var packet = new global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.TeleportNPCThroughPortalPacket
        {
            NpcIndex = value0,
            PortalColorIndex = value1,
            PositionX = value2,
            PositionY = value3,
            VelocityX = value4,
            VelocityY = value5,
        };
        _cursor += read;
        Consumed = read;
        return packet;
    }
}

public sealed class TerrariaV4_TeleportNPCThroughPortalPacketPacketCodecWriter
{

    public TerrariaV4_TeleportNPCThroughPortalPacketPacketCodecWriter()
    {
        _ = global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.ProtocolInputs.Instance;
    }

    public MemoryStream? TryWrite(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.TeleportNPCThroughPortalPacket packet)
    {
        if (packet is null) return null;
        long length = 0L + (long)(2) + (long)(2) + (long)(4) + (long)(4) + (long)(4) + (long)(4);
        if (length > 65532) return null;
        var stream = new MemoryStream((int)length);
        Span<byte> buffer = stackalloc byte[8];
        BinaryPrimitives.WriteUInt16LittleEndian(buffer, packet.NpcIndex);
        stream.Write(buffer[..2]);
        if (stream.Length > 65532) return null;
        BinaryPrimitives.WriteInt16LittleEndian(buffer, packet.PortalColorIndex);
        stream.Write(buffer[..2]);
        if (stream.Length > 65532) return null;
        BinaryPrimitives.WriteSingleLittleEndian(buffer, packet.PositionX);
        stream.Write(buffer[..4]);
        if (stream.Length > 65532) return null;
        BinaryPrimitives.WriteSingleLittleEndian(buffer, packet.PositionY);
        stream.Write(buffer[..4]);
        if (stream.Length > 65532) return null;
        BinaryPrimitives.WriteSingleLittleEndian(buffer, packet.VelocityX);
        stream.Write(buffer[..4]);
        if (stream.Length > 65532) return null;
        BinaryPrimitives.WriteSingleLittleEndian(buffer, packet.VelocityY);
        stream.Write(buffer[..4]);
        if (stream.Length > 65532) return null;
        stream.Position = 0;
        return stream;
    }
}
