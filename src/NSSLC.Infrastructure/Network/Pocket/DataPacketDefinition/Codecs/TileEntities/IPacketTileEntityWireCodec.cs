using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public interface IPacketTileEntityWireCodec
{
    byte Type { get; }
    void WriteExtraData(PacketWireWriter writer, PacketTileEntityRecord entity, bool networkSend);
    object? ReadExtraData(PacketWireReader reader, bool networkSend);
}
