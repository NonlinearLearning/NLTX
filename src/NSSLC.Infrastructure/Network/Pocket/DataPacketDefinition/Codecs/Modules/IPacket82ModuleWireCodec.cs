using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public interface IPacket82ModuleWireCodec
{
    ushort ModuleId { get; }
    void Write(PacketWireWriter writer, object? payload, int tagEffectNpcSlotCount, Func<short, bool> tagEffectUsesProcTimes);
    object? Read(PacketWireReader reader, int tagEffectNpcSlotCount, Func<short, bool> tagEffectUsesProcTimes);
}
