using System;

namespace Terraria.WorldGeneration.Components;

public sealed class OceanBiomeConstraintComponent
{
  public OceanBiomeConstraintComponent(
    long generationId,
    int oceanWaterStartRandomMax = 0,
    int oceanWaterForcedJungleLength = 0,
    int evilBiomeBeachAvoidance = 0,
    int evilBiomeAvoidanceMidFixer = 0,
    int lakesBeachAvoidance = 0,
    int smallHolesBeachAvoidance = 0,
    int surfaceCavesBeachAvoidance = 0,
    int surfaceCavesBeachAvoidance2 = 0)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    GenerationId = generationId;
    ReplaceConstraints(
      oceanWaterStartRandomMax,
      oceanWaterForcedJungleLength,
      evilBiomeBeachAvoidance,
      evilBiomeAvoidanceMidFixer,
      lakesBeachAvoidance,
      smallHolesBeachAvoidance,
      surfaceCavesBeachAvoidance,
      surfaceCavesBeachAvoidance2);
  }

  public long GenerationId { get; }

  public int OceanWaterStartRandomMax { get; private set; }

  public int OceanWaterForcedJungleLength { get; private set; }

  public int EvilBiomeBeachAvoidance { get; private set; }

  public int EvilBiomeAvoidanceMidFixer { get; private set; }

  public int LakesBeachAvoidance { get; private set; }

  public int SmallHolesBeachAvoidance { get; private set; }

  public int SurfaceCavesBeachAvoidance { get; private set; }

  public int SurfaceCavesBeachAvoidance2 { get; private set; }

  public void ReplaceConstraints(
    int oceanWaterStartRandomMax,
    int oceanWaterForcedJungleLength,
    int evilBiomeBeachAvoidance,
    int evilBiomeAvoidanceMidFixer,
    int lakesBeachAvoidance,
    int smallHolesBeachAvoidance,
    int surfaceCavesBeachAvoidance,
    int surfaceCavesBeachAvoidance2)
  {
    OceanWaterStartRandomMax = oceanWaterStartRandomMax;
    OceanWaterForcedJungleLength = oceanWaterForcedJungleLength;
    EvilBiomeBeachAvoidance = evilBiomeBeachAvoidance;
    EvilBiomeAvoidanceMidFixer = evilBiomeAvoidanceMidFixer;
    LakesBeachAvoidance = lakesBeachAvoidance;
    SmallHolesBeachAvoidance = smallHolesBeachAvoidance;
    SurfaceCavesBeachAvoidance = surfaceCavesBeachAvoidance;
    SurfaceCavesBeachAvoidance2 = surfaceCavesBeachAvoidance2;
  }

  public OceanBiomeConstraintSnapshot CreateSnapshot()
  {
    return new OceanBiomeConstraintSnapshot(
      GenerationId,
      OceanWaterStartRandomMax,
      OceanWaterForcedJungleLength,
      EvilBiomeBeachAvoidance,
      EvilBiomeAvoidanceMidFixer,
      LakesBeachAvoidance,
      SmallHolesBeachAvoidance,
      SurfaceCavesBeachAvoidance,
      SurfaceCavesBeachAvoidance2);
  }
}
