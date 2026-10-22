using System.Collections.ObjectModel;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Modules.Creative;

public static class CreativeModuleCodecs
{
  public static IReadOnlyDictionary<ushort, IPacket82ModuleWireCodec> WithCreativeOverrides(
    IReadOnlyDictionary<ushort, IPacket82ModuleWireCodec> baseCodecs)
  {
    ArgumentNullException.ThrowIfNull(baseCodecs);
    if (baseCodecs.Count != Enum.GetValues<Packet82ModuleId>().Length ||
        Enum.GetValues<Packet82ModuleId>().Any(moduleId => !baseCodecs.ContainsKey((ushort)moduleId)))
    {
      throw new ArgumentException(
        "Creative codec overrides require the complete Version4 fifteen-module registry.",
        nameof(baseCodecs));
    }

    // P01's Version4 registry owns the typed five-byte module 6 payload. Do not replace
    // it with the compatibility codec's different local DTO.
    var codecs = new Dictionary<ushort, IPacket82ModuleWireCodec>(baseCodecs);
    return new ReadOnlyDictionary<ushort, IPacket82ModuleWireCodec>(codecs);
  }
}
