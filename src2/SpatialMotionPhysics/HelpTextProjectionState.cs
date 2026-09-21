namespace Terraria.SpatialMotionPhysics;

public sealed class HelpTextProjectionState
{
  public int HelpText { get; private set; }

  public int BartenderHelpTextIndex { get; private set; }

  public void Set(int helpText, int bartenderHelpTextIndex)
  {
    HelpText = helpText;
    BartenderHelpTextIndex = bartenderHelpTextIndex;
  }
}
