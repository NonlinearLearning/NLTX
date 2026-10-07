using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class InstrumentSoundPacket
{
    public static void Write(PacketWireWriter writer, byte player, float pitch)
    {
        writer.WriteByte(player);
        writer.WriteSingle(pitch);
    }

    public static (byte Player, float Pitch) Read(PacketWireReader reader) => (Player: reader.ReadByte(), Pitch: reader.ReadSingle());
}
