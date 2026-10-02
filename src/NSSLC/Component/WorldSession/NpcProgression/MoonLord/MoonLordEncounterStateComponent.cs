using System;

namespace Terraria.WorldSession.NpcProgression.MoonLord;

public sealed class MoonLordEncounterStateComponent
{
  private int _maxMoonLordCountdown;
  private bool _spawnRequestIssued;

  public MoonLordEncounterStateComponent(int maxMoonLordCountdown)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(maxMoonLordCountdown);
    _maxMoonLordCountdown = maxMoonLordCountdown;
  }

  public int MoonLordCountdown { get; private set; }

  public int MaxMoonLordCountdown => _maxMoonLordCountdown;

  public void StartCountdown(int countdown)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(countdown);
    _maxMoonLordCountdown = countdown;
    MoonLordCountdown = countdown;
    _spawnRequestIssued = false;
  }

  public void SetCountdown(int countdown)
  {
    if (countdown < 0 || countdown > _maxMoonLordCountdown)
    {
      throw new ArgumentOutOfRangeException(
        nameof(countdown),
        "Moon Lord countdown must remain within the encounter definition range.");
    }

    MoonLordCountdown = countdown;
    if (countdown > 0)
    {
      _spawnRequestIssued = false;
    }
  }

  public int TickCountdown()
  {
    if (MoonLordCountdown <= 0)
    {
      return 0;
    }

    MoonLordCountdown--;
    return MoonLordCountdown;
  }

  public bool TryIssueSpawnRequest()
  {
    if (MoonLordCountdown != 0 || _spawnRequestIssued)
    {
      return false;
    }

    _spawnRequestIssued = true;
    return true;
  }

  public void Reset()
  {
    MoonLordCountdown = 0;
    _spawnRequestIssued = false;
  }
}
