using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class ItemRotationAndAnimationPacket
{
    public static void Write(PacketWireWriter writer, byte player, float itemRotation, short itemAnimation)
    {
        writer.WriteByte(player);
        writer.WriteSingle(itemRotation);
        writer.WriteInt16(itemAnimation);
    }

    public static (byte Player, float ItemRotation, short ItemAnimation) Read(PacketWireReader reader) => (Player: reader.ReadByte(), ItemRotation: reader.ReadSingle(), ItemAnimation: reader.ReadInt16());
}
