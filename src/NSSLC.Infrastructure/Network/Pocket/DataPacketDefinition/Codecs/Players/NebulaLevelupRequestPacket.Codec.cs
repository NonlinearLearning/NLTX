using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class NebulaLevelupRequestPacket
{
    public static void Write(PacketWireWriter writer, byte player, ushort itemType, PacketVector2 position)
    {
        writer.WriteByte(player);
        writer.WriteUInt16(itemType);
        writer.WriteSingle(position.X);
        writer.WriteSingle(position.Y);
    }

    public static (byte Player, ushort ItemType, PacketVector2 Position) Read(PacketWireReader reader) => (Player: reader.ReadByte(), ItemType: reader.ReadUInt16(), Position: new PacketVector2(reader.ReadSingle(), reader.ReadSingle()));
}
