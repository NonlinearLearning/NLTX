using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class AddPlayerBuffPvPPacket
{
    public static void Write(PacketWireWriter writer, byte player, ushort buffType, int buffTime)
    {
        writer.WriteByte(player);
        writer.WriteUInt16(buffType);
        writer.WriteInt32(buffTime);
    }

    public static (byte Player, ushort BuffType, int BuffTime) Read(PacketWireReader reader) => (Player: reader.ReadByte(), BuffType: reader.ReadUInt16(), BuffTime: reader.ReadInt32());
}
