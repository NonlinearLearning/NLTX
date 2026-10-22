using MinimalImplementation.Combat.Identity;
using MinimalImplementation.Contracts;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Combat.Npcs;

/// <summary>Packet 131 subtype 1 bound to the NPC generation that caused the update.</summary>
public sealed class NpcImmunityPacketIntent
{
  internal NpcImmunityPacketIntent(
    CombatEntityIdentity entity,
    short npcSlot,
    long npcRevision,
    CommitVersion commitVersion,
    int immunityTime,
    short sourcePlayerIndex)
  {
    Entity = entity;
    NpcSlot = npcSlot;
    NpcRevision = npcRevision;
    CommitVersion = commitVersion;
    ImmunityTime = immunityTime;
    SourcePlayerIndex = sourcePlayerIndex;
  }

  public CombatEntityIdentity Entity { get; }

  public short NpcSlot { get; }

  public long NpcRevision { get; }

  public CommitVersion CommitVersion { get; }

  public int ImmunityTime { get; }

  /// <summary>-1 applies the time to every immunity slot; 0–254 selects one player.</summary>
  public short SourcePlayerIndex { get; }

  public TamperWithNPCPacket CreatePacket()
  {
    return new TamperWithNPCPacket
    {
      NpcIndex = checked((ushort)NpcSlot),
      Action = 1,
      Value = ImmunityTime,
      Style = SourcePlayerIndex
    };
  }
}
