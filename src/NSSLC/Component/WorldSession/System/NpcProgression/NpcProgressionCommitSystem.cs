using System;
using Terraria.WorldSession.NpcProgression.Boss;
using Terraria.WorldSession.NpcProgression.Events;

namespace Terraria.WorldSession.NpcProgression;

public sealed class NpcProgressionCommitSystem
{
  private readonly BossDefeatProgressionStateComponent _bossProgression;
  private readonly EventDefeatProgressionStateComponent _eventProgression;

  public NpcProgressionCommitSystem(
    BossDefeatProgressionStateComponent bossProgression,
    EventDefeatProgressionStateComponent eventProgression)
  {
    _bossProgression = bossProgression ?? throw new ArgumentNullException(nameof(bossProgression));
    _eventProgression = eventProgression
      ?? throw new ArgumentNullException(nameof(eventProgression));
  }

  public BossDefeatProgressionCommitResult MarkBossDefeated(
    BossDefeatProgressionKind bossKind)
  {
    if (!Enum.IsDefined(typeof(BossDefeatProgressionKind), bossKind))
    {
      return new BossDefeatProgressionCommitResult(
        NpcProgressionCommitStatus.RejectedUnknownKind,
        bossKind);
    }

    var firstClear = _bossProgression.MarkDefeated(bossKind);
    return new BossDefeatProgressionCommitResult(
      firstClear
        ? NpcProgressionCommitStatus.Committed
        : NpcProgressionCommitStatus.AlreadyCleared,
      bossKind);
  }

  public EventDefeatProgressionCommitResult MarkEventDefeated(
    EventDefeatProgressionKind eventKind)
  {
    if (!Enum.IsDefined(typeof(EventDefeatProgressionKind), eventKind))
    {
      return new EventDefeatProgressionCommitResult(
        NpcProgressionCommitStatus.RejectedUnknownKind,
        eventKind);
    }

    var firstClear = _eventProgression.MarkDefeated(eventKind);
    return new EventDefeatProgressionCommitResult(
      firstClear
        ? NpcProgressionCommitStatus.Committed
        : NpcProgressionCommitStatus.AlreadyCleared,
      eventKind);
  }

  public void Reset()
  {
    _bossProgression.Reset();
    _eventProgression.Reset();
  }
}
