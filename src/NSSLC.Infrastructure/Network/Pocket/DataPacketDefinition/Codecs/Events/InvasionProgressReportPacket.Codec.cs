using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class InvasionProgressReportPacket
{
    public static void Write(
        PacketWireWriter writer,
        int invasionType,
        int progress,
        sbyte wave,
        sbyte maxWave)
    {
        writer.WriteInt32(invasionType);
        writer.WriteInt32(progress);
        writer.WriteSByte(wave);
        writer.WriteSByte(maxWave);
    }

    public static (int InvasionType, int Progress, sbyte Wave, sbyte MaxWave) Read(PacketWireReader reader) => (InvasionType: reader.ReadInt32(), Progress: reader.ReadInt32(), Wave: reader.ReadSByte(), MaxWave: reader.ReadSByte());
}
