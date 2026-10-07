using System.IO.Compression;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class TileSectionPacket
{
    private const int MaximumExpandedBodyBytes = 64 * 1024 * 1024;
    private const int MaximumTileCount = 1_000_000;
    private const int MaximumChestCount = 8_000;
    private const int MaximumSignCount = 32_000;
    private const int MaximumTileEntityCount = 1_000;
    private const byte HasFlags2 = 0x01;
    private const byte ActiveTile = 0x02;
    private const byte HasWall = 0x04;
    private const byte LiquidMask = 0x18;
    private const byte HasHighTileType = 0x20;
    private const byte ShortRun = 0x40;
    private const byte LongRun = 0x80;
    public static void WriteCompressedBody(
        PacketWireWriter writer,
        int startX,
        int startY,
        short width,
        short height,
        Packet10Tile[] tiles,
        Packet10ChestRecord[] chests,
        Packet10SignRecord[] signs,
        PacketTileEntityRecord[] tileEntities,
        bool[] frameImportant,
        bool[] allowsSaveCompressionBatching,
        PacketTileEntityCodecs tileEntityCodecs)
    {
        ArgumentNullException.ThrowIfNull(frameImportant);
        ArgumentNullException.ThrowIfNull(tileEntityCodecs);
        ArgumentNullException.ThrowIfNull(allowsSaveCompressionBatching);
        ValidateBody(width, height, tiles, chests, signs, tileEntities);
        using var uncompressedStream = new MemoryStream();
        var bodyWriter = new PacketWireWriter(uncompressedStream, MaximumExpandedBodyBytes);
        bodyWriter.WriteInt32(startX);
        bodyWriter.WriteInt32(startY);
        bodyWriter.WriteInt16(width);
        bodyWriter.WriteInt16(height);
        WriteTiles(bodyWriter, tiles, frameImportant, allowsSaveCompressionBatching);
        WriteChestTable(bodyWriter, chests);
        WriteSignTable(bodyWriter, signs);
        bodyWriter.WriteInt16(checked((short)tileEntities.Length));
        foreach (PacketTileEntityRecord entity in tileEntities)
            WriteTileEntity(bodyWriter, entity, tileEntityCodecs);
        byte[] uncompressedBytes = uncompressedStream.ToArray();
        using var compressedStream = new MemoryStream();
        using (var deflate = new DeflateStream(compressedStream, CompressionLevel.Optimal, leaveOpen: true))
        {
            deflate.Write(uncompressedBytes);
        }

        writer.WriteBytes(compressedStream.ToArray());
    }

    public static (
        int StartX,
        int StartY,
        short Width,
        short Height,
        Packet10Tile[] Tiles,
        Packet10ChestRecord[] Chests,
        Packet10SignRecord[] Signs,
        PacketTileEntityRecord[] TileEntities) ReadCompressedBody(PacketWireReader reader, bool[] frameImportant, PacketTileEntityCodecs tileEntityCodecs)
    {
        ArgumentNullException.ThrowIfNull(frameImportant);
        ArgumentNullException.ThrowIfNull(tileEntityCodecs);
        (byte[] uncompressedBytes, int compressedLength) = Decompress(reader.RemainingSpan);
        reader.Skip(compressedLength);
        reader.RequireFrameEnd();
        var bodyReader = new PacketWireReader(uncompressedBytes);
        int startX = bodyReader.ReadInt32();
        int startY = bodyReader.ReadInt32();
        short width = bodyReader.ReadInt16();
        short height = bodyReader.ReadInt16();
        int tileCount = GetTileCount(width, height);
        var tiles = ReadTiles(bodyReader, tileCount, frameImportant);
        Packet10ChestRecord[] chests = ReadChestTable(bodyReader);
        Packet10SignRecord[] signs = ReadSignTable(bodyReader);
        short tileEntityCount = ReadCount(bodyReader, MaximumTileEntityCount, "tile entity");
        var entities = new PacketTileEntityRecord[tileEntityCount];
        for (int index = 0; index < entities.Length; index++)
            entities[index] = ReadTileEntity(bodyReader, tileEntityCodecs);
        bodyReader.RequireFrameEnd();
        return (StartX: startX, StartY: startY, Width: width, Height: height, Tiles: tiles, Chests: chests, Signs: signs, TileEntities: entities);
    }

    private static bool ReadTileRule(bool[] table, ushort tileType, string ruleName)
    {
        if (tileType >= table.Length)
            throw new PacketWireFormatException($"Tile type {tileType} is outside the supplied {ruleName} table.");
        return table[tileType];
    }

    private static void ValidateBody(
        short width,
        short height,
        Packet10Tile[] tiles,
        Packet10ChestRecord[] chests,
        Packet10SignRecord[] signs,
        PacketTileEntityRecord[] tileEntities)
    {
        _ = GetTileCount(width, height);
        if (tiles is null || tiles.Length != GetTileCount(width, height))
        {
            throw new PacketWireFormatException("Packet 10 tile count must equal Width * Height in y-major wire order.");
        }

        if (chests is null || chests.Length > MaximumChestCount)
        {
            throw new PacketWireFormatException($"Packet 10 chest count must be between 0 and {MaximumChestCount}.");
        }

        if (signs is null || signs.Length > MaximumSignCount)
        {
            throw new PacketWireFormatException($"Packet 10 sign count must be between 0 and {MaximumSignCount}.");
        }

        if (tileEntities is null || tileEntities.Length > MaximumTileEntityCount)
        {
            throw new PacketWireFormatException($"Packet 10 tile entity count must be between 0 and {MaximumTileEntityCount}.");
        }

        foreach (Packet10ChestRecord chest in chests)
        {
            if (chest.Name is null)
                throw new PacketWireFormatException("Packet 10 chest names cannot be null.");
        }

        foreach (Packet10SignRecord sign in signs)
        {
            if (sign.Text is null)
                throw new PacketWireFormatException("Packet 10 sign text cannot be null.");
        }
    }

    private static void WriteTileEntity(PacketWireWriter writer, PacketTileEntityRecord entity, PacketTileEntityCodecs codecs)
    {
        writer.WriteByte(entity.Type);
        IPacketTileEntityWireCodec codec = codecs.Get(entity.Type);
        if (entity.Id is not int id)
            throw new PacketWireFormatException("Packet 10 tile entities require their Int32 entity ID.");
        writer.WriteInt32(id);
        writer.WriteInt16(entity.X);
        writer.WriteInt16(entity.Y);
        codec.WriteExtraData(writer, entity, networkSend: false);
    }

    private static PacketTileEntityRecord ReadTileEntity(PacketWireReader reader, PacketTileEntityCodecs codecs)
    {
        byte type = reader.ReadByte();
        IPacketTileEntityWireCodec codec = codecs.Get(type);
        int? id = reader.ReadInt32();
        short x = reader.ReadInt16();
        short y = reader.ReadInt16();
        object? extraData = codec.ReadExtraData(reader, networkSend: false);
        return new PacketTileEntityRecord(type, x, y, id, extraData);
    }

    private static int GetTileCount(short width, short height)
    {
        if (width <= 0 || height <= 0)
        {
            throw new PacketWireFormatException("Packet 10 width and height must be positive.");
        }

        int count = checked(width * height);
        if (count > MaximumTileCount)
        {
            throw new PacketWireFormatException($"Packet 10 tile area exceeds the implementation limit of {MaximumTileCount}.");
        }

        return count;
    }

    private static void WriteTiles(
        PacketWireWriter writer,
        Packet10Tile[] tiles,
        bool[] frameImportant,
        bool[] allowsSaveCompressionBatching)
    {
        for (int index = 0; index < tiles.Length;)
        {
            Packet10Tile tile = tiles[index];
            int repeatedTiles = 0;
            while (repeatedTiles < short.MaxValue && index + repeatedTiles + 1 < tiles.Length)
            {
                Packet10Tile candidate = tiles[index + repeatedTiles + 1];
                if (!ReadTileRule(allowsSaveCompressionBatching, candidate.Type, "save-compression batching") || !IsSameForSaveCompression(tile, candidate, frameImportant))
                    break;
                repeatedTiles++;
            }

            WriteTile(writer, tile, repeatedTiles, frameImportant);
            index += repeatedTiles + 1;
        }
    }

    private static bool IsSameForSaveCompression(Packet10Tile left, Packet10Tile right, bool[] frameImportant)
    {
        if (left.Active != right.Active || left.TileColor != right.TileColor || left.Wire != right.Wire || left.Wire2 != right.Wire2 || left.Wire3 != right.Wire3 || left.HalfBrick != right.HalfBrick || left.Actuator != right.Actuator || left.Inactive != right.Inactive || left.Slope != right.Slope || left.FullbrightWall != right.FullbrightWall)
        {
            return false;
        }

        if (left.Active && (left.Type != right.Type || (ReadTileRule(frameImportant, left.Type, "frame-important") && (left.FrameX != right.FrameX || left.FrameY != right.FrameY))))
        {
            return false;
        }

        if (left.Wall != right.Wall || left.LiquidAmount != right.LiquidAmount)
            return false;
        if (left.WallColor != right.WallColor || left.Wire4 != right.Wire4)
            return false;
        if (left.LiquidAmount != 0 && left.LiquidType != right.LiquidType)
            return false;
        return left.InvisibleBlock == right.InvisibleBlock && left.InvisibleWall == right.InvisibleWall && left.FullbrightBlock == right.FullbrightBlock;
    }

    private static void WriteTile(PacketWireWriter writer, Packet10Tile tile, int repeatedTiles, bool[] frameImportant)
    {
        if (tile.Slope > 7)
        {
            throw new PacketWireFormatException("Packet 10 slope must fit three bits.");
        }

        if (repeatedTiles < 0 || repeatedTiles > short.MaxValue)
        {
            throw new PacketWireFormatException("Packet 10 RLE repeat count is outside Int16 range.");
        }

        bool hasTileColor = tile.Active && tile.TileColor != 0;
        bool hasWallColor = tile.Wall != 0 && tile.WallColor != 0;
        bool hasLiquid = tile.LiquidAmount != 0;
        bool hasHighTileType = tile.Active && tile.Type > byte.MaxValue;
        bool hasHighWall = tile.Wall > byte.MaxValue;
        byte flags1 = 0;
        byte flags2 = 0;
        byte flags3 = 0;
        byte flags4 = 0;
        if (tile.Active)
            flags1 |= ActiveTile;
        if (tile.Wall != 0)
            flags1 |= HasWall;
        if (hasHighTileType)
            flags1 |= HasHighTileType;
        if (hasLiquid)
        {
            flags1 |= tile.LiquidType switch
            {
                Packet10LiquidType.Water => 0x08,
                Packet10LiquidType.Lava => 0x10,
                Packet10LiquidType.Honey => 0x18,
                Packet10LiquidType.Shimmer => 0x08,
                _ => throw new PacketWireFormatException("Unknown Packet 10 liquid type.")};
        }

        if (tile.Wire)
            flags2 |= 0x02;
        if (tile.Wire2)
            flags2 |= 0x04;
        if (tile.Wire3)
            flags2 |= 0x08;
        int slopeCode = tile.HalfBrick ? 1 : tile.Slope == 0 ? 0 : tile.Slope + 1;
        flags2 |= (byte)(slopeCode << 4);
        if (tile.Actuator)
            flags3 |= 0x02;
        if (tile.Inactive)
            flags3 |= 0x04;
        if (hasTileColor)
            flags3 |= 0x08;
        if (hasWallColor)
            flags3 |= 0x10;
        if (tile.Wire4)
            flags3 |= 0x20;
        if (hasHighWall)
            flags3 |= 0x40;
        if (hasLiquid && tile.LiquidType == Packet10LiquidType.Shimmer)
            flags3 |= 0x80;
        if (tile.InvisibleBlock)
            flags4 |= 0x02;
        if (tile.InvisibleWall)
            flags4 |= 0x04;
        if (tile.FullbrightBlock)
            flags4 |= 0x08;
        if (tile.FullbrightWall)
            flags4 |= 0x10;
        if (flags4 != 0)
            flags3 |= 0x01;
        if (flags3 != 0)
            flags2 |= 0x01;
        if (flags2 != 0)
            flags1 |= HasFlags2;
        if (repeatedTiles is> 0 and <= byte.MaxValue)
            flags1 |= ShortRun;
        if (repeatedTiles > byte.MaxValue)
            flags1 |= LongRun;
        writer.WriteByte(flags1);
        if (Has(flags1, HasFlags2))
            writer.WriteByte(flags2);
        if (Has(flags2, 0x01))
            writer.WriteByte(flags3);
        if (Has(flags3, 0x01))
            writer.WriteByte(flags4);
        if (tile.Active)
        {
            writer.WriteByte((byte)tile.Type);
            if (hasHighTileType)
                writer.WriteByte((byte)(tile.Type >> 8));
            if (ReadTileRule(frameImportant, tile.Type, "frame-important"))
            {
                writer.WriteInt16(tile.FrameX);
                writer.WriteInt16(tile.FrameY);
            }

            if (hasTileColor)
                writer.WriteByte(tile.TileColor);
        }

        if (tile.Wall != 0)
        {
            writer.WriteByte((byte)tile.Wall);
            if (hasWallColor)
                writer.WriteByte(tile.WallColor);
        }

        if (hasLiquid)
            writer.WriteByte(tile.LiquidAmount);
        if (hasHighWall)
            writer.WriteByte((byte)(tile.Wall >> 8));
        if (repeatedTiles is> 0 and <= byte.MaxValue)
        {
            writer.WriteByte((byte)repeatedTiles);
        }
        else if (repeatedTiles > byte.MaxValue)
        {
            writer.WriteInt16((short)repeatedTiles);
        }
    }

    private static Packet10Tile[] ReadTiles(PacketWireReader reader, int tileCount, bool[] frameImportant)
    {
        var tiles = new Packet10Tile[tileCount];
        int index = 0;
        while (index < tileCount)
        {
            (Packet10Tile tile, int repeatedTiles) = ReadTile(reader, frameImportant);
            if (repeatedTiles > tileCount - index - 1)
            {
                throw new PacketWireFormatException("Packet 10 RLE run expands past the declared tile area.");
            }

            tiles[index++] = tile;
            for (int repeat = 0; repeat < repeatedTiles; repeat++)
            {
                tiles[index++] = tile;
            }
        }

        return tiles;
    }

    private static (Packet10Tile Tile, int RepeatedTiles) ReadTile(PacketWireReader reader, bool[] frameImportant)
    {
        byte flags1 = reader.ReadByte();
        byte flags2 = Has(flags1, HasFlags2) ? reader.ReadByte() : (byte)0;
        byte flags3 = Has(flags2, 0x01) ? reader.ReadByte() : (byte)0;
        byte flags4 = Has(flags3, 0x01) ? reader.ReadByte() : (byte)0;
        if ((flags4 & 0xE1) != 0)
        {
            throw new PacketWireFormatException("Packet 10 contains unsupported Flags4 bits.");
        }

        bool active = Has(flags1, ActiveTile);
        bool hasWall = Has(flags1, HasWall);
        bool hasLiquid = (flags1 & LiquidMask) != 0;
        bool hasHighTileType = Has(flags1, HasHighTileType);
        if (hasHighTileType && !active)
        {
            throw new PacketWireFormatException("Packet 10 has a high tile-type byte without an active tile.");
        }

        if (Has(flags3, 0x08) && !active)
        {
            throw new PacketWireFormatException("Packet 10 has tile color without an active tile.");
        }

        if ((Has(flags3, 0x10) || Has(flags3, 0x40)) && !hasWall)
        {
            throw new PacketWireFormatException("Packet 10 has wall metadata without a wall.");
        }

        int slopeCode = (flags2 >> 4) & 0x0F;
        if (slopeCode > 8)
        {
            throw new PacketWireFormatException("Packet 10 slope code is outside the supported range.");
        }

        ushort type = 0;
        short frameX = 0;
        short frameY = 0;
        byte tileColor = 0;
        if (active)
        {
            type = reader.ReadByte();
            if (hasHighTileType)
            {
                byte high = reader.ReadByte();
                if (high == 0)
                    throw new PacketWireFormatException("Packet 10 has a redundant high tile-type byte.");
                type |= (ushort)(high << 8);
            }

            if (ReadTileRule(frameImportant, type, "frame-important"))
            {
                frameX = reader.ReadInt16();
                frameY = reader.ReadInt16();
            }

            if (Has(flags3, 0x08))
            {
                tileColor = reader.ReadByte();
                if (tileColor == 0)
                    throw new PacketWireFormatException("Packet 10 has a redundant zero tile color.");
            }
        }

        ushort wall = 0;
        byte wallColor = 0;
        if (hasWall)
        {
            wall = reader.ReadByte();
            if (Has(flags3, 0x10))
            {
                wallColor = reader.ReadByte();
                if (wallColor == 0)
                    throw new PacketWireFormatException("Packet 10 has a redundant zero wall color.");
            }
        }

        byte liquidAmount = 0;
        Packet10LiquidType liquidType = Packet10LiquidType.Water;
        if (hasLiquid)
        {
            liquidAmount = reader.ReadByte();
            if (liquidAmount == 0)
                throw new PacketWireFormatException("Packet 10 has a liquid flag with a zero amount.");
            bool shimmer = Has(flags3, 0x80);
            liquidType = (flags1 & LiquidMask, shimmer) switch
            {
                (0x08, false) => Packet10LiquidType.Water,
                (0x08, true) => Packet10LiquidType.Shimmer,
                (0x10, false) => Packet10LiquidType.Lava,
                (0x18, false) => Packet10LiquidType.Honey,
                _ => throw new PacketWireFormatException("Packet 10 liquid flags are inconsistent.")};
        }
        else if (Has(flags3, 0x80))
        {
            throw new PacketWireFormatException("Packet 10 has a shimmer flag without liquid.");
        }

        if (Has(flags3, 0x40))
        {
            byte highWall = reader.ReadByte();
            if (highWall == 0)
                throw new PacketWireFormatException("Packet 10 has a redundant high wall byte.");
            wall |= (ushort)(highWall << 8);
        }

        int repeatedTiles = ReadRunLength(reader, flags1);
        var tile = new Packet10Tile
        {
            Active = active,
            Type = type,
            FrameX = frameX,
            FrameY = frameY,
            TileColor = tileColor,
            Wall = wall,
            WallColor = wallColor,
            LiquidAmount = liquidAmount,
            LiquidType = liquidType,
            Wire = Has(flags2, 0x02),
            Wire2 = Has(flags2, 0x04),
            Wire3 = Has(flags2, 0x08),
            Wire4 = Has(flags3, 0x20),
            HalfBrick = slopeCode == 1,
            Slope = slopeCode > 1 ? (byte)(slopeCode - 1) : (byte)0,
            Actuator = Has(flags3, 0x02),
            Inactive = Has(flags3, 0x04),
            InvisibleBlock = Has(flags4, 0x02),
            InvisibleWall = Has(flags4, 0x04),
            FullbrightBlock = Has(flags4, 0x08),
            FullbrightWall = Has(flags4, 0x10)
        };
        return (tile, repeatedTiles);
    }

    private static int ReadRunLength(PacketWireReader reader, byte flags1)
    {
        byte mode = (byte)(flags1 & (ShortRun | LongRun));
        int repeatedTiles = mode switch
        {
            0 => 0,
            ShortRun => reader.ReadByte(),
            LongRun => reader.ReadInt16(),
            _ => throw new PacketWireFormatException("Packet 10 sets both RLE width flags.")};
        if (mode != 0 && repeatedTiles <= 0)
        {
            throw new PacketWireFormatException("Packet 10 RLE repeat count must be positive.");
        }

        if (mode == LongRun && repeatedTiles <= byte.MaxValue)
        {
            throw new PacketWireFormatException("Packet 10 uses the long RLE form for a short run.");
        }

        return repeatedTiles;
    }

    private static void WriteChestTable(PacketWireWriter writer, Packet10ChestRecord[] chests)
    {
        writer.WriteInt16((short)chests.Length);
        foreach (Packet10ChestRecord chest in chests)
        {
            writer.WriteInt16(chest.Id);
            writer.WriteInt16(chest.X);
            writer.WriteInt16(chest.Y);
            writer.WriteString(chest.Name);
        }
    }

    private static Packet10ChestRecord[] ReadChestTable(PacketWireReader reader)
    {
        short count = ReadCount(reader, MaximumChestCount, "chest");
        var chests = new Packet10ChestRecord[count];
        for (int index = 0; index < chests.Length; index++)
        {
            chests[index] = new Packet10ChestRecord(reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16(), reader.ReadString());
        }

        return chests;
    }

    private static void WriteSignTable(PacketWireWriter writer, Packet10SignRecord[] signs)
    {
        writer.WriteInt16((short)signs.Length);
        foreach (Packet10SignRecord sign in signs)
        {
            writer.WriteInt16(sign.Id);
            writer.WriteInt16(sign.X);
            writer.WriteInt16(sign.Y);
            writer.WriteString(sign.Text);
        }
    }

    private static Packet10SignRecord[] ReadSignTable(PacketWireReader reader)
    {
        short count = ReadCount(reader, MaximumSignCount, "sign");
        var signs = new Packet10SignRecord[count];
        for (int index = 0; index < signs.Length; index++)
        {
            signs[index] = new Packet10SignRecord(reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16(), reader.ReadString());
        }

        return signs;
    }

    private static short ReadCount(PacketWireReader reader, int maximum, string name)
    {
        short count = reader.ReadInt16();
        if (count < 0 || count > maximum)
        {
            throw new PacketWireFormatException($"Packet 10 {name} count {count} is outside 0..{maximum}.");
        }

        return count;
    }

    private static (byte[] Body, int CompressedLength) Decompress(ReadOnlySpan<byte> compressedBytes)
    {
        using var inputMemory = new MemoryStream(compressedBytes.ToArray(), writable: false);
        using var input = new SingleByteReadStream(inputMemory);
        using var output = new MemoryStream();
        byte[] buffer = new byte[8192];
        try
        {
            using var deflate = new DeflateStream(input, CompressionMode.Decompress, leaveOpen: true);
            int read;
            while ((read = deflate.Read(buffer, 0, buffer.Length)) != 0)
            {
                if (output.Length + read > MaximumExpandedBodyBytes)
                {
                    throw new PacketWireFormatException($"Packet 10 expands beyond the implementation limit of {MaximumExpandedBodyBytes} bytes.");
                }

                output.Write(buffer, 0, read);
            }
        }
        catch (InvalidDataException exception)
        {
            throw new PacketWireFormatException("Packet 10 Deflate body is invalid: " + exception.Message);
        }

        int consumed = checked((int)input.BytesRead);
        if (consumed != compressedBytes.Length)
        {
            throw new PacketWireFormatException("Packet 10 compressed body has trailing bytes after the Deflate stream.");
        }

        return (output.ToArray(), consumed);
    }

    private static bool Has(byte flags, byte mask) => (flags & mask) != 0;
    private static bool Has(byte flags, int mask) => (flags & mask) != 0;
    private sealed class SingleByteReadStream : Stream
    {
        private readonly Stream _inner;
        public SingleByteReadStream(Stream inner)
        {
            _inner = inner;
        }

        public long BytesRead => _inner.Position;
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => _inner.Length;
        public override long Position { get => _inner.Position; set => throw new NotSupportedException(); }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, Math.Min(count, 1));
        public override int Read(Span<byte> buffer) => _inner.Read(buffer[..Math.Min(buffer.Length, 1)]);
        public override int ReadByte() => _inner.ReadByte();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _inner.Dispose();
            base.Dispose(disposing);
        }
    }
}
