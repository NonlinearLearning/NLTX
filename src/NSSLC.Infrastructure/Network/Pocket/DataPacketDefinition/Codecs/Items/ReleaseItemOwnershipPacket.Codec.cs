using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class ReleaseItemOwnershipPacket
{
    public static void Write(PacketWireWriter writer, short itemIndex)
    {
        writer.WriteInt16(itemIndex);
    }

    public static short Read(PacketWireReader reader) => reader.ReadInt16();
}
