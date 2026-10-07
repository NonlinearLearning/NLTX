namespace Terraria.WorldSession.Components;

/// <summary>
/// 保存世界存档使用的七类矿石选择。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.WorldGen.SavedOreTiers。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/WorldGen.cs。</para>
/// <para>
/// 主要源成员：Copper（第 3320 行）； Iron（第 3322 行）； Silver（第 3324 行）； Gold（第 3326 行）； Cobalt（第 3328 行）；
/// Mythril（第 3330 行）； Adamantite（第 3332 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-p17-world-terrain-biomes-genvars-component-design.md。
/// </para>
/// <para>依据位置：第 1930 行。</para>
/// </remarks>
public sealed class WorldSavedOreTierStateComponent
{
  public WorldSavedOreTierStateComponent()
    : this(OreTierState.Uninitialized)
  {
  }

  public WorldSavedOreTierStateComponent(OreTierState value)
  {
    Value = value;
  }

  public OreTierState Value { get; private set; }

  public OreTierState CreateSnapshot()
  {
    return Value;
  }

  internal void Replace(OreTierState value)
  {
    Value = value;
  }
}
