using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class MoonlordHorrorPacket
{
    public static void Write(PacketWireWriter writer, int maximumCountdown, int countdown)
    {
        writer.WriteInt32(maximumCountdown);
        writer.WriteInt32(countdown);
    }

    public static (int MaximumCountdown, int Countdown) Read(PacketWireReader reader) => (MaximumCountdown: reader.ReadInt32(), Countdown: reader.ReadInt32());
}
