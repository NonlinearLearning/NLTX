namespace Terraria.WorldSession.Runtime;

public sealed class GameTimePort
{
  public FrameTimingInput Current { get; private set; }

  public void Set(FrameTimingInput input)
  {
    Current = input;
  }
}
