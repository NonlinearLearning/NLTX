using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class SyncLoadoutPacket
{
    public static void Write(PacketWireWriter writer, byte player, byte loadoutIndex, ushort accessoryVisibilityMask)
    {
        writer.WriteByte(player);
        writer.WriteByte(loadoutIndex);
        writer.WriteUInt16(accessoryVisibilityMask);
    }

    public static (byte Player, byte LoadoutIndex, ushort AccessoryVisibilityMask) Read(PacketWireReader reader)
    {
        byte player = reader.ReadByte();
        byte loadout = reader.ReadByte();
        ushort visibility = reader.ReadUInt16();
        return (Player: player, LoadoutIndex: loadout, AccessoryVisibilityMask: visibility);
    }
}
