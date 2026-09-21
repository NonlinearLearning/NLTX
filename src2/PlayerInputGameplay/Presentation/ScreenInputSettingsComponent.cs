namespace NLTX.PlayerInputGameplay.Presentation;

public sealed class ScreenInputSettingsComponent
{
  public int MultiplayerNpcSmoothingRange { get; private set; } = 300;

  public bool UseReducedMaxLiquids { get; private set; }

  public int PlayerOverheadChatMessageDisplayTime { get; private set; } = 400;

  public void Set(int multiplayerNpcSmoothingRange, bool useReducedMaxLiquids, int overheadChatMessageDisplayTime)
  {
    if (multiplayerNpcSmoothingRange < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(multiplayerNpcSmoothingRange));
    }

    if (overheadChatMessageDisplayTime < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(overheadChatMessageDisplayTime));
    }

    MultiplayerNpcSmoothingRange = multiplayerNpcSmoothingRange;
    UseReducedMaxLiquids = useReducedMaxLiquids;
    PlayerOverheadChatMessageDisplayTime = overheadChatMessageDisplayTime;
  }
}
