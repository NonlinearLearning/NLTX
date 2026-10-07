using System;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// 保存海洋水位及邻接群落、湖泊和洞穴的生成限制。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.WorldBuilding.GenVars。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.WorldBuilding/GenVars.cs。</para>
/// <para>
/// 主要源成员：oceanWaterStartRandomMax（第 114 行）； oceanWaterForcedJungleLength（第 116 行）；
/// evilBiomeBeachAvoidance（第 118 行）； evilBiomeAvoidanceMidFixer（第 120 行）； lakesBeachAvoidance（第
/// 122 行）； smallHolesBeachAvoidance（第 124 行）； surfaceCavesBeachAvoidance（第 126 行）；
/// surfaceCavesBeachAvoidance2（第 128 行）。
/// </para>
/// <para>重组说明：GenerationId 用于拆分后的版本或实例生命周期管理。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-p17-world-terrain-biomes-genvars-component-design.md。
/// </para>
/// <para>依据位置：第 690 行。</para>
/// </remarks>
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
