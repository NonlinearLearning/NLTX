using System;

namespace Terraria.Projectile;

public static class ProjectileNetworkStateSystem
{
  public const int SendBudgetThreshold = 60;
  public const int SendBudgetIncrement = 5;

  public static void Initialize(
    ref ProjectileNetworkStateComponent state,
    int playerCapacity = 255)
  {
    state = new ProjectileNetworkStateComponent(playerCapacity);
  }

  public static void SetNetworkImportant(
    ref ProjectileNetworkStateComponent state,
    bool networkImportant)
  {
    state.NetworkImportant = networkImportant;
  }

  public static void RequestPrimaryUpdate(
    ref ProjectileNetworkStateComponent state)
  {
    state.PrimaryUpdatePending = true;
    state.SendRequested = true;
  }

  public static void RequestSecondaryUpdate(
    ref ProjectileNetworkStateComponent state)
  {
    state.SecondaryUpdatePending = true;
    state.SendRequested = true;
  }

  /// <summary>
  /// Begins one entered Version4 projectile update substep. The immediate
  /// update request is transient; a deferred request remains pending.
  /// </summary>
  public static void BeginProjectileUpdateSubstep(
    ref ProjectileNetworkStateComponent state)
  {
    state.PrimaryUpdatePending = false;
    state.SendRequested = state.SecondaryUpdatePending;
  }

  public static bool TryConsumeSendBudget(
    ref ProjectileNetworkStateComponent state)
  {
    if (!state.PrimaryUpdatePending && !state.SecondaryUpdatePending)
    {
      return false;
    }

    if (state.NetSpam >= SendBudgetThreshold)
    {
      state.PrimaryUpdatePending = false;
      state.SecondaryUpdatePending = true;
      state.SendRequested = true;
      return false;
    }

    state.NetSpam = checked(state.NetSpam + SendBudgetIncrement);
    state.PrimaryUpdatePending = false;
    state.SecondaryUpdatePending = false;
    state.SendRequested = false;
    return true;
  }

  public static void AdvanceTick(
    ref ProjectileNetworkStateComponent state)
  {
    if (state.NetSpam > 0)
    {
      state.NetSpam--;
    }
  }

  public static bool IsSectionSyncSkipped(
    in ProjectileNetworkStateComponent state,
    int playerIndex)
  {
    return GetSectionFlag(state, playerIndex);
  }

  public static void MarkSectionSyncSkipped(
    ref ProjectileNetworkStateComponent state,
    int playerIndex)
  {
    SetSectionFlag(ref state, playerIndex, true);
  }

  public static void ClearSectionSyncSkipped(
    ref ProjectileNetworkStateComponent state,
    int playerIndex)
  {
    SetSectionFlag(ref state, playerIndex, false);
  }

  public static void ResetSectionSyncSkipped(
    ref ProjectileNetworkStateComponent state)
  {
    ArgumentNullException.ThrowIfNull(
      state.SectionSyncSkippedForPlayer,
      nameof(state.SectionSyncSkippedForPlayer));
    Array.Clear(
      state.SectionSyncSkippedForPlayer,
      0,
      state.SectionSyncSkippedForPlayer.Length);
  }

  public static void Reset(
    ref ProjectileNetworkStateComponent state)
  {
    ArgumentNullException.ThrowIfNull(
      state.SectionSyncSkippedForPlayer,
      nameof(state.SectionSyncSkippedForPlayer));
    state.NetworkImportant = false;
    state.PrimaryUpdatePending = false;
    state.SecondaryUpdatePending = false;
    state.NetSpam = 0;
    state.SendRequested = false;
    ResetSectionSyncSkipped(ref state);
  }

  private static bool GetSectionFlag(
    in ProjectileNetworkStateComponent state,
    int playerIndex)
  {
    ArgumentNullException.ThrowIfNull(
      state.SectionSyncSkippedForPlayer,
      nameof(state.SectionSyncSkippedForPlayer));
    ValidatePlayerIndex(state.SectionSyncSkippedForPlayer, playerIndex);
    return state.SectionSyncSkippedForPlayer[playerIndex];
  }

  private static void SetSectionFlag(
    ref ProjectileNetworkStateComponent state,
    int playerIndex,
    bool value)
  {
    ArgumentNullException.ThrowIfNull(
      state.SectionSyncSkippedForPlayer,
      nameof(state.SectionSyncSkippedForPlayer));
    ValidatePlayerIndex(state.SectionSyncSkippedForPlayer, playerIndex);
    state.SectionSyncSkippedForPlayer[playerIndex] = value;
  }

  private static void ValidatePlayerIndex(bool[] values, int playerIndex)
  {
    if ((uint)playerIndex >= (uint)values.Length)
    {
      throw new ArgumentOutOfRangeException(nameof(playerIndex));
    }
  }
}
