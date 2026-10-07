using System.Numerics;

namespace Terraria.Npc;

public readonly record struct NpcEyeOfCthulhuProfileResult(
  bool IsSupported,
  NpcEyeOfCthulhuProfileState State,
  Vector2 Velocity,
  int DustRoll,
  bool DustRequested,
  NpcEyeOfCthulhuAttackIntent AttackIntent,
  NpcEyeOfCthulhuExitReason ExitReason,
  bool TargetResetRequested,
  bool NetworkUpdateRequested)
{
  public bool ServantSummonRequested { get; init; }

  public Vector2 ServantSummonPosition { get; init; }

  public Vector2 ServantSummonVelocity { get; init; }

  public int ServantSummonDustCount { get; init; }

  public bool ServantSummonSoundRequested { get; init; }

  public bool IsTransformationTick { get; init; }

  public bool TransformationBurstRequested { get; init; }

  public bool ReflectsProjectiles { get; init; }
}
