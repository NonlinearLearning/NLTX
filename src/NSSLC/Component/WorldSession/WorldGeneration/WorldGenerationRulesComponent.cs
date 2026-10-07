using System;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// 保存世界生成难度、特殊种子和群落规则。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Main。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Main.cs。</para>
/// <para>主要源成员：GameMode（第 1318 行）。</para>
/// <para>拆分来源：Terraria.WorldGen。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/WorldGen.cs。</para>
/// <para>
/// 主要源成员：crimson（第 4113 行）； remixWorldGen（第 4306 行）； everythingWorldGen（第 4308 行）；
/// noTrapsWorldGen（第 4310 行）； drunkWorldGen（第 4312 行）； getGoodWorldGen（第 4314 行）；
/// tenthAnniversaryWorldGen（第 4316 行）； dontStarveWorldGen（第 4318 行）； notTheBees（第 4320 行）；
/// skyblockWorldGen（第 4322 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>拆分依据文件：2026-09-05-version4-world-generation-and-ecology-component-design.md。</para>
/// <para>依据位置：第 163 行。</para>
/// </remarks>
public sealed class WorldGenerationRulesComponent
{
  public WorldGenerationRulesComponent(
    int difficulty,
    string seedVariant,
    WorldEvilType? worldEvil = null)
  {
    if (difficulty < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(difficulty));
    }

    ArgumentException.ThrowIfNullOrWhiteSpace(seedVariant);
    Difficulty = difficulty;
    SeedVariant = seedVariant;
    WorldEvil = worldEvil;
  }

  public int Difficulty { get; }

  public string SeedVariant { get; }

  public WorldEvilType? WorldEvil { get; }

  public bool IsRemixWorld { get; init; }

  public bool IsEverythingWorld { get; init; }

  public bool IsNoTrapsWorld { get; init; }

  public bool IsDrunkWorld { get; init; }

  public bool IsGoodWorld { get; init; }

  public bool IsDontStarveWorld { get; init; }

  public bool IsNotTheBeesWorld { get; init; }

  public bool IsSkyblockWorld { get; init; }

  public bool IsNoSurfaceWorld { get; init; }

  public bool IsSurfaceDesertWorld { get; init; }

  public bool IsIceBiomeWorld { get; init; }

  public bool IsTenthAnniversaryWorld { get; init; }

  public bool NoInfection { get; init; }

  public bool ExtraLiquid { get; init; }

  public bool WorldIsFrozen { get; init; }

  public bool StartInHardmode { get; init; }
}
