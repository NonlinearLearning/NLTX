using System;
using System.Numerics;
using Terraria.Share.Combat;
using Terraria.Share.Relationships;

namespace Terraria.Unshared.Npc;

/// <summary>Unmanaged output from shared AI for the NPC domain adapter.</summary>
public readonly record struct NpcAiIntent
{
  public NpcAiIntent(
      NpcAiIntentKind kind,
      NpcBehaviorPhase phase = NpcBehaviorPhase.Idle,
      Vector2 desiredVelocity = default,
      int actionPhase = 0,
      float stateSlot0 = 0f,
      float stateSlot1 = 0f,
      EntityRuntimeHandle attackTarget = default,
      ulong attackDefinitionKey = 0,
      bool jumpRequested = false,
      float jumpSpeed = 0f)
  {
    if (!Enum.IsDefined(kind) || !Enum.IsDefined(phase))
    {
      throw new ArgumentOutOfRangeException(nameof(kind));
    }

    if (!float.IsFinite(desiredVelocity.X) || !float.IsFinite(desiredVelocity.Y) ||
        !float.IsFinite(stateSlot0) || !float.IsFinite(stateSlot1) ||
        !float.IsFinite(jumpSpeed) || jumpSpeed < 0f)
    {
      throw new ArgumentOutOfRangeException(nameof(desiredVelocity));
    }

    if (actionPhase < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(actionPhase));
    }

    if (attackDefinitionKey != 0 && !attackTarget.IsValid)
    {
      throw new ArgumentException(
          "An NPC attack intent requires a valid target handle and definition.",
          nameof(attackDefinitionKey));
    }

    Kind = kind;
    Phase = phase;
    DesiredVelocity = desiredVelocity;
    JumpRequested = jumpRequested;
    JumpSpeed = jumpSpeed;
    ActionPhase = actionPhase;
    StateSlot0 = stateSlot0;
    StateSlot1 = stateSlot1;
    AttackTarget = attackTarget;
    AttackDefinitionKey = attackDefinitionKey;
  }

  public NpcAiIntentKind Kind { get; }

  public NpcBehaviorPhase Phase { get; }

  public Vector2 DesiredVelocity { get; }

  /// <summary>Requests a jump through the movement system on the current tick.</summary>
  public bool JumpRequested { get; }

  /// <summary>Optional jump speed in world units per second.</summary>
  public float JumpSpeed { get; }

  public int ActionPhase { get; }

  public float StateSlot0 { get; }

  public float StateSlot1 { get; }

  public EntityRuntimeHandle AttackTarget { get; }

  public ulong AttackDefinitionKey { get; }
}
