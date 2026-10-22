using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Combat.Npcs;

public static class NpcBuffPacketProducer
{
  public static NPCBuffsPacket CreateSnapshot(NpcBuffView snapshot)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    var buffs = new List<PacketBuffTime>(snapshot.Buffs.Count);
    foreach (NpcBuffEntry buff in snapshot.Buffs)
    {
      if (buff.Type == 0 || buff.Time <= 0 || buff.Time > ushort.MaxValue)
      {
        throw new InvalidOperationException("NPC buff state cannot be represented by packet 54.");
      }

      buffs.Add(new PacketBuffTime(buff.Type, (ushort)buff.Time));
    }

    return new NPCBuffsPacket
    {
      NpcIndex = snapshot.NpcSlot,
      Buffs = buffs.AsReadOnly()
    };
  }
}
