using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class SyncCavernMonsterTypePacket
{
    public static void Write(
        PacketWireWriter writer,
        ushort type00,
        ushort type01,
        ushort type02,
        ushort type10,
        ushort type11,
        ushort type12)
    {
        writer.WriteUInt16(type00);
        writer.WriteUInt16(type01);
        writer.WriteUInt16(type02);
        writer.WriteUInt16(type10);
        writer.WriteUInt16(type11);
        writer.WriteUInt16(type12);
    }

    public static (
        ushort Type00,
        ushort Type01,
        ushort Type02,
        ushort Type10,
        ushort Type11,
        ushort Type12) Read(PacketWireReader reader)
    {
        ushort type00 = reader.ReadUInt16();
        ushort type01 = reader.ReadUInt16();
        ushort type02 = reader.ReadUInt16();
        ushort type10 = reader.ReadUInt16();
        ushort type11 = reader.ReadUInt16();
        ushort type12 = reader.ReadUInt16();
        return (Type00: type00, Type01: type01, Type02: type02, Type10: type10, Type11: type11, Type12: type12);
    }
}
