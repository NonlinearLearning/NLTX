#nullable enable
using System;
using System.IO;
using System.Buffers.Binary;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Generated;

public sealed class TerrariaV4_SyncNPCPacketPacketCodecReader
{
    private readonly ReadOnlyMemory<byte> _frame;
    private readonly global::System.Boolean[] _input0;
    private int _cursor;

    public TerrariaV4_SyncNPCPacketPacketCodecReader(ReadOnlyMemory<byte> frame)
    {
        _frame = frame;
        var external = global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.ProtocolInputs.Instance;
        _input0 = external.CatchableTypes;
    }

    // Reads one frame out of a larger receive buffer. The declared length is the
    // frame boundary: neither a scalar field nor a codec block can reach past it,
    // so a first frame can never consume bytes belonging to the second.
    public TerrariaV4_SyncNPCPacketPacketCodecReader(ReadOnlyMemory<byte> buffer, int frameLength)
    {
        if (frameLength < 0 || frameLength > buffer.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(frameLength));
        }

        _frame = buffer.Slice(0, frameLength);
        var external = global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.ProtocolInputs.Instance;
        _input0 = external.CatchableTypes;
    }

    public int Consumed { get; private set; }
    public PacketReadError? LastError { get; private set; }

    public global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.SyncNPCPacket? TryRead()
        => TryReadCore(requireFrameEnd: false);

    public global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.SyncNPCPacket? TryReadFrame()
        => TryReadCore(requireFrameEnd: true);

    public PacketReadResult<global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.SyncNPCPacket> ReadDetailed() => ReadResult(false);
    public PacketReadResult<global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.SyncNPCPacket> ReadFrameDetailed() => ReadResult(true);

    private PacketReadResult<global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.SyncNPCPacket> ReadResult(bool requireFrameEnd)
    {
        var packet = TryReadCore(requireFrameEnd);
        if (LastError is { } error) return PacketReadResult<global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.SyncNPCPacket>.Failed(error);
        return PacketReadResult<global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.SyncNPCPacket>.Succeeded(packet!, Consumed);
    }

    private global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.SyncNPCPacket? Fail(PacketReadStatus status, PacketReadErrorCode code, int offset, string? member, string message)
    {
        LastError = new PacketReadError(status, code, _cursor + offset, member, message);
        return null;
    }

    private global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.SyncNPCPacket? TryReadCore(bool requireFrameEnd)
    {
        Consumed = 0;
        LastError = null;
        var read = 0;
        if (requireFrameEnd && _frame.Length - _cursor > 65532) return Fail(PacketReadStatus.InvalidData, PacketReadErrorCode.LengthOutOfRange, read, null, "The packet body exceeds its protocol capacity.");
        var source = _frame.Span.Slice(_cursor, Math.Min(_frame.Length - _cursor, 65532));
        int codecAvailable0 = source.Length - read;
        if (codecAvailable0 < 24) return Fail(PacketReadStatus.Truncated, PacketReadErrorCode.Truncated, read, "Read", "The codec block and following fields are shorter than their minimum length.");
        int codecBound0 = global::System.Math.Min(codecAvailable0 - 0, 65532);
        var codec0 = new global::Terraria.NetWork.Prototype.PacketDesignCompiler.Wire.PacketWireReader(_frame, _cursor + read, codecBound0);
        int codecStart0 = codec0.Position;
        global::System.Int16 codecValue0_1;
        global::System.Single codecValue0_2;
        global::System.Single codecValue0_3;
        global::System.Single codecValue0_4;
        global::System.Single codecValue0_5;
        global::System.UInt16 codecValue0_6;
        global::System.Boolean codecValue0_7;
        global::System.Boolean codecValue0_8;
        global::System.Boolean codecValue0_9;
        global::System.Boolean codecValue0_10;
        global::System.Boolean codecValue0_11;
        global::System.Boolean codecValue0_12;
        global::System.Boolean codecValue0_13;
        global::System.Single[] codecValue0_14;
        global::System.Int16 codecValue0_15;
        global::System.Nullable<global::System.Byte> codecValue0_16;
        global::System.Nullable<global::System.Single> codecValue0_17;
        global::System.Nullable<global::System.Int32> codecValue0_18;
        global::System.Nullable<global::System.Byte> codecValue0_19;
        global::System.Nullable<global::System.Byte> codecValue0_20;
        try
        {
            var codecResult0 = global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.SyncNPCPacket.Read(codec0, _input0);
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
            codecValue0_14 = codecResult0.Rest.Item7!;
            codecValue0_15 = codecResult0.Rest.Rest.Item1;
            codecValue0_16 = codecResult0.Rest.Rest.Item2;
            codecValue0_17 = codecResult0.Rest.Rest.Item3;
            codecValue0_18 = codecResult0.Rest.Rest.Item4;
            codecValue0_19 = codecResult0.Rest.Rest.Item5;
            codecValue0_20 = codecResult0.Rest.Rest.Item6;
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
        if (codecRead0 < 24 || codecRead0 > 65532) return Fail(PacketReadStatus.InvalidData, PacketReadErrorCode.LengthOutOfRange, read, "Read", "The codec consumption is outside its declared length range.");
        read += codecRead0;
        if (requireFrameEnd && _cursor + read != _frame.Length) return Fail(PacketReadStatus.InvalidData, PacketReadErrorCode.TrailingBytes, read, null, "The packet body contains trailing bytes.");
        var packet = new global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.SyncNPCPacket
        {
            NpcSlot = codecValue0_1,
            PositionX = codecValue0_2,
            PositionY = codecValue0_3,
            VelocityX = codecValue0_4,
            VelocityY = codecValue0_5,
            Target = codecValue0_6,
            DirectionPositive = codecValue0_7,
            DirectionYPositive = codecValue0_8,
            SpriteDirectionPositive = codecValue0_9,
            FullLife = codecValue0_10,
            SpawnedFromStatue = codecValue0_11,
            SpawnNeedsSyncing = codecValue0_12,
            Shimmering = codecValue0_13,
            Ai = codecValue0_14,
            NetId = codecValue0_15,
            PlayerCount = codecValue0_16,
            Difficulty = codecValue0_17,
            Life = codecValue0_18,
            EncodedLifeWidth = codecValue0_19,
            CatchableReleaseOwner = codecValue0_20,
        };
        _cursor += read;
        Consumed = read;
        return packet;
    }
}

