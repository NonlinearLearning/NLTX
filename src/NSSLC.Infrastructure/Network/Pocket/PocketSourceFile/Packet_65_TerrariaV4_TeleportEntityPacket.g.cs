#nullable enable
using System;
using System.IO;
using System.Buffers.Binary;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Generated;

public sealed class TerrariaV4_TeleportEntityPacketPacketCodecReader
{
    private readonly ReadOnlyMemory<byte> _frame;
    private int _cursor;

    public TerrariaV4_TeleportEntityPacketPacketCodecReader(ReadOnlyMemory<byte> frame)
    {
        _frame = frame;
        _ = global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.ProtocolInputs.Instance;
    }

    // Reads one frame out of a larger receive buffer. The declared length is the
    // frame boundary: neither a scalar field nor a codec block can reach past it,
    // so a first frame can never consume bytes belonging to the second.
    public TerrariaV4_TeleportEntityPacketPacketCodecReader(ReadOnlyMemory<byte> buffer, int frameLength)
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

    public global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.TeleportEntityPacket? TryRead()
        => TryReadCore(requireFrameEnd: false);

    public global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.TeleportEntityPacket? TryReadFrame()
        => TryReadCore(requireFrameEnd: true);

    public PacketReadResult<global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.TeleportEntityPacket> ReadDetailed() => ReadResult(false);
    public PacketReadResult<global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.TeleportEntityPacket> ReadFrameDetailed() => ReadResult(true);

    private PacketReadResult<global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.TeleportEntityPacket> ReadResult(bool requireFrameEnd)
    {
        var packet = TryReadCore(requireFrameEnd);
        if (LastError is { } error) return PacketReadResult<global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.TeleportEntityPacket>.Failed(error);
        return PacketReadResult<global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.TeleportEntityPacket>.Succeeded(packet!, Consumed);
    }

    private global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.TeleportEntityPacket? Fail(PacketReadStatus status, PacketReadErrorCode code, int offset, string? member, string message)
    {
        LastError = new PacketReadError(status, code, _cursor + offset, member, message);
        return null;
    }

    private global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.TeleportEntityPacket? TryReadCore(bool requireFrameEnd)
    {
        Consumed = 0;
        LastError = null;
        var read = 0;
        if (requireFrameEnd && _frame.Length - _cursor > 65532) return Fail(PacketReadStatus.InvalidData, PacketReadErrorCode.LengthOutOfRange, read, null, "The packet body exceeds its protocol capacity.");
        var source = _frame.Span.Slice(_cursor, Math.Min(_frame.Length - _cursor, 65532));
        int codecAvailable0 = source.Length - read;
        if (codecAvailable0 < 12) return Fail(PacketReadStatus.Truncated, PacketReadErrorCode.Truncated, read, "Read", "The codec block and following fields are shorter than their minimum length.");
        int codecBound0 = global::System.Math.Min(codecAvailable0 - 0, 16);
        var codec0 = new global::Terraria.NetWork.Prototype.PacketDesignCompiler.Wire.PacketWireReader(_frame, _cursor + read, codecBound0);
        int codecStart0 = codec0.Position;
        global::System.Byte codecValue0_1;
        global::System.Int16 codecValue0_2;
        global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.PacketVector2 codecValue0_3;
        global::System.Byte codecValue0_4;
        global::System.Boolean codecValue0_5;
        global::System.Nullable<global::System.Int32> codecValue0_6;
        try
        {
            var codecResult0 = global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.TeleportEntityPacket.Read(codec0);
            codecValue0_1 = codecResult0.Item1;
            codecValue0_2 = codecResult0.Item2;
            codecValue0_3 = codecResult0.Item3;
            codecValue0_4 = codecResult0.Item4;
            codecValue0_5 = codecResult0.Item5;
            codecValue0_6 = codecResult0.Item6;
        }
        catch (global::Terraria.NetWork.Prototype.PacketDesignCompiler.Wire.PacketWireTruncationException exception)
        {
            if (codecAvailable0 > codecBound0 && ReferenceEquals(exception.Reader, codec0)) return Fail(PacketReadStatus.InvalidData, PacketReadErrorCode.LengthOutOfRange, read + codec0.Position, "Read", exception.Message);
            return Fail(PacketReadStatus.Truncated, PacketReadErrorCode.Truncated, read + codec0.Position, "Read", exception.Message);
        }
        catch (global::Terraria.NetWork.Prototype.PacketDesignCompiler.Wire.PacketWireFormatException exception)
        {
            return Fail(PacketReadStatus.InvalidData, exception.Code, read + codec0.Position, "Read", exception.Message);
        }
        catch (InvalidDataException exception)
        {
            return Fail(PacketReadStatus.InvalidData, PacketReadErrorCode.InvalidCodecData, read + codec0.Position, "Read", exception.Message);
        }
        int codecRead0 = codec0.Position - codecStart0;
        if (codecRead0 < 12 || codecRead0 > 16) return Fail(PacketReadStatus.InvalidData, PacketReadErrorCode.LengthOutOfRange, read, "Read", "The codec consumption is outside its declared length range.");
        read += codecRead0;
        if (requireFrameEnd && _cursor + read != _frame.Length) return Fail(PacketReadStatus.InvalidData, PacketReadErrorCode.TrailingBytes, read, null, "The packet body contains trailing bytes.");
        var packet = new global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.TeleportEntityPacket
        {
            EntityKind = codecValue0_1,
            EntityIndex = codecValue0_2,
            Position = codecValue0_3,
            Style = codecValue0_4,
            UseCurrentEntityPosition = codecValue0_5,
            TeleportValue = codecValue0_6,
        };
        _cursor += read;
        Consumed = read;
        return packet;
    }
}

public sealed class TerrariaV4_TeleportEntityPacketPacketCodecWriter
{

    public TerrariaV4_TeleportEntityPacketPacketCodecWriter()
    {
        _ = global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.ProtocolInputs.Instance;
    }

    public MemoryStream? TryWrite(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.TeleportEntityPacket packet)
    {
        if (packet is null) return null;
        var stream = new MemoryStream();
        Span<byte> buffer = stackalloc byte[8];
        long codecAvailable0 = 65532L - stream.Length - 0;
        if (codecAvailable0 < 12) return null;
        int codecBound0 = (int)Math.Min(codecAvailable0, 16);
        var codec0 = new global::Terraria.NetWork.Prototype.PacketDesignCompiler.Wire.PacketWireWriter(stream, codecBound0);
        int codecStart0 = codec0.BytesWritten;
        try
        {
            global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.TeleportEntityPacket.Write(codec0, packet.EntityKind, packet.EntityIndex, packet.Position, packet.Style, packet.UseCurrentEntityPosition, packet.TeleportValue);
        }
        catch (global::Terraria.NetWork.Prototype.PacketDesignCompiler.Wire.PacketWireLimitException)
        {
            return null;
        }
        catch (global::Terraria.NetWork.Prototype.PacketDesignCompiler.Wire.PacketWireFormatException)
        {
            return null;
        }
        int codecWritten0 = codec0.BytesWritten - codecStart0;
        if (codecWritten0 < 12 || codecWritten0 > 16) return null;
        if (stream.Length > 65532) return null;
        if (stream.Length > 65532) return null;
        if (stream.Length > 65532) return null;
        if (stream.Length > 65532) return null;
        if (stream.Length > 65532) return null;
        if (stream.Length > 65532) return null;
        stream.Position = 0;
        return stream;
    }
}
