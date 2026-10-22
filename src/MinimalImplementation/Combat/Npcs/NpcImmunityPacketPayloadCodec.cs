using System.IO;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace MinimalImplementation.Combat.Npcs;

/// <summary>Encodes packet 131 body bytes; packet framing and recipient selection belong to P01/P03.</summary>
public sealed class NpcImmunityPacketPayloadCodec
{
  public byte[] EncodeBody(NpcImmunityPacketIntent intent)
  {
    ArgumentNullException.ThrowIfNull(intent);
    TamperWithNPCPacket packet = intent.CreatePacket();
    using var stream = new MemoryStream();
    var writer = new PacketWireWriter(stream);
    TamperWithNPCPacket.Write(
      writer,
      packet.NpcIndex,
      packet.Action,
      packet.Value,
      packet.Style);
    return stream.ToArray();
  }
}
