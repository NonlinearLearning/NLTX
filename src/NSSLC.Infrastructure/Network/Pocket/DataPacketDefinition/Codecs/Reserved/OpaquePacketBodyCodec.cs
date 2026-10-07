using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

[PacketSharedFunction]
public static class OpaquePacketBodyCodec
{
    public static void Write(PacketWireWriter writer, ReadOnlyMemory<byte> bytes)
    {
        writer.WriteBytes(bytes.Span);
    }

    public static ReadOnlyMemory<byte> Read(PacketWireReader reader)
    {
        ReadOnlyMemory<byte> body = reader.ReadBytes(reader.Remaining);
        return body;
    }
}
