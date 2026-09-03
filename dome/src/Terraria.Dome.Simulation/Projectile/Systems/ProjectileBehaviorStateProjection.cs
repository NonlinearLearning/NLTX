using System;
using System.IO;
using Terraria.Dome.Simulation.Components;

namespace Terraria.Dome.Simulation.Projectile.Systems;

public readonly record struct ProjectileBehaviorReplicationState(
  float Ai0,
  float Ai1,
  float Ai2);

public static class ProjectileBehaviorStateProjection
{
  private const int LinearBehaviorId = 1;
  private const int GravityBehaviorId = 2;
  private const int LegacyAiStyle2BehaviorId = 3;
  private const int LegacyAiStyle2DelayedBehaviorId = 4;
  private const int LegacyAiStyle2ImmediateGravityBehaviorId = 5;
  private const int LegacyAiStyle2FiveTickGravityBehaviorId = 6;
  private const int LegacyAiStyle2SixtyTickGravityBehaviorId = 7;
  private const int LegacyAiStyle2RandomFrameBehaviorId = 8;
  private const int LegacyAiStyle2EighteenTickGravityBehaviorId = 9;
  private const int LegacyAiStyle2SixteenTickGravityBehaviorId = 10;
  private const int LegacyAiStyle29ParentBehaviorId = 11;
  private const int LegacyAiStyle29ChildBehaviorId = 12;
  private const int LegacyAiStyle2Type162BehaviorId = 13;
  private const int LegacyAiStyle49Type281BehaviorId = 14;
  private const int LegacyAiStyle2Type166BehaviorId = 15;
  private const int LegacyAiStyle2Type304BehaviorId = 16;
  private const int LegacyAiStyle2StatusEffectBehaviorId = 17;
  private const int LegacyAiStyle2HitStatusBehaviorId = 18;
  private const int LegacyAiStyle190BehaviorId = 19;
  private const int LegacyAiStyle17BehaviorId = 20;
  private const int LegacyType607BehaviorId = 21;

