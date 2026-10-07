namespace Terraria.WorldGeneration.Components;

/// <summary>
/// 保存世界生成阶段使用的特殊种子开关。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.WorldGen。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/WorldGen.cs。</para>
/// <para>
/// 主要源成员：remixWorldGen（第 4306 行）； everythingWorldGen（第 4308 行）； noTrapsWorldGen（第 4310 行）；
/// drunkWorldGen（第 4312 行）； getGoodWorldGen（第 4314 行）； tenthAnniversaryWorldGen（第 4316 行）；
/// dontStarveWorldGen（第 4318 行）； notTheBees（第 4320 行）； skyblockWorldGen（第 4322 行）；
/// drunkWorldGenText（第 4324 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-p18-world-seeds-skyblock-definitions-component-design.md。
/// </para>
/// <para>依据位置：第 1465 行。</para>
/// </remarks>
public sealed class WorldGenerationSecretSeedFlagsComponent
{
  public WorldGenerationSecretSeedFlagsComponent(
    bool remixWorldGeneration = false,
    bool everythingWorldGeneration = false,
    bool noTrapsWorldGeneration = false,
    bool drunkWorldGeneration = false,
    bool getGoodWorldGeneration = false,
    bool tenthAnniversaryWorldGeneration = false,
    bool dontStarveWorldGeneration = false,
    bool notTheBeesWorld = false,
    bool skyblockWorldGeneration = false)
  {
    RemixWorldGeneration = remixWorldGeneration;
    EverythingWorldGeneration = everythingWorldGeneration;
    NoTrapsWorldGeneration = noTrapsWorldGeneration;
    DrunkWorldGeneration = drunkWorldGeneration;
    GetGoodWorldGeneration = getGoodWorldGeneration;
    TenthAnniversaryWorldGeneration = tenthAnniversaryWorldGeneration;
    DontStarveWorldGeneration = dontStarveWorldGeneration;
    NotTheBeesWorld = notTheBeesWorld;
    SkyblockWorldGeneration = skyblockWorldGeneration;
  }

  public bool RemixWorldGeneration { get; }

  public bool EverythingWorldGeneration { get; }

  public bool NoTrapsWorldGeneration { get; }

  public bool DrunkWorldGeneration { get; }

  public bool GetGoodWorldGeneration { get; }

  public bool TenthAnniversaryWorldGeneration { get; }

  public bool DontStarveWorldGeneration { get; }

  public bool NotTheBeesWorld { get; }

  public bool SkyblockWorldGeneration { get; }

  public bool DrunkWorldGenerationText => DrunkWorldGeneration;
}
