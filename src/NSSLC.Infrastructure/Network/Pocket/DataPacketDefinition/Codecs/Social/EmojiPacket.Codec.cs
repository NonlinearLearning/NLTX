using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class EmojiPacket
{
    public static void Write(PacketWireWriter writer, byte player, byte emote)
    {
        writer.WriteByte(player);
        writer.WriteByte(emote);
    }

    public static (byte Player, byte Emote) Read(PacketWireReader reader) => (Player: reader.ReadByte(), Emote: reader.ReadByte());
}
