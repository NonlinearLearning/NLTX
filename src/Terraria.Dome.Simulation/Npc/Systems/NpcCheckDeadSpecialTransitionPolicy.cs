using System;

namespace Terraria.Dome.Simulation.Npc.Systems;

public static class NpcCheckDeadSpecialTransitionPolicy
{
  public const int Version1456NpcTypeCount = 697;

  private const int Type396 = 396;
  private const int Type397 = 397;
  private const int Type398 = 398;
  private const int Type517 = 517;
  private const int Type422 = 422;
  private const int Type507 = 507;
  private const int Type493 = 493;
  private const int Type548 = 548;
  private const int Type400 = 400;

  public static NpcCheckDeadSpecialTransitionDecision Evaluate(
    NpcCheckDeadSpecialTransitionInput input)
  {
    Validate(input);
    NpcCheckDeadSpecialTransitionState state = new(
      input.Life,
      input.Ai0,
      input.Ai1,
      input.Ai2,
      input.Ai3,
      input.DontTakeDamage,
      input.DontTakeDamageFromHostiles,
      input.NetUpdate);

    if (input.NpcType is Type396 or Type397)
    {
      if (input.Ai0 == -2.0f)
      {
        return new(
          ShouldReturnFromCheckDead: true,
          NpcCheckDeadSpecialTransitionKind.Type396Or397,
          state,
          SpawnIntent: null);
      }

      NpcCheckDeadSpawnIntent spawnIntent = new(
        input.Npc,
        Type400,
        new SimulationVector(
          TruncateToInt(input.Center.X),
          TruncateToInt(input.Center.Y)),
        input.Ai3,
        ChildNetUpdate: true);
      return new(
        ShouldReturnFromCheckDead: true,
        NpcCheckDeadSpecialTransitionKind.Type396Or397,
        state with
        {
          Life = input.MaximumHealth,
          Ai0 = -2.0f,
          DontTakeDamage = true,
          NetUpdate = true
        },
        spawnIntent);
    }

    if (input.NpcType == Type398 && input.Ai0 != 2.0f)
    {
      return new(
        ShouldReturnFromCheckDead: true,
        NpcCheckDeadSpecialTransitionKind.Type398,
        state with
        {
          Life = input.MaximumHealth,
          Ai0 = 2.0f,
          DontTakeDamage = true,
          NetUpdate = true
        },
        SpawnIntent: null);
    }

    if ((input.NpcType is Type517 or Type422 or Type507 or Type493) && input.Ai2 != 1.0f)
    {
      return new(
        ShouldReturnFromCheckDead: true,
        NpcCheckDeadSpecialTransitionKind.Type517Family,
        state with
        {
          Life = input.MaximumHealth,
          Ai1 = 0.0f,
          Ai2 = 1.0f,
          DontTakeDamage = true,
          NetUpdate = true
        },
        SpawnIntent: null);
    }

    if (input.NpcType == Type548 && input.Ai1 != 1.0f)
    {
      return new(
        ShouldReturnFromCheckDead: true,
        NpcCheckDeadSpecialTransitionKind.Type548,
        state with
        {
          Life = input.MaximumHealth,
          Ai0 = 0.0f,
          Ai1 = 1.0f,
          DontTakeDamageFromHostiles = true,
          NetUpdate = true
        },
        SpawnIntent: null);
    }

    return new(
      ShouldReturnFromCheckDead: false,
      NpcCheckDeadSpecialTransitionKind.None,
      state,
      SpawnIntent: null);
  }

  private static void Validate(NpcCheckDeadSpecialTransitionInput input)
  {
    if (!input.Npc.IsValid)
    {
      throw new ArgumentOutOfRangeException(nameof(input.Npc));
    }

    if (input.NpcType < 0 || input.NpcType >= Version1456NpcTypeCount)
    {
      throw new ArgumentOutOfRangeException(nameof(input.NpcType));
    }

    if (input.Life > 0)
    {
      throw new ArgumentOutOfRangeException(nameof(input.Life));
    }

    if (input.MaximumHealth <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(input.MaximumHealth));
    }

    if (!float.IsFinite(input.Ai0))
    {
      throw new ArgumentOutOfRangeException(nameof(input.Ai0));
    }

    if (!float.IsFinite(input.Ai1))
    {
      throw new ArgumentOutOfRangeException(nameof(input.Ai1));
    }

    if (!float.IsFinite(input.Ai2))
    {
      throw new ArgumentOutOfRangeException(nameof(input.Ai2));
    }

    if (!float.IsFinite(input.Ai3))
    {
      throw new ArgumentOutOfRangeException(nameof(input.Ai3));
    }

    if (!CanTruncateToInt(input.Center.X))
    {
      throw new ArgumentOutOfRangeException(nameof(input.Center));
    }

    if (!CanTruncateToInt(input.Center.Y))
    {
      throw new ArgumentOutOfRangeException(nameof(input.Center));
    }
  }

  private static bool CanTruncateToInt(float value)
  {
    return float.IsFinite(value) && value >= int.MinValue && value <= int.MaxValue;
  }

  private static int TruncateToInt(float value)
  {
    return (int)value;
  }
}
