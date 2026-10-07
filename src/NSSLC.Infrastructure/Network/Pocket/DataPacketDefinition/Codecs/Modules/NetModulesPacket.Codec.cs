using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class NetModulesPacket
{
    public static void Write(
        PacketWireWriter writer,
        ushort moduleId,
        object? data,
        IReadOnlyDictionary<ushort, IPacket82ModuleWireCodec> moduleCodecs,
        int tagEffectNpcSlotCount,
        Func<short, bool> tagEffectUsesProcTimes)
    {
        ArgumentNullException.ThrowIfNull(moduleCodecs);
        ArgumentNullException.ThrowIfNull(tagEffectUsesProcTimes);
        if (tagEffectNpcSlotCount is < 0 or > byte.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(tagEffectNpcSlotCount));
        IPacket82ModuleWireCodec codec = GetModuleCodec(moduleCodecs, moduleId);
        writer.WriteUInt16(moduleId);
        codec.Write(writer, data, tagEffectNpcSlotCount, tagEffectUsesProcTimes);
    }

    public static (ushort ModuleId, object? Data) Read(
        PacketWireReader reader,
        IReadOnlyDictionary<ushort, IPacket82ModuleWireCodec> moduleCodecs,
        int tagEffectNpcSlotCount,
        Func<short, bool> tagEffectUsesProcTimes)
    {
        ArgumentNullException.ThrowIfNull(moduleCodecs);
        ArgumentNullException.ThrowIfNull(tagEffectUsesProcTimes);
        if (tagEffectNpcSlotCount is < 0 or > byte.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(tagEffectNpcSlotCount));
        ushort moduleId = reader.ReadUInt16();
        object? payload = GetModuleCodec(moduleCodecs, moduleId).Read(reader, tagEffectNpcSlotCount, tagEffectUsesProcTimes);
        reader.RequireFrameEnd();
        return (ModuleId: moduleId, Data: payload);
    }

    private static IPacket82ModuleWireCodec GetModuleCodec(IReadOnlyDictionary<ushort, IPacket82ModuleWireCodec> moduleCodecs, ushort moduleId) => moduleCodecs.TryGetValue(moduleId, out var codec) ? codec : throw new PacketWireFormatException($"No Packet 82 module codec is registered for id {moduleId}.");
}
