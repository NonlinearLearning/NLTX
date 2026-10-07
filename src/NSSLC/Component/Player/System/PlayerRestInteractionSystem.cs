using System.Numerics;

namespace Terraria.Player;

public readonly record struct PlayerRestStopResult(
  PlayerRestActivity StoppedActivities,
  bool ShouldSendSittingStopPacket,
  bool ShouldSendSleepingStopPacket);

public static class PlayerRestInteractionSystem
{
  public enum EntrySource : byte
  {
    MountPet,
    AnimalPet,
    Sitting,
    Sleeping,
  }

  public enum EntryDecision : byte
  {
    Rejected,
    StopCurrentActivity,
    BeginWithoutRestCleanup,
    StopRestBeforeBegin,
  }

  public readonly record struct SittingEntryDetails(
    TileCoordinate AnchorTile,
    DirectionKind RequiredFacing,
    RestSeatFeatures SeatFeatures,
    Vector2 SeatOffset,
    int StackIndex);

  public readonly record struct SleepingEntryDetails(
    TileCoordinate AnchorTile,
    DirectionKind RequiredFacing,
    int StackIndex,
    Vector2 BedVisualOffset);

  public readonly record struct EntryInput(
    EntrySource Source,
    bool IsEligible,
    bool IsActivityActive,
    bool ResolvedPositionMatches,
    SittingEntryDetails? SittingDetails = null,
    SleepingEntryDetails? SleepingDetails = null);

  public readonly record struct EntryPreparationResult
  {
    internal EntryPreparationResult(
      EntrySource source,
      EntryDecision decision,
      PlayerRestStopResult restStopResult,
      SittingEntryDetails? sittingDetails,
      SleepingEntryDetails? sleepingDetails)
    {
      Source = source;
      Decision = decision;
      RestStopResult = restStopResult;
      SittingDetails = sittingDetails;
      SleepingDetails = sleepingDetails;
    }

    public EntrySource Source { get; }

    public EntryDecision Decision { get; }

    public PlayerRestStopResult RestStopResult { get; }

    public SittingEntryDetails? SittingDetails { get; }

    public SleepingEntryDetails? SleepingDetails { get; }
  }

  public static PlayerRestState QueryState(
    PlayerRestComponent rest,
    PlayerSleepingComponent sleeping)
  {
    ArgumentNullException.ThrowIfNull(rest);
    ArgumentNullException.ThrowIfNull(sleeping);

    return new PlayerRestState(rest.Activities, sleeping.TimeSleeping);
  }

  public static EntryDecision DecideEntry(EntryInput input)
  {
    if (!input.IsEligible)
    {
      return EntryDecision.Rejected;
    }

    EntryDecision decision;
    if (input.Source == EntrySource.MountPet)
    {
      decision = input.IsActivityActive
        ? EntryDecision.StopCurrentActivity
        : EntryDecision.BeginWithoutRestCleanup;
    }
    else if (input.Source is not EntrySource.AnimalPet and
      not EntrySource.Sitting and not EntrySource.Sleeping)
    {
      return EntryDecision.Rejected;
    }
    else if (input.IsActivityActive && input.ResolvedPositionMatches)
    {
      decision = EntryDecision.StopCurrentActivity;
    }
    else
    {
      decision = EntryDecision.StopRestBeforeBegin;
    }

    if (decision == EntryDecision.StopRestBeforeBegin &&
      input.Source == EntrySource.Sitting && !input.SittingDetails.HasValue)
    {
      return EntryDecision.Rejected;
    }

    if (decision == EntryDecision.StopRestBeforeBegin &&
      input.Source == EntrySource.Sleeping && !input.SleepingDetails.HasValue)
    {
      return EntryDecision.Rejected;
    }

    return decision;
  }

