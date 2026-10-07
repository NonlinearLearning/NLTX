#nullable enable
using System;
using System.IO;
using System.Buffers.Binary;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Generated;

public sealed class TerrariaV4_TileSectionPacketPacketCodecReader
{
    private readonly ReadOnlyMemory<byte> _frame;
    private readonly global::System.Boolean[] _input0;
    private readonly global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.PacketTileEntityCodecs _input2;
    private int _cursor;

    public TerrariaV4_TileSectionPacketPacketCodecReader(ReadOnlyMemory<byte> frame)
    {
        _frame = frame;
        var external = global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.ProtocolInputs.Instance;
        _input0 = external.FrameImportant;
        _input2 = external.TileEntityCodecs;
    }

    // Reads one frame out of a larger receive buffer. The declared length is the
    // frame boundary: neither a scalar field nor a codec block can reach past it,
    // so a first frame can never consume bytes belonging to the second.
    public TerrariaV4_TileSectionPacketPacketCodecReader(ReadOnlyMemory<byte> buffer, int frameLength)
    {
        if (frameLength < 0 || frameLength > buffer.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(frameLength));
        }

        _frame = buffer.Slice(0, frameLength);
        var external = global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.ProtocolInputs.Instance;
        _input0 = external.FrameImportant;
        _input2 = external.TileEntityCodecs;
    }

    public int Consumed { get; private set; }
    public PacketReadError? LastError { get; private set; }

    public global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.TileSectionPacket? TryRead()
        => TryReadCore(requireFrameEnd: false);

    public global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.TileSectionPacket? TryReadFrame()
        => TryReadCore(requireFrameEnd: true);

    public PacketReadResult<global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.TileSectionPacket> ReadDetailed() => ReadResult(false);
    public PacketReadResult<global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.TileSectionPacket> ReadFrameDetailed() => ReadResult(true);

    private PacketReadResult<global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.TileSectionPacket> ReadResult(bool requireFrameEnd)
    {
        var packet = TryReadCore(requireFrameEnd);
        if (LastError is { } error) return PacketReadResult<global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.TileSectionPacket>.Failed(error);
        return PacketReadResult<global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.TileSectionPacket>.Succeeded(packet!, Consumed);
    }

    private global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.TileSectionPacket? Fail(PacketReadStatus status, PacketReadErrorCode code, int offset, string? member, string message)
    {
        LastError = new PacketReadError(status, code, _cursor + offset, member, message);
        return null;
    }

    private global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.TileSectionPacket? TryReadCore(bool requireFrameEnd)
    {
        Consumed = 0;
        LastError = null;
        var read = 0;
        if (requireFrameEnd && _frame.Length - _cursor > 65532) return Fail(PacketReadStatus.InvalidData, PacketReadErrorCode.LengthOutOfRange, read, null, "The packet body exceeds its protocol capacity.");
        var source = _frame.Span.Slice(_cursor, Math.Min(_frame.Length - _cursor, 65532));
        int codecAvailable0 = source.Length - read;
        if (codecAvailable0 < 1) return Fail(PacketReadStatus.Truncated, PacketReadErrorCode.Truncated, read, "ReadCompressedBody", "The codec block and following fields are shorter than their minimum length.");
        int codecBound0 = global::System.Math.Min(codecAvailable0 - 0, 65532);
        var codec0 = new global::Terraria.NetWork.Prototype.PacketDesignCompiler.Wire.PacketWireReader(_frame, _cursor + read, codecBound0);
        int codecStart0 = codec0.Position;
        global::System.Int32 codecValue0_1;
        global::System.Int32 codecValue0_2;
        global::System.Int16 codecValue0_3;
        global::System.Int16 codecValue0_4;
        global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.Packet10Tile[] codecValue0_5;
        global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.Packet10ChestRecord[] codecValue0_6;
        global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.Packet10SignRecord[] codecValue0_7;
        global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.PacketTileEntityRecord[] codecValue0_8;
        try
        {
            var codecResult0 = global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.TileSectionPacket.ReadCompressedBody(codec0, _input0, _input2);
            codecValue0_1 = codecResult0.Item1;
            codecValue0_2 = codecResult0.Item2;
            codecValue0_3 = codecResult0.Item3;
            codecValue0_4 = codecResult0.Item4;
            codecValue0_5 = codecResult0.Item5!;
            codecValue0_6 = codecResult0.Item6!;
            codecValue0_7 = codecResult0.Item7!;
            codecValue0_8 = codecResult0.Rest.Item1!;
        }
        catch (global::Terraria.NetWork.Prototype.PacketDesignCompiler.Wire.PacketWireTruncationException exception)
        {
            if (codecAvailable0 > codecBound0 && ReferenceEquals(exception.Reader, codec0)) return Fail(PacketReadStatus.InvalidData, PacketReadErrorCode.LengthOutOfRange, read + codec0.Position, "ReadCompressedBody", exception.Message);
            return Fail(PacketReadStatus.Truncated, PacketReadErrorCode.Truncated, read + codec0.Position, "ReadCompressedBody", exception.Message);
        }
        catch (global::Terraria.NetWork.Prototype.PacketDesignCompiler.Wire.PacketWireFormatException exception)
        {
            return Fail(PacketReadStatus.InvalidData, exception.Code, read + codec0.Position, "ReadCompressedBody", exception.Message);
        }
        catch (InvalidDataException exception)
        {
            return Fail(PacketReadStatus.InvalidData, PacketReadErrorCode.InvalidCodecData, read + codec0.Position, "ReadCompressedBody", exception.Message);
        }
        int codecRead0 = codec0.Position - codecStart0;
        if (codecRead0 < 1 || codecRead0 > 65532) return Fail(PacketReadStatus.InvalidData, PacketReadErrorCode.LengthOutOfRange, read, "ReadCompressedBody", "The codec consumption is outside its declared length range.");
        read += codecRead0;
        if (requireFrameEnd && _cursor + read != _frame.Length) return Fail(PacketReadStatus.InvalidData, PacketReadErrorCode.TrailingBytes, read, null, "The packet body contains trailing bytes.");
        var packet = new global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.TileSectionPacket
        {
            StartX = codecValue0_1,
            StartY = codecValue0_2,
            Width = codecValue0_3,
            Height = codecValue0_4,
            Tiles = codecValue0_5,
            Chests = codecValue0_6,
            Signs = codecValue0_7,
            TileEntities = codecValue0_8,
        };
        _cursor += read;
        Consumed = read;
        return packet;
    }
}

