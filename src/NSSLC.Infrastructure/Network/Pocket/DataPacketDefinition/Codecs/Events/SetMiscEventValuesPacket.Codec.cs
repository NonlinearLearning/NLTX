using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class SetMiscEventValuesPacket
{
    public static void Write(PacketWireWriter writer, byte eventKind, int value)
    {
        writer.WriteByte(eventKind);
        writer.WriteInt32(value);
    }

    public static (byte EventKind, int Value) Read(PacketWireReader reader)
    {
        return (EventKind: reader.ReadByte(), Value: reader.ReadInt32());
    }
}
