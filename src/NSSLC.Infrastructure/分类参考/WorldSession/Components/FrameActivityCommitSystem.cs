namespace Terraria.WorldSession.Components;

public sealed class FrameActivityCommitSystem
{
  public void BeginFrame(FrameActivityStateComponent state)
  {
    ArgumentNullException.ThrowIfNull(state);
    state.ResetContributions();
  }

  public void Record(
    FrameActivityStateComponent state,
    FrameActivityContribution contribution)
  {
    ArgumentNullException.ThrowIfNull(state);
    state.AddContribution(contribution);
  }

  public void Commit(FrameActivityStateComponent state)
  {
    ArgumentNullException.ThrowIfNull(state);
    state.CommitContributions();
  }
}
