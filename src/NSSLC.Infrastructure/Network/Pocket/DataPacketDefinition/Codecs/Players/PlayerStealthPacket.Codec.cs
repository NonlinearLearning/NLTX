using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class PlayerStealthPacket
{
    public static void Write(PacketWireWriter writer, byte player, float stealth)
    {
        writer.WriteByte(player);
        writer.WriteSingle(stealth);
    }

    public static (byte Player, float Stealth) Read(PacketWireReader reader) => (Player: reader.ReadByte(), Stealth: reader.ReadSingle());
}
