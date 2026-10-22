using System;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// 保存世界生成难度、种子变体和特殊世界规则选择。
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
/// <para>
/// 用途：生成步骤读取腐化类型、特殊种子和生态或液体开关，决定本次生成使用的规则组合。
/// </para>
/// <para>
/// 当前实现：难度构造时要求非负，SeedVariant 不能为空白，其余标记在初始化时保存。规则兼容性、地形生成和实际效果由外部生成流程负责。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C197，L2（通用可选玩法）。分级理由：生成器的运行阶段、步骤计划、随机流、选择与取消仅在服务端生成世界时需要，预置地图可省去。
/// </para>
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

  /// <summary>
  /// 世界生成难度编号，构造时要求非负。
  /// </summary>
  public int Difficulty { get; }

  /// <summary>
  /// 世界生成种子变体标识。
  /// </summary>
  public string SeedVariant { get; }

  /// <summary>
  /// 世界邪恶类型选择，可为空。
  /// </summary>
  public WorldEvilType? WorldEvil { get; }

  /// <summary>
  /// 是否选择 Remix 世界规则。
  /// </summary>
  public bool IsRemixWorld { get; init; }

  /// <summary>
  /// 是否选择 Everything 世界规则。
  /// </summary>
  public bool IsEverythingWorld { get; init; }

  /// <summary>
  /// 是否选择 NoTraps 世界规则，具体生成内容以规则定义为准。
  /// </summary>
  public bool IsNoTrapsWorld { get; init; }

  /// <summary>
  /// 是否选择 Drunk 世界规则。
  /// </summary>
  public bool IsDrunkWorld { get; init; }

  /// <summary>
  /// 是否选择 For the worthy 对应的 Good 世界规则。
  /// </summary>
  public bool IsGoodWorld { get; init; }

  /// <summary>
  /// 是否选择饥荒联动世界规则。
  /// </summary>
  public bool IsDontStarveWorld { get; init; }

  /// <summary>
  /// 是否选择 Not the bees 世界规则。
  /// </summary>
  public bool IsNotTheBeesWorld { get; init; }

  /// <summary>
  /// 是否选择空岛世界规则。
  /// </summary>
  public bool IsSkyblockWorld { get; init; }

  /// <summary>
  /// 是否选择无地表世界规则。
  /// </summary>
  public bool IsNoSurfaceWorld { get; init; }

  /// <summary>
  /// 是否选择沙漠地表规则。
  /// </summary>
  public bool IsSurfaceDesertWorld { get; init; }

  /// <summary>
  /// 是否选择冰雪生态规则。
  /// </summary>
  public bool IsIceBiomeWorld { get; init; }

  /// <summary>
  /// 是否选择十周年世界规则。
  /// </summary>
  public bool IsTenthAnniversaryWorld { get; init; }

  /// <summary>
  /// 是否选择禁止感染传播规则。
  /// </summary>
  public bool NoInfection { get; init; }

  /// <summary>
  /// 是否选择额外液体生成规则。
  /// </summary>
  public bool ExtraLiquid { get; init; }

  /// <summary>
  /// 是否选择世界冻结规则。
  /// </summary>
  public bool WorldIsFrozen { get; init; }

  /// <summary>
  /// 是否选择从困难模式开始的规则。
  /// </summary>
  public bool StartInHardmode { get; init; }
}
