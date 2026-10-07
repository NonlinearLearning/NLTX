using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class TileEntitySharingPacket
{
    public static void Write(
        PacketWireWriter writer,
        int entityId,
        bool exists,
        PacketTileEntityRecord? entity,
        PacketTileEntityCodecs tileEntityCodecs)
    {
        ArgumentNullException.ThrowIfNull(tileEntityCodecs);
        PacketTileEntityRecord? entityValue = entity;
        IPacketTileEntityWireCodec? entityCodec = null;
        if (exists)
        {
            if (entityValue is null)
                throw new PacketWireFormatException("Packet 86 requires an entity when Exists is true.");
            if (entityValue.Id is not null)
                throw new PacketWireFormatException("Packet 86 writes the tile-entity ID in the outer payload only.");
            entityCodec = tileEntityCodecs.Get(entityValue.Type);
        }
        else if (entityValue is not null)
        {
            throw new PacketWireFormatException("Packet 86 cannot carry entity data when Exists is false.");
        }

        writer.WriteInt32(entityId);
        writer.WriteBoolean(exists);
        if (entityValue is not null)
        {
            writer.WriteByte(entityValue.Type);
            writer.WriteInt16(entityValue.X);
            writer.WriteInt16(entityValue.Y);
            entityCodec!.WriteExtraData(writer, entityValue, networkSend: true);
        }
    }

    public static (int EntityId, bool Exists, PacketTileEntityRecord? Entity) Read(PacketWireReader reader, PacketTileEntityCodecs tileEntityCodecs)
    {
        ArgumentNullException.ThrowIfNull(tileEntityCodecs);
        int entityId = reader.ReadInt32();
        bool exists = reader.ReadBoolean();
        if (!exists)
            return (EntityId: entityId, Exists: false, Entity: null);
        byte type = reader.ReadByte();
        short x = reader.ReadInt16();
        short y = reader.ReadInt16();
        object? extraData = tileEntityCodecs.Get(type).ReadExtraData(reader, networkSend: true);
        var entity = new PacketTileEntityRecord(type, x, y, null, extraData);
        return (EntityId: entityId, Exists: true, Entity: entity);
    }
}
