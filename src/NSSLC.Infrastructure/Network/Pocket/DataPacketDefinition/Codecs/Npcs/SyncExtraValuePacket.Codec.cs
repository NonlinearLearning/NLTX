using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class SyncExtraValuePacket
{
    public static void Write(
        PacketWireWriter writer,
        short npcIndex,
        int extraValue,
        float valueX,
        float valueY)
    {
        writer.WriteInt16(npcIndex);
        writer.WriteInt32(extraValue);
        writer.WriteSingle(valueX);
        writer.WriteSingle(valueY);
    }

    public static (short NpcIndex, int ExtraValue, float ValueX, float ValueY) Read(PacketWireReader reader) => (NpcIndex: reader.ReadInt16(), ExtraValue: reader.ReadInt32(), ValueX: reader.ReadSingle(), ValueY: reader.ReadSingle());
}
