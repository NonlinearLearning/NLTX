using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class LockAndUnlockPacket
{
    public static void Write(PacketWireWriter writer, byte action, short x, short y)
    {
        writer.WriteByte(action);
        writer.WriteInt16(x);
        writer.WriteInt16(y);
    }

    public static (byte Action, short X, short Y) Read(PacketWireReader reader) => (Action: reader.ReadByte(), X: reader.ReadInt16(), Y: reader.ReadInt16());
}
