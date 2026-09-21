using System;

namespace Terraria.Projectile;

public static class ProjectileHitImmunitySystem
{
  public static void AdvanceTick(
    ref ProjectileHitImmunityStateComponent state,
    in ProjectileHitImmunityPolicyComponent policy)
  {
    DecrementPositive(state.PlayerImmunityTicks, nameof(state.PlayerImmunityTicks));

    if (policy.UsesLocalNpcImmunity)
    {
      DecrementPositive(
        state.LocalNpcImmunityTicks,
        nameof(state.LocalNpcImmunityTicks));
    }

    state.RestrikeDelayTicks = DecrementPositive(state.RestrikeDelayTicks);
  }

  public static bool IsLocalNpcImmune(
    in ProjectileHitImmunityStateComponent state,
    int npcIndex)
  {
    return GetValue(
      state.LocalNpcImmunityTicks,
      npcIndex,
      nameof(state.LocalNpcImmunityTicks)) != 0;
  }

  public static bool IsPlayerImmune(
    in ProjectileHitImmunityStateComponent state,
    int playerIndex)
  {
    return GetValue(
      state.PlayerImmunityTicks,
      playerIndex,
      nameof(state.PlayerImmunityTicks)) > 0;
  }

  public static void SetLocalNpcImmunity(
    ref ProjectileHitImmunityStateComponent state,
    int npcIndex,
    int ticks)
  {
    if (ticks < -1)
    {
      throw new ArgumentOutOfRangeException(nameof(ticks));
    }

    state.LocalNpcImmunityTicks = SetValue(
      state.LocalNpcImmunityTicks,
      npcIndex,
      ticks,
      nameof(state.LocalNpcImmunityTicks));
  }

  public static void SetPlayerImmunity(
    ref ProjectileHitImmunityStateComponent state,
    int playerIndex,
    int ticks)
  {
    if (ticks < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(ticks));
    }

    state.PlayerImmunityTicks = SetValue(
      state.PlayerImmunityTicks,
      playerIndex,
      ticks,
      nameof(state.PlayerImmunityTicks));
  }

  public static void SetRestrikeDelay(
    ref ProjectileHitImmunityStateComponent state,
    int ticks)
  {
    if (ticks < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(ticks));
    }

    state.RestrikeDelayTicks = ticks;
  }

  public static void ResetLocalNpcImmunity(
    ref ProjectileHitImmunityStateComponent state)
  {
    Clear(state.LocalNpcImmunityTicks, nameof(state.LocalNpcImmunityTicks));
  }

  public static void ResetPlayerImmunity(
    ref ProjectileHitImmunityStateComponent state)
  {
    Clear(state.PlayerImmunityTicks, nameof(state.PlayerImmunityTicks));
  }

  private static int DecrementPositive(int value)
  {
    return value > 0 ? value - 1 : value;
  }

  private static void DecrementPositive(int[] values, string parameterName)
  {
    ArgumentNullException.ThrowIfNull(values, parameterName);
    for (int index = 0; index < values.Length; index++)
    {
      values[index] = DecrementPositive(values[index]);
    }
  }

  private static int GetValue(int[] values, int index, string parameterName)
  {
    ArgumentNullException.ThrowIfNull(values, parameterName);
    ValidateIndex(values, index, parameterName);
    return values[index];
  }

  private static int[] SetValue(
    int[] values,
    int index,
    int value,
    string parameterName)
  {
    ArgumentNullException.ThrowIfNull(values, parameterName);
    ValidateIndex(values, index, parameterName);
    values[index] = value;
    return values;
  }

  private static void Clear(int[] values, string parameterName)
  {
    ArgumentNullException.ThrowIfNull(values, parameterName);
    Array.Clear(values, 0, values.Length);
  }

  private static void ValidateIndex(int[] values, int index, string parameterName)
  {
    if ((uint)index >= (uint)values.Length)
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }
}
