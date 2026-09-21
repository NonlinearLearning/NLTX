using Terraria.WorldSession.Components;

namespace Terraria.WorldSession.Runtime;

public sealed class FrameControlSystem
{
  public void Apply(
    FrameControlAndTimeSkipStateComponent state,
    FrameControlCommand command)
  {
    ArgumentNullException.ThrowIfNull(state);
    state.GamePaused = command.GamePaused;
    state.MaxQueryEnabled = command.MaxQueryEnabled;
  }

  public bool TryConsumeCursorRequest(
    FrameControlAndTimeSkipStateComponent state,
    out CursorVisibilityCommand? command)
  {
    ArgumentNullException.ThrowIfNull(state);
    if (!state.ReHideCursorPending)
    {
      command = null;
      return false;
    }

    state.ReHideCursorPending = false;
    command = new CursorVisibilityCommand(HideCursor: true);
    return true;
  }

  public void RequestAutoJoin(FrameControlAndTimeSkipStateComponent state)
  {
    ArgumentNullException.ThrowIfNull(state);
    state.AutoJoinPending = true;
  }

  public bool TryConsumeAutoJoin(
    FrameControlAndTimeSkipStateComponent state,
    out AutoJoinRequest? request)
  {
    ArgumentNullException.ThrowIfNull(state);
    if (!state.AutoJoinPending)
    {
      request = null;
      return false;
    }

    state.AutoJoinPending = false;
    request = new AutoJoinRequest();
    return true;
  }
}
