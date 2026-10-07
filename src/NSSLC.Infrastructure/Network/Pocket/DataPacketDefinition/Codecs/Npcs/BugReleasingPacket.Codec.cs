using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class BugReleasingPacket
{
    public static void Write(
        PacketWireWriter writer,
        int x,
        int y,
        short npcType,
        byte style)
    {
        writer.WriteInt32(x);
        writer.WriteInt32(y);
        writer.WriteInt16(npcType);
        writer.WriteByte(style);
    }

    public static (int X, int Y, short NpcType, byte Style) Read(PacketWireReader reader) => (X: reader.ReadInt32(), Y: reader.ReadInt32(), NpcType: reader.ReadInt16(), Style: reader.ReadByte());
}
