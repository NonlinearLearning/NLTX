using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class ChestNameResponsePacket
{
    public static void Write(
        PacketWireWriter writer,
        short chestIndex,
        short x,
        short y,
        string chestName)
    {
        writer.WriteInt16(chestIndex);
        writer.WriteInt16(x);
        writer.WriteInt16(y);
        writer.WriteString(chestName);
    }

    public static (short ChestIndex, short X, short Y, string ChestName) Read(PacketWireReader reader) => (ChestIndex: reader.ReadInt16(), X: reader.ReadInt16(), Y: reader.ReadInt16(), ChestName: reader.ReadString());
}
