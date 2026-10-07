using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class QuestsCountSyncPacket
{
    public static void Write(PacketWireWriter writer, byte player, int anglerQuestsFinished, int golferScore)
    {
        writer.WriteByte(player);
        writer.WriteInt32(anglerQuestsFinished);
        writer.WriteInt32(golferScore);
    }

    public static (byte Player, int AnglerQuestsFinished, int GolferScore) Read(PacketWireReader reader) => (Player: reader.ReadByte(), AnglerQuestsFinished: reader.ReadInt32(), GolferScore: reader.ReadInt32());
}
