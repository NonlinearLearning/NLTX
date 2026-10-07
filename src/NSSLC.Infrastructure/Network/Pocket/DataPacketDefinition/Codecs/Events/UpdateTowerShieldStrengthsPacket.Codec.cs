using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class UpdateTowerShieldStrengthsPacket
{
    public static void Write(
        PacketWireWriter writer,
        ushort solar,
        ushort vortex,
        ushort nebula,
        ushort stardust)
    {
        writer.WriteUInt16(solar);
        writer.WriteUInt16(vortex);
        writer.WriteUInt16(nebula);
        writer.WriteUInt16(stardust);
    }

    public static (ushort Solar, ushort Vortex, ushort Nebula, ushort Stardust) Read(PacketWireReader reader) => (Solar: reader.ReadUInt16(), Vortex: reader.ReadUInt16(), Nebula: reader.ReadUInt16(), Stardust: reader.ReadUInt16());
}
