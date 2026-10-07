using System;

namespace Terraria.WorldSession.NpcProgression.Invasion;

public sealed class InvasionWaveProgressSystem
{
  private readonly InvasionWaveProgressStateComponent _state;

  public InvasionWaveProgressSystem(InvasionWaveProgressStateComponent state)
  {
    _state = state ?? throw new ArgumentNullException(nameof(state));
  }

  public InvasionWaveProgressCommitResult BeginWave()
  {
    _state.Reset();
    return CommitProgress(0.0f, 0.0f, 1);
  }

  public InvasionWaveProgressCommitResult CommitProgress(
    float totalInvasionPoints,
    float waveKills,
    int waveNumber)
  {
    InvasionWaveProgressSyncView previous = CreateSyncView();
    _state.Commit(totalInvasionPoints, waveKills, waveNumber);
    InvasionWaveProgressSyncView current = CreateSyncView();
    InvasionWaveProgressCommitStatus status = previous == current
      ? InvasionWaveProgressCommitStatus.AlreadyCommitted
      : InvasionWaveProgressCommitStatus.Committed;
    return new InvasionWaveProgressCommitResult(status, current);
  }

  public InvasionWaveProgressCommitResult StopWave()
  {
    InvasionWaveProgressSyncView previous = CreateSyncView();
    _state.Reset();
    InvasionWaveProgressSyncView current = CreateSyncView();
    InvasionWaveProgressCommitStatus status = previous == current
      ? InvasionWaveProgressCommitStatus.AlreadyCommitted
      : InvasionWaveProgressCommitStatus.Committed;
    return new InvasionWaveProgressCommitResult(status, current);
  }

  public InvasionWaveProgressSyncView CreateSyncView()
  {
    return new InvasionWaveProgressSyncView(
      _state.TotalInvasionPoints,
      _state.WaveKills,
      _state.WaveNumber);
  }
}
