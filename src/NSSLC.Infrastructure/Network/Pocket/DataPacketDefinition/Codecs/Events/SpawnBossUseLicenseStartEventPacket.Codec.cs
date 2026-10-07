using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class SpawnBossUseLicenseStartEventPacket
{
    public static void Write(PacketWireWriter writer, short player, short eventOrNpcType)
    {
        writer.WriteInt16(player);
        writer.WriteInt16(eventOrNpcType);
    }

    public static (short Player, short EventOrNpcType) Read(PacketWireReader reader)
    {
        return (Player: reader.ReadInt16(), EventOrNpcType: reader.ReadInt16());
    }
}
