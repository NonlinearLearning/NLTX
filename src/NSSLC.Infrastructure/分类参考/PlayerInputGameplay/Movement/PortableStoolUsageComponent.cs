namespace NLTX.PlayerInputGameplay.Movement;

public sealed class PortableStoolUsageComponent
{
  public bool HasAStool { get; private set; }

  public bool IsInUse { get; private set; }

  public int HeightBoost { get; private set; }

  public int VisualYOffset { get; private set; }

  public int MapYOffset { get; private set; }

  public void SetStats(bool hasAStool, int heightBoost, int visualYOffset, int mapYOffset)
  {
    if (heightBoost < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(heightBoost));
    }

    HasAStool = hasAStool;
    HeightBoost = heightBoost;
    VisualYOffset = visualYOffset;
    MapYOffset = mapYOffset;
  }

  public void SetInUse(bool inUse)
  {
    IsInUse = inUse && HasAStool;
  }

  public void Reset()
  {
    HasAStool = false;
    IsInUse = false;
    HeightBoost = 0;
    VisualYOffset = 0;
    MapYOffset = 0;
  }
}
