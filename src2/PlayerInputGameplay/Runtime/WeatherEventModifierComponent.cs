namespace NLTX.PlayerInputGameplay.Runtime;

public sealed class WeatherEventModifierComponent
{
  public int LadyBugRainBoost { get; private set; }

  public void SetLadyBugRainBoost(int boost)
  {
    if (boost < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(boost));
    }

    LadyBugRainBoost = boost;
  }
}
