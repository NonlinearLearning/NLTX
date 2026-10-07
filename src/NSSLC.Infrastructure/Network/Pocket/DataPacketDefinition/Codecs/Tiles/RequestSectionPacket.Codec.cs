using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class RequestSectionPacket
{
    public static void Write(PacketWireWriter writer, ushort sectionX, ushort sectionY)
    {
        writer.WriteInt16(unchecked((short)sectionX));
        writer.WriteInt16(unchecked((short)sectionY));
    }

    public static (ushort SectionX, ushort SectionY) Read(PacketWireReader reader)
    {
        return (SectionX: reader.ReadUInt16(), SectionY: reader.ReadUInt16());
    }
}
