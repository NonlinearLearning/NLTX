#nullable enable
using System;
using System.IO;
using System.Buffers.Binary;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Generated;

public sealed class TerrariaV4_SyncProjectilePacketPacketCodecReader
{
    private readonly ReadOnlyMemory<byte> _frame;
    private int _cursor;

    public TerrariaV4_SyncProjectilePacketPacketCodecReader(ReadOnlyMemory<byte> frame)
    {
        _frame = frame;
        _ = global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.ProtocolInputs.Instance;
    }

    // Reads one frame out of a larger receive buffer. The declared length is the
    // frame boundary: neither a scalar field nor a codec block can reach past it,
    // so a first frame can never consume bytes belonging to the second.
    public TerrariaV4_SyncProjectilePacketPacketCodecReader(ReadOnlyMemory<byte> buffer, int frameLength)
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

    public global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.SyncProjectilePacket? TryRead()
        => TryReadCore(requireFrameEnd: false);

    public global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.SyncProjectilePacket? TryReadFrame()
        => TryReadCore(requireFrameEnd: true);

    public PacketReadResult<global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.SyncProjectilePacket> ReadDetailed() => ReadResult(false);
    public PacketReadResult<global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.SyncProjectilePacket> ReadFrameDetailed() => ReadResult(true);

    private PacketReadResult<global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.SyncProjectilePacket> ReadResult(bool requireFrameEnd)
    {
        var packet = TryReadCore(requireFrameEnd);
        if (LastError is { } error) return PacketReadResult<global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.SyncProjectilePacket>.Failed(error);
        return PacketReadResult<global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.SyncProjectilePacket>.Succeeded(packet!, Consumed);
    }

    private global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.SyncProjectilePacket? Fail(PacketReadStatus status, PacketReadErrorCode code, int offset, string? member, string message)
    {
        LastError = new PacketReadError(status, code, _cursor + offset, member, message);
        return null;
    }

    private global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.SyncProjectilePacket? TryReadCore(bool requireFrameEnd)
    {
        Consumed = 0;
        LastError = null;
        var read = 0;
        if (requireFrameEnd && _frame.Length - _cursor > 65532) return Fail(PacketReadStatus.InvalidData, PacketReadErrorCode.LengthOutOfRange, read, null, "The packet body exceeds its protocol capacity.");
        var source = _frame.Span.Slice(_cursor, Math.Min(_frame.Length - _cursor, 65532));
        int codecAvailable0 = source.Length - read;
        if (codecAvailable0 < 22) return Fail(PacketReadStatus.Truncated, PacketReadErrorCode.Truncated, read, "Read", "The codec block and following fields are shorter than their minimum length.");
        int codecBound0 = global::System.Math.Min(codecAvailable0 - 0, 47);
        var codec0 = new global::Terraria.NetWork.Prototype.PacketDesignCompiler.Wire.PacketWireReader(_frame, _cursor + read, codecBound0);
        int codecStart0 = codec0.Position;
        global::System.Int16 codecValue0_1;
        global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.PacketVector2 codecValue0_2;
        global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.PacketVector2 codecValue0_3;
        global::System.Byte codecValue0_4;
        global::System.Int16 codecValue0_5;
        global::System.Single codecValue0_6;
        global::System.Single codecValue0_7;
        global::System.Nullable<global::System.UInt16> codecValue0_8;
        global::System.Nullable<global::System.Int16> codecValue0_9;
        global::System.Nullable<global::System.Single> codecValue0_10;
        global::System.Nullable<global::System.Int16> codecValue0_11;
        global::System.Nullable<global::System.Int16> codecValue0_12;
        global::System.Single codecValue0_13;
        try
        {
            var codecResult0 = global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.SyncProjectilePacket.Read(codec0);
            codecValue0_1 = codecResult0.Item1;
            codecValue0_2 = codecResult0.Item2;
            codecValue0_3 = codecResult0.Item3;
            codecValue0_4 = codecResult0.Item4;
            codecValue0_5 = codecResult0.Item5;
            codecValue0_6 = codecResult0.Item6;
            codecValue0_7 = codecResult0.Item7;
            codecValue0_8 = codecResult0.Rest.Item1;
            codecValue0_9 = codecResult0.Rest.Item2;
            codecValue0_10 = codecResult0.Rest.Item3;
            codecValue0_11 = codecResult0.Rest.Item4;
            codecValue0_12 = codecResult0.Rest.Item5;
            codecValue0_13 = codecResult0.Rest.Item6;
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
        if (codecRead0 < 22 || codecRead0 > 47) return Fail(PacketReadStatus.InvalidData, PacketReadErrorCode.LengthOutOfRange, read, "Read", "The codec consumption is outside its declared length range.");
        read += codecRead0;
        if (requireFrameEnd && _cursor + read != _frame.Length) return Fail(PacketReadStatus.InvalidData, PacketReadErrorCode.TrailingBytes, read, null, "The packet body contains trailing bytes.");
        var packet = new global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.SyncProjectilePacket
        {
            Identity = codecValue0_1,
            Position = codecValue0_2,
            Velocity = codecValue0_3,
            Owner = codecValue0_4,
            ProjectileType = codecValue0_5,
            Ai0 = codecValue0_6,
            Ai1 = codecValue0_7,
            BannerId = codecValue0_8,
            Damage = codecValue0_9,
            Knockback = codecValue0_10,
            OriginalDamage = codecValue0_11,
            ProjectileUuid = codecValue0_12,
            Ai2 = codecValue0_13,
        };
        _cursor += read;
        Consumed = read;
        return packet;
    }
}

public sealed class TerrariaV4_SyncProjectilePacketPacketCodecWriter
{
    private readonly global::System.Func<global::System.Int16, global::System.Boolean> _input0;

    public TerrariaV4_SyncProjectilePacketPacketCodecWriter()
    {
        var external = global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.ProtocolInputs.Instance;
        _input0 = external.NeedsUuid;
    }

    public MemoryStream? TryWrite(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.SyncProjectilePacket packet)
    {
        if (packet is null) return null;
        var stream = new MemoryStream();
        Span<byte> buffer = stackalloc byte[8];
        long codecAvailable0 = 65532L - stream.Length - 0;
        if (codecAvailable0 < 22) return null;
        int codecBound0 = (int)Math.Min(codecAvailable0, 47);
        var codec0 = new global::Terraria.NetWork.Prototype.PacketDesignCompiler.Wire.PacketWireWriter(stream, codecBound0);
        int codecStart0 = codec0.BytesWritten;
        try
        {
            global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.SyncProjectilePacket.Write(codec0, packet.Identity, packet.Position, packet.Velocity, packet.Owner, packet.ProjectileType, packet.Ai0, packet.Ai1, packet.BannerId, packet.Damage, packet.Knockback, packet.OriginalDamage, packet.ProjectileUuid, packet.Ai2, _input0);
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
        if (codecWritten0 < 22 || codecWritten0 > 47) return null;
        if (stream.Length > 65532) return null;
        if (stream.Length > 65532) return null;
        if (stream.Length > 65532) return null;
        if (stream.Length > 65532) return null;
        if (stream.Length > 65532) return null;
        if (stream.Length > 65532) return null;
        if (stream.Length > 65532) return null;
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
