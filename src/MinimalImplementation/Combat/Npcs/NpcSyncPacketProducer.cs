using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Combat.Npcs;

/// <summary>
/// Projects committed NPC facts into the infrastructure's packet-23 DTO.
/// </summary>
public sealed class NpcSyncPacketProducer
{
  public SyncNPCPacket Create(NpcView npc)
  {
    ArgumentNullException.ThrowIfNull(npc);
    if (!npc.Entity.IsValid || npc.Slot < 0 || npc.MaximumLife < 0 || npc.Life < 0 ||
        npc.Life > npc.MaximumLife ||
        npc.Ai.Count != 4 || npc.Ai.Any(value => !float.IsFinite(value)) ||
        (npc.Catchable != npc.CatchableReleaseOwner.HasValue))
    {
      throw new InvalidOperationException(
        "Only a valid committed NPC snapshot can produce packet 23.");
    }

    // Vanilla reconstructs full life from SetDefaults; custom maxima must send explicit life.
    bool fullLife = npc.Life == npc.MaximumLife && !npc.HasCustomMaximumLife;
    // MaximumLife is the effective maximum captured with PlayerCount/Difficulty at commit.
    byte lifeWidth = npc.MaximumLife > short.MaxValue ? (byte)4 :
        npc.MaximumLife > sbyte.MaxValue ? (byte)2 : (byte)1;
    return new SyncNPCPacket
    {
      NpcSlot = npc.Slot,
      PositionX = npc.Position.X,
      PositionY = npc.Position.Y,
      VelocityX = npc.Velocity.X,
      VelocityY = npc.Velocity.Y,
      Target = npc.Target,
      DirectionPositive = npc.DirectionPositive,
      DirectionYPositive = npc.DirectionYPositive,
      SpriteDirectionPositive = npc.SpriteDirectionPositive,
      FullLife = fullLife,
      SpawnedFromStatue = npc.SpawnedFromStatue,
      SpawnNeedsSyncing = npc.SpawnNeedsSyncing,
      Shimmering = npc.Shimmering,
      Ai = npc.Ai.ToArray(),
      NetId = npc.Type,
      PlayerCount = npc.PlayerCount,
      Difficulty = npc.Difficulty,
      Life = fullLife ? null : npc.Life,
      EncodedLifeWidth = fullLife ? null : lifeWidth,
      CatchableReleaseOwner = npc.CatchableReleaseOwner
    };
  }
}
