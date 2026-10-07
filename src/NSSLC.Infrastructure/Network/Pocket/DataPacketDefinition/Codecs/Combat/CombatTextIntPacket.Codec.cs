using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class CombatTextIntPacket
{
    public static void Write(
        PacketWireWriter writer,
        float x,
        float y,
        PacketRgb color,
        int amount)
    {
        writer.WriteSingle(x);
        writer.WriteSingle(y);
        writer.WriteByte(color.Red);
        writer.WriteByte(color.Green);
        writer.WriteByte(color.Blue);
        writer.WriteInt32(amount);
    }

    public static (float X, float Y, PacketRgb Color, int Amount) Read(PacketWireReader reader) => (X: reader.ReadSingle(), Y: reader.ReadSingle(), Color: new PacketRgb(reader.ReadByte(), reader.ReadByte(), reader.ReadByte()), Amount: reader.ReadInt32());
}
