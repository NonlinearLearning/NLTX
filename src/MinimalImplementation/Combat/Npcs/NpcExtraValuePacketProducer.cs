using System.IO;
using MinimalImplementation.Contracts;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace MinimalImplementation.Combat.Npcs;

/// <summary>Projects committed NPC extra-value state into packet 92.</summary>
public static class NpcExtraValuePacketProducer
{
  public static SyncExtraValuePacket Create(NpcView npc)
  {
    ArgumentNullException.ThrowIfNull(npc);
    if (!npc.Entity.IsValid || !npc.CommitVersion.IsValid || npc.Revision <= 0)
    {
      throw new InvalidOperationException(
          "Packet 92 requires a committed NPC generation.");
    }

    if (!float.IsFinite(npc.ExtraValuePosition.X) ||
        !float.IsFinite(npc.ExtraValuePosition.Y))
    {
      throw new InvalidOperationException(
          "Packet 92 requires finite extra-value coordinates.");
    }

    return new SyncExtraValuePacket
    {
      NpcIndex = npc.Slot,
      ExtraValue = npc.ExtraValue,
      ValueX = npc.ExtraValuePosition.X,
      ValueY = npc.ExtraValuePosition.Y
    };
  }

  public static byte[] EncodeBody(NpcView npc)
  {
    SyncExtraValuePacket packet = Create(npc);
    using var stream = new MemoryStream();
    var writer = new PacketWireWriter(stream);
    SyncExtraValuePacket.Write(
        writer, packet.NpcIndex, packet.ExtraValue, packet.ValueX, packet.ValueY);
    return stream.ToArray();
  }
}
