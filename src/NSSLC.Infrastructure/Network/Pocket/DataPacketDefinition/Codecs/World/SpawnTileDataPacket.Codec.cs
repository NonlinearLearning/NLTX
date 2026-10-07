using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class SpawnTileDataPacket
{
    public static void Write(PacketWireWriter writer, int x, int y, byte team)
    {
        writer.WriteInt32(x);
        writer.WriteInt32(y);
        writer.WriteByte(team);
    }

    public static (int X, int Y, byte Team) Read(PacketWireReader reader) => (X: reader.ReadInt32(), Y: reader.ReadInt32(), Team: reader.ReadByte());
}
