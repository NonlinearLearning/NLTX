using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class SetTimePacket
{
    public static void Write(
        PacketWireWriter writer,
        byte dayTime,
        int time,
        short sunModY,
        short moonModY)
    {
        writer.WriteByte(dayTime);
        writer.WriteInt32(time);
        writer.WriteInt16(sunModY);
        writer.WriteInt16(moonModY);
    }

    public static (byte DayTime, int Time, short SunModY, short MoonModY) Read(PacketWireReader reader) => (DayTime: reader.ReadByte(), Time: reader.ReadInt32(), SunModY: reader.ReadInt16(), MoonModY: reader.ReadInt16());
}
