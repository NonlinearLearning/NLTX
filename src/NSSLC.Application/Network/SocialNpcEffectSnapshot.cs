using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Terraria.Npc;
using Terraria.Relationships;

namespace Terraria.Network;

/// <summary>Detached NPC state required to construct committed packet-23 updates.</summary>
public sealed class SocialNpcEffectSnapshot
{
  private readonly IReadOnlyList<float> _ai;
  private readonly IReadOnlyList<float> _localAi;

  public SocialNpcEffectSnapshot(
    RuntimeEntityHandle runtimeHandle,
    EntityReference entityReference,
    NpcSlot slot,
    uint slotGeneration,
    int typeId,
    int netId,
    Vector2 position,
    Vector2 velocity,
    int direction,
    int directionY,
    int spriteDirection,
    int target,
    int currentLife,
    int maximumLife,
    bool spawnedFromStatue,
    bool spawnNeedsSyncing,
    bool shimmering,
    bool canBeCaught,
    IEnumerable<float> ai,
    IEnumerable<float> localAi,
    bool networkUpdatePending,
    uint networkUpdateRevision)
  {
    ArgumentNullException.ThrowIfNull(ai);
    ArgumentNullException.ThrowIfNull(localAi);
    if (!runtimeHandle.IsAssigned || entityReference.IsEmpty ||
        entityReference.Scope != EntityReferenceScope.Npc ||
        entityReference.RuntimeId != runtimeHandle.RuntimeId ||
        !slot.IsAssigned || slotGeneration == 0 || typeId <= 0 || netId < 0 ||
        target < -1 || currentLife < 0 || maximumLife <= 0 || currentLife > maximumLife)
    {
      throw new ArgumentException("The NPC effect snapshot requires committed runtime identities.");
    }

    float[] aiValues = ai.ToArray();
    float[] localAiValues = localAi.ToArray();
    if (aiValues.Length != NpcBehaviorComponent.AiSlotCount ||
        localAiValues.Length != NpcLocalBehaviorStateComponent.LocalAiSlotCount)
    {
      throw new ArgumentException("NPC effect snapshots require complete AI and local AI arrays.");
    }

    RuntimeHandle = runtimeHandle;
    EntityReference = entityReference;
    Slot = slot;
    SlotGeneration = slotGeneration;
    TypeId = typeId;
    NetId = netId;
    Position = position;
    Velocity = velocity;
    Direction = direction;
    DirectionY = directionY;
    SpriteDirection = spriteDirection;
    Target = target;
    CurrentLife = currentLife;
    MaximumLife = maximumLife;
    SpawnedFromStatue = spawnedFromStatue;
    SpawnNeedsSyncing = spawnNeedsSyncing;
    Shimmering = shimmering;
    CanBeCaught = canBeCaught;
    _ai = Array.AsReadOnly(aiValues);
    _localAi = Array.AsReadOnly(localAiValues);
    NetworkUpdatePending = networkUpdatePending;
    NetworkUpdateRevision = networkUpdateRevision;
  }

  public RuntimeEntityHandle RuntimeHandle { get; }

  public EntityReference EntityReference { get; }

  public NpcSlot Slot { get; }

  public uint SlotGeneration { get; }

  public int TypeId { get; }

  public int NetId { get; }

  public Vector2 Position { get; }

  public Vector2 Velocity { get; }

  public int Direction { get; }

  public int DirectionY { get; }

  public int SpriteDirection { get; }

  public int Target { get; }

  public int CurrentLife { get; }

  public int MaximumLife { get; }

  public bool SpawnedFromStatue { get; }

  public bool SpawnNeedsSyncing { get; }

  public bool Shimmering { get; }

  public bool CanBeCaught { get; }

  public IReadOnlyList<float> Ai => _ai;

  public IReadOnlyList<float> LocalAi => _localAi;

  public bool NetworkUpdatePending { get; }

  public uint NetworkUpdateRevision { get; }
}
