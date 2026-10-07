using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class ManaEffectPacket
{
    public static void Write(PacketWireWriter writer, byte player, short manaEffect)
    {
        writer.WriteByte(player);
        writer.WriteInt16(manaEffect);
    }

    public static (byte Player, short ManaEffect) Read(PacketWireReader reader) => (Player: reader.ReadByte(), ManaEffect: reader.ReadInt16());
}
