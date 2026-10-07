#nullable enable
using System;
using System.IO;
using System.Buffers.Binary;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Generated;

public sealed class TerrariaV4_WorldDataPacketPacketCodecReader
{
    private readonly ReadOnlyMemory<byte> _frame;
    private int _cursor;

    public TerrariaV4_WorldDataPacketPacketCodecReader(ReadOnlyMemory<byte> frame)
    {
        _frame = frame;
        _ = global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.ProtocolInputs.Instance;
    }

    // Reads one frame out of a larger receive buffer. The declared length is the
    // frame boundary: neither a scalar field nor a codec block can reach past it,
    // so a first frame can never consume bytes belonging to the second.
    public TerrariaV4_WorldDataPacketPacketCodecReader(ReadOnlyMemory<byte> buffer, int frameLength)
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

    public global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.WorldDataPacket? TryRead()
        => TryReadCore(requireFrameEnd: false);

    public global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.WorldDataPacket? TryReadFrame()
        => TryReadCore(requireFrameEnd: true);

    public PacketReadResult<global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.WorldDataPacket> ReadDetailed() => ReadResult(false);
    public PacketReadResult<global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.WorldDataPacket> ReadFrameDetailed() => ReadResult(true);

    private PacketReadResult<global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.WorldDataPacket> ReadResult(bool requireFrameEnd)
    {
        var packet = TryReadCore(requireFrameEnd);
        if (LastError is { } error) return PacketReadResult<global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.WorldDataPacket>.Failed(error);
        return PacketReadResult<global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.WorldDataPacket>.Succeeded(packet!, Consumed);
    }

    private global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.WorldDataPacket? Fail(PacketReadStatus status, PacketReadErrorCode code, int offset, string? member, string message)
    {
        LastError = new PacketReadError(status, code, _cursor + offset, member, message);
        return null;
    }