  public static EntryPreparationResult PrepareEntry(
    EntryInput input,
    PlayerRestComponent rest,
    PlayerSittingComponent sitting,
    PlayerSleepingComponent sleeping,
    bool isLocalPlayer,
    bool multiplayerBroadcast)
  {
    ArgumentNullException.ThrowIfNull(rest);
    ArgumentNullException.ThrowIfNull(sitting);
    ArgumentNullException.ThrowIfNull(sleeping);

    EntryDecision decision = DecideEntry(input);
    PlayerRestStopResult stopResult = decision switch
    {
      EntryDecision.StopCurrentActivity => StopCurrentActivity(
        input.Source,
        rest,
        sitting,
        sleeping,
        isLocalPlayer,
        multiplayerBroadcast),
      EntryDecision.StopRestBeforeBegin => StopAll(
        rest,
        sitting,
        sleeping,
        isLocalPlayer,
        multiplayerBroadcast),
      _ => default,
    };

    return new EntryPreparationResult(
      input.Source,
      decision,
      stopResult,
      input.SittingDetails,
      input.SleepingDetails);
  }

  // Call after caller-owned target and movement effects to preserve the observed entry order.
  public static bool CommitEntry(
    PlayerRestComponent rest,
    PlayerSittingComponent sitting,
    PlayerSleepingComponent sleeping,
    EntryPreparationResult preparation)
  {
    ArgumentNullException.ThrowIfNull(rest);
    ArgumentNullException.ThrowIfNull(sitting);
    ArgumentNullException.ThrowIfNull(sleeping);

    return (preparation.Source, preparation.Decision) switch
    {
      (EntrySource.MountPet, EntryDecision.BeginWithoutRestCleanup) => BeginPetting(rest),
      (EntrySource.AnimalPet, EntryDecision.StopRestBeforeBegin) => BeginPetting(rest),
      (EntrySource.Sitting, EntryDecision.StopRestBeforeBegin) =>
        BeginSitting(rest, sitting, preparation.SittingDetails.GetValueOrDefault()),
      (EntrySource.Sleeping, EntryDecision.StopRestBeforeBegin) =>
        BeginSleeping(rest, sleeping, preparation.SleepingDetails.GetValueOrDefault()),
      _ => false,
    };
  }

  public static bool BeginPetting(PlayerRestComponent rest)
  {
    return BeginActivity(rest, PlayerRestActivity.Petting);
  }

  public static bool BeginSitting(
    PlayerRestComponent rest,
    PlayerSittingComponent sitting,
    SittingEntryDetails details)
  {
    ArgumentNullException.ThrowIfNull(sitting);
    if (!BeginActivity(rest, PlayerRestActivity.Sitting))
    {
      return false;
    }

    sitting.AnchorTile = details.AnchorTile;
    sitting.RequiredFacing = details.RequiredFacing;
    sitting.SeatFeatures = details.SeatFeatures;
    sitting.SeatOffset = details.SeatOffset;
    sitting.StackIndex = details.StackIndex;
    return true;
  }

  public static bool BeginSleeping(
    PlayerRestComponent rest,
    PlayerSleepingComponent sleeping,
    SleepingEntryDetails details)
  {
    ArgumentNullException.ThrowIfNull(sleeping);
    if (!BeginActivity(rest, PlayerRestActivity.Sleeping))
    {
      return false;
    }

    sleeping.AnchorTile = details.AnchorTile;
    sleeping.RequiredFacing = details.RequiredFacing;
    sleeping.StackIndex = details.StackIndex;
    sleeping.BedVisualOffset = details.BedVisualOffset;
    sleeping.TimeSleeping = 0;
    return true;
  }

  public static bool AdvanceSleepTimer(
    PlayerRestComponent rest,
    PlayerSleepingComponent sleeping,
    bool hasReasonToActUp)
  {
    ArgumentNullException.ThrowIfNull(rest);
    ArgumentNullException.ThrowIfNull(sleeping);

    if ((rest.Activities & PlayerRestActivity.Sleeping) == 0)
    {
      sleeping.TimeSleeping = 0;
      return false;
    }

    sleeping.TimeSleeping = unchecked(sleeping.TimeSleeping + 1);
    if (hasReasonToActUp)
    {
      sleeping.TimeSleeping = 0;
    }

    return true;
  }

  public static PlayerRestStopResult StopPetting(PlayerRestComponent rest)
  {
    ArgumentNullException.ThrowIfNull(rest);
    var wasActive = (rest.Activities & PlayerRestActivity.Petting) != 0;
    rest.Activities &= ~PlayerRestActivity.Petting;
    return new PlayerRestStopResult(
      wasActive ? PlayerRestActivity.Petting : PlayerRestActivity.None,
      false,
      false);
  }

