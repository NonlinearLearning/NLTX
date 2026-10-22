using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace MinimalImplementation.Server.Modules;

public static class ModulePacketPayload
{
  public static byte[] Encode(
    ushort moduleId,
    object? payload,
    IReadOnlyDictionary<ushort, IPacket82ModuleWireCodec> codecs,
    int tagEffectNpcSlotCount = 0,
    Func<short, bool>? tagEffectUsesProcTimes = null)
  {
    ArgumentNullException.ThrowIfNull(codecs);
    if (moduleId >= ModuleDispatcher.ModuleCount)
    {
      throw new ArgumentOutOfRangeException(
        nameof(moduleId),
        "Packet 82 module ids must remain within the Version4 range 0-14.");
    }

    using var stream = new MemoryStream();
    var writer = new PacketWireWriter(stream, ushort.MaxValue - 3);
    writer.WriteUInt16(moduleId);
    IPacket82ModuleWireCodec codec = codecs.TryGetValue(moduleId, out IPacket82ModuleWireCodec? found)
      ? found
      : throw new ArgumentException($"No Packet 82 codec is registered for module {moduleId}.", nameof(codecs));
    codec.Write(writer, payload, tagEffectNpcSlotCount, tagEffectUsesProcTimes ?? (static _ => false));
    return stream.ToArray();
  }
}
