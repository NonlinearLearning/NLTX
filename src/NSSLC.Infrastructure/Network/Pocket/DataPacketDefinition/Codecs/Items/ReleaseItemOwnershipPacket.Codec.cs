using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class ReleaseItemOwnershipPacket
{
    public static void Write(PacketWireWriter writer, short itemIndex, bool forceAssignToServer)
    {
        writer.WriteInt16(itemIndex);
        writer.WriteBoolean(forceAssignToServer);
    }

    public static (short ItemIndex, bool ForceAssignToServer) Read(PacketWireReader reader)
    {
        short index = reader.ReadInt16();
        byte force = reader.ReadByte();
        if (index is < 0 or >= 400 || force > 1)
        {
            throw new PacketWireFormatException("Invalid ownership release fields.");
        }
        return (index, force != 0);
    }
}