  public static PlayerRestStopResult StopSitting(
    PlayerRestComponent rest,
    PlayerSittingComponent sitting,
    bool isLocalPlayer,
    bool multiplayerBroadcast)
  {
    ArgumentNullException.ThrowIfNull(rest);
    ArgumentNullException.ThrowIfNull(sitting);

    if ((rest.Activities & PlayerRestActivity.Sitting) == 0)
    {
      return default;
    }

    rest.Activities &= ~PlayerRestActivity.Sitting;
    sitting.AnchorTile = null;
    sitting.RequiredFacing = default;
    sitting.SeatFeatures = RestSeatFeatures.None;
    sitting.SeatOffset = Vector2.Zero;
    sitting.StackIndex = -1;

    return new PlayerRestStopResult(
      PlayerRestActivity.Sitting,
      isLocalPlayer && multiplayerBroadcast,
      false);
  }

  public static PlayerRestStopResult StopSleeping(
    PlayerRestComponent rest,
    PlayerSleepingComponent sleeping,
    bool isLocalPlayer,
    bool multiplayerBroadcast)
  {
    ArgumentNullException.ThrowIfNull(rest);
    ArgumentNullException.ThrowIfNull(sleeping);

    if ((rest.Activities & PlayerRestActivity.Sleeping) == 0)
    {
      return default;
    }

    rest.Activities &= ~PlayerRestActivity.Sleeping;
    sleeping.AnchorTile = null;
    sleeping.RequiredFacing = default;
    sleeping.StackIndex = -1;
    sleeping.TimeSleeping = 0;
    sleeping.BedVisualOffset = Vector2.Zero;

    return new PlayerRestStopResult(
      PlayerRestActivity.Sleeping,
      false,
      isLocalPlayer && multiplayerBroadcast);
  }

  public static PlayerRestStopResult StopAll(
    PlayerRestComponent rest,
    PlayerSittingComponent sitting,
    PlayerSleepingComponent sleeping,
    bool isLocalPlayer,
    bool multiplayerBroadcast)
  {
    ArgumentNullException.ThrowIfNull(rest);
    ArgumentNullException.ThrowIfNull(sitting);
    ArgumentNullException.ThrowIfNull(sleeping);

    var stoppedActivities = PlayerRestActivity.None;

    PlayerRestStopResult pettingResult = StopPetting(rest);
    stoppedActivities |= pettingResult.StoppedActivities;

    PlayerRestStopResult sittingResult = StopSitting(
      rest,
      sitting,
      isLocalPlayer,
      multiplayerBroadcast);
    stoppedActivities |= sittingResult.StoppedActivities;

    PlayerRestStopResult sleepingResult = StopSleeping(
      rest,
      sleeping,
      isLocalPlayer,
      multiplayerBroadcast);
    stoppedActivities |= sleepingResult.StoppedActivities;

    return new PlayerRestStopResult(
      stoppedActivities,
      sittingResult.ShouldSendSittingStopPacket,
      sleepingResult.ShouldSendSleepingStopPacket);
  }

  private static bool BeginActivity(
    PlayerRestComponent rest,
    PlayerRestActivity activity)
  {
    ArgumentNullException.ThrowIfNull(rest);
    if ((rest.Activities & activity) != 0)
    {
      return false;
    }

    rest.Activities |= activity;
    return true;
  }

  private static PlayerRestStopResult StopCurrentActivity(
    EntrySource source,
    PlayerRestComponent rest,
    PlayerSittingComponent sitting,
    PlayerSleepingComponent sleeping,
    bool isLocalPlayer,
    bool multiplayerBroadcast)
  {
    return source switch
    {
      EntrySource.MountPet or EntrySource.AnimalPet => StopPetting(rest),
      EntrySource.Sitting => StopSitting(
        rest,
        sitting,
        isLocalPlayer,
        multiplayerBroadcast),
      EntrySource.Sleeping => StopSleeping(
        rest,
        sleeping,
        isLocalPlayer,
        multiplayerBroadcast),
      _ => default,
    };
  }
}
