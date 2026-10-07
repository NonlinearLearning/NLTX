using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class SyncChestSizePacket
{
    public static void Write(PacketWireWriter writer, short chestIndex, short size)
    {
        writer.WriteInt16(chestIndex);
        writer.WriteInt16(size);
    }

    public static (short ChestIndex, short Size) Read(PacketWireReader reader)
    {
        return (ChestIndex: reader.ReadInt16(), Size: reader.ReadInt16());
    }
}
