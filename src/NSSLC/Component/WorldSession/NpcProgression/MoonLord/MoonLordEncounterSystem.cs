using System;

namespace Terraria.WorldSession.NpcProgression.MoonLord;

public sealed class MoonLordEncounterSystem
{
  private readonly MoonLordEncounterDefinition _definition;
  private readonly MoonLordEncounterStateComponent _state;

  public MoonLordEncounterSystem(
    MoonLordEncounterDefinition definition,
    MoonLordEncounterStateComponent state)
  {
    _definition = definition ?? throw new ArgumentNullException(nameof(definition));
    _state = state ?? throw new ArgumentNullException(nameof(state));
  }

  public MoonLordCountdownSyncView StartNaturalCountdown()
  {
    return StartCountdown(_definition.NaturalMoonlordCountdownTime);
  }

  public MoonLordCountdownSyncView StartItemCountdown()
  {
    return StartCountdown(_definition.ItemMoonlordCountdownTime);
  }

  public MoonLordCountdownSyncView StartCountdown(int countdown)
  {
    _state.StartCountdown(countdown);
    return CreateCountdownSyncView();
  }

  public MoonLordCountdownTickResult TickCountdown(bool hasServerSpawnAuthority)
  {
    if (_state.MoonLordCountdown <= 0)
    {
      return new MoonLordCountdownTickResult(
        MoonLordCountdownTickStatus.NoCountdown,
        _state.MoonLordCountdown);
    }

    int countdown = _state.TickCountdown();
    if (countdown > 0)
    {
      return new MoonLordCountdownTickResult(
        MoonLordCountdownTickStatus.Ticked,
        countdown);
    }

    if (hasServerSpawnAuthority && _state.TryIssueSpawnRequest())
    {
      return new MoonLordCountdownTickResult(
        MoonLordCountdownTickStatus.SpawnRequested,
        countdown);
    }

    return new MoonLordCountdownTickResult(
      MoonLordCountdownTickStatus.ReachedZero,
      countdown);
  }

  public MoonLordCountdownSyncView CreateCountdownSyncView()
  {
    return new MoonLordCountdownSyncView(
      _state.MaxMoonLordCountdown,
      _state.MoonLordCountdown);
  }

  public void Reset()
  {
    _state.Reset();
  }
}
