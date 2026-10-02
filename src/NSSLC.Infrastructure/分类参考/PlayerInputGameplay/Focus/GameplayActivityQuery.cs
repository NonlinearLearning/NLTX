namespace NLTX.PlayerInputGameplay.Focus;

public sealed class GameplayActivityQuery
{
  public bool GameplayActive(bool selectedApplication, bool gamePaused)
  {
    return selectedApplication && !gamePaused;
  }

  public bool UpdateVisualEffects(bool selectedApplication, bool gamePaused)
  {
    return GameplayActive(selectedApplication, gamePaused);
  }

  public bool AllowRain(bool selectedApplication, bool gamePaused)
  {
    return GameplayActive(selectedApplication, gamePaused);
  }

  public bool AllowCountingPlayerTime(
    bool selectedApplication,
    bool gamePaused,
    bool instanceActive,
    bool gameMenu)
  {
    var pausedOutsideWindow = gamePaused && !selectedApplication;
    var result = instanceActive && !pausedOutsideWindow;
    return gameMenu ? false : result;
  }
}
