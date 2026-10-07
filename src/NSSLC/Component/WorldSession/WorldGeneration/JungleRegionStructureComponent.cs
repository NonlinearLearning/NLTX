using System;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// 保存丛林范围、泥墙、小屋和巴斯特雕像生成计数。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.WorldBuilding.GenVars。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.WorldBuilding/GenVars.cs。</para>
/// <para>
/// 主要源成员：extraBastStatueCount（第 162 行）； extraBastStatueCountMax（第 164 行）； jungleOriginX（第 166 行）；
/// jungleMinX（第 168 行）； jungleMaxX（第 170 行）； jungleHut（第 172 行）； mudWall（第 174 行）。
/// </para>
/// <para>重组说明：GenerationId 用于拆分后的版本或实例生命周期管理。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-p17-world-terrain-biomes-genvars-component-design.md。
/// </para>
/// <para>依据位置：第 961 行。</para>
/// </remarks>
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
