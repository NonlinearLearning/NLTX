using System;

namespace Terraria.WorldGeneration.Components;

public sealed class JungleRegionStructureComponent
{
  public JungleRegionStructureComponent(
    long generationId,
    int extraBastStatueCount = 0,
    int extraBastStatueCountMax = 0,
    int jungleOriginX = 0,
    int jungleMinX = 0,
    int jungleMaxX = 0,
    ushort jungleHut = 0,
    bool mudWall = false)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    GenerationId = generationId;
    ReplaceState(
      extraBastStatueCount,
      extraBastStatueCountMax,
      jungleOriginX,
      jungleMinX,
      jungleMaxX,
      jungleHut,
      mudWall);
  }

  public long GenerationId { get; }

  public int ExtraBastStatueCount { get; private set; }

  public int ExtraBastStatueCountMax { get; private set; }

  public int JungleOriginX { get; private set; }

  public int JungleMinX { get; private set; }

  public int JungleMaxX { get; private set; }

  public ushort JungleHut { get; private set; }

  public bool MudWall { get; private set; }

  public void ReplaceState(
    int extraBastStatueCount,
    int extraBastStatueCountMax,
    int jungleOriginX,
    int jungleMinX,
    int jungleMaxX,
    ushort jungleHut,
    bool mudWall)
  {
    ExtraBastStatueCount = extraBastStatueCount;
    ExtraBastStatueCountMax = extraBastStatueCountMax;
    JungleOriginX = jungleOriginX;
    JungleMinX = jungleMinX;
    JungleMaxX = jungleMaxX;
    JungleHut = jungleHut;
    MudWall = mudWall;
  }

  public JungleRegionStructureSnapshot CreateSnapshot()
  {
    return new JungleRegionStructureSnapshot(
      GenerationId,
      ExtraBastStatueCount,
      ExtraBastStatueCountMax,
      JungleOriginX,
      JungleMinX,
      JungleMaxX,
      JungleHut,
      MudWall);
  }
}