public sealed class TerrariaV4_SyncNPCPacketPacketCodecWriter
{
    private readonly global::System.Boolean[] _input0;
    private readonly global::System.Func<global::System.Int16, global::System.Nullable<global::System.Byte>, global::System.Nullable<global::System.Single>, global::System.Byte> _input1;

    public TerrariaV4_SyncNPCPacketPacketCodecWriter()
    {
        var external = global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.ProtocolInputs.Instance;
        _input0 = external.CatchableTypes;
        _input1 = external.LifeWidthResolver;
    }

    public MemoryStream? TryWrite(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.SyncNPCPacket packet)
    {
        if (packet is null) return null;
        var stream = new MemoryStream();
        Span<byte> buffer = stackalloc byte[8];
        long codecAvailable0 = 65532L - stream.Length - 0;
        if (codecAvailable0 < 24) return null;
        int codecBound0 = (int)Math.Min(codecAvailable0, 65532);
        var codec0 = new global::Terraria.NetWork.Prototype.PacketDesignCompiler.Wire.PacketWireWriter(stream, codecBound0);
        int codecStart0 = codec0.BytesWritten;
        try
        {
            global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.SyncNPCPacket.Write(codec0, packet.NpcSlot, packet.PositionX, packet.PositionY, packet.VelocityX, packet.VelocityY, packet.Target, packet.DirectionPositive, packet.DirectionYPositive, packet.SpriteDirectionPositive, packet.FullLife, packet.SpawnedFromStatue, packet.SpawnNeedsSyncing, packet.Shimmering, packet.Ai, packet.NetId, packet.PlayerCount, packet.Difficulty, packet.Life, packet.EncodedLifeWidth, packet.CatchableReleaseOwner, _input0, _input1);
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
        if (codecWritten0 < 24 || codecWritten0 > 65532) return null;
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