public sealed class TerrariaV4_TileSectionPacketPacketCodecWriter
{
    private readonly global::System.Boolean[] _input0;
    private readonly global::System.Boolean[] _input1;
    private readonly global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.PacketTileEntityCodecs _input2;

    public TerrariaV4_TileSectionPacketPacketCodecWriter()
    {
        var external = global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.ProtocolInputs.Instance;
        _input0 = external.FrameImportant;
        _input1 = external.AllowsSaveCompressionBatching;
        _input2 = external.TileEntityCodecs;
    }

    public MemoryStream? TryWrite(global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.TileSectionPacket packet)
    {
        if (packet is null) return null;
        var stream = new MemoryStream();
        Span<byte> buffer = stackalloc byte[8];
        long codecAvailable0 = 65532L - stream.Length - 0;
        if (codecAvailable0 < 1) return null;
        int codecBound0 = (int)Math.Min(codecAvailable0, 65532);
        var codec0 = new global::Terraria.NetWork.Prototype.PacketDesignCompiler.Wire.PacketWireWriter(stream, codecBound0);
        int codecStart0 = codec0.BytesWritten;
        try
        {
            global::Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.TileSectionPacket.WriteCompressedBody(codec0, packet.StartX, packet.StartY, packet.Width, packet.Height, packet.Tiles, packet.Chests, packet.Signs, packet.TileEntities, _input0, _input1, _input2);
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
        if (codecWritten0 < 1 || codecWritten0 > 65532) return null;
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
