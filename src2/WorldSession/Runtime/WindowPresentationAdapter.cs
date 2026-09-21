namespace Terraria.WorldSession.Runtime;

public sealed class WindowPresentationAdapter
{
  public bool IsMouseVisible { get; private set; }

  public void SetMouseVisible(bool value)
  {
    IsMouseVisible = value;
  }
}
