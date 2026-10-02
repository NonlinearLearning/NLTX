using System;

namespace Terraria.Fishing;

// status: proposed
public readonly record struct FishingAttemptId(Guid Value)
{
  public bool IsValid => Value != Guid.Empty;
}

// status: proposed
public readonly record struct PlayerEntityId(Guid Value)
{
  public bool IsValid => Value != Guid.Empty;
}

// status: proposed
public readonly record struct ProjectileEntityId(Guid Value)
{
  public bool IsValid => Value != Guid.Empty;
}

// status: proposed
public readonly record struct ItemInstanceId(Guid Value)
{
  public bool IsValid => Value != Guid.Empty;
}

// status: proposed
public readonly record struct NpcEntityId(Guid Value)
{
  public bool IsValid => Value != Guid.Empty;
}

// status: proposed
public readonly record struct IdempotencyKey(Guid Value)
{
  public bool IsValid => Value != Guid.Empty;
}

// status: proposed
public enum FishingAttemptPhase : byte
{
  Waiting,
  Biting,
  Resolving,
  Retracting,
}

// status: proposed
public enum FishingBiteState : byte
{
  Waiting,
  Biting,
  Retracting,
}

// status: proposed
public enum FishingTerminalReason : byte
{
  Rejected,
  Cancelled,
  Committed,
  Expired,
}

// status: proposed
public enum FishingLiquidKind : byte
{
  Unknown,
  Water,
  Lava,
  Honey,
}

// status: proposed
public enum FishingOutcomeKind : byte
{
  None,
  Item,
  Npc,
  Junk,
  QuestFish,
}

// status: proposed
public enum BaitReservationState : byte
{
  Unbound,
  Reserved,
  Consumed,
  Rejected,
}

// status: proposed
public enum FishingCommitState : byte
{
  NotSubmitted,
  InProgress,
  Committed,
  Rejected,
  TerminalFailure,
}

// status: proposed
public readonly record struct FishingBiomeFlags(ulong Value)
{
  public bool IsEmpty => Value == 0;
}

// status: proposed
public readonly record struct FishingRarityFlags(ulong Value)
{
  public bool IsEmpty => Value == 0;
}

// status: proposed
public readonly record struct FishingRngAuditValue(ulong Value);

// status: proposed
// Compatibility-only value. Its fields retain legacy ai/localAI slots and have no business meaning.
public readonly record struct FishingLegacyBobberState(
  float? Ai0,
  float? Ai1,
  float? LocalAi1,
  float? LocalAi2);

// status: proposed
// A discriminated relation to an externally owned Item or NPC instance.
public readonly record struct FishingExternalResultId(
  ItemInstanceId? Item,
  NpcEntityId? Npc)
{
  public bool IsItem => Item.HasValue && !Npc.HasValue && Item.Value.IsValid;

  public bool IsNpc => Npc.HasValue && !Item.HasValue && Npc.Value.IsValid;

  public bool IsValid => IsItem ^ IsNpc;
}
