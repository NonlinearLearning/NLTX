using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class SyncPlayerZonePacket
{
    public static void Write(
        PacketWireWriter writer,
        byte player,
        byte zone1,
        byte zone2,
        byte zone3,
        byte zone4,
        byte zone5,
        byte townNpcCount)
    {
        writer.WriteByte(player);
        writer.WriteByte(zone1);
        writer.WriteByte(zone2);
        writer.WriteByte(zone3);
        writer.WriteByte(zone4);
        writer.WriteByte(zone5);
        writer.WriteByte(townNpcCount);
    }

    public static (
        byte Player,
        byte Zone1,
        byte Zone2,
        byte Zone3,
        byte Zone4,
        byte Zone5,
        byte TownNpcCount) Read(PacketWireReader reader)
    {
        byte player = reader.ReadByte();
        byte zone1 = reader.ReadByte();
        byte zone2 = reader.ReadByte();
        byte zone3 = reader.ReadByte();
        byte zone4 = reader.ReadByte();
        byte zone5 = reader.ReadByte();
        byte townNpcs = reader.ReadByte();
        return (Player: player, Zone1: zone1, Zone2: zone2, Zone3: zone3, Zone4: zone4, Zone5: zone5, TownNpcCount: townNpcs);
    }
}
