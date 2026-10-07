using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class ItemOwnerPacket
{
    public static void Write(
        PacketWireWriter writer,
        short itemIndex,
        byte reservedForPlayer,
        float positionX,
        float positionY)
    {
        writer.WriteInt16(itemIndex);
        writer.WriteByte(reservedForPlayer);
        writer.WriteSingle(positionX);
        writer.WriteSingle(positionY);
    }

    public static (short ItemIndex, byte ReservedForPlayer, float PositionX, float PositionY) Read(PacketWireReader reader)
    {
        short index = reader.ReadInt16();
        byte owner = reader.ReadByte();
        float x = reader.ReadSingle();
        float y = reader.ReadSingle();
        return (ItemIndex: index, ReservedForPlayer: owner, PositionX: x, PositionY: y);
    }
}
