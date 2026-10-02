namespace Terraria.WorldSession.Components;

public sealed class FrameControlAndTimeSkipStateComponent
{
  public bool MaxQueryEnabled { get; internal set; }

  public bool GamePaused { get; internal set; }

  public DiscoColorState DiscoColor { get; } = new();

  public bool ReHideCursorPending { get; internal set; }

  public int UpdatesInCurrentWindow { get; internal set; }

  public bool AutoJoinPending { get; internal set; }
}