  public static ProjectileBehaviorReplicationState Project(ProjectileBehaviorComponent behavior)
  {
    if (behavior.State.Phase < 0)
    {
      throw new InvalidDataException("Projectile behavior phase is invalid.");
    }

    return behavior.BehaviorId switch
    {
      LinearBehaviorId => new ProjectileBehaviorReplicationState(
        behavior.State.Phase,
        0.0f,
        0.0f),
      GravityBehaviorId when float.IsFinite(behavior.State.Primary) =>
        new ProjectileBehaviorReplicationState(
          behavior.State.Primary,
          behavior.State.Phase,
          0.0f),
      GravityBehaviorId => throw new InvalidDataException(
        "Gravity projectile behavior state is not finite."),
      LegacyAiStyle2BehaviorId when float.IsFinite(behavior.State.Primary) =>
        new ProjectileBehaviorReplicationState(
          behavior.State.Primary,
          behavior.State.Phase,
          0.0f),
      LegacyAiStyle2BehaviorId => throw new InvalidDataException(
        "Legacy aiStyle 2 projectile behavior state is not finite."),
      LegacyAiStyle2DelayedBehaviorId when float.IsFinite(behavior.State.Primary) =>
        new ProjectileBehaviorReplicationState(
          behavior.State.Primary,
          behavior.State.Phase,
          0.0f),
      LegacyAiStyle2DelayedBehaviorId => throw new InvalidDataException(
        "Legacy delayed aiStyle 2 projectile behavior state is not finite."),
      LegacyAiStyle2ImmediateGravityBehaviorId when float.IsFinite(behavior.State.Primary) =>
        new ProjectileBehaviorReplicationState(
          behavior.State.Primary,
          behavior.State.Phase,
          0.0f),
      LegacyAiStyle2ImmediateGravityBehaviorId => throw new InvalidDataException(
        "Legacy immediate aiStyle 2 projectile behavior state is not finite."),
      LegacyAiStyle2FiveTickGravityBehaviorId when float.IsFinite(behavior.State.Primary) =>
        new ProjectileBehaviorReplicationState(
          behavior.State.Primary,
          behavior.State.Phase,
          0.0f),
      LegacyAiStyle2FiveTickGravityBehaviorId => throw new InvalidDataException(
        "Legacy five-tick aiStyle 2 projectile behavior state is not finite."),
      LegacyAiStyle2SixtyTickGravityBehaviorId when float.IsFinite(behavior.State.Primary) =>
        new ProjectileBehaviorReplicationState(
          behavior.State.Primary,
          behavior.State.Phase,
          0.0f),
      LegacyAiStyle2SixtyTickGravityBehaviorId => throw new InvalidDataException(
        "Legacy sixty-tick aiStyle 2 projectile behavior state is not finite."),
      LegacyAiStyle2RandomFrameBehaviorId when
          float.IsFinite(behavior.State.Primary) && float.IsFinite(behavior.State.Secondary) =>
        new ProjectileBehaviorReplicationState(
          behavior.State.Primary,
          behavior.State.Secondary,
          0.0f),
      LegacyAiStyle2RandomFrameBehaviorId => throw new InvalidDataException(
        "Legacy random-frame aiStyle 2 projectile behavior state is not finite."),
      LegacyAiStyle2EighteenTickGravityBehaviorId when float.IsFinite(behavior.State.Primary) =>
        new ProjectileBehaviorReplicationState(
          behavior.State.Primary,
          behavior.State.Phase,
          0.0f),
      LegacyAiStyle2EighteenTickGravityBehaviorId => throw new InvalidDataException(
        "Legacy eighteen-tick aiStyle 2 projectile behavior state is not finite."),
      LegacyAiStyle2SixteenTickGravityBehaviorId when float.IsFinite(behavior.State.Primary) =>
        new ProjectileBehaviorReplicationState(
          behavior.State.Primary,
          behavior.State.Phase,
          0.0f),
      LegacyAiStyle2SixteenTickGravityBehaviorId => throw new InvalidDataException(
        "Legacy sixteen-tick aiStyle 2 projectile behavior state is not finite."),
      LegacyAiStyle29ParentBehaviorId => new ProjectileBehaviorReplicationState(
        0.0f,
        0.0f,
        0.0f),
      LegacyAiStyle29ChildBehaviorId when float.IsFinite(behavior.State.Primary) =>
        new ProjectileBehaviorReplicationState(
          0.0f,
          behavior.State.Primary,
          0.0f),
      LegacyAiStyle29ChildBehaviorId => throw new InvalidDataException(
        "Legacy aiStyle 29 child projectile behavior state is not finite."),
      LegacyAiStyle2Type162BehaviorId when float.IsFinite(behavior.State.Primary) =>
        new ProjectileBehaviorReplicationState(
          behavior.State.Primary,
          behavior.State.Phase,
          0.0f),
      LegacyAiStyle2Type162BehaviorId => throw new InvalidDataException(
        "Legacy type 162 projectile behavior state is not finite."),
      LegacyAiStyle49Type281BehaviorId when float.IsFinite(behavior.State.Primary) =>
        new ProjectileBehaviorReplicationState(
          behavior.State.Primary,
          behavior.State.Phase,
          0.0f),
      LegacyAiStyle49Type281BehaviorId => throw new InvalidDataException(
        "Legacy type 281 projectile behavior state is not finite."),
      LegacyAiStyle2Type166BehaviorId when float.IsFinite(behavior.State.Primary) =>
        new ProjectileBehaviorReplicationState(
          behavior.State.Primary,
          behavior.State.Phase,
          0.0f),
      LegacyAiStyle2Type166BehaviorId => throw new InvalidDataException(
        "Legacy type 166 projectile behavior state is not finite."),
      LegacyAiStyle2Type304BehaviorId when float.IsFinite(behavior.State.Primary) =>
        new ProjectileBehaviorReplicationState(
          behavior.State.Primary,
          behavior.State.Phase,
          0.0f),
      LegacyAiStyle2Type304BehaviorId => throw new InvalidDataException(
        "Legacy type 304 projectile behavior state is not finite."),
      LegacyAiStyle2StatusEffectBehaviorId when float.IsFinite(behavior.State.Primary) =>
        new ProjectileBehaviorReplicationState(
          behavior.State.Primary,
          behavior.State.Phase,
          0.0f),
      LegacyAiStyle2StatusEffectBehaviorId => throw new InvalidDataException(
        "Legacy status-effect aiStyle 2 projectile behavior state is not finite."),
      LegacyAiStyle2HitStatusBehaviorId when float.IsFinite(behavior.State.Primary) =>
        new ProjectileBehaviorReplicationState(
          behavior.State.Primary,
          behavior.State.Phase,
          0.0f),
      LegacyAiStyle2HitStatusBehaviorId => throw new InvalidDataException(
        "Legacy hit-status aiStyle 2 projectile behavior state is not finite."),
      LegacyAiStyle190BehaviorId when float.IsFinite(behavior.State.Primary) &&
          float.IsFinite(behavior.State.Secondary) && float.IsFinite(behavior.State.Tertiary) =>
        new ProjectileBehaviorReplicationState(
          behavior.State.Primary,
          behavior.State.Secondary,
          behavior.State.Tertiary),
      LegacyAiStyle190BehaviorId => throw new InvalidDataException(
        "Legacy aiStyle 190 projectile behavior state is not finite."),
      LegacyAiStyle17BehaviorId when float.IsFinite(behavior.State.Primary) &&
          float.IsFinite(behavior.State.Secondary) && float.IsFinite(behavior.State.Tertiary) =>
        new ProjectileBehaviorReplicationState(
          behavior.State.Primary,
          behavior.State.Secondary,
          behavior.State.Tertiary),
      LegacyAiStyle17BehaviorId => throw new InvalidDataException(
        "Legacy aiStyle 17 projectile behavior state is not finite."),
      LegacyType607BehaviorId when float.IsFinite(behavior.State.Primary) =>
        new ProjectileBehaviorReplicationState(
          behavior.State.Primary,
          behavior.State.Phase,
          behavior.State.Tertiary),
      LegacyType607BehaviorId => throw new InvalidDataException(
        "Legacy type 607 projectile behavior state is not finite."),
      _ => throw new InvalidDataException(
        "Projectile behavior has no authoritative replication projection.")
    };
  }
}
