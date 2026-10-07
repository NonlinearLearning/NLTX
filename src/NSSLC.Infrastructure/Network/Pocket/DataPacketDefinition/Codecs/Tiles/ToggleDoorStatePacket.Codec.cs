using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class ToggleDoorStatePacket
{
    public static void Write(
        PacketWireWriter writer,
        byte action,
        short x,
        short y,
        byte direction)
    {
        writer.WriteByte(action);
        writer.WriteInt16(x);
        writer.WriteInt16(y);
        writer.WriteByte(direction);
    }

    public static (byte Action, short X, short Y, byte Direction) Read(PacketWireReader reader) => (Action: reader.ReadByte(), X: reader.ReadInt16(), Y: reader.ReadInt16(), Direction: reader.ReadByte());
}
