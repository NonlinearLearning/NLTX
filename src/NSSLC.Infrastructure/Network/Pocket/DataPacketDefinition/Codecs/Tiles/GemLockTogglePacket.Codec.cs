using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class GemLockTogglePacket
{
    public static void Write(PacketWireWriter writer, short x, short y, bool enabled)
    {
        writer.WriteInt16(x);
        writer.WriteInt16(y);
        writer.WriteBoolean(enabled);
    }

    public static (short X, short Y, bool Enabled) Read(PacketWireReader reader) => (X: reader.ReadInt16(), Y: reader.ReadInt16(), Enabled: reader.ReadBoolean());
}
