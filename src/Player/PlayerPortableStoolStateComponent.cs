namespace Terraria.Player;

public sealed class PlayerPortableStoolStateComponent
{
  public bool HasAStool { get; internal set; }

  public bool IsInUse { get; internal set; }

  public int HeightBoost { get; internal set; }

  public int VisualYOffset { get; internal set; }

  public int MapYOffset { get; internal set; }

  internal void ResetForLifecycle()
  {
    HasAStool = false;
    IsInUse = false;
    HeightBoost = 0;
    VisualYOffset = 0;
    MapYOffset = 0;
  }
}
