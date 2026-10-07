namespace EntityEcs.Components;

/// <summary>
/// 保存实体液体接触类别和接触计时。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Entity。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Entity.cs。</para>
/// <para>
/// 主要源成员：wet（第 26 行）； shimmerWet（第 28 行）； honeyWet（第 30 行）； wetCount（第 32 行）； lavaWet（第 34 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/baseline/。</para>
/// <para>拆分依据文件：组件设计报告.md。</para>
/// <para>依据位置：第 29 行。</para>
/// </remarks>
public struct LiquidComponent
{
  public LiquidKind? DominantLiquidKind;
  public LiquidKind InLiquid;
  public bool IsHoneyWet;
  public bool IsLavaWet;
  public bool IsShimmerWet;
  public bool IsWet;
  public byte LiquidTimer;
  public long? ResolvedAtTick;

  public bool AnyWet => InLiquid != LiquidKind.Nano || IsWet || IsLavaWet ||
    IsHoneyWet || IsShimmerWet;
  public bool HasAnyLiquidContact => AnyWet;
  public bool HasNonWaterContact => InLiquid is LiquidKind.Lava or LiquidKind.Honey or
    LiquidKind.Shimmer || IsLavaWet || IsHoneyWet || IsShimmerWet;
  public bool HasNonWaterLiquidContact => HasNonWaterContact;
  public byte WetTickCount
  {
    get => LiquidTimer;
    set => LiquidTimer = value;
  }

}

public enum LiquidKind : byte
{
  Nano,
  Water,
  Shimmer,
  Honey,
  Lava,
}
