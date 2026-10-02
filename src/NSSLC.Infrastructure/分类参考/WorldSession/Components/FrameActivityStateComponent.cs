namespace Terraria.WorldSession.Components;

public sealed class FrameActivityStateComponent
{
  private int _activePlayerCount;
  private int _sleepingPlayerCount;
  private bool _anyActiveBoss;
  private bool _hadActiveInteractableProjectile;

  public int ActivePlayerCount { get; private set; }

  public int SleepingPlayerCount { get; private set; }

  public bool AnyActiveBoss { get; private set; }

  public bool HadActiveInteractableProjectile { get; private set; }

  internal void ResetContributions()
  {
    _activePlayerCount = 0;
    _sleepingPlayerCount = 0;
    _anyActiveBoss = false;
    _hadActiveInteractableProjectile = false;
  }

  internal void AddContribution(FrameActivityContribution contribution)
  {
    if (contribution.ActivePlayerCount < 0 || contribution.SleepingPlayerCount < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(contribution));
    }

    checked
    {
      _activePlayerCount += contribution.ActivePlayerCount;
      _sleepingPlayerCount += contribution.SleepingPlayerCount;
    }

    _anyActiveBoss |= contribution.AnyActiveBoss;
    _hadActiveInteractableProjectile |= contribution.HadActiveInteractableProjectile;
  }

  internal void CommitContributions()
  {
    ActivePlayerCount = _activePlayerCount;
    SleepingPlayerCount = _sleepingPlayerCount;
    AnyActiveBoss = _anyActiveBoss;
    HadActiveInteractableProjectile = _hadActiveInteractableProjectile;
  }
}