    private global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.WorldDataPacket? TryReadCore(bool requireFrameEnd)
    {
        Consumed = 0;
        LastError = null;
        var read = 0;
        if (requireFrameEnd && _frame.Length - _cursor > 65532) return Fail(PacketReadStatus.InvalidData, PacketReadErrorCode.LengthOutOfRange, read, null, "The packet body exceeds its protocol capacity.");
        var source = _frame.Span.Slice(_cursor, Math.Min(_frame.Length - _cursor, 65532));
        int codecAvailable0 = source.Length - read;
        if (codecAvailable0 < 160) return Fail(PacketReadStatus.Truncated, PacketReadErrorCode.Truncated, read, "Read", "The codec block and following fields are shorter than their minimum length.");
        int codecBound0 = global::System.Math.Min(codecAvailable0 - 0, 65532);
        var codec0 = new global::Terraria.NetWork.Prototype.PacketDesignCompiler.Wire.PacketWireReader(_frame, _cursor + read, codecBound0);
        int codecStart0 = codec0.Position;
        global::System.Int32 codecValue0_1;
        global::System.Byte codecValue0_2;
        global::System.Byte codecValue0_3;
        global::System.Int16 codecValue0_4;
        global::System.Int16 codecValue0_5;
        global::System.Int16 codecValue0_6;
        global::System.Int16 codecValue0_7;
        global::System.Int16 codecValue0_8;
        global::System.Int16 codecValue0_9;
        global::System.Int32 codecValue0_10;
        global::System.String codecValue0_11;
        global::System.Byte codecValue0_12;
        global::System.Byte[] codecValue0_13;
        global::System.UInt64 codecValue0_14;
        global::System.Byte codecValue0_15;
        global::System.Byte[] codecValue0_16;
        global::System.Byte[] codecValue0_17;
        global::System.Single codecValue0_18;
        global::System.Byte codecValue0_19;
        global::System.Int32[] codecValue0_20;
        global::System.Byte[] codecValue0_21;
        global::System.Int32[] codecValue0_22;
        global::System.Byte[] codecValue0_23;
        global::System.Byte[] codecValue0_24;
        global::System.Single codecValue0_25;
        global::System.Byte[] codecValue0_26;
        global::System.Byte codecValue0_27;
        global::System.Byte codecValue0_28;
        global::System.Int16[] codecValue0_29;
        global::System.SByte codecValue0_30;
        global::System.UInt64 codecValue0_31;
        global::System.Single codecValue0_32;
        global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.Packet7ExtraSpawnPoint[] codecValue0_33;
        try
        {
            var codecResult0 = global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.WorldDataPacket.Read(codec0);
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
            codecValue0_11 = codecResult0.Rest.Item4!;
            codecValue0_12 = codecResult0.Rest.Item5;
            codecValue0_13 = codecResult0.Rest.Item6!;
            codecValue0_14 = codecResult0.Rest.Item7;
            codecValue0_15 = codecResult0.Rest.Rest.Item1;
            codecValue0_16 = codecResult0.Rest.Rest.Item2!;
            codecValue0_17 = codecResult0.Rest.Rest.Item3!;
            codecValue0_18 = codecResult0.Rest.Rest.Item4;
            codecValue0_19 = codecResult0.Rest.Rest.Item5;
            codecValue0_20 = codecResult0.Rest.Rest.Item6!;
            codecValue0_21 = codecResult0.Rest.Rest.Item7!;
            codecValue0_22 = codecResult0.Rest.Rest.Rest.Item1!;
            codecValue0_23 = codecResult0.Rest.Rest.Rest.Item2!;
            codecValue0_24 = codecResult0.Rest.Rest.Rest.Item3!;
            codecValue0_25 = codecResult0.Rest.Rest.Rest.Item4;
            codecValue0_26 = codecResult0.Rest.Rest.Rest.Item5!;
            codecValue0_27 = codecResult0.Rest.Rest.Rest.Item6;
            codecValue0_28 = codecResult0.Rest.Rest.Rest.Item7;
            codecValue0_29 = codecResult0.Rest.Rest.Rest.Rest.Item1!;
            codecValue0_30 = codecResult0.Rest.Rest.Rest.Rest.Item2;
            codecValue0_31 = codecResult0.Rest.Rest.Rest.Rest.Item3;
            codecValue0_32 = codecResult0.Rest.Rest.Rest.Rest.Item4;
            codecValue0_33 = codecResult0.Rest.Rest.Rest.Rest.Item5!;
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
        if (codecRead0 < 160 || codecRead0 > 65532) return Fail(PacketReadStatus.InvalidData, PacketReadErrorCode.LengthOutOfRange, read, "Read", "The codec consumption is outside its declared length range.");
        read += codecRead0;
        if (requireFrameEnd && _cursor + read != _frame.Length) return Fail(PacketReadStatus.InvalidData, PacketReadErrorCode.TrailingBytes, read, null, "The packet body contains trailing bytes.");
        var packet = new global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.WorldDataPacket
        {
            Time = codecValue0_1,
            TimeFlags = codecValue0_2,
            MoonPhase = codecValue0_3,
            MaxTilesX = codecValue0_4,
            MaxTilesY = codecValue0_5,
            SpawnTileX = codecValue0_6,
            SpawnTileY = codecValue0_7,
            WorldSurface = codecValue0_8,
            RockLayer = codecValue0_9,
            WorldId = codecValue0_10,
            WorldName = codecValue0_11,
            GameMode = codecValue0_12,
            WorldGuid = codecValue0_13,
            WorldGeneratorVersion = codecValue0_14,
            MoonType = codecValue0_15,
            BackgroundTypes = codecValue0_16,
            SpecialBackgroundStyles = codecValue0_17,
            WindSpeedTarget = codecValue0_18,
            CloudCount = codecValue0_19,
            TreePositions = codecValue0_20,
            TreeStyles = codecValue0_21,
            CaveBackgroundPositions = codecValue0_22,
            CaveBackgroundStyles = codecValue0_23,
            TreeTopStyles = codecValue0_24,
            MaximumRain = codecValue0_25,
            WorldFlagGroups = codecValue0_26,
            SundialCooldown = codecValue0_27,
            MoondialCooldown = codecValue0_28,
            SavedOreTiers = codecValue0_29,
            InvasionType = codecValue0_30,
            LobbyId = codecValue0_31,
            SandstormSeverity = codecValue0_32,
            ExtraSpawnPoints = codecValue0_33,
        };
        _cursor += read;
        Consumed = read;
        return packet;
    }
}

public sealed class TerrariaV4_WorldDataPacketPacketCodecWriter
{

    public TerrariaV4_WorldDataPacketPacketCodecWriter()
    {
        _ = global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.ProtocolInputs.Instance;
    }

    public MemoryStream? TryWrite(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.WorldDataPacket packet)
    {
        if (packet is null) return null;
        var stream = new MemoryStream();
        Span<byte> buffer = stackalloc byte[8];
        long codecAvailable0 = 65532L - stream.Length - 0;
        if (codecAvailable0 < 160) return null;
        int codecBound0 = (int)Math.Min(codecAvailable0, 65532);
        var codec0 = new global::Terraria.NetWork.Prototype.PacketDesignCompiler.Wire.PacketWireWriter(stream, codecBound0);
        int codecStart0 = codec0.BytesWritten;
        try
        {
            global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.WorldDataPacket.Write(codec0, packet.Time, packet.TimeFlags, packet.MoonPhase, packet.MaxTilesX, packet.MaxTilesY, packet.SpawnTileX, packet.SpawnTileY, packet.WorldSurface, packet.RockLayer, packet.WorldId, packet.WorldName, packet.GameMode, packet.WorldGuid, packet.WorldGeneratorVersion, packet.MoonType, packet.BackgroundTypes, packet.SpecialBackgroundStyles, packet.WindSpeedTarget, packet.CloudCount, packet.TreePositions, packet.TreeStyles, packet.CaveBackgroundPositions, packet.CaveBackgroundStyles, packet.TreeTopStyles, packet.MaximumRain, packet.WorldFlagGroups, packet.SundialCooldown, packet.MoondialCooldown, packet.SavedOreTiers, packet.InvasionType, packet.LobbyId, packet.SandstormSeverity, packet.ExtraSpawnPoints);
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
        if (codecWritten0 < 160 || codecWritten0 > 65532) return null;
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
